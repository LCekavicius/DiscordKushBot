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

namespace KushBot;

public class DiscordBotService : ModuleBase<SocketCommandContext>
{
    //public static DiscordSocketClient _client;
    public readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private static IConfiguration _configuration;
    public static ManualResetEventSlim _discordReadyEvent = new ManualResetEventSlim(false);

    public DiscordBotService(CommandService commands, DiscordSocketClient client, InteractionService interactions, IServiceProvider services)
    {
        _client = client;
        _commands = commands;
        _services = services;
        _interactions = interactions;
    }

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

    public static List<Package> GivePackages;
    public static List<ExistingDuel> Duels;

    public static Dictionary<ulong, DateTime> IgnoredUsers = new Dictionary<ulong, DateTime>();

    public static int PictureCount = 99;

    public static List<CursedPlayer> CursedPlayers = new List<CursedPlayer>();

    public static ulong DumpChannelId = 641612898493399050;

    //Todo replace with actual logic for setting up servers
    public static List<ulong> AllowedKushBotChannels = new List<ulong>();

    public static ulong BossChannelId = 946752140603453460;

    public static List<string> WeebPaths = new List<string>();
    public static List<string> CarPaths = new List<string>();
    public static List<string> ItemPaths = new List<string>();
    public static List<string> ArchonItemPaths = new List<string>();

    public static List<ulong> Engagements = new List<ulong>();

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
        { VendorWare.Egg, "CustomEmojis.Egg" },
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

        //_client = new DiscordSocketClient(config);

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
        TutorialManager.LoadInitial();

        GivePackages = new List<Package>();
        Duels = new List<ExistingDuel>();

        //MainChnl
        AllowedKushBotChannels.Add(946752080318709780);
        AllowedKushBotChannels.Add(946752098882707466);
        AllowedKushBotChannels.Add(946752113730539553);
        AllowedKushBotChannels.Add(946752126892257301);
        AllowedKushBotChannels.Add(946829857407529020);
        //boss
        AllowedKushBotChannels.Add(945817014247776378);
        //hidden
        AllowedKushBotChannels.Add(641612898493399050);

        WeebPaths = Data.Data.ReadWeebShit();
        CarPaths = Data.Data.ReadCarShit();

        if (BotTesting)
        {
            AllowedKushBotChannels.Add(902541957694390298);
            AllowedKushBotChannels.Add(494199544582766610);
            AllowedKushBotChannels.Add(640865006740832266);
            BossChannelId = 902541957694390298;
            DumpChannelId = 902541958117990534;
            //await guild.DownloadUsersAsync();
        }

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

        await Data.Data.MakeRowForUser(interaction.User.Id);

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
        _ = Task.Run(async () => await HandleCommandAsync(arg));

        return Task.CompletedTask;
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        var message = arg as SocketUserMessage;

        if (IsBotUseProhibited && message.Author.Id != 192642414215692300)
        {
            return;
        }

        if (IsDisabled && message.Author.Id != 192642414215692300)
        {
            return;
        }
        if (message is null || message.Author.IsBot)
        {
            return;
        }
        int argPos = 0;
        string Prefix;

        Prefix = "Kush ";

        await DealWithAbilities(message as SocketUserMessage);

        if (IgnoredUsers.ContainsKey(message.Author.Id) && IgnoredUsers[message.Author.Id] < DateTime.Now)
        {
            IgnoredUsers.Remove(message.Author.Id);
        }

        await HandleNyaTradeAsync(message);

