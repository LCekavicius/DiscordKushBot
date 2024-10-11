using Discord;
using Discord.WebSocket;
using KushBot.EventHandler.Interactions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KushBot.DataClasses.Vendor
{
    public class Vendor
    {
        public string Name => "Vendor";
        public List<Ware> Wares { get; set; } = new();
        [JsonIgnore]
        public IMessageChannel Channel { get; set; }
        public ulong MessageId { get; set; }
        public DateTime NextRestockDate => GetNextRestockDate();
        public DateTime LastRestockDateTime { get; set; } = DateTime.MinValue;
        [JsonIgnore]
        public Dictionary<ulong, DateTime> UserPurchases { get; set; } = new();

        private DateTime GetNextRestockDate()
        {
            DateTime current = DateTime.Now;
            DateTime nextRestock = new DateTime(current.Year, current.Month, current.Day, 18, 0, 0);

            if (current.Hour >= 18 && current.Minute >= 0)
            {
                nextRestock = nextRestock.AddDays(1);
            }

            return nextRestock;
        }

        public void GenerateWares()
        {
            VendorWareFactory factory = new();
            Wares = factory.GetRandomWares();
        }

        public async Task RestockAsync()
        {
            VendorWareFactory factory = new();
            Wares = factory.GetRandomWares();

            await Channel.ModifyMessageAsync(MessageId, e =>
            {
                e.Embed = BuildEmbed();
                e.Components = BuildComponents();
            });

            LastRestockDateTime = DateTime.Now;
            SaveVendorToJson();
        }

        public async Task HandleRestockAsync()
        {
            if (NextRestockDate - LastRestockDateTime > TimeSpan.FromHours(24))
            {
                await RestockAsync();
            }
        }

        public void SaveVendorToJson()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            File.WriteAllText(DiscordBotService.VendorJsonPath, json);
        }

        public void GetChannel(ulong guildId)
        {
            var guild = DiscordBotService._client.GetGuild(guildId);
            Channel = guild.GetChannel(DiscordBotService.VendorChannelId) as IMessageChannel;
        }

        public MessageComponent BuildComponents()
        {
            var builder = new ComponentBuilder();
            foreach (var ware in Wares)
            {
                IEmote emote = null;
                if (!Emoji.TryParse(DiscordBotService.LeftSideVendorWareEmojiMap[ware.Type], out var value))
                {
                    emote = Emote.Parse(DiscordBotService.LeftSideVendorWareEmojiMap[ware.Type]);
                }
                else
                {
                    emote = value;
                }

                builder.WithButton(ware.EnumDisplayName,
                    customId: $"{InteractionHandlerFactory.VendorComponentId}_{Wares.IndexOf(ware)}",
                    emote: emote,
                    style: ButtonStyle.Success);
            }

            return builder.Build();
        }

        public Embed BuildEmbed()
        {
            EmbedBuilder builder = new EmbedBuilder();
            return builder.WithTitle(Name)
                .AddField("Description", "My wife :). I love her very very much and she loves me too.")
                //.AddField("Current Wares", string.Join("\n", Wares.Select(e => e.GetWareString())), true)
                .AddField(Wares[0].DisplayName, Wares[0].GetWareString(), true)
                .AddField("\u200b", "\u200b", true)
                .AddField(Wares[1].DisplayName, Wares[1].GetWareString(), true)
                .AddField(Wares[2].DisplayName, Wares[2].GetWareString(), true)
                .AddField("\u200b", "\u200b", true)
                .AddField("Next restock", $"<t:{(NextRestockDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds}:R>", true)
                .WithColor(Color.Gold)
                .WithImageUrl("https://cdn.discordapp.com/attachments/263345049486622721/1228786887423168542/boVENDORsai-ezgif.com-video-to-gif-converter.gif?ex=662d4ff7&is=661adaf7&hm=e931f766e7bb9ca5b81de80a1c7ea4071f069bc7a866f9c8af1a5b3500cc5a27&")
                .WithFooter("You can only buy ((ONE)) ware a day (for now)\nUse the command 'kush vendor' in any of the teleloto channels for detailed wares")
                .Build();
        }
    }
}
