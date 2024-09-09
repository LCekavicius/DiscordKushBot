using Discord;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using KushBot.Data;
using System.Threading.Tasks;
using KushBot.DataClasses;
using KushBot.Modules;
using System.Xml.Schema;

namespace KushBot
{
    public class BossObject
    {
        public enum NoDmgReason
        {
            None,
            Miss,
            Paralyze
        }

        public class Log
        {

            public virtual string GetActor()
            {
                throw new NotImplementedException();
            }

            public virtual string GetAction()
            {
                throw new NotImplementedException();
            }
        }

        public class UserLog : Log
        {
            public ulong UserId { get; set; }
            public int DamageDealt { get; set; }
            public NoDmgReason NoDamageReason { get; set; }

            public UserLog(ulong userId, int damageDealt, NoDmgReason noDamageReason = NoDmgReason.None)
            {
                UserId = userId;
                DamageDealt = damageDealt;
                NoDamageReason = noDamageReason;
            }

            public override string GetActor()
            {
                return Program._client.GetUser(UserId).Username;
            }

            public override string GetAction()
            {
                if (DamageDealt > 0)
                    return this.ToString();

                if (NoDamageReason == NoDmgReason.Miss)
                {
                    return $"{GetActor()} **misses**\n";
                }

                if (NoDamageReason == NoDmgReason.Paralyze)
                {
                    return $"{GetActor()} Is paralyzed\n";
                }

                return "????XD";
            }

            public override string ToString()
            {
                return $"{GetActor()} dealt {DamageDealt} dmg\n";
            }

            public string GetActorlessString()
            {
                return $"Dmg dealt: {DamageDealt}\n";
            }
        }

        public class ArchonLog : Log
        {
            public string UsedAbility { get; set; }
            public string AbilityResult { get; set; }
            public ulong? AffectedParticipantId { get; set; }

            public ArchonLog(string usedAbility, string abilityResult)
            {
                UsedAbility = usedAbility;
                AbilityResult = abilityResult;
            }

            public override string GetActor()
            {
                return "The archon";
            }

            public override string GetAction()
            {
                return $"{AbilityResult}";
            }


            public override string ToString()
            {
                return $"{GetActor()}\n";
            }
        }

        public Boss Boss { get; set; }
        public List<ulong> Participants { get; set; }
        public RestUserMessage Message { get; set; }
        public DateTime StartDate { get; set; }
        public ulong? SummonerId { get; set; }


        public BossObject(Boss boss, RestUserMessage message, DateTime startDate)
        {
            this.Boss = boss;
            Participants = new List<ulong>();

            this.Message = message;
            StartDate = startDate;
        }

        public async Task SignOff(ulong userId)
        {
            if (!Participants.Contains(userId))
            {
                return;
            }
            Participants.Remove(userId);
            await Data.Data.SaveTicket(userId, true);

            EmbedBuilder builder = UpdateBuilder(Boss.IsArchon);

            await Message.ModifyAsync(x =>
            {
                x.Embed = builder.Build();
            });
        }

        public async Task SignUp(ulong userId)
        {
            if (Participants.Count >= Boss.MaxParticipants)
            {
                return;
            }
            if (Participants.Contains(userId))
            {
                return;
            }
            if (Data.Data.GetPets(userId).Length < 2)
            {
                return;
            }
            if (Data.Data.GetTicketCount(userId) <= 0)
            {
                return;
            }

            Participants.Add(userId);

            await Data.Data.SaveTicket(userId, false);

            EmbedBuilder builder = UpdateBuilder(Boss.IsArchon);

            await Message.ModifyAsync(x =>
            {
                x.Embed = builder.Build();
            });

        }

        public int ItemBossDam(ulong ownedId)
        {
            int BossDam = 0;
            List<Item> items = Data.Data.GetUserItems(ownedId);
            List<int> equiped = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                equiped.Add(Data.Data.GetEquipedItem(ownedId, i + 1));
                if (equiped[i] != 0)
                {
                    Item temp = items.Where(x => x.Id == equiped[i]).FirstOrDefault();
                    if (temp.BossDmg != 0)
                    {
                        BossDam += temp.BossDmg;
                    }

                }
            }

            return BossDam;
        }

