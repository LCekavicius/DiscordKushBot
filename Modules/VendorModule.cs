using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KushBot.Data;
using KushBot.DataClasses;

namespace KushBot.Modules
{
    public class VendorModule : ModuleBase<SocketCommandContext>
    {

        [Command("vendor")]
        public async Task showVendorWares()
        {
            EmbedBuilder builder = new();

            builder.WithTitle("Vendor");
            builder.AddField("Description", "My wife :). I love her very very much and she loves me too.");
            builder.WithColor(Color.Gold);
            builder.WithImageUrl("https://cdn.discordapp.com/attachments/263345049486622721/1228786887423168542/boVENDORsai-ezgif.com-video-to-gif-converter.gif?ex=662d4ff7&is=661adaf7&hm=e931f766e7bb9ca5b81de80a1c7ea4071f069bc7a866f9c8af1a5b3500cc5a27&");
            foreach (var item in Program.VendorObj.Wares)
            {
                var price = await item.GetPriceAsync(Context.User.Id);
                builder.AddField(item.DisplayName, $"{item.GetWareDescription()}\nPrice: **{(price == 0 ? "unobtainable" : $"{price} baps")}** ");
            }

            await ReplyAsync(embed: builder.Build());

        }
    }
}
