using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace KushBot.BackgroundJobs;

internal class ProvideQuestsJob : IJob
{
    private readonly ILogger<ProvideQuestsJob> _logger;
    public ProvideQuestsJob(ILogger<ProvideQuestsJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Hello from job");

        return Task.CompletedTask;
    }
}
