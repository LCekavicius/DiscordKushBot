using Discord;
using Discord.Rest;
using Discord.WebSocket;
using KushBot.Global;
using KushBot.Modules.Interactions;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.BackgroundJobs;

[DisallowConcurrentExecution]
public class AirDropJob(
    ILogger<AirDropJob> logger,
    ISchedulerFactory schedulerFactory,
    SqliteDbContext dbContext,
    DiscordSocketClient client) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var scheduler = await schedulerFactory.GetScheduler();

        var triggers = await scheduler.GetTriggersOfJob(context.JobDetail.Key);

        if (!triggers.Any())
        {
            var trigger = TriggerFactory.CreateAirDropTrigger();
            await scheduler.RescheduleJob(context.Trigger.Key, trigger);
        }

        logger.LogInformation($"{DateTime.Now} Dropping airdrop");

        var configuredChannelIds = await dbContext.ChannelPerms.Where(e => e.PermitsAirDrop).Select(e => e.Id).ToListAsync();

        if (!configuredChannelIds.Any())
        {
            logger.LogError("No channels set up for airdrop");
            return;
        }

        var channels = configuredChannelIds.Select(e => client.GetChannel(e) as ITextChannel).ToList();

        var channel = channels[Random.Shared.Next(0, channels.Count)];

        var component = LootAirdrop.BuildMessageComponent(false);

        var embed = new EmbedBuilder()
            .WithTitle("Airdrop")
            .WithColor(Discord.Color.Orange)
            .AddField("Loots remaining:", $"**{4}**")
            .WithFooter("Click on the button to collect the airdrop")
            .WithImageUrl("https://cdn.discordapp.com/attachments/902541957694390298/1223740109451432047/cat-hedgehog.gif?ex=661af3ca&is=66087eca&hm=ed2188ec15aff97fed417ed47da7855c11d7714e95f5a67b2106a72208bc8862&")
            .Build();

        var message = await channel.SendMessageAsync(embed: embed, components: component);
        AirDrops.Current.Add(new(message as RestUserMessage));
    }
}
