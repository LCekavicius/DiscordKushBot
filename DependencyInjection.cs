using KushBot.BackgroundJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace KushBot;

public static class DependencyInjection
{
    public static void AddQuartzInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQuartz(options =>
        {
            var jobKey = JobKey.Create(nameof(ProvideQuestsJob));
            options
                .AddJob<ProvideQuestsJob>(jobKey)
                .AddTrigger(trigger =>
                    {
                        trigger
                            .ForJob(jobKey)
                            .WithCronSchedule("0 0 0 * * ?");
                    }
                );

            if (bool.TryParse(configuration["development"], out var isDev) && isDev)
            {
                options.AddTrigger(trigger =>
                {
                    trigger
                        .ForJob(jobKey)
                        .StartNow();
                });
            }
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
    }
}
