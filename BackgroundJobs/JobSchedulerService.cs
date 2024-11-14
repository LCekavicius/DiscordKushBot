using KushBot.BackgroundJobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.Configuration;
using KushBot.Extensions;

public class JobSchedulerService : IHostedService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<JobSchedulerService> _logger;
    private readonly IConfiguration _configuration;

    private bool IsDev => _configuration.IsDev();

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
        await TryScheduleVendorRefresh(scheduler, cancellationToken);
        await TryCreateRemoveRoleJobAsync(scheduler, cancellationToken);
        await TryScheduleSpawnBossJobAsync(scheduler, cancellationToken);
        await TryCreateBossFightJobAsync(scheduler, cancellationToken);
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

    private async Task TryScheduleVendorRefresh(IScheduler scheduler, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Vendor refresh job");

        var jobKey = JobKey.Create(nameof(RefreshVendorJob));

        var job = await scheduler.GetJobDetail(jobKey, cancellationToken);
        var triggers = await scheduler.GetTriggersOfJob(jobKey, cancellationToken);

        if (job == null)
        {
            job = JobBuilder.Create<RefreshVendorJob>()
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
                .WithIdentity($"{nameof(RefreshVendorJob)}_CronTrigger")
                .WithCronSchedule("0 0 18 * * ?")
                .Build();

            await scheduler.ScheduleJob(job, trigger, cancellationToken);
        }
        else
        {
            _logger.LogWarning($"Trigger for {nameof(RefreshVendorJob)} already exists");
        }
    }

    public async Task TryCreateRemoveRoleJobAsync(IScheduler scheduler, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating remove role job");

        var jobKey = JobKey.Create(nameof(RemoveRoleJob));

        var job = await scheduler.GetJobDetail(jobKey, cancellationToken);

        if (job == null)
        {
            job = JobBuilder.Create<RemoveRoleJob>()
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build();

            await scheduler.AddJob(job, true);
        }
    }

    private async Task TryScheduleSpawnBossJobAsync(IScheduler scheduler, CancellationToken cancellationToken)
    {
        var jobKey = JobKey.Create(nameof(SpawnBossJob));

        var job = await scheduler.GetJobDetail(jobKey, cancellationToken);
        var triggers = await scheduler.GetTriggersOfJob(jobKey, cancellationToken);

        if (job == null)
        {
            job = JobBuilder.Create<SpawnBossJob>()
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
                .WithIdentity($"{nameof(SpawnBossJob)}_CronTrigger")
                .WithCronSchedule(IsDev ? "0 0/5 * * * ?" : "0 0 0/2 * * ?")
                .Build();

            await scheduler.ScheduleJob(job, trigger, cancellationToken);
        }
        else
        {
            _logger.LogWarning($"Trigger for {nameof(ProvideQuestsJob)} already exists");
        }
    }

    public async Task TryCreateBossFightJobAsync(IScheduler scheduler, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Boss fight job");

        var jobKey = JobKey.Create(nameof(BossFightJob));

        var job = await scheduler.GetJobDetail(jobKey, cancellationToken);

        if (job == null)
        {
            job = JobBuilder.Create<BossFightJob>()
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build();

            await scheduler.AddJob(job, true);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}