        public async Task Combat(bool isArchonHandler)
        {
            //await Task.Delay(10 * 1000);
            //if (Program.BotTesting)
            //    await Task.Delay(20 * 1000);
            //else
            //    await Task.Delay((isArchonHandler ? 10 : 30) * 60 * 1000);

            if (isArchonHandler)
                Program.ArchonObject = null;
            else
                Program.BossObject = null;

            int TempHp = Boss.HP;


            List<UserLog> logs = new List<UserLog>();
            List<Log> fightLogs = new List<Log>();

            if (!isArchonHandler)
            {
                TempHp = await HandleRegularCombat(TempHp, logs);
            }
            else
            {
                if (!Participants.Any())
                {
                    EmbedBuilder builder = new();
                    builder.AddField("Logs", "No1 showed up lol");
                    builder.WithColor(Color.Red);

                    await Message.Channel.SendMessageAsync("", false, builder.Build());
                    return;
                }
                TempHp = await HandleArchonCombat(TempHp, logs, fightLogs);
            }

            bool IsVictory = false;

            if (TempHp <= 0)
            {
                IsVictory = true;
            }

            if (IsVictory)
            {
                foreach (var item in Participants)
                {
                    await TutorialManager.AttemptSubmitStepCompleteAsync(item, 5, 1, Message.Channel);
                }
            }

            if (isArchonHandler)
            {
                await SendProgressMessageAsync(fightLogs, IsVictory, TempHp);
            }
            await SendResultMessage(logs, IsVictory, TempHp);
        }

        private async Task<int> HandleArchonCombat(int tempHp, List<UserLog> resultLogs, List<Log> progressLogs)
        {
            Dictionary<ulong, int> userItemDmg = new();
            Dictionary<ulong, int> userDmgDealt = new();
            Dictionary<ulong, int> userBuffDmg = new();
            foreach (var participant in Participants)
            {
                userDmgDealt.Add(participant, 0);
                userItemDmg.Add(participant, ItemBossDam(participant));
                ConsumableBuff artillery = Data.Data.GetConsumableBuff(participant, BuffType.BossArtillery);
                if (artillery != null)
                    await Data.Data.ReduceOrRemoveBuffAsync(participant, BuffType.BossArtillery);

                userBuffDmg.Add(participant, (artillery != null ? (int)artillery.Potency : 0));
            }
            HashSet<ulong> ParalyzedUsers = new();

            Random rnd = new Random();
            ArchonAbility lastArchonAbilityUsed = null;
            HashSet<ulong> paralyzedUsers = new();

            bool isDismantleActive = false;
            bool isDemoralizeActive = false;
            //For each round
            for (int i = 0; i < 3; i++)
            {
                foreach (var userId in Participants)
                {
                    if (paralyzedUsers.Contains(userId))
                    {
                        progressLogs.Add(new UserLog(userId, 0, NoDmgReason.Paralyze));
                        continue;
                    }

                    if (lastArchonAbilityUsed != null && lastArchonAbilityUsed.Name == "Dodge")
                    {
                        if (rnd.NextDouble() < (lastArchonAbilityUsed.Effect / 100))
                        {
                            progressLogs.Add(new UserLog(userId, 0, NoDmgReason.Miss));
                            continue;
                        }
                    }

                    int attack = CalculateDamage(userId, isDismantleActive, isDemoralizeActive) + (isDismantleActive ? 0 : ((userItemDmg[userId] + userBuffDmg[userId]) / 3));

                    if (lastArchonAbilityUsed != null && lastArchonAbilityUsed.Name == "Toughen hide")
                    {
                        attack = (int)(attack * (1 - lastArchonAbilityUsed.Effect / 100));
                    }

                    tempHp -= attack;
                    userDmgDealt[userId] += attack;

                    progressLogs.Add(new UserLog(userId, attack));

                }
                lastArchonAbilityUsed = GetUsedAbility();

                if (lastArchonAbilityUsed.Name == "Paralyze" || lastArchonAbilityUsed.Name == "Dismantle")
                    Boss.ArchonAbilities.FirstOrDefault(e => e.Name == lastArchonAbilityUsed.Name).Prevent = true;

                if (lastArchonAbilityUsed.Name == "Dismantle")
                    isDismantleActive = true;

                if (lastArchonAbilityUsed.Name == "Demoralize")
                {
                    isDemoralizeActive = true;
                }
                else
                    isDemoralizeActive = false;

                if (i != 2)
                {
                    ArchonLog log = HandleArchonAbility(ref tempHp, progressLogs, lastArchonAbilityUsed, paralyzedUsers);
                    progressLogs.Add(log);
                }
            }

            foreach (var kvp in userDmgDealt)
            {
                var log = new UserLog(kvp.Key, kvp.Value);
                resultLogs.Add(log);
            }

            return tempHp;
        }

