using KushBot.BackgroundJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KushBot;

public class Worker : BackgroundService
{

    private readonly KushBot.DiscordBotService _discordBotService;
    private readonly ISchedulerFactory _schedulerFactory;

    public Worker(ISchedulerFactory schedulerFactory, DiscordBotService discordBotService)
    {
        _discordBotService = discordBotService;
        _schedulerFactory = schedulerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _discordBotService.RunBotAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
