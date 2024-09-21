using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace KushBot.BackgroundJobs;

[DisallowConcurrentExecution]
public class ProvideQuestsJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Hello from job");

        return Task.CompletedTask;
    }
}
