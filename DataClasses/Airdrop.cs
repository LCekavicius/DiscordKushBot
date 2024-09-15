using Discord;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using KushBot.Data;
using System.Threading.Tasks;
using KushBot.DataClasses;

namespace KushBot
{
    public class Airdrop
    {
        public int TimesLooted { get; set; }
        public List<ulong> LootedPlayers { get; set; }
        public RestUserMessage Message { get; set; }

        public Airdrop(RestUserMessage message)
        {
            TimesLooted = 0;
            LootedPlayers = new List<ulong>();
            Message = message;
        }

        public async Task Loot(ulong userId)
        {
            if(TimesLooted >= 4)
            {
                return;
            }
            if (LootedPlayers.Contains(userId))
            {
                return;
            }

            LootedPlayers.Add(userId);
            TimesLooted++;

            EmbedBuilder builder = UpdateBuilder();

            await Message.ModifyAsync(x =>
            {
                x.Embed = builder.Build();
            });
            
            int baps = GetBaps(userId);

            await Data.Data.SaveBalance(userId, baps, false);

        }

        public EmbedBuilder UpdateBuilder()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Airdrop");
            builder.WithColor(Color.Orange);
            builder.AddField("Loots remaining:", $"**{4 - TimesLooted}**");

            string text = "";

            foreach (ulong item in LootedPlayers)
            {
                text +=  $"{Program._client.GetUser(item).Username} looted **{GetBaps(item)}** baps\n";
            }

            builder.AddField("Looted by:",text);

            builder.WithFooter("Click on the ima reaction to collect the airdrop");
            builder.WithImageUrl("https://cdn.discordapp.com/attachments/902541957694390298/1223740109451432047/cat-hedgehog.gif?ex=661af3ca&is=66087eca&hm=ed2188ec15aff97fed417ed47da7855c11d7714e95f5a67b2106a72208bc8862&");
            return builder;
        }

        public int GetBaps(ulong userId)
        {
            Random rad = new Random();

            int pos = LootedPlayers.IndexOf(userId) + 1;

            int rawBaps = 100 + Program.GetTotalPetLvl(userId) * 2;
            List<Item> items = Data.Data.GetUserItems(userId);
            //items
            int bapsFlat = 0;
            double BapsPercent = 0;

            List<int> equiped = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                equiped.Add(Data.Data.GetEquipedItem(userId, i+1));
                if(equiped[i] != 0)
                {
                    Item item = items.Where(x => x.Id == equiped[i]).FirstOrDefault();
                    if(item.AirDropFlat != 0)
                    {
                        bapsFlat += item.AirDropFlat;
                    }
                    if(item.AirDropPercent != 0)
                    {
                        BapsPercent += item.AirDropPercent;
                    }
                }
            }

            return (int)(((rawBaps + bapsFlat) * (1 + BapsPercent / 100)) * (1.5 - (pos * 0.2)));
        }

    }
}
