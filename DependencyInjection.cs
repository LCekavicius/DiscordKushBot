using KushBot.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;

namespace KushBot;

public static class DependencyInjection
{
    public static IServiceCollection AddQuartzInfrastructure(this IServiceCollection services)
    {
        services.AddQuartz(options =>
        {
            var jobKey = JobKey.Create(nameof(ProvideQuestsJob));
            options
                .AddJob<ProvideQuestsJob>(jobKey)
                .AddTrigger(trigger =>
                {
                    trigger.ForJob(jobKey)
                    .WithSimpleSchedule(schedule =>
                    {
                        schedule.WithIntervalInSeconds(5).RepeatForever();
                    });
                });

            Console.WriteLine("Job added");
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
            Console.WriteLine("Quartz Hosted Service Started");
        });

        return services;
    }
}
