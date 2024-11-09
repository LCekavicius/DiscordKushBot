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
