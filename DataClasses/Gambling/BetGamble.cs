using Discord;
using Discord.Commands;
using KushBot.DataClasses.Enums;
using KushBot.Global;
using System;
using System.Threading.Tasks;

namespace KushBot.DataClasses;

public sealed class BetGamble : BaseGamble
{
    public BetGamble(SocketCommandContext context) : base(context) { }

    private int Transfusion { get; set; }
    private double Modifier { get; set; }

    public override GambleResults Calculate()
    {
        int chance = Rnd.Next(0, 1000);
        Modifier = GetModifier(chance);

        Transfusion = (int)Math.Round((Modifier * Amount) / 100);

        if (Modifier < 100)
        {
            return new(Amount - Transfusion, false);
        }
        else
        {
            return new(Transfusion - Amount, true);
        }
    }

    private float GetModifier(int chance)
    {
        int loseChance = 633;

        if (chance < loseChance)
        {
            return Rnd.Next(0, 100);
        }
        else if (chance < 914)
        {
            return Rnd.Next(100, 200);
        }
        else if (chance < 999)
        {
            return Rnd.Next(200, 401);
        }
        else
        {
            return Rnd.Next(1000, 1500);
        }
    }

    protected override DataForEvent GetUserEventType(GambleResults result)
    {
        return new DataForEvent(result.IsWin ? UserEventType.BetWin : UserEventType.BetLose, Modifier / 100);
    }

    public override async Task SendReplyAsync(GambleResults result)
    {
        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 2, 2, Context.Channel);

        if (result.IsWin && result.GymProc)
        {
            await Context.Message.ReplyAsync($"{Context.User.Mention} bet **{Amount}** Baps which transfused into **{Transfusion}** with **{Modifier / 100}** as a multiplier, gym buff netted extra **{Transfusion - Amount}**, and he now has {BotUser.Balance} {CustomEmojis.Pepew}");
        }
        else if (!result.IsWin && result.RodProc)
        {
            await Context.Message.ReplyAsync($"{Context.User.Mention} bet **{Amount}** Baps transfused into **{Transfusion}** with **{Modifier / 100}** as a multiplier but you pulled the money out with ur fishing rod {CustomEmojis.Pepew}");
        }
        else
        {
            await Context.Message.ReplyAsync($"{Context.User.Mention} bet **{Amount}** Baps which transfused into **{Transfusion}** with **{Modifier / 100}** as a multiplier, and now has {BotUser.Balance} {CustomEmojis.Pepew}");
        }
    }
}
