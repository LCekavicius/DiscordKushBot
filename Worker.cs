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

        Yea();
        await _discordBotService.RunBotAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    protected async Task Yea()
    {
        //await Task.Delay(5000);
        //var scheduler = await _schedulerFactory.GetScheduler();

        //var jobKey = JobKey.Create(nameof(ProvideQuestsJob), "DEFAULT"); // Ensure "DEFAULT" is used for group

        //// Trigger the job manually
        //if (await scheduler.CheckExists(jobKey))
        //{
        //    await scheduler.TriggerJob(jobKey);
        //}
        //else
        //{
        //    throw new Exception($"Job with key {jobKey} does not exist.");
        //}
    }
}
