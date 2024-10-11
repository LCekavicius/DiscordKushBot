using KushBot.Global;
using Quartz;
using System;

namespace KushBot.BackgroundJobs;

public static class TriggerFactory
{
    public static ITrigger CreateAirDropTrigger(JobKey jobKey = null)
    {
        int minAirdropDelay = (int)TimeSpan.FromMinutes(150).TotalSeconds;
        int maxAirdropDelay = (int)TimeSpan.FromMinutes(4 * 60).TotalSeconds;
        int airdropDelay = Random.Shared.Next(minAirdropDelay, maxAirdropDelay);

        var builder = TriggerBuilder.Create();
        if (jobKey != null)
        {
            builder.ForJob(jobKey);
        }

        return builder
            .WithIdentity($"{nameof(AirDropJob)}_RandomTrigger")
            .StartAt(TimeHelper.Now.AddSeconds(airdropDelay))
            .Build();
    }

    public static ITrigger CreateInstantTrigger(string identity, JobKey jobKey = null)
    {
        var builder = TriggerBuilder.Create();
        if (jobKey != null)
        {
            builder.ForJob(jobKey);
        }

        return builder
            .WithIdentity(identity)
            .StartNow()
            .Build();
    }
}