        if (message.HasStringPrefix(Prefix, ref argPos, StringComparison.OrdinalIgnoreCase))
        {
            bool newJew = await Data.Data.MakeRowForUser(message.Author.Id);
            if (newJew)
                Test = message.Author.Id;


            if (!AllowedKushBotChannels.Contains(message.Channel.Id) && !message.Content.Contains("yike") && !message.Content.Contains("nya") && !message.Content.Contains("redeem")
                && !message.Content.Contains("moteris") && !message.Content.Contains("vroom"))
            {
                return;
            }

            if ((message.Content.ToLower().Contains("nya") || message.Content.ToLower().Contains("vroom")) && message.Channel.Id == 337945443252305920)
            {
                await message.AddReactionAsync(Emoji.Parse("❌"));
                return;
            }

            using var scope = _services.CreateScope();

            await HandleInfestationEventAsync(message);
            await HandleInfectionBapsConsumeAsync(message);

            var context = new SocketCommandContext(_client, message);

            var result = await _commands.ExecuteAsync(context, argPos, scope.ServiceProvider);

            if (!result.IsSuccess)
            {
                Console.WriteLine(result.ErrorReason);
            }
        }
    }

    private async Task HandleInfectionBapsConsumeAsync(SocketUserMessage message)
    {
        if (Random.Shared.NextDouble() < 0.97)
            return;

        int consumedBaps = await Data.Data.InfectionConsumeBapsAsync(message.Author.Id);

        if (consumedBaps == 0)
            return;


        await message.Channel.SendMessageAsync($"{message.Author.Mention} A parasite eats away at your flesh, draining you out of {consumedBaps} baps");
    }

    private async Task HandleNyaTradeAsync(SocketMessage message)
    {
        if (!NyaClaimGlobals.NyaTrades.Any(e => e.Respondee.UserId == message.Author.Id || e.Suggester.UserId == message.Author.Id))
            return;

        NyaClaimGlobals.NyaTrades.RemoveAll(e => (DateTime.Now - e.DateTime).TotalSeconds > 90 && (e.Respondee.UserId == message.Author.Id || e.Suggester.UserId == message.Author.Id));

        try
        {
            if (NyaClaimGlobals.NyaTrades.Any(e => e.Respondee.UserId == message.Author.Id))
            {
                var trade = NyaClaimGlobals.NyaTrades.FirstOrDefault(e => e.Respondee.UserId == message.Author.Id);
                if (message.Content.Any(e => char.IsDigit(e)) && !message.Content.ToLower().Contains("kush"))
                {
                    var claim = NyaClaimGlobals.ParseTradeInput(message.Content);

                    if (claim == null)
                        return;

                    EmbedBuilder builder = new();
                    builder.WithColor(Color.Magenta);
                    builder.WithAuthor($"{message.Author.Username}'s trade response", message.Author.GetAvatarUrl());

                    var nyaClaim = Data.Data.GetClaimBySortIndex(message.Author.Id, (claim ?? 5000) - 1);

                    if (nyaClaim == null && claim != null)
                        return;

                    if (nyaClaim != null)
                    {
                        builder.WithImageUrl(nyaClaim.Url);
                        builder.AddField("Keys", ":key2: (0)", true);
                    }

                    trade.Respondee.NyaClaim = nyaClaim;

                    await message.Channel.SendMessageAsync($"{_client.GetUser(trade.Suggester.UserId).Mention} {message.Author.Username} has responded, type 'confirm' in chat if you wish to confirm", embed: builder.Build());
                }
            }

            if (NyaClaimGlobals.NyaTrades.Any(e => e.Suggester.UserId == message.Author.Id && e.Respondee.HasResponded))
            {
                var trade = NyaClaimGlobals.NyaTrades.FirstOrDefault(e => e.Suggester.UserId == message.Author.Id && e.Respondee.HasResponded);
                if (trade != null && message.Content.ToLower() == "confirm")
                {
                    await message.Channel.SendMessageAsync($"{message.Author.Mention} :handshake: {_client.GetUser(trade.Respondee.UserId).Mention} The trade has concluded.");
                    await Data.Data.ConcludeNyaTradeAsync(trade);
                }
            }
        }
        catch { }
    }

    private async Task HandleInfestationEventAsync(SocketMessage message)
    {
        if (!AllowedKushBotChannels.Contains(message.Channel.Id))
            return;

        if (InfestationIgnoredUsers.ContainsKey(message.Author.Id))
        {
            if (InfestationIgnoredUsers[message.Author.Id] < DateTime.Now)
                InfestationIgnoredUsers.Remove(message.Author.Id);
            else
                return;
        }

        Random rnd = new Random();

        if (InfestedChannelId != null && (InfestedChannelDate + InfestedChannelDuration) > DateTime.Now)
        {
            if (message.Channel.Id != InfestedChannelId)
                return;

            if (rnd.NextDouble() > 0.9935)
            {
                await Data.Data.InfestUserAsync(message.Author.Id);
                DiscordBotService.InfestationIgnoredUsers.Add(message.Author.Id, DateTime.Now.AddHours(8));
            }
        }
        else
        {
            if (rnd.NextDouble() > 0.995)
            {
                var component = InfestationStart.BuildMessageComponent(false);
                await message.Channel.SendMessageAsync($"An odd looking egg appears from the ground. Best leave it be.", components: component);
            }
        }
    }

    public async Task DealWithAbilities(SocketUserMessage message)
    {
        CursedPlayer cp = CursedPlayers.Where(x => x.ID == message.Author.Id).FirstOrDefault();

        if (cp == null)
        {
            return;
        }

        if (cp.CurseName == "asked")
        {
            if (cp.Duration > 0)
            {
                await message.Channel.SendMessageAsync($"{message.Author.Mention} :warning: KLAUSEM :warning:");

                if (!cp.lastMessages.Contains(message.Content) && message.Content.Length > 2)
                {
                    cp.Duration -= 1;
                }

                if (cp.Duration <= 0)
                {
                    CursedPlayers.Remove(cp);
                    return;
                }

                cp.lastMessages.Add(message.Content);
            }
        }
        else if (cp.CurseName == "isnyk")
        {
            if (cp.Duration > 0)
            {
                await message.DeleteAsync();

                if (!cp.lastMessages.Contains(message.Content))
                {
                    cp.Duration -= 1;
                }

                if (cp.Duration == 0)
                {
                    CursedPlayers.Remove(cp);
                    return;
                }

                cp.lastMessages.Add(message.Content);
            }
        }
        else if (cp.CurseName == "degenerate")
        {
            if (cp.Duration > 0)
            {
                Random rnd = new Random();
                await message.Channel.SendFileAsync(WeebPaths[rnd.Next(0, WeebPaths.Count)]);

                if (!cp.lastMessages.Contains(message.Content))
                {
                    cp.Duration -= 1;
                }

                if (cp.Duration == 0)
                {
                    CursedPlayers.Remove(cp);
                    return;
                }

                cp.lastMessages.Add(message.Content);
            }
        }

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
    public static int GetAveragePetLvl(ulong id)
    {
        var pets = Data.Data.GetUserPets(id);
        return (int)pets.Average(e => e.Value.Level + e.Value.Tier);
    }

    public static int GetTotalPetLvl(ulong id)
    {
        var pets = Data.Data.GetUserPets(id);
        return pets.Sum(e => e.Value.CombinedLevel);
    }
}
