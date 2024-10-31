using KushBot.Resources.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using System.IO;
using System.Data;
using Microsoft.Data.Sqlite;
using KushBot.DataClasses;
using Microsoft.EntityFrameworkCore;
using KushBot.Global;
using KushBot.DataClasses.Enums;
using KushBot.Services;

namespace KushBot.Data;

[Flags]
public enum UserDtoFeatures : long
{
    None = 0,
    Pets = 1 << 0,
    Items = 1 << 1,
    Plots = 1 << 2,
    Claims = 1 << 3,
    Buffs = 1 << 4,
    Quests = 1 << 5,
    Pictures = 1 << 6,
    Infections = 1 << 7,
    All = -1L
}

public static class Data
{
    public static KushBotUser GetKushBotUser(ulong userId, UserDtoFeatures features = UserDtoFeatures.None)
    {
        using var dbContext = new SqliteDbContext();

        var query = dbContext.Users.Where(e => e.Id == userId);

        if (features.HasFlag(UserDtoFeatures.Items))
        {
            query = query
                .Include(e => e.Items)
                    .ThenInclude(e => e.ItemStats);
        }

        if (features.HasFlag(UserDtoFeatures.Buffs))
        {
            query = query.Include(e => e.UserBuffs);
        }

        var user = query.FirstOrDefault();

        if (features.HasFlag(UserDtoFeatures.Quests))
        {
            user.UserQuests = new UserQuests(dbContext.Quests.Where(e => e.UserId == user.Id).Include(e => e.Requirements.OrderBy(e => e.Type)).ToList());

            var relevantLogTypes = user.UserQuests.Where(e => !e.IsCompleted).SelectMany(e => e.GetRelevantEventTypes()).Distinct();

            user.UserEvents = new UserEvents(dbContext.UserEvents.Where(e => e.UserId == user.Id
                                                    && e.CreationTime > TimeHelper.LastMidnight
                                                    && relevantLogTypes.Contains(e.Type))
                                            .AsNoTracking()
                                            .OrderBy(e => e.CreationTime)
                                            .ToList());

            if (user.UserQuests.Any(e => !e.IsCompleted) && user.Items == null)
            {
                user.Items = GetUserItemsInternal(dbContext, userId, true);
            }

            if (user.UserQuests.Any(e => !e.IsCompleted) && user.Pets == null)
            {
                user.Pets = GetUserPetsInternal(dbContext, userId, user.Items);
            }
        }

        if (features.HasFlag(UserDtoFeatures.Pets))
        {
            user.Pets = GetUserPetsInternal(dbContext, userId, user.Items);
        }

        return user;
    }

    public static UserPets GetUserPets(ulong userId)
    {
        using var dbContext = new SqliteDbContext();
        return GetUserPetsInternal(dbContext, userId);
    }

    private static UserPets GetUserPetsInternal(SqliteDbContext dbContext, ulong userId, UserItems items = null)
    {
        var pets = new UserPets(dbContext.UserPets.Where(e => e.UserId == userId).ToList());
        var equippedItems = items ?? GetUserItemsInternal(dbContext, userId, true);

        foreach (var itemStat in equippedItems.SelectMany(e => e.ItemStats))
        {
            if (itemStat.PetType != null && pets.ContainsKey(itemStat.PetType.Value))
            {
                pets[itemStat.PetType.Value].ItemLevel += (int)itemStat.Bonus;
            }
        }

        return pets;
    }

    private static UserItems GetUserItemsInternal(SqliteDbContext dbContext, ulong userId, bool? isEquipped = null)
    {
        return (dbContext.Items
            .Include(e => e.ItemStats)
            .Where(e => e.OwnerId == userId && (!isEquipped.HasValue || e.IsEquipped == isEquipped.Value))
            .ToList() as UserItems) ?? new();
    }

    public static UserItems GetUserItems(ulong userId, bool? isEquipped = null)
    {
        using var dbContext = new SqliteDbContext();
        return GetUserItemsInternal(dbContext, userId, isEquipped);
    }

