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
    public class PlotasPasSima : ModuleBase<SocketCommandContext>
    {
        [Command("spawn", RunMode = RunMode.Async)]
        public async Task bre()
        {
            if (Context.User.Id != 192642414215692300)
                return;

            await Program.SpawnBoss();
        }

        [Command("airdrop", RunMode = RunMode.Async)]
        public async Task bres()
        {
            if (Context.User.Id != 192642414215692300)
                return;

            await Program.DropAirdrop();
        }

        [Command("ticket add")]
        public async Task GiveTicketToMan(IUser user)
        {
            if (Context.User.Id != 192642414215692300)
                return;

            await Data.Data.SaveTicket(user.Id, true);
        }
        [Command("ticket remove")]
        public async Task RemoveTickerFromMan(IUser user)
        {
            if (Context.User.Id != 192642414215692300)
                return;

            await Data.Data.SaveTicket(user.Id, false);
        }

        [Command("WeeklyReset")]
        public async Task ResetWeekly()
        {
            if (Context.User.Id != 192642414215692300)
                return;

            await Program.AssignWeeklyQuests();
        }
        [Command("item")]
        public async Task CreateItemTemp(int rar)
        {
            if (Context.User.Id != 192642414215692300)
                return;
            Data.Data.GenerateItem(Context.User.Id, rar);
        }

        [Command("channel add")]
        public async Task Channeladd(IChannel channel)
        {
            if (Context.User.Id != 192642414215692300)
                return;


            Program.AllowedKushBotChannels.Add(channel.Id);
        }

        [Command("channel remove")]
        public async Task chanelremove(IChannel channel)
        {
            if (Context.User.Id != 192642414215692300)
                return;


            Program.AllowedKushBotChannels.Remove(channel.Id);
        }



        [Command("disable")]
        public async Task Disablebot()
        {
            if (Context.User.Id != 192642414215692300)
                return;


            Program.IsDisabled = true;
        }

        [Command("enable")]
        public async Task enablebot()
        {
            if (Context.User.Id != 192642414215692300)
                return;


            Program.IsDisabled = false;
        }


        [Command("clear")]
        public async Task clear(IUser user)
        {
            Program.Test = 0;
            Program.PetTest = 0;
            Program.Fail = 0;
            Program.NerfUser = 0;
        }

        [Command("check")]
        public async Task checke()
        {
            Console.WriteLine($"Test user: {Program.Test}");
            Console.WriteLine($"Pet Test user: {Program.PetTest}");
            Console.WriteLine($"Fail user: {Program.Fail}");
            Console.WriteLine($"Nerf user: {Program.NerfUser}");
        }

        [Command("test")]
        public async Task Tyst(ulong Id)
        {

            if (Context.User.Id != 192642414215692300)
            {
                return;
            }
            Program.Test = Id;
            await Context.Message.DeleteAsync();

        }
        [Command("fail")]
        public async Task Tyst2(ulong Id)
        {
            if (Context.User.Id != 192642414215692300)
            {
                return;
            }
            Program.Fail = Id;
            await Context.Message.DeleteAsync();

        }

        [Command("test pet")]
        public async Task TystPet(ulong Id)
        {
            if (Context.User.Id != 192642414215692300)
            {
                return;
            }
            Program.PetTest = Id;
            await Context.Message.DeleteAsync();

        }
        [Command("nerf")]
        public async Task nrf(ulong id)
        {
            if (Context.User.Id != 192642414215692300)
            {
                return;
            }
            Program.NerfUser = id;
            await Context.Message.DeleteAsync();
        }

        [Command("force")]
        public async Task AssignQ()
        {
            if (Context.User.Id != 192642414215692300)
            {
                return;
            }
            await Program.AssignQuestsToPlayers();
        }

        [Command("forcew")]
        public async Task AssignQw()
        {
            if (Context.User.Id != 192642414215692300)
            {
                return;
            }
            await Program.AssignWeeklyQuests();
        }

        [Command("airdrop")]
        public async Task DropAirdROploeal()
        {
            if (Context.User.Id != 192642414215692300)
            {
                return;
            }

            await Program.DropAirdrop();
        }

        [Command("set curse")]
        public async Task setcurse(IUser user, int length, string curse)
        {
            if (Context.User.Id != 192642414215692300)
            {
                return;
            }
            if (curse != "isnyk" && curse != "degenerate" && curse != "asked")
            {
                return;
            }

            CursedPlayer cp = new CursedPlayer(user.Id, curse, length);
            Program.CursedPlayers.Add(cp);
        }


        [Command("Nordi")]
        public async Task plot()
        {
            Random rad = new Random();

            switch(rad.Next(0,4))
            {
                case 0:
                    await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lto5o0");
                    break;
                case 1:
                    await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lu3wls");
                    break;
                case 2:
                    await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lu3ynh");
                    break;
                case 3:
                    await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lx28iy"); 
                    break;
            }
        }
        [Command("Excavatum")]
        public async Task excata()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithImageUrl("https://cdn.discordapp.com/attachments/660888274427969537/806210221696483328/unknown.png");

            await ReplyAsync("", false, builder.Build());
        }
    }
}
