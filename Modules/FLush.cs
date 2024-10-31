using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace KushBot.Modules;

public class FLush : ModuleBase<SocketCommandContext>
{
    [Command("Flush")]
    public async Task PingAsync()
    {
        DiscordBotService.IgnoredUsers.Clear();
    }
}
