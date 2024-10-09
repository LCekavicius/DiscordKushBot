using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KushBot;

public static class SocketCommandContextExtensions
{
    public static async Task CompleteQuestsAsync(this SocketCommandContext context, List<Quest> quests, bool completedLast)
    {
        foreach (var quest in quests)
        {
            await CompleteQuestAsync(context, quest);
        }

        if (quests.Count > 0 && completedLast)
        {
            var baps = quests[0].User.GetFullQuestCompleteReward();
            await context.Message.ReplyAsync($"{context.Message.Author.Mention} you've completed all of your quests and gained {baps} baps");
        }
    }

    private static async Task CompleteQuestAsync(SocketCommandContext context, Quest quest)
    {
        var baps = quest.GetQuestReward();
        string eggText = quest.ProvidesEgg ? "\nThe quest giver liked you and gave you a free egg!" :  "";
        await context.Message.ReplyAsync($"{context.Message.Author.Mention} Quest completed, rewarded: {baps} baps" + eggText);
    }
}
