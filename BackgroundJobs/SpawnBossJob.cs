using Discord;
using Discord.WebSocket;
using KushBot.DataClasses;
using KushBot.DataClasses.enums;
using KushBot.Extensions;
using KushBot.Global;
using KushBot.Modules.Interactions;
using KushBot.Resources.Database;
using KushBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KushBot.BackgroundJobs;

[DisallowConcurrentExecution]
public class SpawnBossJob(
    ILogger<RefreshVendorJob> logger,
    SqliteDbContext dbContext,
    DiscordSocketClient client,
    IConfiguration configuration,
    ISchedulerFactory schedulerFactory,
    BossService bossService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var channelId = DiscordBotService.ChannelPerms.FirstOrDefault(e => e.Permissions.HasFlag(Permissions.Boss)).Id;

            var channel = await client.GetChannelAsync(channelId) as ITextChannel;

            var level = context.Trigger.JobDataMap.Get("Level") as int?;
            level ??= RollLevel();

            var boss = new Boss
            {
                Level = level.Value,
                Health = CalculateHealth(level.Value),
                ParticipantSlots = Random.Shared.Next(4, 9),
                Abilities = RollAbilities(level > 50 ? 2 : 1),
                Participants = [],
                StartDate = TimeHelper.Now.AddMinutes(configuration.IsDev() ? 1 : 30)
            };

            boss.BossBaseIndex = GetBossBaseIndex(boss.Rarity);

            var message = await channel.SendMessageAsync(
                embed: await bossService.GetBossEmbed(boss),
                components: BossParticipation.BuildMessageComponent(false, true)
            );

            boss.MessageId = message.Id;

            await dbContext.Bosses.AddAsync(boss);
            await dbContext.SaveChangesAsync();

            await ScheduleFightAsync(boss);
        }
        catch (Exception ex)
        {
            logger.LogError($"Spawning boss failed with exception: {ex.Message}");
        }
    }

    private async Task ScheduleFightAsync(Boss boss)
    {
        var scheduler = await schedulerFactory.GetScheduler();
        var builder = TriggerBuilder.Create();

        var fightBossJobKey = JobKey.Create(nameof(BossFightJob), "DEFAULT");
        if (fightBossJobKey != null)
        {
            builder.ForJob(fightBossJobKey);
        }

        var triggerDataMap = new JobDataMap
        {
            { "Id", boss.Id.ToString() }
        };

        var trigger = builder
            .WithIdentity($"{nameof(BossFightJob)}_RandomTrigger")
            .StartAt(boss.StartDate)
            .UsingJobData(triggerDataMap)
            .Build();

        await scheduler.ScheduleJob(trigger);
    }

    public int GetBossBaseIndex(RarityType rarity)
    {
        var bossBase = BossBases.Bases.Where(e => e.Rarity == rarity).OrderBy(e => Random.Shared.NextDouble()).FirstOrDefault();
        return BossBases.Bases.IndexOf(bossBase);
    }

    private int RollLevel()
    {
        return Random.Shared.Next(1, 51);
    }

    private int CalculateHealth(int level)
    {
        return 3 * (int)(level * 5 + (10 + 0.1 * Math.Pow(level, 2)));
    }

    private BossAbilities RollAbilities(int rollCount)
    {
        var rolled = BossAbilities.None;

        for (int i = 0; i < rollCount; i++)
        {
            var values = Enum.GetValues<BossAbilities>()
                .Where(x => x != BossAbilities.None && x != BossAbilities.All && !rolled.HasFlag(x))
                .ToArray();

            rolled |= values[Random.Shared.Next(values.Length)];
        }

        return rolled;
    }
}
