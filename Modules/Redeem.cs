using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KushBot.DataClasses;
using KushBot.Resources.Database;
using KushBot.Services;
using Quartz;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules;

[Group("redeem")]
public class Redeem(
    SqliteDbContext dbContext,
    TutorialManager tutorialManager,
    DiscordSocketClient client,
    ISchedulerFactory schedulerFactory) : ModuleBase<SocketCommandContext>
{
    private static List<RedeemReward> RedeemRewards = [
        new Nupumpuoti(2_000_000),
        new Skibidi(250_000),
        new Gyatt(15_000),
        new Asked(250),
        new Isnyk(500),
        new Degenerate(400),
        new Pakeisk(350),
        new Dink(700),
    ];

    private RedeemContext GetRedeemContext(IUser target = null, string remainder = "", bool setCd = true)
        => new RedeemContext(dbContext, Context, tutorialManager, client, schedulerFactory, target, remainder, setCd);

    [Command("")]
    public async Task ListRewardsAsync()
    {
        var builder = new EmbedBuilder()
            .WithTitle("Redeem")
            .WithColor(Color.Blue);

        foreach (var item in RedeemRewards)
        {
            builder.AddField($"**{item.GetType().Name}**", item.DisplayText);
        }

        await ReplyAsync(embed: builder.Build());
    }

    [Command(nameof(Nupumpuoti))]
    public async Task RedeemNupumpuoti()
    {
        await RedeemRewards
            .FirstOrDefault(e => e is Nupumpuoti)
            .RedeemAsync(GetRedeemContext(setCd: false));
    }

    [Command(nameof(Skibidi))]
    public async Task RedeemSkibidi()
    {
        await RedeemRewards
            .FirstOrDefault(e => e is Skibidi)
            .RedeemAsync(GetRedeemContext(setCd: false));
    }

    [Command(nameof(Gyatt))]
    public async Task RedeemGyatt()
    {
        await RedeemRewards
            .FirstOrDefault(e => e is Gyatt)
            .RedeemAsync(GetRedeemContext(setCd: false));
    }

    [Command(nameof(Asked))]
    public async Task RedeemAsked(IUser target)
    {
        await RedeemRewards
            .FirstOrDefault(e => e is Asked)
            .RedeemAsync(GetRedeemContext(target));
    }

    [Command(nameof(Isnyk))]
    public async Task RedeemIsnyk(IUser target)
    {
        await RedeemRewards
            .FirstOrDefault(e => e is Isnyk)
            .RedeemAsync(GetRedeemContext(target));
    }

    [Command(nameof(Degenerate))]
    public async Task RedeemDegenerate(IUser target)
    {
        await RedeemRewards
            .FirstOrDefault(e => e is Degenerate)
            .RedeemAsync(GetRedeemContext(target));
    }

    [Command(nameof(Pakeisk))]
    public async Task RedeemPakeisk(IUser target, [Remainder] string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            await ReplyAsync($"{Context.User.Mention} mby write a name retar?");
            return;
        }

        await RedeemRewards
            .FirstOrDefault(e => e is Pakeisk)
            .RedeemAsync(GetRedeemContext(target, name));
    }

    [Command(nameof(Dink))]
    public async Task RedeemDink(IUser target)
    {
        await RedeemRewards
            .FirstOrDefault(e => e is Dink)
            .RedeemAsync(GetRedeemContext(target));
    }
}
