using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.Modules
{
    [Group("C17H13ClN4")]
    public class LSDEaster : ModuleBase<SocketCommandContext>
    {
        bool enabled = true;
        [Command("")]
        public async Task lsd()
        {
            if(!enabled)
            {
                return;
            }
            await ReplyAsync($"{Context.User.Mention} http://prntscr.com/ltorz1");
        }
        [Command("stop")]
        public async Task sto()
        {
            enabled = false;
        }
    }
}
