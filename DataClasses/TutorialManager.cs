using Discord;
using KushBot.DataClasses.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.DataClasses;

public class TutorialPageReward
{
    public int? Baps { get; set; }
    public string? ItemRarity { get; set; }
    public TutorialPageReward(int? baps = null, string? rarity = null)
    {
        this.Baps = baps;
        this.ItemRarity = rarity;
    }

    public override string ToString()
    {
        return $"{(ItemRarity != null ? $"a {ItemRarity} {GetRarityEmote(ItemRarity)} item " : "")}{(Baps != null ? $"{Baps} baps :four_leaf_clover:" : "")}";
    }

    private string GetRarityEmote(string rarity)
        => rarity switch
        {
            _ => ":white_large_square:"
        };

}
public static class TutorialManager
{
    private static Dictionary<int, TutorialPageReward> TutorialPageRewardDict { get; set; } =
        new Dictionary<int, TutorialPageReward>()
        {
            { 1, new TutorialPageReward(50) },
            { 2, new TutorialPageReward(100) },
            { 3, new TutorialPageReward(300) },
            { 4, new TutorialPageReward(400) },
            { 5, new TutorialPageReward(rarity: "Common", baps: 500) },
        };

    private static Dictionary<ulong, List<UserTutoProgress>> UserTutorialProgressDict { get; set; } = new();
    private static List<int> StepsPerPage { get; set; } = new()
    {
        4,
        5,
        3,
        3,
        3,
    };

    public static int PageCount => StepsPerPage.Count;

    public static bool IsPageCompleted(ulong userId, int page)
    {
        return UserTutorialProgressDict.ContainsKey(userId) && UserTutorialProgressDict[userId].Count >= StepsPerPage[page - 1];
    }

    public static bool IsPageCompletedForDisplay(ulong userId, int page)
    {
        if (!UserTutorialProgressDict.ContainsKey(userId))
            return false;

        if(UserTutorialProgressDict[userId].Min(e => e.Page) > page)
        {
            return true;
        }

        if (UserTutorialProgressDict[userId].Where(e => e.Page == page).Count() >= StepsPerPage[page - 1])
            return true;

        return false;
    }

    public static TutorialPageReward GetPageReward(ulong userId)
    {
        var page = GetCurrentUserPage(userId);
        if (!TutorialPageRewardDict.ContainsKey(page))
            return null;

        return TutorialPageRewardDict[page];
    }

    public static TutorialPageReward GetPageReward(int page)
    {
        return TutorialPageRewardDict[page];
    }

    public static int GetCurrentUserPage(ulong userId)
    {
        if (!UserTutorialProgressDict.ContainsKey(userId) || !UserTutorialProgressDict[userId].Any())
            return 1;

        int currentPage = UserTutorialProgressDict[userId].Max(e => e.Page);

        if (UserTutorialProgressDict[userId].Where(e => e.Page == currentPage).Count() == StepsPerPage[currentPage - 1])
            return currentPage + 1;

        return currentPage;
    }

    public static int GetUserStepCompletedCount(ulong userId)
    {
        if (!UserTutorialProgressDict.ContainsKey(userId))
            return 0;

        int count = UserTutorialProgressDict[userId].Count();

        return count;
    }

    public static void LoadInitial()
    {
        UserTutorialProgressDict = Data.Data.LoadAllUsersTutorialProgress();
    }

    public static bool IsStepComplete(ulong userId, int stepIndex, int page)
    {
        if (!UserTutorialProgressDict.ContainsKey(userId))
            return false;

        return UserTutorialProgressDict[userId].Any(e => (e.Page > page || (e.TaskIndex == stepIndex && e.Page == page)));
    }

    public static async Task RemoveUserPageStepsAsync(ulong userId)
    {
        int page = GetCurrentUserPage(userId) - 1;

        try
        {
            await Data.Data.RemoveDeprecatedTutoStepsAsync(userId, page);
            UserTutorialProgressDict[userId].RemoveAll(e => e.Page <= page);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static async Task AttemptSubmitStepCompleteAsync(ulong userId, int page, int stepIndex, IMessageChannel channel)
    {
        if (IsStepComplete(userId, stepIndex, page) || page > GetCurrentUserPage(userId))
            return;


        await SubmitStepCompletedAsync(userId, stepIndex);

        if (UserTutorialProgressDict.ContainsKey(userId) && UserTutorialProgressDict[userId].Any(e => e.Page < page))
            await RemoveUserPageStepsAsync(userId);

        if (IsPageCompleted(userId, page))
        {
            await channel.SendMessageAsync($":sparkler: :sparkler: **tutorial page completed! +{TutorialPageRewardDict[page]} (see 'kush tuto')** :sparkler: :sparkler:");
            
            if (TutorialPageRewardDict[page].Baps.HasValue)
            {
                await Data.Data.SaveBalance(userId, TutorialPageRewardDict[page].Baps.Value, false);
            }
            if (!string.IsNullOrEmpty(TutorialPageRewardDict[page].ItemRarity))
            {
                var manager = new ItemManager();
                var item = manager.GenerateRandomItem(userId, RarityType.Common);
                var user = Data.Data.GetKushBotUser(userId, Data.UserDtoFeatures.Items);
                user.Items.Add(item);
                await Data.Data.SaveKushBotUserAsync(user);
            }
        }
        else
        {
            await channel.SendMessageAsync(":four_leaf_clover: **Sub-step completed! (see 'kush tuto')** :four_leaf_clover: ");
        }
    }

    private static async Task SubmitStepCompletedAsync(ulong userId, int stepIndex)
    {
        int page = GetCurrentUserPage(userId);

        try
        {
            var step = await Data.Data.InsertTutoStepCompletedAsync(userId, page, stepIndex);
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
