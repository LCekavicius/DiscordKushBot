using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace KushBot.Modules
{
    public class Egg : ModuleBase<SocketCommandContext>
    {
        [Command("egg")]
        public async Task BuyEgg()
        {
            int price = 350;

            if (Data.Data.GetEgg(Context.User.Id) == true)
            {
                await ReplyAsync($"{Context.User.Mention} you dumb fuck shit retard fuck you.");
                return;
            }
            if(Data.Data.GetBalance(Context.User.Id) < price)
            {
                await ReplyAsync($"{Context.User.Mention} You don't have {price} Baps to buy DN");
                return;
            }

            await ReplyAsync($"{Context.User.Mention} You bought my egg for {price} Baps");
            await Data.Data.SaveBalance(Context.User.Id,price * -1, false);
            await Data.Data.SaveEgg(Context.User.Id, true);

        }

    }
}
