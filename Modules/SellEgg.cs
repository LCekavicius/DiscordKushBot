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
    public class SellEgg : ModuleBase<SocketCommandContext>
    {
        [Command("Sell"),Alias("Sell egg","sellegg")]
        public async Task SellyEggy()
        {
            if (!Data.Data.GetEgg(Context.User.Id))
            {
                await ReplyAsync($"You don't even have an egg, 10 iq? <:eggsleep:610494851557097532> ");
                return;
            }


            Random rad = new Random();
            int baps = rad.Next(100, 200);

            await ReplyAsync($"You have sold your egg and got **{baps}** baps!");
            await Data.Data.SaveBalance(Context.User.Id, baps, false);
            await Data.Data.SaveEgg(Context.User.Id, false);
        }

    }
}