        private ArchonLog HandleArchonAbility(ref int tempHp, List<Log> logs, ArchonAbility ability, HashSet<ulong> paralyzedUsers)
        {
            Random rnd = new();
            if (ability.Name == "Regeneration")
            {
                int healedAmount = (int)((Boss.HP - tempHp) * ((ability.Effect / 100)));
                tempHp += healedAmount;
                return new("Regeneration", $"Archon uses **regeneration** and heals himself for {healedAmount} hp!");
            }
            if (ability.Name == "Dodge")
            {
                return new("Dodge", "Archon uses **dodge** and obtains evasive maneuvers");
            }
            if (ability.Name == "Toughen hide")
            {
                return new("Toughen hide", "Archon uses **Toughen hide** and starts absorbing damage");
            }
            if (ability.Name == "Paralyze")
            {
                ulong paralyzedUser = Participants[rnd.Next(0, Participants.Count)];
                paralyzedUsers.Add(paralyzedUser);
                return new("Paralyze", $"Archon uses **Paralyze** on {Program._client.GetUser(paralyzedUser).Username}");
            }
            if (ability.Name == "Dismantle")
            {
                return new("Dismantle", "Archon uses **Dismantle** and removes your tier and item based damage");
            }

            return new("Demoralize", "Archon uses **Demoralize** making you attack with your weakest pets.");
        }

        private ArchonAbility GetUsedAbility()
        {
            Random rnd = new();
            return Boss.ArchonAbilities.Where(e => !e.Prevent).OrderBy(e => rnd.Next()).FirstOrDefault();
        }

        private async Task<int> HandleRegularCombat(int tempHp, List<UserLog> logs)
        {
            foreach (var item in Participants)
            {
                int BossDam = ItemBossDam(item);
                int att1 = CalculateDamage(item);
                int att2 = CalculateDamage(item);
                ConsumableBuff artillery = Data.Data.GetConsumableBuff(item, BuffType.BossArtillery);
                if (artillery != null)
                    await Data.Data.ReduceOrRemoveBuffAsync(item, BuffType.BossArtillery);

                UserLog log = new UserLog(item, att1 + att2 + BossDam + (artillery != null ? (int)artillery.Potency : 0));

                logs.Add(log);
                tempHp -= att1;
                tempHp -= att2;
                tempHp -= BossDam;
                tempHp -= (artillery != null ? (int)artillery.Potency : 0);
            }

            return tempHp;
        }

        public int RarityStringToInt(string rarity)
        {
            if (rarity == "Common")
            {
                return 1;
            }
            else if (rarity == "Uncommon")
            {
                return 2;
            }
            else if (rarity == "Rare")
            {
                return 3;
            }
            else if (rarity == "Epic")
            {
                return 4;
            }
            else if (rarity == "Legendary")
            {
                return 5;
            }


            return 1;
        }

