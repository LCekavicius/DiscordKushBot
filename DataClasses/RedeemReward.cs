using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KushBot.BackgroundJobs;
using KushBot.Global;
using KushBot.Resources.Database;
using KushBot.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.DataClasses;

public sealed record RedeemResponse(string message);
public sealed record RedeemContext(
    SqliteDbContext DbContext,
    SocketCommandContext Context,
    TutorialManager TutorialManager,
    DiscordSocketClient Client,
    ISchedulerFactory SchedulerFactory,
    IUser Target = null,
    string remainder = "",
    bool setCd = true);

public abstract class RedeemReward
{
    protected static NumberFormatInfo format = new() { NumberGroupSeparator = " " };
    public abstract int Baps { get; init; }
    public abstract string DisplayText { get; init; }

    protected abstract Task<RedeemResponse> HandleRedeemAsync(KushBotUser user, RedeemContext redeemContext);
    public async Task RedeemAsync(RedeemContext redeemContext)
    {
        var context = redeemContext.Context;
        var user = await redeemContext.DbContext.GetKushBotUserAsync(context.User.Id);

        if (user.Balance < Baps)
        {
            await context.Message.ReplyAsync($"{context.User.Mention} poor");
            return;
        }

        if (user.RedeemDate.AddHours(2) > TimeHelper.Now)
        {
            await context.Message.ReplyAsync($"{context.User.Mention} Your redeem is on cooldown");
            return;
        }

        if (redeemContext.Target is { IsBot: true })
        {
            await context.Message.ReplyAsync($"{context.User.Mention} funny guy :D");
            return;
        }

        var response = await HandleRedeemAsync(user, redeemContext);

        await redeemContext.TutorialManager.AttemptSubmitStepCompleteAsync(user, 5, 2, context.Channel);

        if (redeemContext.setCd)
        {
            user.RedeemDate = TimeHelper.Now;
        }

        user.Balance -= Baps;

        await redeemContext.DbContext.SaveChangesAsync();

        await context.Message.ReplyAsync(response.message);
    }
}

public class Nupumpuoti(int baps) : RedeemReward
{
    public override int Baps { get; init; } = baps;
    public override string DisplayText { get; init; } = $"Type 'kush redeem {nameof(Nupumpuoti)}' to buy a color of your choosing for {baps.ToString("#,0", format)} Baps";

    protected override Task<RedeemResponse> HandleRedeemAsync(KushBotUser user, RedeemContext redeemContext)
    {
        return Task.FromResult(
            new RedeemResponse($"{CustomEmojis.Kitadimensija}{CustomEmojis.Kitadimensija}{CustomEmojis.Kitadimensija}{redeemContext.Context.User.Mention} You've redeemed a color! PM an admin with a color of your choosing to receive it {CustomEmojis.Kitadimensija}{CustomEmojis.Kitadimensija}{CustomEmojis.Kitadimensija}")
        );
    }
}

public class Skibidi(int baps) : RedeemReward
{
    public override int Baps { get; init; } = baps;
    public override string DisplayText { get; init; } = $"Type 'kush redeem {nameof(Skibidi)}' to buy a permanent role __**{nameof(Skibidi)}**__ for {baps.ToString("#,0", format)} Baps";

    protected override async Task<RedeemResponse> HandleRedeemAsync(KushBotUser user, RedeemContext redeemContext)
    {
        var roleId = await redeemContext.DbContext.SeasonParameters.Select(e => e.BlueRoleId).FirstOrDefaultAsync();

        var guild = redeemContext.Client.GetGuild(redeemContext.Context.Guild.Id);
        var guildUser = guild.GetUser(redeemContext.Context.User.Id);
        await guildUser.AddRoleAsync(roleId);

        await redeemContext.Client.GetUserAsync(redeemContext.Context.User.Id);

        return new RedeemResponse($"{CustomEmojis.Kitadimensija}{CustomEmojis.Kitadimensija}{CustomEmojis.Kitadimensija}{redeemContext.Context.User.Mention} You've redeemed a role! {CustomEmojis.Kitadimensija}{CustomEmojis.Kitadimensija}{CustomEmojis.Kitadimensija}");
    }
}

public class Gyatt(int baps) : RedeemReward
{
    public override int Baps { get; init; } = baps;
    public override string DisplayText { get; init; } = $"Type 'kush redeem {nameof(Gyatt)}' to buy a temporary role __**{nameof(Gyatt)}**__ for {baps.ToString("#,0", format)} Baps";

    protected override async Task<RedeemResponse> HandleRedeemAsync(KushBotUser user, RedeemContext redeemContext)
    {
        var roleId = await redeemContext.DbContext.SeasonParameters.Select(e => e.OrangeRoleId).FirstOrDefaultAsync();

        var guild = redeemContext.Client.GetGuild(redeemContext.Context.Guild.Id);
        var guildUser = guild.GetUser(redeemContext.Context.User.Id);
        await guildUser.AddRoleAsync(roleId);

        await redeemContext.Client.GetUserAsync(redeemContext.Context.User.Id);

        return new RedeemResponse($"{CustomEmojis.Kitadimensija}{CustomEmojis.Kitadimensija}{CustomEmojis.Kitadimensija}{redeemContext.Context.User.Mention} You've redeemed a role! {CustomEmojis.Kitadimensija}{CustomEmojis.Kitadimensija}{CustomEmojis.Kitadimensija}");
    }
}

