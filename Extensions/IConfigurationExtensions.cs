using Microsoft.Extensions.Configuration;

namespace KushBot.Extensions;

public static class IConfigurationExtensions
{
    public static bool IsDev(this IConfiguration configuration)
    {
        return bool.TryParse(configuration["development"], out var value) && value;
    }
}
