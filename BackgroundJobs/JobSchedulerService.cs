using KushBot.BackgroundJobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.Configuration;

public class JobSchedulerService : IHostedService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<JobSchedulerService> _logger;
    private readonly IConfiguration _configuration;

    private bool IsDev { get => bool.TryParse(_configuration["development"], out var value) && value; }

    public JobSchedulerService(ISchedulerFactory schedulerFactory, ILogger<JobSchedulerService> logger, IConfiguration configuration)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var triggerListener = new TriggerListener();
        scheduler.ListenerManager.AddTriggerListener(triggerListener);
        
        await TryScheduleAirDropAsync(scheduler, cancellationToken);
        await TryScheduleDailyQuestsJobAsync(scheduler, cancellationToken);
        await TryScheduleWeeklyQuestsJobAsync(scheduler, cancellationToken);
    }

    private async Task TryScheduleDailyQuestsJobAsync(IScheduler scheduler, CancellationToken cancellationToken)
    {
        var jobKey = JobKey.Create(nameof(ProvideQuestsJob));

        var job = await scheduler.GetJobDetail(jobKey, cancellationToken);
        var triggers = await scheduler.GetTriggersOfJob(jobKey, cancellationToken);

        if (job == null)
        {
            job = JobBuilder.Create<ProvideQuestsJob>()
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build();
        }

        if (!triggers.Any())
        {
            var builder = TriggerBuilder.Create();
            if (jobKey != null)
            {
                builder.ForJob(jobKey);
            }

            var trigger = builder
                .WithIdentity($"{nameof(ProvideQuestsJob)}_CronTrigger")
                .WithCronSchedule(IsDev ? "0 0/5 * * * ?" : "0 0 0 * * ?")
                .Build();

            await scheduler.ScheduleJob(job, trigger, cancellationToken);
        }
        else
        {
            _logger.LogWarning($"Trigger for {nameof(ProvideQuestsJob)} already exists");
        }

        if (IsDev)
        {
            await scheduler.TriggerJob(jobKey);
        }
    }

    private async Task TryScheduleWeeklyQuestsJobAsync(IScheduler scheduler, CancellationToken cancellationToken)
    {
        var jobKey = JobKey.Create(nameof(ProvideWeekliesJob));

        var job = await scheduler.GetJobDetail(jobKey, cancellationToken);
        var triggers = await scheduler.GetTriggersOfJob(jobKey, cancellationToken);

        if (job == null)
        {
            job = JobBuilder.Create<ProvideWeekliesJob>()
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build();
        }

        if (!triggers.Any())
        {
            var builder = TriggerBuilder.Create();
            if (jobKey != null)
            {
                builder.ForJob(jobKey);
            }

            var trigger = builder
                .WithIdentity($"{nameof(ProvideWeekliesJob)}_CronTrigger")
                .WithCronSchedule(IsDev ? "0 0/10 * * * ?" : "0 0 0 ? * MON")
                .Build();

            await scheduler.ScheduleJob(job, trigger, cancellationToken);
        }
        else
        {
            _logger.LogWarning($"Trigger for {nameof(ProvideWeekliesJob)} already exists");
        }

        if (IsDev)
        {
            await scheduler.TriggerJob(jobKey);
        }
    }

    private async Task TryScheduleAirDropAsync(IScheduler scheduler, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating AirdropJob");

        var airdropJobKey = JobKey.Create(nameof(AirDropJob));

        var job = await scheduler.GetJobDetail(airdropJobKey, cancellationToken);
        var triggers = await scheduler.GetTriggersOfJob(airdropJobKey, cancellationToken);

        if (job == null)
        {
            job = JobBuilder.Create<AirDropJob>()
                .WithIdentity(airdropJobKey)
                .StoreDurably()
                .Build();

            await scheduler.AddJob(job, false, cancellationToken);
        }

        if (!triggers.Any())
        {
            _logger.LogInformation("Scheduling AirdropJob");
            var trigger = TriggerFactory.CreateAirDropTrigger(airdropJobKey);
            await scheduler.ScheduleJob(trigger);
        }
        else
        {
            _logger.LogWarning("Trigger for AirdropJob already exists");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}