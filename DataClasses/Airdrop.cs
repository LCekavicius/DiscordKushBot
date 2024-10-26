using Discord;
using Discord.Rest;
using Discord.WebSocket;
using KushBot.DataClasses.Enums;
using KushBot.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot;

public class Airdrop
{
    private const int MaxLoots = 4;
    public int TimesLooted { get; set; }
    public Dictionary<ulong, int> UserLoots { get; set; }
    public RestUserMessage Message { get; set; }

    public Airdrop(RestUserMessage message)
    {
        TimesLooted = 0;
        UserLoots = new();
        Message = message;
    }

    public void Loot(KushBotUser user)
    {
        if (TimesLooted >= MaxLoots)
        {
            AirDrops.Current.Remove(AirDrops.Current.FirstOrDefault(e => e.Message.Id == Message.Id));
            return;
        }

        if (UserLoots.ContainsKey(user.Id))
        {
            return;
        }

        UserLoots.Add(user.Id, 0);
        TimesLooted++;

        int baps = GetBaps(user);

        UserLoots[user.Id] = baps;

        user.Balance += baps;
    }

    public EmbedBuilder UpdateBuilder(DiscordSocketClient _client)
    {
        EmbedBuilder builder = new EmbedBuilder();

        builder.WithTitle("Airdrop");
        builder.WithColor(Color.Orange);
        builder.AddField("Loots remaining:", $"**{MaxLoots - TimesLooted}**");

        string text = "";

        foreach (var item in UserLoots)
        {
            text += $"{_client.GetUser(item.Key).Username} looted **{item.Value}** baps\n";
        }

        builder.AddField("Looted by:", text);

        builder.WithFooter("Click on the button to collect the airdrop");
        builder.WithImageUrl("https://cdn.discordapp.com/attachments/902541957694390298/1223740109451432047/cat-hedgehog.gif?ex=661af3ca&is=66087eca&hm=ed2188ec15aff97fed417ed47da7855c11d7714e95f5a67b2106a72208bc8862&");
        return builder;
    }

    public int GetBaps(KushBotUser user)
    {
        Random rad = new Random();

        int pos = UserLoots.Select(e => e.Key).ToList().IndexOf(user.Id) + 1;

        int rawBaps = 100 + user.Pets.TotalCombinedPetLevel * 2;

        double bapsFlat = user.Items.Equipped.GetStatTypeBonus(ItemStatType.AirDropFlat);
        double BapsPercent = user.Items.Equipped.GetStatTypeBonus(ItemStatType.AirDropPercent);

        return (int)(((rawBaps + bapsFlat) * (1 + BapsPercent / 100)) * (1.5 - (pos * 0.2)));
    }
}
