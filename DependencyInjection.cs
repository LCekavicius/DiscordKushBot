using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace KushBot;

public static class DependencyInjection
{
    public static IServiceCollection AddQuartzInfrastructure(this IServiceCollection services)
    {
        services.AddQuartz();
        services.AddQuartzHostedService();
        return services;
    }
}
