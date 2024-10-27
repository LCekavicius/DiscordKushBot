using Discord;
using Discord.Rest;
using Discord.WebSocket;
using KushBot.EventHandler.Interactions;
using KushBot.Extensions;
using KushBot.Global;
using KushBot.Modules;
using KushBot.Modules.Interactions;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KushBot.BackgroundJobs;

[DisallowConcurrentExecution]
public class AirDropJob : IJob
{
    private readonly ILogger<AirDropJob> _logger;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IConfiguration _configuration;
    private readonly SqliteDbContext _dbContext;
    private readonly DiscordSocketClient _client;

    public AirDropJob(
        ILogger<AirDropJob> logger,
        ISchedulerFactory schedulerFactory,
        IConfiguration configuration,
        SqliteDbContext dbContext,
        DiscordSocketClient client)
    {
        _logger = logger;
        _schedulerFactory = schedulerFactory;
        _configuration = configuration;
        _dbContext = dbContext;
        _client = client;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var scheduler = await _schedulerFactory.GetScheduler();

        var triggers = await scheduler.GetTriggersOfJob(context.JobDetail.Key);

        if (!triggers.Any())
        {
            var trigger = TriggerFactory.CreateAirDropTrigger();
            await scheduler.RescheduleJob(context.Trigger.Key, trigger);
        }

        _logger.LogInformation($"{DateTime.Now} Dropping airdrop");

        var configuredChannelIds = await _dbContext.ChannelPerms.Where(e => e.PermitsAirDrop).Select(e => e.Id).ToListAsync();

        if (!configuredChannelIds.Any())
        {
            _logger.LogError("No channels set up for airdrop");
            return;
        }
        
        var channels = configuredChannelIds.Select(e => _client.GetChannel(e) as ITextChannel).ToList();

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
