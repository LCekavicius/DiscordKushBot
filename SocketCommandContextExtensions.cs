using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using System.Threading.Tasks;

namespace KushBot;

public static class SocketCommandContextExtensions
{
    public static async Task CompleteQuestAsync(this SocketCommandContext context, Quest quest)
    {
        var baps = quest.GetQuestReward(quest.User);
        await context.Message.ReplyAsync($"{context.Message.Author.Mention} Quest completed, rewarded: {baps} baps");
    }
}
