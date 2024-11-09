using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using KushBot.DataClasses;
using Microsoft.Extensions.Configuration;
using KushBot.DataClasses.Vendor;
using KushBot.Global;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Discord.Interactions;
using KushBot.Modules.Interactions;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;

namespace KushBot;

public class DiscordBotService(CommandService _commands,
    DiscordSocketClient _client,
    InteractionService _interactions,
    IServiceProvider _services,
    SqliteDbContext _context,
    MessageHandler _messageHandler) : ModuleBase<SocketCommandContext>
{
    private static IConfiguration _configuration;
    public static ManualResetEventSlim _discordReadyEvent = new ManualResetEventSlim(false);

    public static bool BotTesting = false;

    public static List<string> ArchonAbilityList = new() { "Regeneration", "Toughen hide", "Paralyze", "Dismantle", "Demoralize", "Dodge" };
    public static Dictionary<string, string> ArchonAbilityDescription = new Dictionary<string, string>()
    {
        { "Regeneration", "Regenerate % of missing hp" },
        { "Toughen hide", "Absorb % of damage taken next round" },
        { "Paralyze", "Paralyze a participant, making them useless for the remainder of the combat" },
        { "Dismantle", "Ignore damage from items and pet tiers for the remainder of the combat" },
        { "Demoralize", "Make all participants attack with their weakest pets next round" },
        { "Dodge", "Obtain a chance to dodge user attacks for the next round" },
    };

    public static Dictionary<string, string> ArchonAbilityEmoji = new Dictionary<string, string>()
    {
        { "Regeneration", ":heartpulse:" },
        { "Toughen hide", ":shield:" },
        { "Paralyze", ":syringe:" },
        { "Dismantle", ":screwdriver:" },
        { "Demoralize", ":speaking_head:" },
        { "Dodge", ":dash:" },
    };

    public static Dictionary<ulong, DateTime> InfestationIgnoredUsers { get; set; } = new Dictionary<ulong, DateTime>();

    public static ulong Test;
    public static ulong PetTest;
    public static ulong Fail;
    public static ulong NerfUser;
    public static ulong TierTest;

    public static Dictionary<ulong, DateTime> IgnoredUsers = new Dictionary<ulong, DateTime>();

    public static int PictureCount = 99;

    public static ulong DumpChannelId = 902541958117990534;

    public static ulong BossChannelId = 946752140603453460;

    public static List<string> WeebPaths = new List<string>();
    public static List<string> CarPaths = new List<string>();
    public static List<string> ItemPaths = new List<string>();
    public static List<string> ArchonItemPaths = new List<string>();

    public static List<ulong> TestingPhaseAllowedIds = new List<ulong>();

    public static int DailyGiveLimit = 3000;
    public static int MaxPlots = 9;
    public static int MaxTickets = 3;

    public static bool IsDisabled = false;
    public static bool IsBotUseProhibited = false;

    //Goes up whenever a rarer boss spawns, down when worse spawns
    public static double BossNerfer = 0;

    //Infest mechanic
    public static ulong? InfestedChannelId = null;
    public static DateTime? InfestedChannelDate = null;
    public static TimeSpan InfestedChannelDuration = TimeSpan.FromHours(1);

    public static List<ChannelPerms> ChannelPerms = new();

    public async Task RunBotAsync()
    {
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All
        };

        var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", false, true)
            .AddEnvironmentVariables();

        _configuration = builder.Build();

        if (bool.TryParse(_configuration["development"], out var value) && value)
        {
            BotTesting = value;
        }

        //event subscriptions
        _client.Log += Log;
        _client.InteractionCreated += OnInteractionCreatedAsync;
        _client.Ready += OnClientReady;
        _client.MessageReceived += MessageReceivedAsync;

        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        ChannelPerms = await _context.ChannelPerms.ToListAsync();

        await _client.LoginAsync(TokenType.Bot, _configuration["token"]);

        await _client.StartAsync();


        if (UserStatus.TryParse<UserStatus>(_configuration["status"], out UserStatus status))
        {
            await _client.SetStatusAsync(status);
        }
        else
        {
            await _client.SetStatusAsync(UserStatus.Online);
        }

        //InitializeBosses();

        WeebPaths = Data.Data.ReadWeebShit();
        CarPaths = Data.Data.ReadCarShit();

        await Task.Delay(-1);
    }

    public async Task OnClientReady()
    {
        await _client.SetGameAsync("rasyk kush tutorial");
        _discordReadyEvent.Set();
    }

    private Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        _ = Task.Run(async () => await HandleInteractionAsync(interaction));
        return Task.CompletedTask;
    }

    public async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        if (interaction is not SocketMessageComponent component)
            return;

        using var scope = _services.CreateScope();

        await _context.MakeRowForUser(interaction.User.Id);

        var context = new SocketInteractionContext(_client, interaction);
        await _interactions.ExecuteCommandAsync(context, scope.ServiceProvider);

        var test = interaction as SocketMessageComponent;

        if (!interaction.HasResponded)
            await interaction.DeferAsync();
    }

    private Task Log(LogMessage arg)
    {
        Console.WriteLine(arg);

        return Task.CompletedTask;
    }

    public static async Task RedeemMessage(string name, string everyone, string desc, ulong channelId)
    {
        //ulong id = AllowedKushBotChannels[0];

        //if (BotTesting)
        //{
        //    id = 494199544582766610;
        //}
        //var chnl = _client.GetChannel(channelId) as IMessageChannel;

        //if (everyone == "")
        //{
        //    await chnl.SendMessageAsync($"{name} Has redeemed {desc}");
        //}
        //else
        //{
        //    await chnl.SendMessageAsync($"{everyone}, {name} Has redeemed {desc}");

        //}
    }

    private Task MessageReceivedAsync(SocketMessage arg)
    {
        _ = Task.Run(async () => await _messageHandler.HandleCommandAsync(arg));

        return Task.CompletedTask;
    }

    public static async Task EndRage(ulong userId, int RageCash, IMessageChannel channelForRage = null)
    {

        //ulong id;
        //if (BotTesting)
        //{
        //    id = 902541957694390298;
        //}
        //else
        //{
        //    id = AllowedKushBotChannels[0];
        //}

        //channelForRage ??= _client.GetChannel(id) as IMessageChannel;

        //await channelForRage.SendMessageAsync($"<@{userId}> after calming down you count **{RageCash}** extra baps from all that raging");
    }

    //public static void InitializeBosses()
    //{
    //    #region Archons
    //    ArchonList.Add(new BossDetails(
    //        name: "Abyssal Archon",
    //        rarity: "Epic",
    //        desc: "Subject ZYL has been lost. Possible containment breach.  Specifications are as follows:\r\nthe parasitic entity incubates, hatches, and matures all within the host. Detectable only by those at the Precipice. Chances of finding the host in case of escape are nearly non existent with the current Lords at the peak. If our analysis is correct, the subject, at maturity, is instantly capable destruction theorized only ascended being can wield. If our analysis is correct, it is related to the Archon that appeared in the distant past. If our analysis is correct, there is no god that could save us. Not this time, at least.",
    //        imageUrl: "https://cdn.discordapp.com/attachments/263345049486622721/1224467436963889183/ezgif.com-animated-gif-maker.gif?ex=661d992a&is=660b242a&hm=9fecb03b5fb58c04aab64a06b56aead04617ab76193b5c53241bdac5e1419f2c&"));
    //    #endregion

    //    var text = File.ReadAllText("Data/Bosses.json");

    //    BossList = JsonConvert.DeserializeObject<List<BossDetails>>(text);
    //}

    //Gets avg Pet lvl + pet tier
}