    public static async Task SaveKushBotUserAsync(KushBotUser user, UserDtoFeatures features = UserDtoFeatures.None)
    {
        using var dbContext = new SqliteDbContext();

        dbContext.Users.Update(user);

        if (user.Items != null && features.HasFlag(UserDtoFeatures.Items))
        {
            dbContext.Items.UpdateRange(user.Items);
        }

        if (user.UserBuffs != null && user.UserBuffs.Any() && features.HasFlag(UserDtoFeatures.Buffs))
        {
            SaveUserBuffsInternal(dbContext, user.UserBuffs);
        }

        if (user.Pets != null && user.Pets.Any() && features.HasFlag(UserDtoFeatures.Pets))
        {
            dbContext.UserPets.UpdateRange(user.Pets.Select(e => e.Value));
        }

        await dbContext.SaveChangesAsync();
    }

    public static void SaveUserBuffsInternal(SqliteDbContext context, UserBuffs buffs)
    {
        context.ConsumableBuffs.UpdateRange(buffs.NotDepleted);
        context.ConsumableBuffs.RemoveRange(buffs.Depleted);
    }

    public static void AddUserEvent(KushBotUser user, UserEventType type)
    {
        user.UserEvents.Add(new UserEvent
        {
            CreationTime = DateTime.Now,
            Type = type,
            UserId = user.Id,
            User = user,
        });
    }

    public static (List<Quest> freshCompleted, bool lastDailyCompleted, bool lastWeeklyCompleted) AttemptCompleteQuests(KushBotUser user)
    {
        var relevantQuests = user.UserQuests.InProgress;

        if (!relevantQuests.Any())
            return ([], false, false);

        var freshCompletedQuests = new List<Quest>();

        int eggs = 0;

        foreach (var quest in relevantQuests)
        {
            var relevantEventTypes = quest.User.UserEvents.Where(e => quest.GetRelevantEventTypes().Contains(e.Type)).ToList();

            quest.IsCompleted = quest.Requirements.All(e => e.Validate(relevantEventTypes));

            if (quest.IsCompleted)
            {
                freshCompletedQuests.Add(quest);
                if (Random.Shared.NextDouble() > 0.97)
                {
                    eggs++;
                }
            }
        }


        var lastDailyCompleted = freshCompletedQuests.Any(e => e.IsDaily) && user.UserQuests.Where(e => e.IsDaily).All(e => e.IsCompleted);
        var lastWeeklyCompleted = freshCompletedQuests.Any(e => !e.IsDaily) && user.UserQuests.Where(e => !e.IsDaily).All(e => e.IsCompleted);

        user.Eggs += eggs;
        user.Balance += relevantQuests.Where(e => e.IsCompleted).Sum(e => e.GetQuestReward());

        if (lastDailyCompleted)
        {
            user.Balance += user.GetDailiesCompleteReward();
        }

        if (lastWeeklyCompleted)
        {
            //Todo check if works if finished by beg. Also gonna have to refactor to account for variability of item rarity
            user.Items.Add(user.GetWeekliesCompleteReward());
        }

        return (freshCompletedQuests, lastDailyCompleted, lastWeeklyCompleted);
    }

    public static IEnumerable<Quest> CreateQuestEntities(KushBotUser user)
    {
        QuestRequirementFactory factory = new();

        var additionalCount = (user.Pets?[PetType.Maybich]?.Tier ?? 0) * 25 + user?.Items?.GetStatTypeBonus(ItemStatType.QuestSlotChance) ?? 0;
        int count = 3 + (int)(additionalCount / 100) + (Random.Shared.Next(1, 101) < (additionalCount % 100) ? 1 : 0);

        var permittedQuests = QuestBases.QuestBaseList;

        if ((user.Pets?.Count ?? 0) == 0)
        {
            permittedQuests = permittedQuests.Where(e => !e.Prerequisites.Contains(Prerequisite.AnyPet)).ToList();
        }

        if ((user.Pets?.All(e => e.Value.Level == 99) ?? true))
        {
            permittedQuests = permittedQuests.Where(e => !e.Prerequisites.Contains(Prerequisite.AnyPetNotMaxLevel)).ToList();
        }

        if (!user.Pets?.Any() ?? true)
        {
            permittedQuests = permittedQuests.Where(e => !e.Prerequisites.Contains(Prerequisite.AnyPet)).ToList();
        }

        if (user.Pets?[PetType.Jew] == null)
        {
            permittedQuests = permittedQuests.Where(e => !e.Prerequisites.Contains(Prerequisite.JewPet)).ToList();
        }

        var selectedQuests = permittedQuests.OrderBy(e => Random.Shared.NextDouble()).Take(count).ToList();

        foreach (var questBase in selectedQuests)
        {
            yield return new Quest
            {
                Type = questBase.Type,
                UserId = user.Id,
                IsCompleted = false,
                IsDaily = true,
                QuestBaseIndex = QuestBases.QuestBaseList.IndexOf(questBase),
                Requirements = questBase.RequirementRewardMap
                    .Select(e => factory.Create(e.Key, GetQuestRequirementValue(user, e.Value.From, e.Key).ToString()))
                    .ToList()
            };
        }
    }

