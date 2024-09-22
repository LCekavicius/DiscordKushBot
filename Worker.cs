using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace KushBot;

public class Worker : BackgroundService
{
    private readonly KushBot.DiscordBotService _discordBotService;

    public Worker(KushBot.DiscordBotService discordBotService)
    {
        _discordBotService = discordBotService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _discordBotService.RunBotAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
