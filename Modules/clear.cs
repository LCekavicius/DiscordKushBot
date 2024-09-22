using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.Modules
{
    public class clearpp : ModuleBase<SocketCommandContext>
    {
        [Command("clear")]
        public async Task cl()
        {
            DiscordBotService.Fail = 0;
            DiscordBotService.Test = 0;
            DiscordBotService.NerfUser = 0;
            DiscordBotService.PetTest = 0;
        }
    }
}
