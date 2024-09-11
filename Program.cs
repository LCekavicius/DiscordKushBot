using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using KushBot.Resources.Database;
using KushBot.DataClasses;
using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json;
using KushBot.EventHandler.Interactions;
using KushBot.DataClasses.Vendor;
using KushBot.Global;

namespace KushBot
{
    class Program : ModuleBase<SocketCommandContext>
    {

        static void Main(string[] args)
        => new Program().RunBotAsync().GetAwaiter().GetResult();

        public static int GambleDelay = 350;

        public static DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public static bool BotTesting = false;

        static System.Timers.Timer Timer;
        static System.Timers.Timer AirDropTimer;

        //public static List<Pet> Pets = new List<Pet>();
        //public static List<Boss> Bosses = new List<Boss>();
        //public static List<BossDetails> BossList = new List<BossDetails>();
        //public static List<BossDetails> ArchonList = new List<BossDetails>();

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

        //public static BossObject BossObject;
        //public static BossObject ArchonObject;

        public static List<Quest> Quests = new List<Quest>();
        public static List<Quest> WeeklyQuests = new List<Quest>();
        public static ulong RaceFinisher = 0;

        //public static List<CatchUp> CatchupMechanic;

        public static ulong Test;
        public static ulong PetTest;
        public static ulong Fail;
        public static ulong NerfUser;
        public static ulong TierTest;

        public static List<Package> GivePackages;
        public static List<ExistingDuel> Duels;

        public static Dictionary<ulong, DateTime> IgnoredUsers = new Dictionary<ulong, DateTime>();

        public static int RewardForFullQuests;

        public static int PictureCount = 99;

        public static List<CursedPlayer> CursedPlayers = new List<CursedPlayer>();

        public static ulong DumpChannelId = 641612898493399050;

        public static Airdrop airDrop;

        public static List<ulong> AllowedKushBotChannels = new List<ulong>();

        public static ulong BossChannelId = 946752140603453460;

        public static List<string> WeebPaths = new List<string>();
        public static List<string> CarPaths = new List<string>();
        public static List<string> ItemPaths = new List<string>();
        public static List<string> ArchonItemPaths = new List<string>();

        public static List<string> GetItemPathsByRarity(int rarity) { return rarity == 6 ? ArchonItemPaths : ItemPaths; }

        public static List<ulong> Engagements = new List<ulong>();

        public static DateTime LastWeebSend = DateTime.Now;

        public static List<ulong> TestingPhaseAllowedIds = new List<ulong>();

        public static int DailyGiveLimit = 3000;
        public static int MaxPlots = 9;
        public static int MaxTickets = 3;

        public static bool IsDisabled = false;
        public static bool IsBotUseProhibited = false;

        public static int ItemCap = 15;

        //Goes up whenever a rarer boss spawns, down when worse spawns
        public static double BossNerfer = 0;

        public static IConfiguration configuration;

        //Infest mechanic
        public static ulong? InfestedChannelId = null;
        public static DateTime? InfestedChannelDate = null;
        public static TimeSpan InfestedChannelDuration = TimeSpan.FromHours(1);

        public static HashSet<ulong> lowTierUsers = new();

        public static string InfestationEventComponentId = "infestation-start";
        public static string VendorComponentId = "vendor";
        public static string ParasiteComponentId = "kill";
        public static string NyaClaimComponentId = "nyaClaim";
        public static string PaginatedComponentId = "paginated";


        public static int BaseMaxNyaClaims = 12;

        public static ulong VendorChannelId = 1228798440088142005;
        public static Vendor VendorObj;
        public static string VendorJsonPath = "Data/Vendor.json";

        public static int TimerSecond = 59;


