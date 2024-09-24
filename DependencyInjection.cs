using KushBot.BackgroundJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;

namespace KushBot;

public static class DependencyInjection
{
    //public static void AddQuartzInfrastructure(this IServiceCollection services, IConfiguration configuration)
    //{
    //    services.AddQuartz(options =>
    //    {
    //        var jobKey = JobKey.Create(nameof(ProvideQuestsJob));
    //        options
    //            .AddJob<ProvideQuestsJob>(jobKey)
    //            .AddTrigger(trigger =>
    //                {
    //                    trigger
    //                        .ForJob(jobKey)
    //                        .WithCronSchedule("0 0 0 * * ?");
    //                }
    //            );

    //        if (bool.TryParse(configuration["development"], out var isDev) && isDev)
    //        {
    //            options.AddTrigger(trigger =>
    //            {
    //                trigger
    //                    .ForJob(jobKey)
    //                    .StartNow();
    //            });
    //        }
    //    });

    //    services.AddQuartzHostedService(options =>
    //    {
    //        options.WaitForJobsToComplete = true;
    //    });

    //    services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
    //}
    public static void AddQuartzInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQuartz(options =>
        {
            var jobKey = JobKey.Create(nameof(ProvideQuestsJob)); // DEFAULT group will be used

            options
                .AddJob<ProvideQuestsJob>(opts => opts.WithIdentity(jobKey)) // Ensure job is added with identity
                .AddTrigger(trigger =>
                {
                    trigger
                        .ForJob(jobKey)
                        .WithIdentity($"{nameof(ProvideQuestsJob)}_CronTrigger") // Optionally, name the trigger
                        .WithCronSchedule("0 0 0 * * ?"); // Set the cron schedule
                });

            // Optionally add immediate execution in development mode
            if (bool.TryParse(configuration["development"], out var isDev) && isDev)
            {
                options.AddTrigger(trigger =>
                {
                    trigger
                        .ForJob(jobKey)
                        .WithIdentity($"{nameof(ProvideQuestsJob)}_DevTrigger") // Another trigger name for immediate run
                        .StartNow();
                });
            }
        });

        // Ensure the Quartz hosted service is running
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
    }
}
