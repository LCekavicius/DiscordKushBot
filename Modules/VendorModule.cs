﻿using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using KushBot.Resources.Database;
using KushBot.Services;
using System.Threading.Tasks;

namespace KushBot.Modules;

[RequirePermissions(Permissions.Core)]
public class VendorModule(VendorService vendorService, SqliteDbContext dbContext) : ModuleBase<SocketCommandContext>
{
    [Command("vendor")]
    public async Task showVendorWares()
    {
        EmbedBuilder builder = new();

        var user = await dbContext.GetKushBotUserAsync(Context.User.Id, UserDtoFeatures.Pets | UserDtoFeatures.Pictures | UserDtoFeatures.Buffs);

        builder.WithTitle("Vendor");
        builder.AddField("Description", "My wife :). I love her very very much and she loves me too.");
        builder.WithColor(Color.Gold);
        builder.WithImageUrl("https://cdn.discordapp.com/attachments/263345049486622721/1228786887423168542/boVENDORsai-ezgif.com-video-to-gif-converter.gif?ex=662d4ff7&is=661adaf7&hm=e931f766e7bb9ca5b81de80a1c7ea4071f069bc7a866f9c8af1a5b3500cc5a27&");
        foreach (var item in vendorService.Properties.Wares)
        {
            var price = item.GetPrice(user);
            builder.AddField(item.DisplayName, $"{item.GetWareDescription()}\nPrice: **{(price == 0 ? "unobtainable" : $"{price} baps")}** ");
        }

        await ReplyAsync(embed: builder.Build());

    }
}
