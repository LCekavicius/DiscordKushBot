using KushBot.BackgroundJobs;
using Quartz;
using System;

namespace KushBot.Extensions;

public static class ISchedulerExtensions
{
    public static ITrigger GetAirDropTrigger(this TriggerBuilder scheduler)
    {
        var airdropJobKey = JobKey.Create(nameof(AirDropJob));
        var triggerKey = new TriggerKey($"{nameof(AirDropJob)}_RandomTrigger");

        int minAirdropDelay = (int)TimeSpan.FromSeconds(10).TotalSeconds;
        int maxAirdropDelay = (int)TimeSpan.FromSeconds(25).TotalSeconds;
        int airdropDelay = Random.Shared.Next(minAirdropDelay, maxAirdropDelay);

        var trigger = TriggerBuilder.Create()
                .ForJob(airdropJobKey)
                .WithIdentity(triggerKey)
                .StartNow()
                .Build();

        return trigger;
    }
}