public class Asked(int baps) : RedeemReward
{
    public override int Baps { get; init; } = baps;
    public override string DisplayText { get; init; } = $"Type 'kush redeem {nameof(Asked)} @user' (e.g. kush redeem {nameof(Asked)} @tabobanda) to attach ':warning: KLAUSEM :warning:' bot reply to the user's messages (lasts for 20 unique messages) cost: {baps.ToString("#,0", format)} Baps";

    protected override Task<RedeemResponse> HandleRedeemAsync(KushBotUser user, RedeemContext redeemContext)
    {
        var curse = new CursedPlayer(redeemContext.Target.Id, "asked", 20);
        MessageHandler.CursedPlayers.Add(curse);
        return Task.FromResult(
            new RedeemResponse($"{redeemContext.Context.User.Mention} you cursed {redeemContext.Target.Mention} with ASKED for 20 messages :warning:")
        );
    }
}

public class Isnyk(int baps) : RedeemReward
{
    public override int Baps { get; init; } = baps;
    public override string DisplayText { get; init; } = $"Type 'kush redeem {nameof(Isnyk)} @user' (e.g. kush redeem {nameof(Isnyk)} @tabobanda) to delete messages as the user types them (lasts for 20 unique messages) cost: {baps.ToString("#,0", format)} Baps";

    protected override Task<RedeemResponse> HandleRedeemAsync(KushBotUser user, RedeemContext redeemContext)
    {
        var curse = new CursedPlayer(redeemContext.Target.Id, "isnyk", 20);
        MessageHandler.CursedPlayers.Add(curse);

        return Task.FromResult(
            new RedeemResponse($"{redeemContext.Context.User.Mention} you cursed {redeemContext.Target.Mention} with disappearance for 20 messages {CustomEmojis.Gana}")
        );
    }
}

public class Degenerate(int baps) : RedeemReward
{
    public override int Baps { get; init; } = baps;
    public override string DisplayText { get; init; } = $"Type 'kush redeem {nameof(Degenerate)} @user' (e.g. kush redeem {nameof(Degenerate)} @tabobanda) to attach a kush nya to the user's messages (lasts for 15 unique messages) {baps.ToString("#,0", format)} Baps";

    protected override Task<RedeemResponse> HandleRedeemAsync(KushBotUser user, RedeemContext redeemContext)
    {
        var curse = new CursedPlayer(redeemContext.Target.Id, "degenerate", 20);
        MessageHandler.CursedPlayers.Add(curse);

        return Task.FromResult(
            new RedeemResponse($"{redeemContext.Context.User.Mention} you cursed {redeemContext.Target.Mention} with degeneracy for 20 messages {CustomEmojis.Tf}")
        );
    }
}

public class Pakeisk(int baps) : RedeemReward
{
    public override int Baps { get; init; } = baps;
    public override string DisplayText { get; init; } = $"Type 'kush redeem pakeisk @user newName' (e.g. kush redeem pakeisk @tabobanda FAGGOT) to change the name of a user. cost: {baps.ToString("#,0", format)} Baps";

    protected override async Task<RedeemResponse> HandleRedeemAsync(KushBotUser user, RedeemContext redeemContext)
    {
        var roleId = await redeemContext.DbContext.SeasonParameters.Select(e => e.MutedRoleId).FirstOrDefaultAsync();

        var guild = redeemContext.Client.GetGuild(redeemContext.Context.Guild.Id);
        var guildUser = guild.GetUser(redeemContext.Target.Id);
        await guildUser.ModifyAsync(e => e.Nickname = redeemContext.remainder);

        return new RedeemResponse($"{redeemContext.Context.User.Mention} you changed {redeemContext.Target.Mention} into {redeemContext.remainder} {CustomEmojis.Tf}");
    }
}

public class Dink(int baps) : RedeemReward
{
    public override int Baps { get; init; } = baps;
    public override string DisplayText { get; init; } = $"Type 'kush redeem dink @user' (e.g. kush redeem dink @tabobanda) to lock the user in <#945764667287031859> for 3 minutes. cost: {baps.ToString("#,0", format)} Baps";

    protected override async Task<RedeemResponse> HandleRedeemAsync(KushBotUser user, RedeemContext redeemContext)
    {
        var roleId = await redeemContext.DbContext.SeasonParameters.Select(e => e.MutedRoleId).FirstOrDefaultAsync();

        var guild = redeemContext.Client.GetGuild(redeemContext.Context.Guild.Id);
        var guildUser = guild.GetUser(redeemContext.Target.Id);
        await guildUser.AddRoleAsync(roleId);

        await redeemContext.Client.GetUserAsync(redeemContext.Context.User.Id);

        var scheduler = await redeemContext.SchedulerFactory.GetScheduler();
        var jobKey = JobKey.Create(nameof(RemoveRoleJob), "DEFAULT");

        var triggerDataMap = new JobDataMap
        {
            { "UserId", guildUser.Id.ToString() },
            { "RoleId", roleId.ToString() },
            { "GuildId", guild.Id.ToString() }
        };

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"RemoveRoleJobTrigger_{Guid.NewGuid()}", "DEFAULT")
            .StartAt(TimeHelper.Now.AddMinutes(3))
            .UsingJobData(triggerDataMap)
            .ForJob(jobKey)
            .Build();

        await scheduler.ScheduleJob(trigger);

        return new RedeemResponse($"{redeemContext.Context.User.Mention} you locked {redeemContext.Target.Mention} in sad for 3 minutes {CustomEmojis.Gana}");
    }
}
