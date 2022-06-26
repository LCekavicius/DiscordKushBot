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

namespace KushBot
{
    public class BossObject
    {
        public class Log
        {
            public ulong UserId { get; set; }
            public int DamageDealt { get; set; }

            public Log(ulong userId, int damageDealt)
            {
                UserId = userId;
                DamageDealt = damageDealt;
            }
        }

        public Boss Boss { get; set; }
        public List<ulong> Participants { get; set; }
        public RestUserMessage Message { get; set; }

        public DateTime StartDate { get; set; }


        public BossObject(Boss boss, RestUserMessage message)
        {
            this.Boss = boss;
            Participants = new List<ulong>();
            this.Message = message;
            StartDate = DateTime.Now.AddMinutes(30);
            //Participants.Add(254287232548995073);
            //Participants.Add(230743424263782400);
            //Participants.Add(189771487715000340);
            //Participants.Add(132111645831987200);
        }        

        public async Task SignOff(ulong userId)
        {
            if (!Participants.Contains(userId))
            {
                return;
            }
            Participants.Remove(userId);
            await Data.Data.SaveTicket(userId, true);

            EmbedBuilder builder = UpdateBuilder();

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

            EmbedBuilder builder = UpdateBuilder();

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

        public async Task Combat()
        {
            //await Task.Delay(10 * 1000);
            if(Program.BotTesting)
                await Task.Delay(20 * 1000);
            else
                await Task.Delay(30 * 60 * 1000);

            Program.BossObject = null;

            int TempHp = Boss.HP;

            List<Log> logs = new List<Log>();


            foreach (var item in Participants)
            {
                int BossDam = ItemBossDam(item);
                int att1 = CalculateDamage(item);
                int att2 = CalculateDamage(item);
                // int att3 = CalculateDamage(item);
                Log log = new Log(item, att1 + att2 + BossDam);
                
                logs.Add(log);
                TempHp -= att1;
                TempHp -= att2;
                TempHp -= BossDam;

            }


            bool IsVictory = false;

            if(TempHp <= 0)
            {
                IsVictory = true;
            }
            await SendResultMessage(logs, IsVictory, TempHp);

        }
        
        public int RarityStringToInt(string rarity)
        {
            if(rarity == "Common")
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

            if(Participants.Count > 1)
            {
                itemsToGive = Participants.Count / 2;

                if (Participants.Count % 2 != 0)
                {
                    Random rad = new Random();

                    if (rad.NextDouble() > 0.5)
                        itemsToGive++;
                }
            }
            Random rnd = new Random();
            List<ulong> receivers = Participants.OrderBy(x => rnd.Next()).Take(itemsToGive).ToList();

            for (int i = 0; i < Participants.Count; i++)
            {
                if (receivers.Contains(Participants[i]))
                {
                    List<Item> inv = Data.Data.GetUserItems(Participants[i]);
                    if(inv.Count >= Program.ItemCap)
                    {
                        RewardStrings[i] += $", Full inv moment <:tf:946039048789688390>";
                    }
                    else
                    {
                        RewardStrings[i] += $", a {Boss.Rarity} item";
                        Data.Data.GenerateItem(Participants[i], rarity);
                    }
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
            else if(Boss.Rarity == "Rare")
            {

                for (int i = 0; i < Participants.Count; i++)
                {
                    RewardStrings[i] += ", an egg";
                    await Data.Data.SaveEgg(Participants[i], true);

                    int petIndex = rnd.Next(0, 2);
                    int c = 1;
                    await HandlePetDupes(Participants[i], petIndex);

                    if(rnd.NextDouble() < 0.45)
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
                    if(petFeedPetIndex != -1)
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

                    int petFeedPetIndex = Data.Data.GetRandomPetId(Participants[i]);
                    if (petFeedPetIndex != -1)
                    {
                        RewardStrings[i] += $", Food for {Program.Pets[petFeedPetIndex].Name}";
                        await Data.Data.SavePetLevels(Participants[i], petFeedPetIndex, Data.Data.GetPetLevel(Participants[i], petFeedPetIndex) + 1, false);
                    }

                    int petFeedPetIndex2 = Data.Data.GetRandomPetId(Participants[i]);
                    if (petFeedPetIndex != -1)
                    {
                        RewardStrings[i] += $", Food for {Program.Pets[petFeedPetIndex2].Name}";
                        await Data.Data.SavePetLevels(Participants[i], petFeedPetIndex2, Data.Data.GetPetLevel(Participants[i], petFeedPetIndex2) + 1, false);
                    }

                    if(rnd.NextDouble() < 0.5)
                    {
                        int petFeedPetIndex3 = Data.Data.GetRandomPetId(Participants[i]);
                        if (petFeedPetIndex != -1)
                        {
                            RewardStrings[i] += $", Food for {Program.Pets[petFeedPetIndex3].Name}";
                            await Data.Data.SavePetLevels(Participants[i], petFeedPetIndex3, Data.Data.GetPetLevel(Participants[i], petFeedPetIndex3) + 1, false);
                        }
                    }

                    if(rnd.NextDouble() < 0.15)
                    {
                        //RewardStrings[i] += $", **Custom icon (PM ADMIN)**";
                    }

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

        public async Task SendResultMessage(List<Log> logs, bool isVictory, int remainderHp)
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
                    totalDmg += item.DamageDealt;
                    logsText += $"{Program._client.GetUser(item.UserId).Username} dealt {item.DamageDealt} dmg\n";
                    if (isVictory)
                        logsText += $"--Received: {RewardStrings[logs.IndexOf(item)]}\n";
                }
            }
            builder.AddField("Log:", logsText + $"\nTotal damage dealt: {totalDmg}");

            await Message.Channel.SendMessageAsync("", false, builder.Build());

        }

        private int CalculateDamage(ulong userId)
        {
            string pets = Data.Data.GetPets(userId);
            Random rnd = new Random();

            int petIndex = int.Parse(pets[rnd.Next(0, pets.Length)].ToString());

            return Data.Data.GetPetLevel(userId, petIndex) + Data.Data.GetPetTier(userId, petIndex);

        }

        public EmbedBuilder UpdateBuilder()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle(Boss.Name);
            builder.WithColor(Boss.GetColor());
            builder.WithImageUrl(Boss.ImageUrl);
            builder.AddInlineField("Level:", $"**{Boss.Level}** 🎚️");
            builder.AddInlineField("Boss hp:", $"**{Boss.HP} ❤️**");
            builder.AddField("Rarity:", $"**{Boss.Rarity} 💠**\n{Boss.Desc}");
            string text = "";

            foreach (ulong item in Participants)
            {
                SUser jew = Data.Data.GetJew(item);
                int minDmg = Leaderboard.GetMinimumDmg(jew, true);
                int maxDmg = Leaderboard.GetMinimumDmg(jew, false);
                text += $"{Program._client.GetUser(item).Username}, Dmg: {minDmg}-{maxDmg}\n";
            }

            if(text == "")
            {
                text += "---";
            }

            builder.AddField($"Participants ({Participants.Count}/{Boss.MaxParticipants}):", text);

            builder.AddField("Results", $"The battle will start in 30 minutes");

            builder.WithFooter("Click on the Booba reaction to sign up by using a boss ticket, click the ❌ to sign off");
            return builder;
        }
    }
}
