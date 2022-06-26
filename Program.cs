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

        public static bool BotTesting = true;

        static System.Timers.Timer questTimer;
        static System.Timers.Timer AirDropTimer;

        public static List<Pet> Pets = new List<Pet>();
        public static List<Boss> Bosses = new List<Boss>();
        public static List<BossNameImageRarityDesc> BossList = new List<BossNameImageRarityDesc>();
        public static BossObject BossObject;

        public static List<Quest> Quests = new List<Quest>();
        public static List<Quest> WeeklyQuests = new List<Quest>();
        public static ulong RaceFinisher = 0;

        public static List<CatchUp> CatchupMechanic;

        public static ulong Test;
        public static ulong PetTest;
        public static ulong Fail;
        public static ulong NerfUser;

        public static List<Package> GivePackages;
        public static List<ExistingDuel> Duels;

        public static List<ulong> IgnoredUsers = new List<ulong>();

        public static int RewardForFullQuests;

        public static int PictureCount = 90;

        public static List<CursedPlayer> CursedPlayers = new List<CursedPlayer>();

        public static ulong DumpChannelId = 641612898493399050;

        public static Airdrop airDrop;

        public static List<GardenAffectedSUser> GardenAffectedPlayers = new List<GardenAffectedSUser>();

        public static List<ulong> AllowedKushBotChannels = new List<ulong>();

        public static ulong BossChannelId = 946752140603453460;

        public static List<string> WeebPaths = new List<string>();
        public static List<string> CarPaths = new List<string>();
        public static List<string> ItemPaths = new List<string>();

        public static List<ulong> Engagements = new List<ulong>();

        public static DateTime LastWeebSend = DateTime.Now;

        public static List<ulong> TestingPhaseAllowedIds = new List<ulong>();

        public static int DailyGiveLimit = 3000;

        public static bool IsDisabled = false;

        public static int ItemCap = 15;

        //Goes up whenever a rarer boss spawns, down when worse spawns
        public static double BossNerfer = 0;


        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();


            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string botToken;
            if (BotTesting)
            {
                //botToken = "NDkzMTY4MTEzNjcyNzgxODI2.DohDFA.cFgHCeEvON7JaZKNnK3VZOFuj4g";
                botToken = "NDkzMTY4MTEzNjcyNzgxODI2.W6awmA.VkAzIkvuxUVIPFjJRYCDGsuiBao";
            }
            else
            {
                botToken = "NDkwODg4NjMwNTQyNTMyNjE5.Dn_3ag.qUOyw_Y5vFxBPCtZQVpr0g_ED80";
            }
            

            //event subscriptions
            _client.Log += Log;
            _client.ReactionAdded += OnReactionAdded;
            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, botToken);

            await _client.StartAsync();

            await _client.SetStatusAsync(UserStatus.Online);

            AddPets();
            InitializeBosses();
            AddQuests();
            AddWeeklyQuests();
            CatchupMechanic = new List<CatchUp>();

            Random rad = new Random();
            RewardForFullQuests = rad.Next(75, 200);

            await _client.SetGameAsync("rasyk kush help");

            AirDropTimer = new System.Timers.Timer(3 * 60 * 60 * 1000);
            AirDropTimer.Elapsed += DropAirdropEvent;
            AirDropTimer.AutoReset = true;
            AirDropTimer.Enabled = true;

            

            questTimer = new System.Timers.Timer(1000 * 60);
            questTimer.Elapsed += AssignQuestsEvent;
            questTimer.AutoReset = true;
            questTimer.Enabled = true;

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
            AllowedKushBotChannels.Add(660888274427969537);



            //bottest
            

            WeebPaths = Data.Data.ReadWeebShit();
            CarPaths = Data.Data.ReadCarShit();
            ItemPaths = Data.Data.ReadItems();
            

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

        public async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel chnl, SocketReaction reaction)
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
                }
            }

            if (BossObject != null && reaction.MessageId == BossObject.Message.Id)
            {
                var guild = _client.GetGuild(337945443252305920);
                string emoteName = "Booba";

                if (BotTesting)
                {
                    guild = _client.GetGuild(902541957149106256);
                }

                if (reaction.Emote.Name == emoteName)
                {
                    await BossObject.SignUp(reaction.UserId);

                }
                else if(reaction.Emote.Name == "❌")
                {
                    await BossObject.SignOff(reaction.UserId);
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
            
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
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

            if (BotTesting)
            {
                Prefix = "!";
            }
            else
            {
                Prefix = "kush ";
            }

            await DealWithAbilities(message);

            if (message.HasStringPrefix(Prefix, ref argPos) || message.HasStringPrefix("Kush ", ref argPos)
                || message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix("kush ", ref argPos))
            {

                await Data.Data.MakeRowForJew(message.Author.Id);
                await Data.Data.HandleAbuseChamber(message.Author.Id);

                if(!AllowedKushBotChannels.Contains(arg.Channel.Id) && !message.Content.Contains("yike") && !message.Content.Contains("nya") && !message.Content.Contains("redeem")
                    && !message.Content.Contains("moteris") && !message.Content.Contains("vroom"))
                //if(arg.Channel.Id != 660888274427969537 && arg.Channel.Id != 659831062792503296 && arg.Channel.Id != 666311949990100993 && arg.Channel.Id != 651464612901945375 && !BotTesting && !message.Content.Contains("redeem"))
                {
                    return;
                }

                string PlayerQUestsString = Data.Data.GetQuestIndexes(message.Author.Id);
                if (PlayerQUestsString.Contains(10.ToString()))
                {
                    if (Data.Data.GetBalance(message.Author.Id) > Quests[10].GetCompleteReq(message.Author.Id))
                    {
                        ulong channel;
                        if (BotTesting)
                        {
                            channel = 494199544582766610;
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
                            Console.WriteLine("FUcking error, someone probabbly bugged out");
                        }
                    }
                }

                if ((message.Content.ToLower().Contains("flip")
                    || message.Content.ToLower().Contains("bet")) && Program.GetTotalPetLvl(message.Author.Id) <= 28)
                {
                    Random rnd = new Random();

                    if (!CatchupMechanic.Any(x => x.userId == message.Author.Id))
                    {
                        CatchupMechanic.Add(new CatchUp(message.Author.Id));
                        Console.WriteLine($"added {CatchupMechanic.Where(x => x.userId == message.Author.Id).FirstOrDefault().remainingBuffs}");
                    }
                        
                    //Console.WriteLine("rolling rng");
                    if (rnd.NextDouble() < 0.08 
                        && CatchupMechanic.Where(x => x.userId == message.Author.Id).FirstOrDefault().remainingBuffs > 0)
                    {
                        CatchupMechanic.Where(x => x.userId == message.Author.Id).FirstOrDefault().remainingBuffs -= 1;
                        //Console.WriteLine("HIT");
                        Test = message.Author.Id;
                    }
                }

                //if (message.Channel.Id != 490934717408083990 || message.Channel.Id != 491605808254156802)
                //{
                //    return;
                //}
                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }

        private static string GetSpawnRarity()
        {
            Random rnd = new Random();

            double t = rnd.NextDouble();
            
            if(t <= 0.05 - BossNerfer / 9)
            {
                //BossNerfer += 0.05;
                return "Legendary";
            }
            else if(t <= 0.15 - BossNerfer / 6)
            {
                //BossNerfer += 0.03;
                return "Epic";
            }
            else if(t <= 0.3 - BossNerfer / 3)
            {
                //BossNerfer += 0.01;
                return "Rare";
            }
            else if(t <= 0.525 - BossNerfer)
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

        public static async Task SpawnBoss()
        {


            EmbedBuilder builder = new EmbedBuilder();
            string rarity = GetSpawnRarity();

            List<BossNameImageRarityDesc> appropriateBosses = new List<BossNameImageRarityDesc>();

            appropriateBosses = BossList.FindAll(x => x.Rarity == rarity);

            Random rnd = new Random();
            Boss Boss = new Boss(appropriateBosses[rnd.Next(0, appropriateBosses.Count)]);
            //Boss Boss = new Boss(BossList[bossIndex]);

            //bossIndex++;

            builder.WithTitle(Boss.Name);
            builder.WithColor(Boss.GetColor());
            builder.WithImageUrl(Boss.ImageUrl);
            builder.AddInlineField("Level:", $"**{Boss.Level}** 🎚️");
            builder.AddInlineField("Boss hp:", $"**{Boss.HP} ❤️**");
            builder.AddField("Rarity:", $"**{Boss.Rarity} 💠**\n{Boss.Desc}");

            builder.AddField($"Participants (0/{Boss.MaxParticipants}):", "---");
            builder.AddField("Results", $"The battle will start in 30 minutes");

            builder.WithFooter("Click on the Booba reaction to sign up by using a boss ticket");

            var emoteguild = _client.GetGuild(902541957149106256);
            //server guild
            var guild = _client.GetGuild(337945443252305920);

            if (BotTesting)
            {
                guild = _client.GetGuild(902541957149106256);
            }

            var chnl = guild.GetTextChannel(BossChannelId);

            var msg = await chnl.SendMessageAsync("", false, builder.Build());

            var emote = "<:Booba:944937036702441554>";

            GuildEmote ge = emoteguild.Emotes.FirstOrDefault(x => emote.Contains(x.Id.ToString()));

            await msg.AddReactionAsync(ge);
            await msg.AddReactionAsync(new Emoji("❌"));
            BossObject = new BossObject(Boss, msg);

            List<ulong> userIds = Data.Data.GetFollowingByRarity(Boss.Rarity);
            var users = userIds.Select(x => guild.GetUser(x));

            string txt = "WAKE UP ";
            foreach (var item in users)
            {
                if(guild.Users.Contains(item))
                    txt += $"{item.Mention} ";
            }

            await chnl.SendMessageAsync(txt);


            await BossObject.Combat();
        }

        public static async Task DropAirdrop()
        {

            //drops in every except last 2 entries of
            List<ulong> channelIds = AllowedKushBotChannels;
            Random rad = new Random();

            int channel = rad.Next(0, channelIds.Count - 2);

            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Airdrop");
            builder.WithColor(Color.Orange);
            builder.AddField("Loots remaining:",$"**{4}**");
            builder.WithFooter("Click on the ima reaction to collect the airdrop");
            builder.WithImageUrl("https://media.discordapp.net/attachments/337945443252305920/924089610697572402/ezgif.com-gif-maker_4.gif");

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

            if(cp == null)
            {
                return;
            }
            
            if(cp.CurseName == "asked")
            {
                if(cp.Duration > 0)
                {
                    await message.Channel.SendMessageAsync($"{message.Author.Mention} :warning: KLAUSEM :warning:");

                    if (!cp.lastMessages.Contains(message.Content) && message.Content.Length > 2)
                    {
                        cp.Duration -= 1;
                    }
                    
                    if(cp.Duration <= 0)
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

        static async void AssignQuestsEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (DateTime.Now.Hour == 22 && DateTime.Now.Minute == 0)
            {
                await AssignQuestsToPlayers();

                if(DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                    await AssignWeeklyQuests();
            }
            if((DateTime.Now.Hour == 8 && DateTime.Now.Minute == 0) ||
                (DateTime.Now.Hour == 10 && DateTime.Now.Minute == 0) ||
                (DateTime.Now.Hour == 12 && DateTime.Now.Minute == 0) || 
                (DateTime.Now.Hour == 14 && DateTime.Now.Minute == 0) ||
                (DateTime.Now.Hour == 16 && DateTime.Now.Minute == 0) ||
                (DateTime.Now.Hour == 18 && DateTime.Now.Minute == 0) ||
                (DateTime.Now.Hour == 18 && DateTime.Now.Minute == 0) ||
                (DateTime.Now.Hour == 20 && DateTime.Now.Minute == 0) ||
                (DateTime.Now.Hour == 22 && DateTime.Now.Minute == 0) ||
                (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0))
            {
                await SpawnBoss();
            }

        }

        public static int ParsePetIndex(string _PetIndex)
        {
            if (_PetIndex.Equals(Program.Pets[0].Name, StringComparison.CurrentCultureIgnoreCase)
                || _PetIndex == 0.ToString() || _PetIndex.Equals("superned", StringComparison.CurrentCultureIgnoreCase))
            {
                return 0;
            }
            else if (_PetIndex == 1.ToString() || _PetIndex.Equals("Pinata", StringComparison.CurrentCultureIgnoreCase)
                || _PetIndex.Equals("Baps Pinata", StringComparison.CurrentCultureIgnoreCase))
            {
                return 1;
            }
            else if (_PetIndex == 2.ToString() || _PetIndex.Equals(Program.Pets[2].Name, StringComparison.CurrentCultureIgnoreCase)
                || _PetIndex.Equals("goran", StringComparison.CurrentCultureIgnoreCase))
            {
                return 2;
            }
            else if (_PetIndex == 3.ToString() || _PetIndex.Equals(Program.Pets[3].Name, StringComparison.CurrentCultureIgnoreCase)
                || _PetIndex.Equals("gambeat", StringComparison.CurrentCultureIgnoreCase))
            {
                return 3;
            }
            else if (_PetIndex == 4.ToString() || _PetIndex.Equals(Program.Pets[4].Name, StringComparison.CurrentCultureIgnoreCase)
                || _PetIndex.Equals("jew", StringComparison.CurrentCultureIgnoreCase))
            {
                return 4;
            }
            else if (_PetIndex == 5.ToString() || _PetIndex.Equals(Program.Pets[5].Name, StringComparison.CurrentCultureIgnoreCase)
                || _PetIndex.Equals("tylerjuan", StringComparison.CurrentCultureIgnoreCase))
            {
                return 5;
            }
            else if (_PetIndex == 6.ToString() || _PetIndex.Equals(Program.Pets[6].Name, StringComparison.CurrentCultureIgnoreCase)
                || _PetIndex.Equals("Maybich", StringComparison.CurrentCultureIgnoreCase))
            {
                return 6;
            }
            else
            {
                return -1;
            }
        }

        public static string GetPetName(int index)
        {
            return Pets[index].Name;
        }

        public static async Task AssignWeeklyQuests()
        {
            Data.Data.SetWeeklyQuests();

            using (var DbContext = new SqliteDbContext())
            {
                List<SUser> jews = new List<SUser>();

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
                List<SUser> jews = new List<SUser>();

                //Console.WriteLine("OOOGAA" + DbContext.Jews.Count());
                
                foreach (var jew in DbContext.Jews)
                {
                    jews.Add(jew);
                }

                Data.Data.ResetDailyStuff(jews);

                for (int i = 0; i < CatchupMechanic.Count; i++)
                {
                    CatchupMechanic[i].remainingBuffs = 10;
                }
            }
        }

        public static async Task EndRage(ulong userId, int RageCash)
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

            

            var chnl = _client.GetChannel(id) as IMessageChannel;

            await chnl.SendMessageAsync($"<@{userId}> after calming down you count **{RageCash}** extra baps from all that raging");
        }

        public static void InitializeBosses()
        {
            #region Common
            string DrunkenGruntDesc = "People say he knows the streets better than he knows himself.Who knows, maybe that’s how things should be in an ideal world.Although by looking at him you could think he’s some lowly gangster, hooligan or alcoholic, by doing so you would fall right into his trap.You see, not many people have been in The Hall and lived to tell the story. This is, however, one of those people. Memories of trapped souls still haunt him to this day, making alcohol his only salvation. Nevertheless, he is extremely diligent in his work, never failing to execute an order. It’s rumored that these orders are coming from a very formidable galactic criminal organization.";
            BossList.Add(new BossNameImageRarityDesc("Drunken Grunt", "Common", DrunkenGruntDesc, "https://cdn.discordapp.com/attachments/902541958117990535/946396698828210217/Drunken_Grunt.jpg"));

            string HephaestapenkoDesc = "A devout follower of Hephus, this powerful human blacksmith above all seeks the revival of the Ancient God. Having read the ancient scriptures and found out about Tuwmod’s Core, he has made it his life mission to defeat the Machine and extract its core in hopes of bringing his Lord back to life. If he succeeds, the world will finally remember what fear feels like.";
            BossList.Add(new BossNameImageRarityDesc("Hephaestapenko", "Common", HephaestapenkoDesc, "https://cdn.discordapp.com/attachments/902541957694390293/946143296563077160/unknown.png"));

            string HallOfMemoriesDesc = "You find yourself in a damp, desolate hallway. You can’t remember how you got here and a slow sensation of panic is building up within your stomach. And yet, this place seems strangely familiar. You feel as if you’re being watched. No, you’re definitely being watched. The hallway seems to go on forever with no end in sight. Will you struggle? Will you try to escape? Whatever happens, do not stare at the walls. I repeat. DO NOT. STARE. AT. THE WALLS.";
            BossList.Add(new BossNameImageRarityDesc("Hall Of Forgotten Memories", "Common", HallOfMemoriesDesc, "https://cdn.discordapp.com/attachments/902541957694390293/946144140956172338/unknown.png"));

            string GiteonDesc = "Despite being recently appointed with the title of Magus, the founder of the budding Sabolian tradition of magic is, in fact, but a novice in the art of bending the cosmos to his unbreakable will.";
            BossList.Add(new BossNameImageRarityDesc("Giteon IV", "Common", GiteonDesc, "https://cdn.discordapp.com/attachments/902541957694390293/946143873204387840/unknown.png"));

            string UnderBossDesc = "Having been dealt an adverse hand - raised in the streets, degraded by poverty, - he is indifferent to morality. While not being at the top of the Umbral Syndicate's hierarchy, this timid looking gentleman and his henchmen are capable of carrying out operations ranging from intel gathering to mass murder. His expertise is vital to the Syndicate, as he bridges the gap between ideas and physical actions. Through him the Syndicate‘s higher-ups can act anonymously and retain a position of dominance.";
            BossList.Add(new BossNameImageRarityDesc("The Underboss", "Common", UnderBossDesc, "https://cdn.discordapp.com/attachments/902541957694390296/946490445863735356/The_Underboss.jpg"));

            #endregion

            #region Uncommon
            string ThePeaceMakerDesc = "Created by the brightest minds of the Aakhian Empire, this intergalactic weapon of mass destruction saw entire civilizations succumb before its destructive power. Armed not only with immense offensive armaments, but also with 27 Vauk boosters, it can traverse galaxies in mere days with ease. It also houses a crew of more than 1500 people – soldiers, workers and engineers. Who knows, maybe one day this machine could challenge the Forge God himself?";
            BossList.Add(new BossNameImageRarityDesc("The Peacemaker", "Uncommon", ThePeaceMakerDesc, "https://cdn.discordapp.com/attachments/902541958117990535/946397797488414770/The_Peacemaker.jpg"));

            string ProgenitorOfSinDesc = "Having seen and perhaps greatly influenced the destruction of both the cities of Sodom and Gomorrah, this literal embodiment of sin dedicated his life to nothing but chasing hedonistic pleasures. Being the right-hand man of The Harbinger, he oversees all the dirty work of the Order, whether it’s torturing, kidnapping, or transporting people to the Hall of Forgotten Memories. He also possesses the ability to hypnotize his victims into a state of shameless self-indulgence, making them seek nothing but pleasure and relinquish their last ounce of human decency.";
            BossList.Add(new BossNameImageRarityDesc("Progenitor of Sin","Uncommon", ProgenitorOfSinDesc, "https://cdn.discordapp.com/attachments/902541958117990535/946396820026830878/Progenitor_of_Sin.jpg"));

            string TorturedEffigyDesc = "A project of none other than Hephus, this copper wire contraption remains his most regrettable design. What was supposed to be a highly functional and sentient machine, turned out to be a deplorable abomination. Like some pathetic worm it wiggles and inches towards any nearby forms of life in a futile attempt of communication. Originally intended to heal and cure diseases, this abandoned amalgamation of copper discovered a new use for itself after being found by the Order of the Sanguine Reaper. It has since been used as a torture device, being perhaps the most efficient out of all the ones that the Order possesses.";
            BossList.Add(new BossNameImageRarityDesc("Tortured Effigy", "Uncommon", TorturedEffigyDesc, "https://cdn.discordapp.com/attachments/902541958117990535/946399786599321610/Tortured_Effigy.jpg"));

            string WitnessDesc = "Once an ordinary man, his hunger for money and power led him into making more enemies than allies. With a bounty placed on his head, he had the first-hand experience of what it feels like to be hunted by the Syndicate’s own Hit Squad. After he was captured, his eyes and tongue were torn out. His gut was then cut open in a ritualistic manner as he was left to die in a gigantic pit near all of the other victims’ disemboweled bodies. However, just before he could die, he was found and taken by a powerful blacksmith looking for something in the pile of cadavers. The Witness woke up with two implants in his eye sockets, allowing him to see again, and a few other Forgetech upgrades that made him more akin to a machine than human. However, he never got the chance to meet his savior, as the blacksmith proceeded on his own journey before The Witness could wake up. He now hunts members of the Syndicate in search of his stolen eyes.";
            BossList.Add(new BossNameImageRarityDesc("The Witness", "Uncommon", WitnessDesc, "https://cdn.discordapp.com/attachments/902541957694390293/946467534012575744/unknown.png"));
            #endregion

            #region rare
            string UmbralHitSquadDesc = "Nobody’s ever seen their faces. Nobody even knows whether they’re real or not." +
                " However, the mysterious disappearances of high-ranking galactic officials suggest the existence of a horrifying" +
                " force currently inhabiting our system and its ties to a mysterious criminal organization. One can only hope a bounty" +
                " has not yet been placed on their heads.";
            BossList.Add(new BossNameImageRarityDesc("Umbral Hit Squad", "Rare", UmbralHitSquadDesc, "https://cdn.discordapp.com/attachments/902541958117990535/946398623116197888/Umbral_Hit_Squad.jpg"));


            string JoaqiDesc = @"This demented freak of nature is said to tear the skin off of any unfortunate enough being to cross its path. When having laid eyes on its prey, this entity enters a state of frenzy, screeching and laughing hysterically, leaving the victim not only unrecognizable physically, but also immensely deranged. Joaqi was not born this way. His inability to accept his own mortality, however, made him seek power that no human should ever possess. And so the Harbinger gave him what he sought";

            BossList.Add(new BossNameImageRarityDesc("Joaqi, 3rd Earl of dukkha", "Rare", JoaqiDesc, "https://cdn.discordapp.com/attachments/902541958117990535/946399182435016704/Joaqi_3rd_Earl_of_Dukkha.jpg"));


            string FleshShifterDesc = "What is this? I’ve ███er seen this ███ before. It looks like it’s F̵͒̓L̸̒̏Ė̶̅S̴̏̔H̶̏͑  made of some kind of… m█tal? I’ve never seen B̵̃͝L̷̃͌O̵͂̋█D metal d██ls before. ███, what would this beK̸͋̊i̸͊́l̸̍̊l̷̊͒ỉ̷̚n̷̊̕g̷̎̀ doing here? Wait. Did you just F̵̋̓L̶͆̽██F̴͂͝L̶͋̉Ë̵́̉S̴̰̕H̸̾̀██E̷̾͝S̵͐̋H̷̛̚see that? Did ██ just move? Must be ██ lack of sleep. No, wait. It definitely K̸͊͊İ̵̓█L̶̇̌Ȇ̵̇D̵͐͠moved ███ now. What... a██ you? S██P. ██ Á̴̔Ẃ̶̓A̴͌̑Y̴͑̕ FROM M█. STO█O̴̾̔O̸͆̈́██O̴̊̒P̷̈͘\n...\n...\n..lovely…..flesh……";
            BossList.Add(new BossNameImageRarityDesc("Fleshshifter", "Rare", FleshShifterDesc, "https://cdn.discordapp.com/attachments/902541958117990535/946493172673052682/Fleshshifter.png"));
            #endregion

            #region Epic
            string HarbingerOfLightDesc = "*Thou shalt see the beginning, for to my killing\nThere appears to be no limit, neither a ceiling*\n\nThese are the chants of what had been the living in front of you.They were sung nearly a millennia ago, now. The only thing that ties the former entity to the present one, is their bard - like nature and wordplay, for what has become of this life form is inconceivable. Seeking divinity in the name of Apollyon, it had devoted its life to his serfdom. It slaved away for centuries, yielding to each and every single one of His demands. At last, the master was superseded, in what is now now known as the Arcanum Intervention.Though what had seemed as a positive shift in power, became the extinction of any virtue in the realm, for it now had a new master who was willing to grant power in exchange for submission.";
            BossList.Add(new BossNameImageRarityDesc("The Harbinger of Light", "Epic", HarbingerOfLightDesc, "https://cdn.discordapp.com/attachments/902541958117990535/946493079446249552/The_Harbinger_of_Light.jpg"));

            string KilnFatheerDesc = "An entity borne in a chance conjunction of the Prime Elements and the Primordial Chaos unknowable eons ago, spawned in what would be the birthplace of an unknown Elemental Plane. An adopted brother of the Forge God, the Kilnfather possesses a form of a shifting, continuous combustion. Due to circumstance known to few, and reasons understood by even less, a being of such power currently guards the sanctum of an ancient order of Magi.";
            BossList.Add(new BossNameImageRarityDesc("The Kilnfather", "Epic", KilnFatheerDesc, "https://cdn.discordapp.com/attachments/902541957694390293/946143982621179944/unknown.png"));

            string TuwmodDesc = "Tuwmod, also known as The Ultimate War Machine Of Death, was born in fire. Having been completely handcrafted and forged by Hephus himself long before the Age of Man, Tuwmod possesses a powerful core protected by an even more powerful metallic armor. Multiple planet-sized weapons of mass destruction makes it feared by entire galactic civilizations. Tuwmod, however, seems disinterested in using its powers for malevolent or selfish reasons. Only one thing ever crosses its dull metallic mind - the longing of a long-lost brother.";
            BossList.Add(new BossNameImageRarityDesc("Tuwmod, the Forge God", "Epic", TuwmodDesc, "https://cdn.discordapp.com/attachments/902541958117990535/946493540328947823/Tuwmod_the_Forge_God.jpg"));


            #endregion

            #region Legendary
            string wtf = "Shattered minds, ruined civilizations, entire swathes of galaxies left in nothing but a mass of untraversable corrupted reality never to recover, are but remnants of the true destruction left in the wake of the Ravagers. Only the fervently fanatical, the destitute and the clinically insane would ever willing come into contact with the residents of the Realm of the Everwatcher, less colloquially known as ᛊᚨᛞᚠᚨᛊᛞᚠᛁᛃᚾᛞ. What little knowledge can be retained of these entities suggests the presence of a caste system, and a hierarchy of power. The Ravager in question is one of the “generals” of the Everwatcher itself, but I do not dare mention it explicitly, for creatures of such power can sense the presence of even extended thought regarding them and cast their presence upon the trespass- N̵͆̈́Ó̸̍ ̸̒̒Š̴͒À̷͌L̴̰̎V̸͂͝A̷̛͐T̴͗̑I̶͒̄O̸̔͝N̷̛̓";
            BossList.Add(new BossNameImageRarityDesc("ᚾᚨᚷᚨᛚᛟᚱᛞᚨᛊᛋ", "Legendary", wtf, "https://media.discordapp.net/attachments/492815134268456981/775489395217661972/realybe.jpg?width=270&height=270"));

            string theQuestionmarkDesc = "The Commissioner, The Contractor, The Mastermind. The Umbral Syndicate’s reputation as the most menacing yet secretive organization in the galaxy has only grown stronger during the past centuries. The biggest mystery, however, lies at the very top of this organization - countless reports from captured high-profile criminals, who allegedly interacted with the mysterious leader of the Syndicate, described him as an exceptionally tall and handsome middle-aged Caucasian man. The oldest such report was 422 years ago, the earliest – 73 days ago. This seemingly unaging man’s power and reach are growing each and every day, so much that even The Imperial Council is starting to look powerless in his shadow.";
            BossList.Add(new BossNameImageRarityDesc("The ???", "Legendary", theQuestionmarkDesc, "https://cdn.discordapp.com/attachments/902541958117990535/946494147144056842/The_KlaustukasKlaustukasKlaustukas.jpg"));

            #endregion
        }

        public static void AddPets()
        {
            Pets.Add(new Pet("SuperNed", 1));
            Pets.Add(new Pet("Baps Pinata", 1));
            Pets.Add(new Pet("Goran Jelić", 1));
            Pets.Add(new Pet("Maybich", 1));
            Pets.Add(new Pet("Jew", 1));
            Pets.Add(new Pet("TylerJuan", 1));
        }

        static int GetQuestRequirement(ulong id, int bapsReq)
        {
            int petLvls = GetTotalPetLvl(id);

            int ret = (int)((3.5 * Math.Pow(petLvls, 1.075)) * (double)(1500/(double)bapsReq));
            
            return ret + bapsReq;
        }

        static void AddQuests()
        {
            Quests.Add(new Quest(Quests.Count, 1500, $"**Win 1500 baps** from **gambling**", true, 170));
            Quests.Add(new Quest(Quests.Count, 1500, "**Lose 1500 baps** from **gambling**", true, 180));
            Quests.Add(new Quest(Quests.Count, 700, "**Win 700 baps** from **flipping**", true, 130));
            Quests.Add(new Quest(Quests.Count, 700, "**Lose 700 baps** from **flipping**", true, 140));
            Quests.Add(new Quest(Quests.Count, 700, "**Win 700 baps** from **Betting**", true, 130));
            Quests.Add(new Quest(Quests.Count, 700, "**Lose 700 baps** from **Betting**", true, 140));
            Quests.Add(new Quest(Quests.Count, 700, "**Win 700 baps** from **Risking**", true, 130));
            Quests.Add(new Quest(Quests.Count, 700, "**Lose 700 baps** from **Risking**", true, 140));
            Quests.Add(new Quest(Quests.Count, 32, "**Get 32 or more baps** as a base roll on **begging**", false, 100));
            Quests.Add(new Quest(Quests.Count, 0, "**?Nekenciu.**", false, 70));
            Quests.Add(new Quest(Quests.Count, 2000, "**Reach** 2000 baps ", true, 325));
            Quests.Add(new Quest(Quests.Count, 5, "**Beg** 5 times", true, 135));
            Quests.Add(new Quest(Quests.Count, 7, "**Beg** 7 times", true, 200));
            //13
            Quests.Add(new Quest(Quests.Count, 1, "**Feed** any pet once", true, 100));
            Quests.Add(new Quest(Quests.Count, 750, "**Flip 750 or more baps** in one flip", true, 240));
            Quests.Add(new Quest(Quests.Count, 3, "**Yoink** Succesfully 3 times", true, 135));
            Quests.Add(new Quest(Quests.Count, 1, "**Fail to Yoink** a target", true, 85));
            Quests.Add(new Quest(Quests.Count, 3, "**Flip 60 or more baps** and win 3 times in a row ", true, 200));
            Quests.Add(new Quest(Quests.Count, 3, "**Get** a **bet** modifier that's more than **3**", true, 200));
            Quests.Add(new Quest(Quests.Count, 20, "**Win** a **Risk** of 20 or more baps with a min modifier of **8**", true, 140));
            Quests.Add(new Quest(Quests.Count, 850, "**Bet 850 or more baps** in one bet", true, 250));
            Quests.Add(new Quest(Quests.Count, 400, $"**risk 400 or more baps** in one risk", true, 220));
            Quests.Add(new Quest(Quests.Count, 1500, "**Win 1500 baps** from **flipping**", true, 275));
            Quests.Add(new Quest(Quests.Count, 1500, "**Win 1500 baps** from **Betting**", true, 275));
            Quests.Add(new Quest(Quests.Count, 1500, "**Win 1500 baps** from **Risking**", true, 275));
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


            if(Data.Data.GetCompletedWeekly(user.Id, 0) == 1 && weeklyIds[0] == qIndex)
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

            if (Data.Data.GetPets(user.Id).Contains("3"))
            {
                double _BapsFromPet = (Math.Pow(Data.Data.GetPetLevel(user.Id, 3), 1.3) + Data.Data.GetPetLevel(user.Id, 3) * 3) + (WeeklyQuests[qIndex].Baps / 100 * Data.Data.GetPetLevel(user.Id, 3));
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

            if(index != 2)
                await Data.Data.SaveCompletedWeekly(user.Id, index);

            string Reward = $"{user.Mention} Quest completed, rewarded: {(int)((WeeklyQuests[qIndex].Baps + BapsFromPet + bapsFlat) * (bapsPercent / 200 + 1))} baps";

            
            if (Data.Data.GetPets(user.Id).Contains("3"))
            {
                Reward += $", of which {BapsFromPet} is because MayBich is a boss\n";
            }

            if (Data.Data.CompletedAllWeeklies(user.Id) && (index == 0 || index == 1))
            {
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

            Random rad = new Random();

            bool completedQs = true;

            int BapsFromPet;

            int abuseStrength = Data.Data.GetPetAbuseStrength(user.Id, 3);

            //Smth wrong here error occurs when adding 10th quest (reach 3.5k)
            if (Data.Data.GetPets(user.Id).Contains("3"))
            {
                double _BapsFromPet = (Math.Pow(Data.Data.GetPetLevel(user.Id, 3), 1.3)
                    + Data.Data.GetPetLevel(user.Id, 3) * 3) + (Quests[qIndex].Baps / 100 * Data.Data.GetPetLevel(user.Id, 3));

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

            string Reward = $"{user.Mention} Quest completed, rewarded: {(int)((Quests[qIndex].Baps + BapsFromPet + bapsFlat) * (bapsPercent/100 + 1))} baps";
            if (Data.Data.GetPets(user.Id).Contains("3"))
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
                int extrabaps = (int)Math.Round((BapsFromPet - (Quests[qIndex].Baps / 100 * Data.Data.GetPetLevel(user.Id, 3))) * 1.9);
                Reward += $"\n With that you've completed all of your quests and gained {RewardForFullQuests + extrabaps} Baps";

                Random rnd = new Random();
                int multiplier = Data.Data.GetTicketMultiplier(user.Id);

                if(rnd.NextDouble() < 0.2857 || Data.Data.GetTicketMultiplier(user.Id) >= 3)
                {
                    Reward += $"\nThe sack of baps contained a **boss ticket** <:pog:668851849675407371>";
                    await Data.Data.ResetTicketMultiplier(user.Id);
                    await Data.Data.SaveTicket(user.Id, true);
                }
                else
                {
                    await Data.Data.IncrementTicketMultiplier(user.Id);
                    Console.WriteLine(Data.Data.GetTicketMultiplier(user.Id));
                }
                    

                if (Data.Data.GetPets(user.Id).Contains("3"))
                {
                    Reward += $", of which {(int)Math.Round((BapsFromPet - (Quests[qIndex].Baps / 100 * Data.Data.GetPetLevel(user.Id, 3))) * 1.9)} is because of MayBich's charm";
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
                if (picturesOwned.Contains(BotId + i + 1))
                {

                }
                else
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
            int temp = 0;
            int c = 0;
            for (int i = 0; i < Pets.Count; i++)
            {
                int lvl = Data.Data.GetPetLevel(id, i) - Data.Data.GetItemPetLevel(id, i);
                if (lvl > 0)
                {
                    c++;
                    temp += lvl + Data.Data.GetPetTier(id, i);
                }
            }
            return temp / c;
        }

        public static int GetTotalPetLvl(ulong id)
        {
            int temp = 0;
            string pets = Data.Data.GetPets(id);

            for (int i = 0; i < Pets.Count; i++)
            {
                
                if (pets.Contains(i.ToString()))
                {
                    int lvl = Data.Data.GetPetLevel(id, i);
                    temp += lvl;
                }
            }
            return temp;
        }
    }
}

