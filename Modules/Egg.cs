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

            var user = Data.Data.GetKushBotUser(Context.User.Id);

            if(user.Balance <= price)
            {
                await ReplyAsync($"{Context.User.Mention} You don't have {price} Baps to buy DN");
                return;
            }

            user.Balance -= price;
            user.Eggs += 1;
            await Data.Data.SaveKushBotUserAsync(user);
            await ReplyAsync($"{Context.User.Mention} You bought my egg for {price} Baps");
        }

    }
}