        public async Task GiveOutRewards(List<string> RewardStrings)
        {
            Random rnd = new Random();
            int BapsRewardFull = Boss.GetBapsReward(Participants.Count);
            int BapsRewardEa = BapsRewardFull / Participants.Count;

            //Baps
            foreach (var item in Participants)
            {
                RewardStrings.Add($"{BapsRewardEa} baps");
                await Data.Data.SaveBalance(item, BapsRewardEa, false);
            }

            //items
            int rarity = RarityStringToInt(Boss.Rarity);

            int itemsToGive = 1;

            if (Participants.Count > 1)
            {
                itemsToGive = Participants.Count / 2;

                if (Participants.Count % 2 != 0)
                {


                    if (rnd.NextDouble() > 0.5)
                        itemsToGive++;
                }
            }

            //Archon item
            HashSet<ulong> archonReceiver = new();
            if (Boss.IsArchon && SummonerId.HasValue)
            {
                bool summonerGetsArchonItem = rnd.NextDouble() > 0.5;
                List<ulong> eligibleForArchonItem = Participants;
                if (summonerGetsArchonItem && Participants.Contains(SummonerId.Value))
                {
                    archonReceiver.Add(SummonerId.Value);
                    eligibleForArchonItem = eligibleForArchonItem.Except(new List<ulong>() { SummonerId.Value }).ToList();
                }

                archonReceiver.Add(eligibleForArchonItem[rnd.Next(0, eligibleForArchonItem.Count)]);
            }

            List<ulong> receivers = Participants.OrderBy(x => rnd.Next()).Take(itemsToGive).ToList();

            for (int i = 0; i < Participants.Count; i++)
            {
                if (receivers.Contains(Participants[i]))
                {
                    List<Item> inv = Data.Data.GetUserItems(Participants[i]);
                    if (inv.Count >= Program.ItemCap)
                    {
                        RewardStrings[i] += $", Full inv moment <:tf:946039048789688390>";
                    }
                    else
                    {
                        RewardStrings[i] += $", a {Boss.Rarity} item";
                        Data.Data.GenerateItem(Participants[i], rarity);
                    }

                }

                if (archonReceiver.Contains(Participants[i]))
                {
                    RewardStrings[i] += $", an <:p7:1224001356650774578> Archon <:p7:1224001356650774578> item";
                    Data.Data.GenerateItem(Participants[i], 6);
                }
            }

            //Eggs
            if (Boss.Rarity == "Uncommon")
            {
                for (int i = 0; i < Participants.Count; i++)
                {
                    if (!Data.Data.GetEgg(Participants[i]) && rnd.NextDouble() <= 0.3)
                    {
                        RewardStrings[i] += ", an egg";
                        await Data.Data.SaveEgg(Participants[i], true);
                    }
                }
            }
            else if (Boss.Rarity == "Rare")
            {

                for (int i = 0; i < Participants.Count; i++)
                {
                    RewardStrings[i] += ", an egg";
                    await Data.Data.SaveEgg(Participants[i], true);

                    int petIndex = rnd.Next(0, 2);
                    int c = 1;
                    await HandlePetDupes(Participants[i], petIndex);

                    if (rnd.NextDouble() < 0.45)
                    {
                        int petIndex2 = rnd.Next(0, 2);
                        await HandlePetDupes(Participants[i], petIndex2);
                        if (petIndex == petIndex2)
                        {
                            c++;
                        }
                        else
                        {
                            RewardStrings[i] += $", 1x {Program.Pets[petIndex2].Name}";
                        }

                    }
                    RewardStrings[i] += $", {c}x {Program.Pets[petIndex].Name}\n";
                }
            }
            else if (Boss.Rarity == "Epic")
            {

                for (int i = 0; i < Participants.Count; i++)
                {
                    int petIndex = rnd.Next(2, 4);
                    int c = 1;
                    await HandlePetDupes(Participants[i], petIndex);

                    if (rnd.NextDouble() < 0.4)
                    {
                        int petIndex2 = rnd.Next(2, 4);
                        await HandlePetDupes(Participants[i], petIndex2);
                        if (petIndex == petIndex2)
                        {
                            c++;
                        }
                        else
                        {
                            RewardStrings[i] += $", 1x {Program.Pets[petIndex2].Name}";
                        }

                    }
                    RewardStrings[i] += $", {c}x {Program.Pets[petIndex].Name}";

                    int petFeedPetIndex = Data.Data.GetRandomPetId(Participants[i]);
                    if (petFeedPetIndex != -1)
                    {
                        RewardStrings[i] += $", Food for {Program.Pets[petFeedPetIndex].Name}";
                        await Data.Data.SavePetLevels(Participants[i], petFeedPetIndex, Data.Data.GetPetLevel(Participants[i], petFeedPetIndex)
                            - Data.Data.GetItemPetLevel(Participants[i], petFeedPetIndex) + 1, false);
                    }

                    RewardStrings[i] += "\n";

                }
            }
            else if (Boss.Rarity == "Legendary")
            {
                for (int i = 0; i < Participants.Count; i++)
                {
                    int petIndex = rnd.Next(4, 6);
                    int c = 1;
                    await HandlePetDupes(Participants[i], petIndex);

                    if (rnd.NextDouble() < 0.4)
                    {
                        int petIndex2 = rnd.Next(4, 6);
                        await HandlePetDupes(Participants[i], petIndex2);
                        if (petIndex == petIndex2)
                        {
                            c++;
                        }
                        else
                        {
                            RewardStrings[i] += $", 1x {Program.Pets[petIndex2].Name}";
                        }
                    }

                    RewardStrings[i] += $", {c}x {Program.Pets[petIndex].Name}";

                    for (int feedCount = 0; feedCount < 3; feedCount++)
                    {
                        if (feedCount == 2 && rnd.NextDouble() < 0.5)
                            break;

                        int petFeedPetIndex = Data.Data.GetRandomPetId(Participants[i]);
                        if (petFeedPetIndex != -1)
                        {
                            RewardStrings[i] += $", Food for {Program.Pets[petFeedPetIndex].Name}";
                            await Data.Data.SavePetLevels(Participants[i], petFeedPetIndex, Data.Data.GetPetLevel(Participants[i], petFeedPetIndex)
                                - Data.Data.GetItemPetLevel(Participants[i], petFeedPetIndex) + 1, false);
                        }
                    }

                    //int petFeedPetIndex = Data.Data.GetRandomPetId(Participants[i]);
                    //if (petFeedPetIndex != -1)
                    //{
                    //    RewardStrings[i] += $", Food for {Program.Pets[petFeedPetIndex].Name}";
                    //    await Data.Data.SavePetLevels(Participants[i], petFeedPetIndex, Data.Data.GetPetLevel(Participants[i], petFeedPetIndex)
                    //        - Data.Data.GetItemPetLevel(Participants[i], petFeedPetIndex) + 1, false);
                    //}

                    //int petFeedPetIndex2 = Data.Data.GetRandomPetId(Participants[i]);
                    //if (petFeedPetIndex2 != -1)
                    //{
                    //    RewardStrings[i] += $", Food for {Program.Pets[petFeedPetIndex2].Name}";
                    //    await Data.Data.SavePetLevels(Participants[i], petFeedPetIndex2, Data.Data.GetPetLevel(Participants[i], petFeedPetIndex2)
                    //        - Data.Data.GetItemPetLevel(Participants[i], petFeedPetIndex2) + 1, false);
                    //}

                    //if (rnd.NextDouble() < 0.5)
                    //{
                    //    int petFeedPetIndex3 = Data.Data.GetRandomPetId(Participants[i]);
                    //    if (petFeedPetIndex3 != -1)
                    //    {
                    //        RewardStrings[i] += $", Food for {Program.Pets[petFeedPetIndex3].Name}";
                    //        await Data.Data.SavePetLevels(Participants[i], petFeedPetIndex3, Data.Data.GetPetLevel(Participants[i], petFeedPetIndex3)
                    //            - Data.Data.GetItemPetLevel(Participants[i], petFeedPetIndex3) + 1, false);
                    //    }
                    //}

                    RewardStrings[i] += "\n";

                }
            }


        }

