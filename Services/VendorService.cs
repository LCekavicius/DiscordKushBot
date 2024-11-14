using Discord;
using Discord.WebSocket;
using KushBot.DataClasses;
using KushBot.DataClasses.Vendor;
using KushBot.Global;
using KushBot.Modules.Interactions;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KushBot.Services;

public class VendorProperties
{
    public string Name => "Vendor";
    public List<Ware> Wares { get; set; } = new();
    public ulong MessageId { get; set; }
    public DateTime NextRestockDate => GetNextRestockDate();
    public DateTime LastRestockDateTime { get; set; } = DateTime.MinValue;

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
}

public class VendorService
{
    public VendorProperties Properties { get; private set; }
    public IMessageChannel Channel { get; set; }
    public Dictionary<ulong, DateTime> UserPurchases { get; set; } = new();
    private const string JsonPath = @"Data/Vendor.json";
    
    public static ManualResetEventSlim VendorReadyEventSlim = new ManualResetEventSlim(false);

    private readonly ILogger<VendorService> _logger;
    private readonly SqliteDbContext _context;
    private readonly DiscordSocketClient _client;

    public static Dictionary<VendorWare, string> LeftSideVendorWareEmojiMap = new()
    {
        { VendorWare.Cheems, CustomEmojis.Cheems },
        { VendorWare.Item, ":shield:" },
        { VendorWare.PetFoodCommon, ":canned_food:" },
        { VendorWare.PetFoodRare, ":canned_food:" },
        { VendorWare.PetFoodEpic, ":canned_food:" },
        { VendorWare.BossTicket, ":ticket:" },
        { VendorWare.Icon, ":frame_photo:" },
        { VendorWare.Rejuvenation, ":recycle:" },
        { VendorWare.Egg, CustomEmojis.Egg },
        { VendorWare.PetDupeCommon, ":gemini:" },
        { VendorWare.PetDupeRare, ":gemini:" },
        { VendorWare.PetDupeEpic, ":gemini:" },
        { VendorWare.PlotBoost, ":arrow_up:" },
        { VendorWare.KushGym, ":muscle:" },
        { VendorWare.FishingRod, ":fishing_pole_and_fish:" },
        { VendorWare.Parasite, "<:tf:946039048789688390>" },
        { VendorWare.Artillery, ":rocket:" },
        { VendorWare.Adderal, ":pill:" },
        { VendorWare.SlotsTokens, ":coin:" },
    };

    public VendorService(ILogger<VendorService> logger, SqliteDbContext context, DiscordSocketClient client)
    {
        _logger = logger;
        _context = context;
        _client = client;
        _client.Ready += StartAsync;
    }

    public async Task StartAsync()
    {
        if (!File.Exists(JsonPath))
        {
            _logger.LogWarning("Vendor is not attached");
            return;
        }

        string json = File.ReadAllText(JsonPath);
        var vendor = JsonConvert.DeserializeObject<VendorProperties>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });

        bool requiresRestock = DateTime.Now > vendor.NextRestockDate;

        IMessageChannel channel = await GetVendorChannelAsync();

        if(channel == null)
        {
            _logger.LogError("No channel with vendor permissions found");
            return;
        }

        Properties = vendor;
        Channel = channel;

        VendorReadyEventSlim.Set();
    }

    public async Task GenerateVendorAsync()
    {
        var channel = await GetVendorChannelAsync();

        if (channel == null)
        {
            _logger.LogError("No channel with vendor permissions found");
            return;
        }

        Properties = new VendorProperties();
        GenerateWares();

        var message = await channel.SendMessageAsync(embed: BuildEmbed(), components: BuildComponents());

        Properties.MessageId = message.Id;

        if (!File.Exists(JsonPath))
        {
            File.Create(JsonPath).Close();
        }

        SerializeVendor();
    }

    public void GenerateWares()
    {
        VendorWareFactory factory = new();
        Properties.Wares = factory.GetRandomWares();
    }

    private async Task<IMessageChannel> GetVendorChannelAsync()
    {
        var channelId = await _context.ChannelPerms
            .Where(e => (e.PermissionsValue & (int)Permissions.Vendor) != 0)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();
        
        return await _client.GetChannelAsync(channelId) as IMessageChannel;
    }

    public async Task RestockAsync()
    {
        VendorWareFactory factory = new();
        Properties.Wares = factory.GetRandomWares();

        await Channel.ModifyMessageAsync(Properties.MessageId, e =>
        {
            e.Embed = BuildEmbed();
            e.Components = BuildComponents();
        });

        Properties.LastRestockDateTime = DateTime.Now;
        SerializeVendor();
    }

    public void SerializeVendor()
    {
        string json = JsonConvert.SerializeObject(Properties, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });
        File.WriteAllText(JsonPath, json);
    }

    public MessageComponent BuildComponents()
    {
        var builder = new ComponentBuilder();
        foreach (var ware in Properties.Wares)
        {
            IEmote emote = null;
            if (!Emoji.TryParse(LeftSideVendorWareEmojiMap[ware.Type], out var value))
            {
                emote = Emote.Parse(LeftSideVendorWareEmojiMap[ware.Type]);
            }
            else
            {
                emote = value;
            }

            builder.WithButton(ware.EnumDisplayName,
                customId: $"{nameof(PurchaseVendorWare)}_{Properties.Wares.IndexOf(ware)}",
                emote: emote,
                style: ButtonStyle.Success);
        }

        return builder.Build();
    }

    public Embed BuildEmbed()
    {
        EmbedBuilder builder = new EmbedBuilder();
        return builder.WithTitle(Properties.Name)
            .AddField("Description", "My wife :). I love her very very much and she loves me too.")
            .AddField(Properties.Wares[0].DisplayName, Properties.Wares[0].GetWareString(), true)
            .AddField("\u200b", "\u200b", true)
            .AddField(Properties.Wares[1].DisplayName, Properties.Wares[1].GetWareString(), true)
            .AddField(Properties.Wares[2].DisplayName, Properties.Wares[2].GetWareString(), true)
            .AddField("\u200b", "\u200b", true)
            .AddField("Next restock", $"<t:{(Properties.NextRestockDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds}:R>", true)
            .WithColor(Color.Gold)
            .WithImageUrl("https://cdn.discordapp.com/attachments/263345049486622721/1228786887423168542/boVENDORsai-ezgif.com-video-to-gif-converter.gif?ex=662d4ff7&is=661adaf7&hm=e931f766e7bb9ca5b81de80a1c7ea4071f069bc7a866f9c8af1a5b3500cc5a27&")
            .WithFooter("You can only buy ((ONE)) ware a day (for now)\nUse the command 'kush vendor' in any of the teleloto channels for detailed wares")
            .Build();
    }
}
