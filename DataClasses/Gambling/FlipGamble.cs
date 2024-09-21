using Discord;
using Discord.Commands;
using KushBot.Global;
using System.Threading.Tasks;

namespace KushBot.DataClasses;

public sealed class FlipGamble : BaseGamble
{
    public FlipGamble(SocketCommandContext context) : base(context) { }

    public override GambleResults Calculate()
    {
        return new(Amount, base.Rnd.NextDouble() > 0.5);
    }

    public override async Task CreateUserEventAsync(GambleResults result)
    {
        await Data.Data.CreateUserEventAsync(Context.User.Id, result.IsWin ? Enums.UserEventType.FlipWin : Enums.UserEventType.FlipLose, result.Baps);
    }

    public override async Task SendReplyAsync(GambleResults result)
    {
        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 2, 0, Context.Channel);
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