        public async Task HandlePetDupes(ulong id, int petId)
        {
            if (!Data.Data.GetPets(id).Contains(petId.ToString()))
            {
                await Data.Data.SavePets(id, petId);
                await Data.Data.SavePetLevels(id, petId, 1, true);
            }
            else
            {
                await Data.Data.SavePetDupes(id, petId, Data.Data.GetPetDupe(id, petId) + 1);
            }
        }

        public async Task SendProgressMessageAsync(List<Log> logs, bool isVictory, int remainderHp)
        {
            string kushActions = "";

            await Message.Channel.SendMessageAsync($"{string.Join(" ", Participants.Select(e => Program._client.GetUser(e).Mention))}");
            await Task.Delay(2_000);

            EmbedBuilder builder = new EmbedBuilder();

            for (int i = 0; i < logs.Count; i++)
            {
                if (logs[i] is UserLog)
                {
                    kushActions += $"{logs[i].GetAction()}";
                    if (i != logs.Count - 1)
                        continue;
                }

                builder = new EmbedBuilder();
                builder.WithTitle($"{Boss.Name} Fight progress");
                builder.WithColor(Color.Blue);

                builder.AddField("Kush actions", kushActions);
                await Message.Channel.SendMessageAsync(embed: builder.Build());

                builder = new EmbedBuilder();
                builder.WithTitle($"{Boss.Name} Fight progress");
                builder.WithColor(Color.Blue);

                await Task.Delay(4_000);

                kushActions = "";
                if (i != logs.Count - 1)
                {
                    builder.AddField("Archon action", logs[i].GetAction());
                    await Message.Channel.SendMessageAsync(embed: builder.Build());
                    await Task.Delay(4_000);
                }
            }
        }

