using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace KushBot;

public static class IServiceCollectionExtensions
{
    public static void AddQuartzInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQuartz(options =>
        {
            options.UsePersistentStore(persistence =>
            {
                persistence.UseNewtonsoftJsonSerializer();
                persistence.UseProperties = true;
                persistence.UseSQLite(provider =>
                {
                    provider.ConnectionString = $@"Data Source= Data/Database.sqlite;Version=3";
                });
            });
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        services.AddHostedService<JobSchedulerService>();
    }
}