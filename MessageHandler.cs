using Discord.Commands;
using Discord.WebSocket;
using KushBot.Global;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot;

public partial class MessageHandler(
    CommandService commands,
    IServiceProvider services,
    DiscordSocketClient client)
{
    protected SqliteDbContext dbContext = null;

    public async Task HandleCommandAsync(SocketMessage arg)
    {
        var message = arg as SocketUserMessage;
        
        if (message is null || message.Author.IsBot)
        {
            return;
        }

        if (DiscordBotService.IsBotUseProhibited && message.Author.Id != 192642414215692300)
        {
            return;
        }

        int argPos = 0;
        string Prefix = "Kush ";

        await HandleCurseAsync(message);

        if (DiscordBotService.IgnoredUsers.ContainsKey(message.Author.Id) && DiscordBotService.IgnoredUsers[message.Author.Id] < TimeHelper.Now)
        {
            DiscordBotService.IgnoredUsers.Remove(message.Author.Id);
        }

        await HandleNyaTradeAsync(message);

        if (message.HasStringPrefix(Prefix, ref argPos, StringComparison.OrdinalIgnoreCase))
        {
            using var scope = services.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();

            await dbContext.MakeRowForUser(message.Author.Id);

            await HandleInfestationEventAsync(message);
            await HandleInfectionBapsConsumeAsync(message);

            var context = new SocketCommandContext(client, message);

            var result = await commands.ExecuteAsync(context, argPos, scope.ServiceProvider);

            if (!result.IsSuccess && !result.ErrorReason.Contains("missing permissions"))
            {
                Console.WriteLine(result.ErrorReason);
            }
        }
    }
}
