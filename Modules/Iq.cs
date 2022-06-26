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
    public class Iq : ModuleBase<SocketCommandContext>
    {
        [Command("iq")]
        public async Task plot()
        {
            Random rad = new Random();

            switch (rad.Next(0, 2))
            {
                case 0:
                    await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lu3xtu");
                    break;
                case 1:
                    await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lu3xwq");
                    break;
            }
        }

        [Command("tetis")]
        public async Task plots()
        {
            Random rad = new Random();

            switch (rad.Next(0, 2))
            {
                case 0:
                    await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lu3y7z");
                    break;
                case 1:
                    await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lu3yct");
                    break;
            }
        }

        [Command("dievas")]
        public async Task plotss()
        {
            Random rad = new Random();

            switch (rad.Next(0, 3))
            {
                case 0:
                    await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lu3z9d");
                    break;
                case 1:
                    await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lu3ze9");
                    break;
                case 2:
                    await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lu3xjg");
                    break;
            }
        }
    }
}
