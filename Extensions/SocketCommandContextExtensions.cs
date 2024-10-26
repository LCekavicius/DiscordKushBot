using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KushBot;



public static class SocketCommandContextExtensions
{
    public static async Task CompleteQuestsAsync(this SocketCommandContext context, List<Quest> quests, bool lastDailyCompleted, bool lastWeeklyCompleted)
    {
        foreach (var quest in quests)
        {
            await CompleteQuestAsync(context, quest);
        }

        if (quests.Count > 0 && lastDailyCompleted)
        {   var baps = quests[0].User.GetDailiesCompleteReward();
            await context.Message.ReplyAsync($"{context.Message.Author.Mention} you've completed all of your quests and gained {baps} baps");
        }

        if (quests.Count > 0 && lastWeeklyCompleted)
        {
            var item = quests[0].User.GetWeekliesCompleteReward();
            await context.Message.ReplyAsync($"{context.Message.Author.Mention} you've completed all of your weekly quests and gained a **{item.Rarity} item**");
        }
    }

    private static async Task CompleteQuestAsync(SocketCommandContext context, Quest quest)
    {
        var baps = quest.GetQuestReward();
        string eggText = quest.ProvidesEgg ? "\nThe quest giver liked you and gave you a free egg!" :  "";
        await context.Message.ReplyAsync($"{context.Message.Author.Mention} Quest completed, rewarded: {baps} baps" + eggText);
    }
}
