﻿using Discord;
using Discord.Commands;
using KushBot.DataClasses.Enums;
using KushBot.Global;
using KushBot.Resources.Database;
using KushBot.Services;
using System.Threading.Tasks;

namespace KushBot.DataClasses;

public sealed class RiskGamble : BaseGamble
{
    private int Modifier { get; init; }
    public RiskGamble(SqliteDbContext dbContext, TutorialManager tutorialManager, SocketCommandContext context, int modifier)
        : base(dbContext, tutorialManager, context)
    {
        Modifier = modifier;
    }

    public override GambleResults Calculate()
    {
        int roll = Rnd.Next(0, Modifier);
        int winnings = (Amount * (Modifier - 1));

        if (roll == 0)
        {
            return new(winnings, true);
        }
        else
        {
            return new(Amount, false);
        }
    }

    protected override DataForEvent GetUserEventType(GambleResults result)
    {
        return new DataForEvent(result.IsWin ? UserEventType.RiskWin : UserEventType.RiskLose, Modifier);
    }

    protected override async Task HandleTutorialAsync()
    {
        await TutorialManager.AttemptSubmitStepCompleteAsync(BotUser, 2, 1, Context.Channel);
    }

    public override async Task SendReplyAsync(GambleResults result)
    {
        if (result.IsWin)
        {
            if (result.GymProc)
            {
                await Context.Message.ReplyAsync($"{CustomEmojis.Monkaw} {Context.User.Mention} Risked and won {result.Baps / 2} Baps, his gym-plant transfused into {result.Baps / 2} more baps. and he now has {BotUser.Balance} {CustomEmojis.Kitadimensija}");

            }
            else
            {
                await Context.Message.ReplyAsync($"{CustomEmojis.Monkaw} {Context.User.Mention} Risked and won {result.Baps} Baps, and now has {BotUser.Balance} {CustomEmojis.Kitadimensija}");
            }
        }
        else
        {
            if (result.RodProc)
            {
                await Context.Message.ReplyAsync($"{CustomEmojis.Zvej2} {Context.User.Mention} would've lost his {Amount} Baps, but he reeled them back in with his fishing rod {CustomEmojis.Zvej}");
            }
            else
            {
                await Context.Message.ReplyAsync($"{CustomEmojis.Zvej2} {Context.User.Mention} Risked and Lost {Amount} Baps, and now has {BotUser.Balance} {CustomEmojis.Zltr}");
            }
        }
    }

    protected override string Validate()
    {
        if (Modifier < 4)
        {
            return $"{Context.User.Mention} Risk modifier of 4 is the minimum, bruh {CustomEmojis.EggSleep}";
        }
        return base.Validate();
    }
}
