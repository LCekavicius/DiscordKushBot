using KushBot.Resources.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Data;
using KushBot.DataClasses;
using KushBot.Global;

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