        public async Task SendResultMessage(List<UserLog> logs, bool isVictory, int remainderHp)
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle($"{Boss.Name} fight results:");
            builder.WithColor(Color.DarkRed);

            List<string> RewardStrings = new List<string>();
            if (isVictory)
            {
                builder.AddField("**🎇 Victory 🎇**", $"The {Boss.Name} has been killed");
                await GiveOutRewards(RewardStrings);
                builder.WithColor(Color.Green);
            }
            else
            {
                builder.AddField("**❌ Defeat ❌**", $"The {Boss.Name} has successfully escaped.\n HP: **({remainderHp}/{Boss.HP})** ❤️");
            }


            string logsText = "";
            int totalDmg = 0;

            if (logs.Count == 0)
            {
                logsText = "No one showed up lol";
            }
            else
            {
                foreach (var item in logs)
                {
                    if (item is UserLog)
                    {
                        totalDmg += ((UserLog)item).DamageDealt;
                    }

                    logsText += $"{item.GetActorlessString()}";
                    if (isVictory)
                    {
                        logsText += $"--Received: {RewardStrings[logs.IndexOf(item)]}\n";
                    }
                    builder.AddField(item.GetActor(), logsText);
                    logsText = "";
                }
            }
            builder.AddField("\u200b", $"\nTotal damage dealt: {totalDmg}");

            await Message.Channel.SendMessageAsync("", false, builder.Build());

        }

        private int CalculateDamage(ulong userId, bool isDismantled = false, bool isDemoralized = false)
        {
            if (isDemoralized)
            {
                return Data.Data.GetLowestPetDamage(userId, isDismantled);
            }
            string pets = Data.Data.GetPets(userId);
            Random rnd = new Random();

            int petIndex = int.Parse(pets[rnd.Next(0, pets.Length)].ToString());

            int petLvlFromItems = 0;
            if (isDismantled)
            {
                petLvlFromItems = Data.Data.GetItemPetLevel(userId, petIndex);
            }

            return (Data.Data.GetPetLevel(userId, petIndex) - petLvlFromItems) + (isDismantled ? 0 : Data.Data.GetPetTier(userId, petIndex));

        }

        public EmbedBuilder UpdateBuilder(bool isArchon)
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle(Boss.Name);
            builder.WithColor(Boss.GetColor());
            builder.WithImageUrl(Boss.ImageUrl);
            builder.AddField("Level:", $"**{Boss.Level}** 🎚️", true);
            builder.AddField("Boss hp:", $"**{Boss.HP} ❤️**", true);
            builder.AddField("Rarity:", $"**{(isArchon ? "Archon" : Boss.Rarity)} 💠**\n{Boss.Desc}");
            string text = "";

            foreach (ulong item in Participants)
            {
                KushBotUser jew = Data.Data.GetKushBotUser(item);
                int minDmg = (int)(Leaderboard.GetMinimumDmg(jew, true) * (isArchon ? 1.5 : 1));
                int maxDmg = (int)(Leaderboard.GetMinimumDmg(jew, false) * (isArchon ? 1.5 : 1));
                ConsumableBuff artillery = Data.Data.GetConsumableBuff(item, BuffType.BossArtillery);
                text += $"{Program._client.GetUser(item).Username}, Dmg: {minDmg}-{maxDmg}{(artillery != null ? $" +{(int)artillery.Potency}" : "")}\n";
            }

            if (text == "")
            {
                text += "---";
            }

            builder.AddField($"Participants ({Participants.Count}/{Boss.MaxParticipants}):", text, isArchon);
            if (isArchon)
            {
                builder.AddField($"Archon Abilities", $"{string.Join("\n", Boss.ArchonAbilities)}", isArchon);
            }

            builder.AddField("Results", $"The battle will start in <t:{((StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds).ToString().Split('.')[0]}:R>");

            if (isArchon)
            {
                builder.WithFooter(string.Join("\n", Boss.ArchonAbilities.Select(e => $"{e.Name} - {Program.ArchonAbilityDescription[e.Name]}")));
            }
            else
            {
                builder.WithFooter("Click on the Booba reaction to sign up by using a boss ticket");
            }
            return builder;
        }
    }
}