    public static IEnumerable<Quest> CreateWeeklyQuestEntities(KushBotUser user)
    {
        QuestRequirementFactory factory = new();

        int count = 2;

        var selectedQuests = QuestBases.WeeklyQuestBaseList.OrderBy(e => Random.Shared.NextDouble()).Take(count).ToList();

        foreach (var questBase in selectedQuests)
        {
            yield return new Quest
            {
                Type = questBase.Type,
                UserId = user.Id,
                IsCompleted = false,
                IsDaily = false,
                QuestBaseIndex = QuestBases.WeeklyQuestBaseList.IndexOf(questBase),
                Requirements = questBase.RequirementRewardMap
                    .Select(e => factory.Create(e.Key, GetQuestRequirementValue(user, e.Value.From, e.Key).ToString()))
                    .ToList()
            };
        }
    }

    private static int GetQuestRequirementValue(KushBotUser user, int requiredValue, QuestRequirementType type)
    {
        if (type == QuestRequirementType.Chain)
        {
            return requiredValue;
        }

        var TPL = user.Pets?.Sum(e => e.Value.CombinedLevel) ?? 0;

        return requiredValue + ((int)(4 * Math.Pow(TPL, 1.08) * ((double)requiredValue / 1400)));

        //if (Desc.Contains("Reach"))
        //{
        //    int reachRet = (int)(13 * Math.Pow(petlvl, 1.15));
        //    return reachRet + CompleteReq;
        //}
        //if (Desc.Contains("Beg") || Desc.Contains("Yoink") || Desc.Contains("begging"))
        //{
        //    return CompleteReq;
        //}

        //if (Desc.Contains("**Flip 60"))
        //{
        //    return 3;
        //}

        //if (Desc.Contains("Duel"))
        //{
        //    int temp = (int)(4 * Math.Pow(petlvl, 1.08) * ((double)CompleteReq / 1400));
        //    return temp + CompleteReq;
        //}
    }

    public static async Task DeleteUser(ulong id)
    {
        using var DbContext = new SqliteDbContext();
        var user = DbContext.Users.FirstOrDefault(e => e.Id == id);
        DbContext.Users.Remove(user);
        await DbContext.SaveChangesAsync();
    }

    public static async Task RefreshLastVendorPurchaseAsync(ulong id)
    {
        using var DbContext = new SqliteDbContext();
        var user = DbContext.Users.FirstOrDefault(e => e.Id == id);
        user.LastVendorPurchase = DateTime.MinValue;
        DbContext.Users.Update(user);
        await DbContext.SaveChangesAsync();
    }

    private static string GetConnectionString()
    {
        return $@"Data Source= ./Data/Database.sqlite";
    }

    public static List<string> ReadWeebShit()
    {
        string path = "Data/Kemonos";
        string[] files = Directory.GetFiles(path);

        return files.ToList();

    }

    public static List<string> ReadCarShit()
    {
        string path = "Data/Cars";
        string[] files = Directory.GetFiles(path);

        return files.ToList();
    }

