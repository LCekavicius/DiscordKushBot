using Discord.Commands;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using System;
using System.Threading.Tasks;

namespace KushBot.Modules;

[RequirePermissions(Permissions.Core)]
public class Egg(SqliteDbContext dbContext) : ModuleBase<SocketCommandContext>
{
    [Command("egg")]
    public async Task BuyEgg()
    {
        int price = 350;

        var user = await dbContext.GetKushBotUserAsync(Context.User.Id);

        if (user.Balance <= price)
        {
            await ReplyAsync($"{Context.User.Mention} You don't have {price} Baps to buy DN");
            return;
        }

        user.Balance -= price;
        user.Eggs += 1;

        await dbContext.SaveChangesAsync();
        await ReplyAsync($"{Context.User.Mention} You bought my egg for {price} Baps");
    }

    [Command("Sell"), Alias("Sell egg", "sellegg")]
    public async Task SellyEggy()
    {
        var user = await dbContext.GetKushBotUserAsync(Context.User.Id);

        if (user.Eggs <= 0)
        {
            await ReplyAsync($"You don't even have an egg, 10 iq? {CustomEmojis.EggSleep}");
            return;
        }

        int baps = Random.Shared.Next(100, 200);

        user.Balance += baps;
        user.Eggs -= 1;
        await dbContext.SaveChangesAsync();
        await ReplyAsync($"You have sold your egg and got **{baps}** baps!");
    }
}
