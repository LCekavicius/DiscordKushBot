using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KushBot.Data;
using System.IO;
using Discord.Rest;
using System.Linq;

namespace KushBot.Modules
{
    public class Nya : ModuleBase<SocketCommandContext>
    {
        public static bool test = false;
        public static string testString = "lhYgEDF";

        [Command("nya test")]
        public async Task nyaTest()
        {
            if (Context.User.Id == 192642414215692300)
                test = true;
        }

        [Command("nya marry", RunMode = RunMode.Async)]
        public async Task NyaMarry()
        {
            DateTime lastNyaMarry = Data.Data.GetNyaMarryDate(Context.User.Id);
            if (lastNyaMarry > DateTime.Now)
            {
                TimeSpan ts = lastNyaMarry - DateTime.Now;
                await ReplyAsync($"<:pepeshy:948015871199182858> {Context.User.Mention} You still need to wait {ts.Hours:d2}:{ts.Minutes:d2}:{ts.Seconds:d2} before you " +
                    $"can remarry <:pepeshy:948015871199182858>");
                return;
            }

            if (Program.Engagements.Contains(Context.User.Id))
            {
                await ReplyAsync($"{Context.User.Mention} You dumb fucking shit fuck you dumb fuck retard bitch die adopted shit ape nigger");
                return;
            }


            Program.Engagements.Add(Context.User.Id);

            await ReplyAsync($"<:pepeshy:948015871199182858> {Context.User.Mention} you're prepared to engage. The next kush nya (or kush vroom <:Pepew:945806849406566401>) you " +
                $"roll will get to be on your stats page <:pepeshy:948015871199182858>");
        }


        [Command("nya", RunMode = RunMode.Async)]
        public async Task Throw()
        {
            Random rnd = new Random();

            int index = rnd.Next(0, Program.WeebPaths.Count);

            string send = Program.WeebPaths[index];

            if (test && Context.User.Id == 192642414215692300)
            {
                send = Program.WeebPaths.Where(x => x.Contains(testString)).FirstOrDefault();
                test = false;
            }
            var picture = await Context.Channel.SendFileAsync($"{send}") as RestUserMessage;

            if (Program.Engagements.Contains(Context.User.Id))
            {
                await Data.Data.SaveNyaMarry(Context.User.Id, picture.Attachments.First().Url);
                Program.Engagements.Remove(Context.User.Id);
                await Data.Data.AddToNyaMarryDate(Context.User.Id, 6);
                await ReplyAsync($"{Context.User.Mention} You succesfully married <:Pog:948018159665938462><:Pepew:945806849406566401><:pepeshy:948015871199182858>");
            }

            if(rnd.NextDouble() <= 0.0005)
            {
                int baps = rnd.Next(10, 25);
                await Data.Data.SaveBalance(Context.User.Id, baps, false);
                await ReplyAsync($":3 {Context.User.Mention} WOAH you uncovered {baps} baps :0");
            }
        }

        [Command("vroom", RunMode = RunMode.Async)]
        public async Task ThrowCar()
        {
            Random rnd = new Random();

            int index = rnd.Next(0, Program.CarPaths.Count);

            var picture = await Context.Channel.SendFileAsync($"{Program.CarPaths[index]}") as RestUserMessage;

            if (Program.Engagements.Contains(Context.User.Id))
            {
                await Data.Data.SaveNyaMarry(Context.User.Id, picture.Attachments.First().Url);
                Program.Engagements.Remove(Context.User.Id);
                await Data.Data.AddToNyaMarryDate(Context.User.Id, 6);
                await ReplyAsync($"{Context.User.Mention} You succesfully married <:Pog:948018159665938462><:Pepew:945806849406566401><:pepeshy:948015871199182858>");
            }

        }
    }
}
