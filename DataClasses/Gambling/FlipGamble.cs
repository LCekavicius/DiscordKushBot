using Discord;
using Discord.Commands;
using KushBot.DataClasses.Enums;
using KushBot.Global;
using KushBot.Resources.Database;
using System.Threading.Tasks;

namespace KushBot.DataClasses;

public sealed class FlipGamble : BaseGamble
{
    public FlipGamble(SqliteDbContext dbContext, TutorialManager tutorialManager, SocketCommandContext context)
        : base(dbContext, tutorialManager, context) { }

    public override GambleResults Calculate()
    {
        return new(Amount, base.Rnd.NextDouble() > 0.5);
    }

    protected override DataForEvent GetUserEventType(GambleResults result)
    {
        return new DataForEvent(result.IsWin ? UserEventType.FlipWin : UserEventType.FlipLose);
    }

    protected override async Task HandleTutorialAsync()
    {
        await TutorialManager.AttemptSubmitStepCompleteAsync(BotUser, 2, 0, Context.Channel);
    }

    public override async Task SendReplyAsync(GambleResults result)
    {
        if (result.IsWin)
        {
            string insert = result.GymProc ? $" his gym-plant transfused into some extra baps and got {Amount} more baps!" : "";
            await Context.Message.ReplyAsync($"{CustomEmojis.Ipisa} {Context.User.Mention} won {Amount} Baps,{insert} and now has {BotUser.Balance} {CustomEmojis.Stovi}");
        }
        else
        {
            if (result.RodProc)
            {
                await Context.Message.ReplyAsync($"{CustomEmojis.Zvejas} {Context.User.Mention} would've lost his {Amount} Baps, but he reeled them back in with his fishing rod {CustomEmojis.Fisher}");
            }
            else
            {
                await Context.Message.ReplyAsync($"{CustomEmojis.Zvejas} {Context.User.Mention} Lost {Amount} Baps, and now has {BotUser.Balance} {CustomEmojis.Egg}");
            }
        }
    }
}