        public static Dictionary<VendorWare, string> LeftSideVendorWareEmojiMap = new()
        {
            { VendorWare.Cheems, "<:Cheems:945704650378707015>" },
            { VendorWare.Item, ":shield:" },
            { VendorWare.PetFoodCommon, ":canned_food:" },
            { VendorWare.PetFoodRare, ":canned_food:" },
            { VendorWare.PetFoodEpic, ":canned_food:" },
            { VendorWare.BossTicket, ":ticket:" },
            { VendorWare.Icon, ":frame_photo:" },
            { VendorWare.Rejuvenation, ":recycle:" },
            { VendorWare.Egg, "<:egg:945783802867879987>" },
            { VendorWare.PetDupeCommon, ":gemini:" },
            { VendorWare.PetDupeRare, ":gemini:" },
            { VendorWare.PetDupeEpic, ":gemini:" },
            { VendorWare.PlotBoost, ":arrow_up:" },
            { VendorWare.KushGym, ":muscle:" },
            { VendorWare.FishingRod, ":fishing_pole_and_fish:" },
            { VendorWare.Parasite, "<:tf:946039048789688390>" },
            { VendorWare.Artillery, ":rocket:" },
            //{ VendorWare.Concerta, ":headstone:" },
            //{ VendorWare.Ambien, ":zany_face:" },
            { VendorWare.Adderal, ":pill:" },
            { VendorWare.SlotsTokens, ":coin:" },
            //{ VendorWare.Plot, ":park:" },
        };

        public async Task RunBotAsync()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            };

            _client = new DiscordSocketClient(config);
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", false, true)
                .AddEnvironmentVariables();

            configuration = builder.Build();

            if (bool.TryParse(configuration["development"], out var value) && value)
            {
                Program.BotTesting = value;
            }

            if (BotTesting)
            {
                VendorChannelId = 902541957694390302;
            }

