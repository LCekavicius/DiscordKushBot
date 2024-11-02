using Discord;
using Discord.WebSocket;
using KushBot.DataClasses;
using KushBot.DataClasses.enums;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Services;

public class TutorialManager
{
    private readonly SqliteDbContext dbContext;
    private readonly DiscordSocketClient client;

    public TutorialManager(SqliteDbContext _dbContext, DiscordSocketClient _client)
    {
        dbContext = _dbContext;
        client = _client;
        client.Ready += LoadInitial;
    }

    public static Dictionary<ulong, List<UserTutoProgress>> UserTutorialProgressDict { get; set; } = new();

    private static Dictionary<int, TutorialPageReward> TutorialPageRewardDict { get; set; } =
        new Dictionary<int, TutorialPageReward>()
        {
            { 1, new TutorialPageReward(50) },
            { 2, new TutorialPageReward(100) },
            { 3, new TutorialPageReward(300) },
            { 4, new TutorialPageReward(400) },
            { 5, new TutorialPageReward(rarity: RarityType.Common, baps: 500) },
        };

    private List<int> StepsPerPage { get; set; } = new()
    {
        4,
        5,
        3,
        3,
        3,
    };

    public int PageCount => StepsPerPage.Count;

    public bool IsPageCompleted(ulong userId, int page)
    {
        return UserTutorialProgressDict.ContainsKey(userId) && UserTutorialProgressDict[userId].Count >= StepsPerPage[page - 1];
    }

    public bool IsPageCompletedForDisplay(ulong userId, int page)
    {
        if (!UserTutorialProgressDict.ContainsKey(userId))
            return false;

        if (UserTutorialProgressDict[userId].Min(e => e.Page) > page)
        {
            return true;
        }

        if (UserTutorialProgressDict[userId].Where(e => e.Page == page).Count() >= StepsPerPage[page - 1])
            return true;

        return false;
    }

    public TutorialPageReward GetPageReward(ulong userId)
    {
        var page = GetCurrentUserPage(userId);
        if (!TutorialPageRewardDict.ContainsKey(page))
            return null;

        return TutorialPageRewardDict[page];
    }

    public TutorialPageReward GetPageReward(int page)
    {
        return TutorialPageRewardDict[page];
    }

    public int GetCurrentUserPage(ulong userId)
    {
        if (!UserTutorialProgressDict.ContainsKey(userId) || !UserTutorialProgressDict[userId].Any())
            return 1;

        int currentPage = UserTutorialProgressDict[userId].Max(e => e.Page);

        if (UserTutorialProgressDict[userId].Where(e => e.Page == currentPage).Count() == StepsPerPage[currentPage - 1])
            return currentPage + 1;

        return currentPage;
    }

    public async Task LoadInitial()
    {
        var raw = await dbContext.UserTutoProgress.ToListAsync();
        UserTutorialProgressDict = raw.GroupBy(e => e.UserId).ToDictionary(e => e.Key, e => e.ToList());
    }

    public bool IsStepComplete(ulong userId, int stepIndex, int page)
    {
        if (!UserTutorialProgressDict.ContainsKey(userId))
            return false;

        return UserTutorialProgressDict[userId].Any(e => e.Page > page || e.TaskIndex == stepIndex && e.Page == page);
    }

    public async Task RemoveUserPageStepsAsync(ulong userId)
    {
        int page = GetCurrentUserPage(userId) - 1;

        try
        {
            await dbContext.UserTutoProgress
                .Where(e => e.Page <= page && e.UserId == userId)
                .ExecuteDeleteAsync();

            UserTutorialProgressDict[userId].RemoveAll(e => e.Page <= page);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task<bool> AttemptSubmitStepCompleteAsync(KushBotUser user, int page, int stepIndex, IMessageChannel channel)
    {
        if (IsStepComplete(user.Id, stepIndex, page) || page > GetCurrentUserPage(user.Id))
            return false;

        await SubmitStepCompletedAsync(user.Id, stepIndex);

        if (UserTutorialProgressDict.ContainsKey(user.Id) && UserTutorialProgressDict[user.Id].Any(e => e.Page < page))
            await RemoveUserPageStepsAsync(user.Id);

        if (IsPageCompleted(user.Id, page))
        {
            await channel.SendMessageAsync($":sparkler: :sparkler: **tutorial page completed! +{TutorialPageRewardDict[page]} (see 'kush tuto')** :sparkler: :sparkler:");

            if (TutorialPageRewardDict[page].Baps.HasValue)
            {
                user.Balance += TutorialPageRewardDict[page].Baps.Value;
            }

            if (TutorialPageRewardDict[page].ItemRarity != RarityType.None)
            {
                user.Items ??= new();
                var manager = new ItemManager();
                var item = manager.GenerateRandomItem(user.Id, RarityType.Common);
                user.Items.Add(item);
            }

            return true;
        }
        else
        {
            await channel.SendMessageAsync(":four_leaf_clover: **Sub-step completed! (see 'kush tuto')** :four_leaf_clover: ");
            return false;
        }
    }

    public async Task<bool> AttemptCompleteQuestSteps(KushBotUser user, IMessageChannel channel,
        (List<Quest> freshCompleted, bool lastDailyCompleted, bool lastWeeklyCompleted) result)
    {
        var stepCompleted = false;

        if (result.freshCompleted.Any())
        {
            stepCompleted = await AttemptSubmitStepCompleteAsync(user, 2, 4, channel);
        }

        if (result.lastWeeklyCompleted)
        {
            stepCompleted = stepCompleted || await AttemptSubmitStepCompleteAsync(user, 3, 0, channel);
        }

        return stepCompleted;
    }

    private async Task SubmitStepCompletedAsync(ulong userId, int stepIndex)
    {
        int page = GetCurrentUserPage(userId);

        try
        {
            UserTutoProgress step = new()
            {
                Id = Guid.NewGuid(),
                Page = page,
                UserId = userId,
                TaskIndex = stepIndex,
            };

            dbContext.UserTutoProgress.Add(step);

            await dbContext.SaveChangesAsync();

            if (!UserTutorialProgressDict.ContainsKey(userId))
            {
                UserTutorialProgressDict.Add(userId, new());
            }
            UserTutorialProgressDict[userId].Add(step);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}

public class TutorialPageReward(int? baps, RarityType rarity = RarityType.None)
{
    public int? Baps { get; set; } = baps;
    public RarityType ItemRarity { get; set; } = rarity;

    public override string ToString()
    {
        return $"{(ItemRarity != RarityType.None ? $"a {ItemRarity} {ItemRarity.GetRarityEmote()} item " : "")}{(Baps != null ? $"{Baps} baps :four_leaf_clover:" : "")}";
    }
}