    public static async Task<bool> MakeRowForUser(ulong UserId)
    {
        using var DbContext = new SqliteDbContext();

        if (DbContext.Users.Where(x => x.Id == UserId).Count() < 1)
        {
            KushBotUser newUser = new KushBotUser(UserId);

            string path = @"D:\KushBot\Kush Bot\KushBot\KushBot\Data\";
            char seperator = '\\';

            if (!DiscordBotService.BotTesting)
            {
                seperator = '/';
                path = @"Data/";
            }
            try
            {
                System.IO.File.Copy($@"{path}Pictures{seperator}{newUser.SelectedPicture}.jpg", $@"{path}Portraits{seperator}{newUser.Id}.png");
            }
            catch { }

            DbContext.Users.Add(newUser);
            DbContext.Quests.AddRange(CreateQuestEntities(newUser));

            await DbContext.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public static async Task SaveDailyGiveBaps(ulong UserId, int subtraction)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Users.Where(x => x.Id == UserId).Count() < 1)
            {
                DbContext.Users.Add(new KushBotUser(UserId));
            }
            else
            {
                KushBotUser Current = DbContext.Users.Where(x => x.Id == UserId).FirstOrDefault();

                Current.DailyGive -= subtraction;

                DbContext.Users.Update(Current);
            }
            await DbContext.SaveChangesAsync();
        }
    }

    public static int GetRemainingDailyGiveBaps(ulong UserId)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Users.Where(x => x.Id == UserId).Count() < 1)
                return 0;

            return DbContext.Users.Where(x => x.Id == UserId).Select(x => x.DailyGive).FirstOrDefault();

        }
    }

    public static int GetRageDuration(ulong UserId)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Users.Where(x => x.Id == UserId).Count() < 1)
                return 0;

            return DbContext.Users.Where(x => x.Id == UserId).Select(x => x.RageDuration).FirstOrDefault();
        }
    }

    public static DateTime GetLastRage(ulong UserId)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Users.Where(x => x.Id == UserId).Count() < 1)
                return DateTime.Now.AddHours(-9);

            return DbContext.Users.Where(x => x.Id == UserId).Select(x => x.LastTylerRage).FirstOrDefault();
        }
    }

    public static DateTime GetLastYoink(ulong UserId)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Users.Where(x => x.Id == UserId).Count() < 1)
                return DateTime.Now.AddHours(-9);

            return DbContext.Users.Where(x => x.Id == UserId).Select(x => x.LastYoink).FirstOrDefault();
        }
    }

    public static int GetBalance(ulong UserId)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Users.Where(x => x.Id == UserId).Count() < 1)
                return 0;

            return DbContext.Users.Where(x => x.Id == UserId).Select(x => x.Balance).FirstOrDefault();
        }
    }

    public static DateTime GetRedeemDate(ulong UserId)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Users.Where(x => x.Id == UserId).Count() < 1)
                return DateTime.Now;

            return DbContext.Users.Where(x => x.Id == UserId).Select(x => x.RedeemDate).FirstOrDefault();
        }
    }

    public static async Task SaveRedeemDate(ulong UserId, DateTime? date = null)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Users.Where(x => x.Id == UserId).Count() < 1)
            {
                //no row for user, create one
                DbContext.Users.Add(new KushBotUser(UserId));
            }
            else
            {
                KushBotUser Current = DbContext.Users.Where(x => x.Id == UserId).FirstOrDefault();
                Current.RedeemDate = (date ?? DateTime.Now);
                DbContext.Users.Update(Current);
            }
            await DbContext.SaveChangesAsync();
        }
    }

    public static async Task SaveRageDuration(ulong UserId, int rageDuration)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Users.Where(x => x.Id == UserId).Count() < 1)
            {
                //no row for user, create one
                DbContext.Users.Add(new KushBotUser(UserId));
            }
            else
            {
                KushBotUser Current = DbContext.Users.Where(x => x.Id == UserId).FirstOrDefault();
                Current.RageDuration += rageDuration;
                DbContext.Users.Update(Current);
            }
            await DbContext.SaveChangesAsync();
        }
    }

    public static async Task SaveLastRage(ulong UserId, DateTime lastRage)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Users.Where(x => x.Id == UserId).Count() < 1)
            {
                //no row for user, create one
                DbContext.Users.Add(new KushBotUser(UserId));
            }
            else
            {
                KushBotUser Current = DbContext.Users.Where(x => x.Id == UserId).FirstOrDefault();
                Current.LastTylerRage = lastRage;
                DbContext.Users.Update(Current);
            }
            await DbContext.SaveChangesAsync();
        }
    }

    public static async Task SaveBalance(ulong UserId, int Amount, bool Gambling, IMessageChannel channelForRage = null, int gambleAmount = 0)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Users.Where(x => x.Id == UserId).Count() < 1)
            {
                //no row for user, create one
                DbContext.Users.Add(new KushBotUser(UserId));
            }
            else
            {
                KushBotUser Current = GetKushBotUser(UserId, UserDtoFeatures.Pets);
                Current.Balance += Amount;
                if (Gambling && GetRageDuration(UserId) > 0)
                {
                    double RageCashDbl = 0;
                    int RageCash = 0;

                    const double A = 2;
                    const double B = 5;
                    const double D = 0.9;

                    if (Amount > 0)
                    {
                        int lvl = Current.Pets[PetType.TylerJuan].CombinedLevel;
                        //(a*Baps-(Baps^2/(b+Lv+Baps/c)))*d 
                        //RageCashDbl = (A * Amount - ((Math.Pow(Amount, 2)) / (B + lvl + (Amount / A)))) * D;
                        RageCashDbl = (A * gambleAmount - ((Math.Pow(gambleAmount, 2)) / (B + lvl + (gambleAmount / A)))) * D;
                        double coefficient = RageCashDbl / gambleAmount;
                        RageCashDbl = coefficient * Amount;

                    }
                    else
                    {
                        Random rnd = new();
                        RageCashDbl = rnd.Next(2, 7);
                    }

                    RageCash = (int)Math.Round(RageCashDbl);
                    Current.RageDuration -= 1;


                    Current.RageCash += RageCash;

                    if (Current.RageDuration == 0)
                    {
                        int temp = Current.RageCash;
                        if (temp < 0)
                        {
                            temp = temp * -1;
                        }

                        await DiscordBotService.EndRage(UserId, Current.RageCash, channelForRage);
                        Current.Balance += temp;
                        Current.RageCash = 0;

                    }
                }
                DbContext.Users.Update(Current);
            }
            await DbContext.SaveChangesAsync();
        }
    }

    public static int GetTicketCount(ulong userId)
    {
        using (var DbContext = new SqliteDbContext())
        {
            return DbContext.Users.Where(x => x.Id == userId).Select(x => x.Tickets).FirstOrDefault();
        }
    }

    public static async Task SaveTicket(ulong UserId, bool addition)
    {
        using (var DbContext = new SqliteDbContext())
        {
            KushBotUser current = DbContext.Users.Where(x => x.Id == UserId).FirstOrDefault();

            if (addition)
            {
                if (GetTicketCount(UserId) < 3)
                    current.Tickets += 1;
            }
            else
            {
                current.Tickets -= 1;
            }
            DbContext.Users.Update(current);

            await DbContext.SaveChangesAsync();
        }
    }

    public static async Task<int> InfectionConsumeBapsAsync(ulong userId)
    {
        using var DbContext = new SqliteDbContext();
        List<Infection> qualifiedInfections = await DbContext.UserInfections.Where(e => e.OwnerId == userId).ToListAsync();

        qualifiedInfections = qualifiedInfections
            .Where(e => (e.State == InfectionState.Tyrant || e.State == InfectionState.NecroticSovereign || e.State == InfectionState.EldritchPatriarch)
                        && e.BapsDrained <= 6000)
            .ToList();

        if (!qualifiedInfections.Any())
            return 0;

        int bapsConsumed = 0;

        Random rnd = new Random();

        Infection infection = qualifiedInfections[rnd.Next(0, qualifiedInfections.Count)];

        switch (infection.State)
        {
            case InfectionState.Tyrant:
                bapsConsumed += rnd.Next(80, 141);
                break;
            case InfectionState.NecroticSovereign:
                bapsConsumed += rnd.Next(180, 281);
                break;
            case InfectionState.EldritchPatriarch:
                bapsConsumed += rnd.Next(300, 561);
                break;
            default:
                break;
        }


        int userBaps = Data.GetBalance(userId);
        bapsConsumed = Math.Min(bapsConsumed, userBaps);
        bapsConsumed = bapsConsumed < 0 ? 0 : bapsConsumed;
        infection.BapsDrained += bapsConsumed;
        DbContext.UserInfections.Update(infection);

        await DbContext.SaveChangesAsync();

        await Data.SaveBalance(userId, -1 * bapsConsumed, false);

        return bapsConsumed;
    }

    public static Dictionary<ulong, List<UserTutoProgress>> LoadAllUsersTutorialProgress()
    {
        try
        {
            using var DbContext = new SqliteDbContext();
            var allData = DbContext.UserTutoProgress.ToList();

            var ret = allData.GroupBy(e => e.UserId).ToDictionary(e => e.Key, e => e.ToList());
            return ret;
        }
        catch (SqliteException ex)
        {
            return [];
        }
    }

    public static async Task<UserTutoProgress> InsertTutoStepCompletedAsync(ulong userId, int page, int stepIndex)
    {
        using var DbContext = new SqliteDbContext();

        UserTutoProgress step = new()
        {
            Id = Guid.NewGuid(),
            Page = page,
            UserId = userId,
            TaskIndex = stepIndex,
        };

        DbContext.UserTutoProgress.Add(step);

        await DbContext.SaveChangesAsync();

        return step;
    }


    public static async Task RemoveDeprecatedTutoStepsAsync(ulong userId, int maxPage)
    {
        using (var cnn = new SqliteDbContext())
        {
            var conn = new SqliteConnection(GetConnectionString());
            conn.Open();
            SqliteCommand cmd = new SqliteCommand(
                $"""
                DELETE
                FROM UserTutoProgress
                WHERE [Page] <= {maxPage} AND [UserId] = {userId} 
                """
                );

            cmd.Connection = conn;
            await cmd.ExecuteNonQueryAsync();
            conn.Close();
        }
    }

    public static async Task<bool> UserHasBuffsAsync(ulong userId)
    {
        using var dbContext = new SqliteDbContext();
        return await dbContext.ConsumableBuffs.AnyAsync(e => e.OwnerId == userId);
    }

    public static List<ConsumableBuff> GetConsumableBuffList(ulong userId)
    {
        using var dbContext = new SqliteDbContext();
        return dbContext.ConsumableBuffs.Where(e => e.OwnerId == userId).ToList();
    }


    public static NyaClaim GetClaimBySortIndex(ulong userId, int sortIndex)
    {
        using var dbContext = new SqliteDbContext();
        return dbContext.NyaClaims.FirstOrDefault(e => e.OwnerId == userId && e.SortIndex == sortIndex);
    }

    public static async Task FixUserClaimSortIndexes(ulong userId)
    {
        using var dbContext = new SqliteDbContext();
        var nyaClaims = dbContext.NyaClaims.Where(e => e.OwnerId == userId).ToList();
        nyaClaims.FixSortIndex();
        await dbContext.SaveChangesAsync();
    }

    public static async Task ConcludeNyaTradeAsync(NyaClaimTrade trade)
    {
        using var dbContext = new SqliteDbContext();

        if (trade.Suggester.NyaClaim != null)
        {
            var trackedSugesteeClaim = dbContext.NyaClaims.FirstOrDefault(e => e.Id == trade.Suggester.NyaClaim.Id);
            trackedSugesteeClaim.OwnerId = trade.Respondee.UserId;
        }


        if (trade.Respondee.NyaClaim != null)
        {
            var trackedRespondeeClaim = dbContext.NyaClaims.FirstOrDefault(e => e.Id == trade.Respondee.NyaClaim.Id);
            trackedRespondeeClaim.OwnerId = trade.Suggester.UserId;
        }



        await dbContext.SaveChangesAsync();
        NyaClaimGlobals.NyaTrades.RemoveAll(e => e.Suggester.UserId == trade.Suggester.UserId);

        await FixUserClaimSortIndexes(trade.Suggester.UserId);
        await FixUserClaimSortIndexes(trade.Respondee.UserId);
    }
}