            //event subscriptions
            _client.Log += Log;
            _client.ReactionAdded += OnReactionAdded;
            _client.InteractionCreated += OnInteractionCreatedAsync;
            _client.Ready += OnClientReady;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, configuration["token"]);

            await _client.StartAsync();

            if (UserStatus.TryParse<UserStatus>(configuration["status"], out UserStatus status))
            {
                await _client.SetStatusAsync(status);
            }
            else
            {
                await _client.SetStatusAsync(UserStatus.Online);
            }

            //InitializeBosses();
            AddQuests();
            AddWeeklyQuests();
            TutorialManager.LoadInitial();
            //lowTierUsers = Data.Data.GetUsersWithLowProgress();
            //CatchupMechanic = new List<CatchUp>();

            Random rad = new Random();
            RewardForFullQuests = rad.Next(75, 200);

            await _client.SetGameAsync("rasyk kush tutorial");

            AirDropTimer = new System.Timers.Timer(3 * 60 * 60 * 1000);
            AirDropTimer.Elapsed += DropAirdropEvent;
            AirDropTimer.AutoReset = true;
            AirDropTimer.Enabled = true;



            Timer = new System.Timers.Timer(1000 * 60);
            Timer.Elapsed += TimerEvent;
            Timer.AutoReset = true;
            Timer.Enabled = true;

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
            ItemPaths = Data.Data.ReadItems("Items");
            ArchonItemPaths = Data.Data.ReadItems("ArchonItems");


            if (BotTesting)
            {
                AllowedKushBotChannels.Add(902541957694390298);
                AllowedKushBotChannels.Add(494199544582766610);
                AllowedKushBotChannels.Add(640865006740832266);
                await AssignQuestsToPlayers();
                //await AssignWeeklyQuests();
                BossChannelId = 902541957694390298;
                DumpChannelId = 902541958117990534;
                //await DropAirdrop();


                //await guild.DownloadUsersAsync();
                //await SpawnBoss();
            }

            await Task.Delay(-1);

        }

        public async Task OnClientReady()
        {
            await _client.SetGameAsync("rasyk kush tutorial");

            if (VendorObj != null)
                return;

            if (!File.Exists(VendorJsonPath))
                return;


            string json = File.ReadAllText(VendorJsonPath);
            Vendor deserialized = JsonConvert.DeserializeObject<Vendor>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            bool requiresRestock = DateTime.Now > deserialized.NextRestockDate;

            IMessageChannel channel = _client.GetChannel(VendorChannelId) as IMessageChannel;

            VendorObj = deserialized;
            VendorObj.Channel = channel;

            if (requiresRestock)
            {
                await VendorObj.RestockAsync();
            }
        }

        public async Task OnInteractionCreatedAsync(SocketInteraction interaction)
        {
            if (interaction is not SocketMessageComponent component)
                return;

            InteractionHandlerFactory factory = new();
            ComponentHandler kushInteraction = factory.GetComponentHandler(component.Data.CustomId, interaction.User.Id, interaction, component);

            await kushInteraction.HandleClick();

            if (!interaction.HasResponded)
                await interaction.DeferAsync();
        }

        public async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot)
            {
                return;
            }

            if (airDrop != null && reaction.MessageId == airDrop.Message.Id)
            {
                var guild = _client.GetGuild(337945443252305920);
                string emoteName = "ima";
                if (BotTesting)
                {
                    guild = _client.GetGuild(902541957149106256);
                    emoteName = "ima";
                }

                if (reaction.Emote.Name == emoteName)
                {
                    await airDrop.Loot(reaction.UserId);
                    await TutorialManager.AttemptSubmitStepCompleteAsync(reaction.UserId, 4, 1, reaction.Channel);
                }
            }

            return;
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);

            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public static async Task RedeemMessage(string name, string everyone, string desc, ulong channelId)
        {
            ulong id = AllowedKushBotChannels[0];

            if (BotTesting)
            {
                id = 494199544582766610;
            }
            var chnl = _client.GetChannel(channelId) as IMessageChannel;

            if (everyone == "")
            {
                await chnl.SendMessageAsync($"{name} Has redeemed {desc}");
            }
            else
            {
                await chnl.SendMessageAsync($"{everyone}, {name} Has redeemed {desc}");

            }
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


            //if (message.Content.StartsWith(Prefix) || message.Content.StartsWith("Kush ") || message.Content.StartsWith("kush "))
            if (message.HasStringPrefix(Prefix, ref argPos) || message.HasStringPrefix(Prefix.ToLower(), ref argPos))
            {

                bool newJew = await Data.Data.MakeRowForJew(message.Author.Id);
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

                await HandleInfestationEventAsync(message);
                await HandleInfectionBapsConsumeAsync(message);

                string PlayerQUestsString = Data.Data.GetQuestIndexes(message.Author.Id);
                if (PlayerQUestsString.Contains(10.ToString()))
                {
                    if (Data.Data.GetBalance(message.Author.Id) > Quests[10].GetCompleteReq(message.Author.Id))
                    {
                        ulong channel;
                        if (BotTesting)
                        {
                            channel = 902541957694390298;
                        }
                        else
                        {
                            channel = AllowedKushBotChannels[0];
                        }
                        try
                        {
                            List<int> PlayerQuests = new List<int>();
                            string[] values = PlayerQUestsString.Split(',');
                            foreach (var item in values)
                            {
                                PlayerQuests.Add(int.Parse(item));
                            }
                            await CompleteQuest(10, PlayerQuests, _client.GetChannel(channel) as IMessageChannel, message.Author);
                        }
                        catch
                        {
                            Console.WriteLine("Failed finish a quest");
                        }
                    }
                }

                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }

        private async Task HandleInfectionBapsConsumeAsync(SocketUserMessage message)
        {
            Random rnd = new Random();

            if (rnd.NextDouble() < 0.97)
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
                }
            }
            else
            {
                if (rnd.NextDouble() > 0.995)
                {
                    InteractionHandlerFactory factory = new();
                    ComponentHandler handler = factory.GetComponentHandler(InfestationEventComponentId, message.Author.Id);

                    await message.Channel.SendMessageAsync($"An odd looking egg appears from the ground. Best leave it be.", components: await handler.BuildMessageComponent(false));
                    Program.InfestationIgnoredUsers.Add(message.Author.Id, DateTime.Now.AddHours(16));
                }
            }
        }

        private static string GetSpawnRarity()
        {
            Random rnd = new Random();

            double t = rnd.NextDouble();

            if (t <= 0.05 - BossNerfer / 9)
            {
                //BossNerfer += 0.05;
                return "Legendary";
            }
            else if (t <= 0.15 - BossNerfer / 6)
            {
                //BossNerfer += 0.03;
                return "Epic";
            }
            else if (t <= 0.3 - BossNerfer / 3)
            {
                //BossNerfer += 0.01;
                return "Rare";
            }
            else if (t <= 0.525 - BossNerfer)
            {
                //BossNerfer += 0.005;
                return "Uncommon";
            }
            else
            {
                //BossNerfer = 0;
                return "Common";
            }
        }

        //public static async Task SpawnBoss(bool isArchonHandler = false, ulong? summonerId = null)
        //{
        //    EmbedBuilder builder = new EmbedBuilder();
        //    string rarity = GetSpawnRarity();

        //    List<BossDetails> appropriateBosses = new List<BossDetails>();

        //    appropriateBosses = isArchonHandler ? ArchonList : BossList.FindAll(x => x.Rarity == rarity);

        //    Random rnd = new Random();
        //    Boss Boss = new Boss(appropriateBosses[rnd.Next(0, appropriateBosses.Count)], isArchon: isArchonHandler);
        //    //Boss Boss = new Boss(BossList[bossIndex]);

        //    //bossIndex++;

        //    builder.WithTitle(Boss.Name);
        //    builder.WithColor(Boss.GetColor());
        //    builder.WithImageUrl(Boss.ImageUrl);
        //    builder.AddField("Level:", $"**{Boss.Level}** 🎚️", true);
        //    builder.AddField("Boss hp:", $"**{Boss.HP} ❤️**", true);
        //    builder.AddField("Rarity:", $"**{(isArchonHandler ? "Archon" : Boss.Rarity)} 💠**\n{Boss.Desc}");

        //    DateTime now = DateTime.Now;
        //    DateTime date = new(now.Year, now.Month, now.Day, now.Hour, now.Minute, TimerSecond);

        //    DateTime startDate = BotTesting
        //        ? date.AddMinutes(1)
        //        : isArchonHandler
        //            ? date.AddMinutes(10)
        //            : date.AddMinutes(30);

        //    builder.AddField($"Participants (0/{Boss.MaxParticipants}):", "---", isArchonHandler);
        //    if (isArchonHandler)
        //    {
        //        builder.AddField($"Archon Abilities", $"{string.Join("\n", Boss.ArchonAbilities)}", isArchonHandler);
        //    }
        //    builder.AddField("Results", $"The battle will start <t:{((startDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds).ToString().Split('.')[0]}:R>");

        //    if (isArchonHandler)
        //    {
        //        builder.WithFooter(string.Join("\n", Boss.ArchonAbilities.Select(e => $"{e.Name} - {ArchonAbilityDescription[e.Name]}")));
        //    }
        //    else
        //    {
        //        builder.WithFooter("Click on the Booba reaction to sign up by using a boss ticket");
        //    }

        //    var emoteguild = _client.GetGuild(902541957149106256);
        //    //server guild
        //    var guild = _client.GetGuild(337945443252305920);

        //    if (BotTesting)
        //    {
        //        guild = _client.GetGuild(902541957149106256);
        //    }

        //    var chnl = guild.GetTextChannel(BossChannelId);

        //    var msg = await chnl.SendMessageAsync("", false, builder.Build());

        //    var emote = "<:Booba:944937036702441554>";

        //    GuildEmote ge = emoteguild.Emotes.FirstOrDefault(x => emote.Contains(x.Id.ToString()));

        //    await msg.AddReactionAsync(ge);
        //    await msg.AddReactionAsync(new Emoji("❌"));
        //    if (isArchonHandler)
        //    {
        //        ArchonObject = new BossObject(Boss, msg, startDate);
        //        ArchonObject.SummonerId = summonerId;
        //    }
        //    else
        //    {
        //        BossObject = new BossObject(Boss, msg, startDate);
        //    }

        //    List<ulong> userIds = Data.Data.GetFollowingByRarity(isArchonHandler ? "Archon" : Boss.Rarity);
        //    var users = userIds.Select(x => guild.GetUser(x));

        //    string txt = "WAKE UP ";
        //    foreach (var item in users)
        //    {
        //        if (guild.Users.Contains(item))
        //            txt += $"{item.Mention} ";
        //    }

        //    if (users.Any())
        //        await chnl.SendMessageAsync(txt);
        //}

        public static async Task DropAirdrop()
        {
            //drops in every except last 2 entries of
            List<ulong> channelIds = AllowedKushBotChannels;
            Random rad = new Random();

            int channel = rad.Next(0, channelIds.Count - 2);

            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Airdrop");
            builder.WithColor(Discord.Color.Orange);
            builder.AddField("Loots remaining:", $"**{4}**");
            builder.WithFooter("Click on the ima reaction to collect the airdrop");
            builder.WithImageUrl("https://cdn.discordapp.com/attachments/902541957694390298/1223740109451432047/cat-hedgehog.gif?ex=661af3ca&is=66087eca&hm=ed2188ec15aff97fed417ed47da7855c11d7714e95f5a67b2106a72208bc8862&");

            var guild = _client.GetGuild(337945443252305920);

            if (BotTesting)
            {
                guild = _client.GetGuild(902541957149106256);
            }

            ulong chosenChannel = channelIds[channel];

            if (BotTesting)
            {
                chosenChannel = 902541957694390298;
            }

            var chnl = guild.GetTextChannel(chosenChannel);


            var msg = await chnl.SendMessageAsync("", false, builder.Build());

            var emote = "<:ima:642437972968603689>";

            if (BotTesting)
            {
                emote = "<:ima:945342040529567795>";
            }
            GuildEmote ge = guild.Emotes.FirstOrDefault(x => emote.Contains(x.Id.ToString()));

            await msg.AddReactionAsync(ge);

            airDrop = new Airdrop(msg);
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

        static async void DropAirdropEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            await DropAirdrop();
            Random rad = new Random();
            int minutes = rad.Next(150, 241);
            AirDropTimer.Interval = minutes * 60 * 1000;
        }

        static async void TimerEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            TimerSecond = e.SignalTime.Second;

            if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0)
            {
                await AssignQuestsToPlayers();

                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                    await AssignWeeklyQuests();
            }

            if (VendorObj != null && DateTime.Now.Hour == 18 && DateTime.Now.Minute == 0)
            {
                await VendorObj.RestockAsync();
            }

            //if (ArchonObject != null && ArchonObject.StartDate.Hour == DateTime.Now.Hour && ArchonObject.StartDate.Minute == DateTime.Now.Minute)
            //{
            //    await ArchonObject.Combat(true);
            //}

            //if (BossObject != null && BossObject.StartDate.Hour == DateTime.Now.Hour && BossObject.StartDate.Minute == DateTime.Now.Minute)
            //{
            //    await BossObject.Combat(false);
            //}

            //if ((DateTime.Now.Hour % 2 == 0 && DateTime.Now.Minute == 0))
            //{
            //    await SpawnBoss();
            //}

        }

        public static async Task AssignWeeklyQuests()
        {
            Data.Data.SetWeeklyQuests();

            using (var DbContext = new SqliteDbContext())
            {
                List<KushBotUser> jews = new List<KushBotUser>();

                foreach (var item in DbContext.Jews)
                {
                    jews.Add(item);
                }

                await Data.Data.ResetWeeklyStuff(jews);

            }

        }

        public static async Task AssignQuestsToPlayers()
        {
            Console.WriteLine("Givign qs");

            Random rad = new Random();
            RewardForFullQuests = rad.Next(80, 200);

            using (var DbContext = new SqliteDbContext())
            {
                int QuestsForPlayer = 3;
                List<KushBotUser> users = DbContext.Jews.ToList();

                foreach (var user in users)
                {
                    user.Pets2 = Data.Data.GetUserPets(user.Id);
                }

                Data.Data.ResetDailyStuff(users);
            }
        }

        public static async Task EndRage(ulong userId, int RageCash, IMessageChannel channelForRage = null)
        {

            ulong id;
            if (BotTesting)
            {
                id = 902541957694390298;
            }
            else
            {
                id = AllowedKushBotChannels[0];
            }

            channelForRage ??= _client.GetChannel(id) as IMessageChannel;

            await channelForRage.SendMessageAsync($"<@{userId}> after calming down you count **{RageCash}** extra baps from all that raging");
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

        static void AddQuests()
        {
            Quests.Add(new Quest(Quests.Count, 1500, $"**Win 1500 baps** from **gambling**", true, 170)); // 0
            Quests.Add(new Quest(Quests.Count, 1500, "**Lose 1500 baps** from **gambling**", true, 180));
            Quests.Add(new Quest(Quests.Count, 700, "**Win 700 baps** from **flipping**", true, 130)); // 2
            Quests.Add(new Quest(Quests.Count, 700, "**Lose 700 baps** from **flipping**", true, 140));
            Quests.Add(new Quest(Quests.Count, 700, "**Win 700 baps** from **Betting**", true, 130)); // 4
            Quests.Add(new Quest(Quests.Count, 700, "**Lose 700 baps** from **Betting**", true, 140));
            Quests.Add(new Quest(Quests.Count, 700, "**Win 700 baps** from **Risking**", true, 130)); // 6
            Quests.Add(new Quest(Quests.Count, 700, "**Lose 700 baps** from **Risking**", true, 140));
            Quests.Add(new Quest(Quests.Count, 38, "**Get 32 or more baps** as a base roll on **begging**", false, 100)); // 8
            Quests.Add(new Quest(Quests.Count, 0, "**?Nekenciu.**", false, 70));
            Quests.Add(new Quest(Quests.Count, 2000, "**Reach** 2000 baps ", true, 325)); // 10
            Quests.Add(new Quest(Quests.Count, 5, "**Beg** 5 times", true, 135));
            Quests.Add(new Quest(Quests.Count, 7, "**Beg** 7 times", true, 200)); // 12
            //13
            Quests.Add(new Quest(Quests.Count, 1, "**Feed** any pet once", true, 100));
            Quests.Add(new Quest(Quests.Count, 750, "**Flip 750 or more baps** in one flip", true, 240)); // 14
            Quests.Add(new Quest(Quests.Count, 3, "**Yoink** Succesfully 3 times", true, 135));
            Quests.Add(new Quest(Quests.Count, 1, "**Fail to Yoink** a target", true, 85)); // 16
            Quests.Add(new Quest(Quests.Count, 3, "**Flip 60 or more baps** and win 3 times in a row ", true, 200));
            Quests.Add(new Quest(Quests.Count, 3, "**Get** a **bet** modifier that's more than **3**", true, 200)); // 18
            Quests.Add(new Quest(Quests.Count, 20, "**Win** a **Risk** of 20 or more baps with a min modifier of **8**", true, 140));
            Quests.Add(new Quest(Quests.Count, 850, "**Bet 850 or more baps** in one bet", true, 250)); // 20
            Quests.Add(new Quest(Quests.Count, 400, $"**risk 400 or more baps** in one risk", true, 220));
            Quests.Add(new Quest(Quests.Count, 1500, "**Win 1500 baps** from **flipping**", true, 275)); // 22
            Quests.Add(new Quest(Quests.Count, 1500, "**Win 1500 baps** from **Betting**", true, 275));
            Quests.Add(new Quest(Quests.Count, 1500, "**Win 1500 baps** from **Risking**", true, 275)); // 24
            //Quests.Add(new Quest(Quests.Count, 400, "**Win 400 baps** from **Dueling**", true, 120));
            //Quests.Add(new Quest(Quests.Count, 800, "**Win 800 baps** from **Dueling**", true, 160));

        }

        static void AddWeeklyQuests()
        {
            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 13000, $"**Win 13000 baps** from **gambling**", true, 850));
            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 13000, $"**Lose 13000 baps** from **gambling**", true, 850));

            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 17000, $"**Win 17000 baps** from **gambling**", true, 960));
            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 17000, $"**Lose 17000 baps** from **gambling**", true, 980));

            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 9000, $"**Win 9000 baps** from **flipping**", true, 750));
            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 9000, $"**Lose 9000 baps** from **flipping**", true, 760));

            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 12000, $"**Win 12000 baps** from **flipping**", true, 950));
            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 12000, $"**Lose 12000 baps** from **flipping**", true, 960));

            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 10500, $"**Win 10500 baps** from **betting**", true, 750));
            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 10500, $"**Lose 10500 baps** from **betting**", true, 760));

            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 12000, $"**Win 12000 baps** from **betting**", true, 950));
            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 12000, $"**Lose 12000 baps** from **betting**", true, 960));

            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 9000, $"**Win 9000 baps** from **risking**", true, 750));
            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 9000, $"**Lose 9000 baps** from **risking**", true, 760));

            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 12000, $"**Win 12000 baps** from **risking**", true, 950));
            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 12000, $"**Lose 12000 baps** from **risking**", true, 960));

            WeeklyQuests.Add(new Quest(WeeklyQuests.Count, 48, "**Beg** 48 times", true, 950));

        }

        public static async Task CompleteWeeklyQuest(int qIndex, IMessageChannel channel, IUser user)
        {
            Random rad = new Random();
            bool completedWeeklies = true;
            List<Quest> weeklies = new List<Quest>();

            List<int> weeklyIds = new List<int>();

            weeklyIds = Data.Data.GetWeeklyQuest();


            if (Data.Data.GetCompletedWeekly(user.Id, 0) == 1 && weeklyIds[0] == qIndex)
            {
                return;
            }

            if (Data.Data.GetCompletedWeekly(user.Id, 1) == 1 && weeklyIds[1] == qIndex)
            {
                return;
            }

            foreach (int item in weeklyIds)
            {
                weeklies.Add(WeeklyQuests.Where(x => x.Id == item).FirstOrDefault());
            }

            bool raceFirst = false;

            int BapsFromPet;

            var userPets = Data.Data.GetUserPets(user.Id);
            var pet = userPets[PetType.Maybich];

            if (pet != null)
            {
                double _BapsFromPet = (Math.Pow(pet.CombinedLevel, 1.3) + pet.CombinedLevel * 3) + (WeeklyQuests[qIndex].Baps / 100 * pet.CombinedLevel);
                BapsFromPet = (int)Math.Round(_BapsFromPet);
            }
            else
            {
                BapsFromPet = 0;
            }

            //Items
            int bapsFlat = 0;
            double bapsPercent = 0;
            List<Item> items = Data.Data.GetUserItems(user.Id);
            List<int> equiped = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                equiped.Add(Data.Data.GetEquipedItem(user.Id, i + 1));
                if (equiped[i] != 0)
                {
                    Item item = items.Where(x => x.Id == equiped[i]).FirstOrDefault();
                    if (item.QuestBapsFlat != 0)
                    {
                        bapsFlat += item.QuestBapsFlat;
                    }
                    if (item.QuestBapsPercent != 0)
                    {
                        bapsPercent += item.QuestBapsPercent;
                    }
                }
            }


            int index = weeklyIds.IndexOf(qIndex);

            if (index != 2)
                await Data.Data.SaveCompletedWeekly(user.Id, index);

            string Reward = $"{user.Mention} Quest completed, rewarded: {(int)((WeeklyQuests[qIndex].Baps + BapsFromPet + bapsFlat) * (bapsPercent / 200 + 1))} baps";


            if (pet != null)
            {
                Reward += $", of which {BapsFromPet} is because MayBich is a boss\n";
            }

            if (Data.Data.CompletedAllWeeklies(user.Id) && (index == 0 || index == 1))
            {
                await TutorialManager.AttemptSubmitStepCompleteAsync(user.Id, 3, 0, channel);

                Reward += "\nAfter finishing all weekly quests, you earn yourself a **boss ticket**";


                int petlvl = GetTotalPetLvl(user.Id);
                int rarity = 1;

                if (petlvl >= 240)
                    rarity = 5;
                else if (petlvl >= 180)
                    rarity = 4;
                else if (petlvl >= 120)
                    rarity = 3;
                else if (petlvl >= 60)
                    rarity = 2;


                Data.Data.GenerateItem(user.Id, rarity);

                string rarityString;
                switch (rarity)
                {
                    case 5:
                        rarityString = "Legendary";
                        break;
                    case 4:
                        rarityString = "Epic";
                        break;
                    case 3:
                        rarityString = "Rare";
                        break;
                    case 2:
                        rarityString = "Uncommon";
                        break;
                    default:
                        rarityString = "Common";
                        break;
                }

                Reward += $" as well as a {rarityString} item. Check your inventory with 'kush inv'";

                await Data.Data.SaveTicket(user.Id, true);
            }

            int raceGain = 0;

            if (weeklyIds[2] == qIndex)
            {
                raceFirst = true;
                Data.Data.RaceFinished();
                RaceFinisher = user.Id;
            }

            if (raceFirst)
            {
                raceGain = GetTotalPetLvl(user.Id) + 100;
                Reward += $"\nYOU Finished a Race quest and got {raceGain} extra baps!";
            }

            await channel.SendMessageAsync(Reward);

            await Data.Data.SaveBalance(user.Id, (int)((WeeklyQuests[qIndex].Baps + BapsFromPet + raceGain + bapsFlat) * (bapsPercent / 200 + 1)), false);
        }



        public static async Task CompleteQuest(int qIndex, List<int> QuestIndexes, IMessageChannel channel, IUser user)
        {
            await TutorialManager.AttemptSubmitStepCompleteAsync(user.Id, 2, 4, channel);
            Random rad = new Random();

            bool completedQs = true;

            int BapsFromPet;

            int abuseStrength = Data.Data.GetPetAbuseStrength(user.Id, 3);

            var pets = Data.Data.GetUserPets(user.Id);
            var pet = pets[PetType.Maybich];
            //Smth wrong here error occurs when adding 10th quest (reach 3.5k)
            if (pet != null)
            {
                double _BapsFromPet = (Math.Pow(pet.CombinedLevel, 1.3)
                    + pet.CombinedLevel * 3) + (Quests[qIndex].Baps / 100 * pet.CombinedLevel);

                for (int i = 0; i < abuseStrength; i++)
                {
                    _BapsFromPet *= 1.4;
                }

                BapsFromPet = (int)Math.Round(_BapsFromPet);

            }
            else
            {
                BapsFromPet = 0;
            }


            //items
            int bapsFlat = 0;
            double bapsPercent = 0;
            List<Item> items = Data.Data.GetUserItems(user.Id);
            List<int> equiped = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                equiped.Add(Data.Data.GetEquipedItem(user.Id, i + 1));
                if (equiped[i] != 0)
                {
                    Item item = items.Where(x => x.Id == equiped[i]).FirstOrDefault();
                    if (item.QuestBapsFlat != 0)
                    {
                        bapsFlat += item.QuestBapsFlat;
                    }
                    if (item.QuestBapsPercent != 0)
                    {
                        bapsPercent += item.QuestBapsPercent;
                    }
                }
            }

            //eoi

            string Reward = $"{user.Mention} Quest completed, rewarded: {(int)((Quests[qIndex].Baps + BapsFromPet + bapsFlat) * (bapsPercent / 100 + 1))} baps";
            if (pet != null)
            {
                Reward += $", of which {BapsFromPet} is because MayBich is a boss";
            }


            int delete = QuestIndexes.IndexOf(qIndex);
            QuestIndexes[delete] = -1;
            await Data.Data.SaveQuestIndexes(user.Id, string.Join(',', QuestIndexes));


            foreach (int quest in QuestIndexes)
            {
                if (quest != -1)
                {
                    completedQs = false;
                }
            }

            if (rad.Next(1, 101) <= 3)
            {
                //await channel.SendMessageAsync($"{user.Mention} Quest completed, rewarded: {Quests[qIndex].Baps} baps, the quest giver liked you and gave u a free egg! <:egg1:505082960081584148>");
                Reward += $", the quest giver liked you and gave u a free egg! <:pog:668851849675407371>";
                await Data.Data.SaveEgg(user.Id, true);
            }
            if (completedQs)
            {
                int extrabaps = (int)Math.Round((BapsFromPet - (Quests[qIndex].Baps / 100 * pet.CombinedLevel)) * 1.9);
                Reward += $"\n With that you've completed all of your quests and gained {RewardForFullQuests + extrabaps} Baps";

                Random rnd = new Random();
                int multiplier = Data.Data.GetTicketMultiplier(user.Id);

                if (pet != null)
                {
                    Reward += $", of which {(int)Math.Round((BapsFromPet - (Quests[qIndex].Baps / 100 * pet.CombinedLevel)) * 1.9)} is because of MayBich's charm";
                }

                if (rnd.NextDouble() < 0.2857 || Data.Data.GetTicketMultiplier(user.Id) >= 3)
                {
                    Reward += $"\nThe sack of baps contained a **boss ticket** {CustomEmojis.Pog}";
                    await Data.Data.ResetTicketMultiplier(user.Id);
                    await Data.Data.SaveTicket(user.Id, true);
                }
                else
                {
                    await Data.Data.IncrementTicketMultiplier(user.Id);
                }

                await Data.Data.SaveBalance(user.Id, RewardForFullQuests + extrabaps, false);
            }


            await channel.SendMessageAsync(Reward);

            await Data.Data.SaveBalance(user.Id, (int)((Quests[qIndex].Baps + BapsFromPet + bapsFlat) * (bapsPercent / 100 + 1)), false);

            if (Data.Data.GetBalance(user.Id) >= Quests[10].GetCompleteReq(user.Id) && QuestIndexes.Contains(10))
            {
                await CompleteQuest(10, QuestIndexes, channel, user);
            }

        }

        public static bool CompletedIconBlock(ulong userId, int chosen)
        {
            int BotId = GetBracket(chosen);
            int TopId = BotId + 9;

            List<int> picturesOwned = Data.Data.GetPictures(userId);

            for (int i = 0; i < 9; i++)
            {
                if (!picturesOwned.Contains(BotId + i + 1))
                {
                    return false;
                }
            }

            return true;

        }

        static int GetBracket(int chosen)
        {
            int num = (chosen - 1) / 9;
            return num * 9;
        }

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
}

