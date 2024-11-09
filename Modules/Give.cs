using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using KushBot.Resources.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules;

public class Give(SqliteDbContext dbContext) : ModuleBase<SocketCommandContext>
{
    public static List<Package> GivePackages = new();

    [Command("give"), Alias("pay")]
    [RequirePermissions(Permissions.Core)]
    public async Task Send(string _amount, IUser user)
    {
        var botUser = await dbContext.GetKushBotUserAsync(Context.User.Id);
        int amount = _amount == "all" ? botUser.Balance : int.Parse(_amount);
        
        if (botUser.Balance < amount)
        {
            await ReplyAsync($"{Context.User.Mention}, you don't even have that kind of cash, dumbass");
            return;
        }
        
        if (amount <= 0)
        {
            await ReplyAsync($"{Context.User.Mention}, not how it works, niggy");
            return;
        }

        if (botUser.DailyGive < amount)
        {
            await ReplyAsync($"{Context.User.Mention}, too much giveaway action, cringe");
            return;
        }


        string code = RandomString(5);
        int flyTime = 8;
        flyTime = flyTime switch
        {
            >= 1000 => flyTime + 2,
            >= 100 => flyTime + 1,
            _ => flyTime
        };

        botUser.DailyGive -= amount;
        botUser.Balance -= amount;

        var package = new Package(code, amount, Context.User.Id, user.Id);

        GivePackages.Add(package);
        
        await dbContext.SaveChangesAsync();

        await ReplyAsync($"{Context.User.Mention}'s package, holding **{amount}** Baps, 'Code **{code}** ' is on it's way to {user.Mention}, it'll arrive in **{flyTime}** seconds \n " +
            $"if you have pet Jew, you can use 'kush yoink CODE'(e.g. kush yoink B9JZF) to steal some baps off the package (Possible even if on cooldown)");

        await Task.Delay(flyTime * 1000);
        await dbContext.MakeRowForUser(user.Id);
        var target = await dbContext.GetKushBotUserAsync(user.Id);

        bool stolen = false;

        if (!GivePackages.Contains(package))
        {
            stolen = true;
        }

        target.Balance += package.Baps;
        await dbContext.SaveChangesAsync();

        if (!stolen)
        {
            await ReplyAsync($"{Context.User.Mention} Gave {package.Baps} Baps to {user.Mention}, what a generous shitstain");
            GivePackages.Remove(package);
        }
        else
        {
            await ReplyAsync($"{Context.User.Mention} Gave {package.Baps} Baps to {user.Mention}, tho {amount - package.Baps} baps were stolen");
        }
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(e => e[Random.Shared.Next(e.Length)]).ToArray());
    }
}
