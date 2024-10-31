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
    public class Give : ModuleBase<SocketCommandContext>
    {
        [Command("give", RunMode = RunMode.Async), Alias("pay")]
        public async Task Send(string _amount, IUser user)
        {
            int amount = 0;
            if (_amount == "all")
            {
                amount = Data.Data.GetBalance(Context.User.Id);
            }
            else
            {
                amount = int.Parse(_amount);
            }

            if (Data.Data.GetBalance(Context.User.Id) < amount)
            {
                await ReplyAsync($"{Context.User.Mention}, you don't even have that kind of cash, dumbass");
                return;
            }
            if (amount <= 0)
            {
                await ReplyAsync($"{Context.User.Mention}, not how it works, niggy");
                return;
            }

            if (Data.Data.GetRemainingDailyGiveBaps(Context.User.Id) < amount)
            {
                await ReplyAsync($"{Context.User.Mention}, too much giveaway action, cringe");
                return;
            }

            await Data.Data.SaveDailyGiveBaps(Context.User.Id, amount);

            string PackageCode = RandomString(5);
            int FlyTime = 8;
            if (amount > 100)
            {
                FlyTime++;
            }
            if (amount > 1000)
            {
                FlyTime++;
            }
            if (amount > 10000)
            {
                FlyTime += 2;
            }

            await Data.Data.SaveBalance(Context.User.Id, amount * -1, false);


            Package packet = new Package(PackageCode, amount, Context.User.Id, user.Id);

            DiscordBotService.GivePackages.Add(packet);

            await ReplyAsync($"{Context.User.Mention}'s package, holding **{amount}** Baps, 'Code **{PackageCode}** ' is on it's way to {user.Mention}, it'll arrive in **{FlyTime}** seconds \n " +
                $"if you have pet Jew, you can use 'kush yoink CODE'(e.g. kush yoink B9JZF) to steal some baps off the package (Possible even if on cooldown)");

            await Task.Delay(FlyTime * 1000);

            bool stolen = false;

            if (!DiscordBotService.GivePackages.Contains(packet))
            {
                stolen = true;
            }

            if (!stolen)
            {
                await ReplyAsync($"{Context.User.Mention} Gave {packet.Baps} Baps to {user.Mention}, what a generous shitstain");
                DiscordBotService.GivePackages.Remove(packet);
            }
            else
            {
                await ReplyAsync($"{Context.User.Mention} Gave {packet.Baps} Baps to {user.Mention}, tho {amount - packet.Baps} baps were stolen");
            }
            await Data.Data.SaveBalance(user.Id, packet.Baps, false);
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
