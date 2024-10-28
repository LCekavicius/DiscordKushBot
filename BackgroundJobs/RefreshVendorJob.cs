using Discord.WebSocket;
using KushBot.Resources.Database;
using KushBot.Services;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace KushBot.BackgroundJobs;

[DisallowConcurrentExecution]
public class RefreshVendorJob(
    ILogger<RefreshVendorJob> logger,
    VendorService vendorService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            logger.LogInformation("Restocking vendor");
            await vendorService.RestockAsync();
        }
        catch (Exception ex)
        {
            logger.LogError($"Restocking vendor failed with exception: {ex.Message}");
        }
    }
}
