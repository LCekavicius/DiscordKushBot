using Discord.Commands;
using KushBot.Global;
using System;
using System.Threading.Tasks;

namespace KushBot.Modules;

public class SellEgg : ModuleBase<SocketCommandContext>
{
    [Command("Sell"), Alias("Sell egg", "sellegg")]
    public async Task SellyEggy()
    {
        var user = Data.Data.GetKushBotUser(Context.User.Id);

        if (user.Eggs <= 0)
        {
            await ReplyAsync($"You don't even have an egg, 10 iq? {CustomEmojis.EggSleep}");
            return;
        }


        Random rad = new Random();
        int baps = rad.Next(100, 200);

        await ReplyAsync($"You have sold your egg and got **{baps}** baps!");
        user.Balance += baps;
        user.Eggs -= 1;
        await Data.Data.SaveKushBotUserAsync(user);
    }

}
