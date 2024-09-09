using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KushBot.Data;
using KushBot.DataClasses;
using System.Linq;

namespace KushBot.Modules
{
    enum RewardType { BAPS, ITEM, CHEEMS }
    public class Slots : ModuleBase<SocketCommandContext>
    {
        private class Slot
        {
            public string emoji { get; set; }
            public RewardType rewardType { get; set; }
            public double rewardModifier { get; set; }
            public double Weight { get; set; }

            public Slot(string emoji, RewardType rewardType, double rewardModifier, double weight = 0)
            {
                this.emoji = emoji;
                this.rewardType = rewardType;
                this.rewardModifier = rewardModifier;
                Weight = weight;
            }
        }

        private List<Slot> emotes;

        private void SetEmojiList(double add)
        {
            emotes = new List<Slot>();
            emotes.Add(new Slot("<:ima:945342040529567795>", RewardType.ITEM, 1, 0.09));
            emotes.Add(new Slot("<:Booba:944937036702441554>", RewardType.BAPS, 8 + add / 3, 0.16));
            emotes.Add(new Slot("<:kitadimensija:945779895164895252>", RewardType.BAPS, 7 + add / 4, 0.24));
            emotes.Add(new Slot("<:Cheems:945704650378707015>", RewardType.CHEEMS, 2, 0.32));
            emotes.Add(new Slot("<:stovi:945780098332774441>", RewardType.BAPS, 6 + add / 5, 0.41));
            emotes.Add(new Slot("<:rieda:945781493291184168>", RewardType.BAPS, 5 + add / 6, 0.5));
            emotes.Add(new Slot("<:Pepejam:945806412049702972>", RewardType.BAPS, 4 + add / 7, 0.6));
            emotes.Add(new Slot("<:widepep:945703091876020245>", RewardType.BAPS, 3 + add / 8, 0.7));
            emotes.Add(new Slot("<:Omega:945781765899952199>", RewardType.BAPS, 1.7 + add / 9, 1));

        }

        public static int rowsCount = 3;

        [Command("slots")]
        public async Task PingAsync()
        {
            SetEmojiList(45.75);

            //Console.WriteLine(SimulateSlots());
            //return;

            if (Program.IgnoredUsers.ContainsKey(Context.User.Id))
            {
                return;
            }

            var slotTokensBuff = Data.Data.GetConsumableBuff(Context.User.Id, BuffType.SlotTokens);
            
            int amount = 40;

            if (Program.GetTotalPetLvl(Context.User.Id) > 0)
                amount += (Program.GetTotalPetLvl(Context.User.Id)) + 5 * Program.GetAveragePetLvl(Context.User.Id);

            if (Data.Data.GetBalance(Context.User.Id) < amount && slotTokensBuff == null)
            {
                await ReplyAsync($"{Context.User.Mention} POOR KEW. slot machines require {amount} baps for you");
                return;
            }

            await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 2, 3, Context.Channel);

            Program.IgnoredUsers.Add(Context.User.Id, DateTime.Now.AddMilliseconds(Program.GambleDelay + 150));

            List<Slot> picks;

            int won = Roll(emotes, amount, out picks);//0;

            string replyString = "";
            int i = 0;

            List<Slot> winningSlots = new List<Slot>();

            int centralSlot = -1;

            foreach (var item in picks)
            {
                replyString += item.emoji;
                i++;

                if (i % 3 == 0)
                {
                    if (picks[i - 3].Weight == picks[i - 2].Weight && picks[i - 3].Weight == picks[i - 1].Weight)
                    {
                        replyString += "🔔";
                        winningSlots.Add(picks[i - 3]);

                        if (i == 6)
                        {
                            centralSlot = winningSlots.Count - 1;
                        }

                    }
                    else
                        replyString += "❌";

                    replyString += "\n";
                }
                else
                {
                    replyString += " ";
                }
            }

            int wonCheems = 0;
            List<Item> generatedItems = new List<Item>();
            Random rnd = new Random();
            int c = 0;
            foreach (var item in winningSlots)
            {
                if (item.rewardType == RewardType.CHEEMS)
                {
                    int tempCheems = 0;

                    int baseR = 30 + rnd.Next(0, 11);
                    tempCheems += (int)(item.rewardModifier * (double)baseR);
                    tempCheems += amount / 2;



                    if (centralSlot != -1 && centralSlot == c)
                    {
                        tempCheems *= 2;
                    }

                    wonCheems += tempCheems;

                }
                else if (item.rewardType == RewardType.ITEM)
                {
                    if (Data.Data.GetUserItems(Context.User.Id).Count >= Program.ItemCap)
                    {
                        Item getFucked = new Item();
                        getFucked.Id = -1;
                        generatedItems.Add(getFucked);
                    }
                    else
                    {
                        double roll = rnd.NextDouble();

                        if (centralSlot != -1 && centralSlot == c)
                        {
                            roll -= 0.05;
                        }

                        int rarity = 0;
                        if (roll > 0.4 + (double)((double)amount / 2000))
                            rarity = 1;
                        else if (roll > 0.3 + (double)((double)amount / 3500))
                            rarity = 2;
                        else if (roll > 0.15 + (double)((double)amount / 5000))
                            rarity = 3;
                        else if (roll > 0.06 + (double)((double)amount / 6500))
                            rarity = 4;
                        else
                            rarity = 5;

                        generatedItems.Add(Data.Data.GenerateItem(Context.User.Id, rarity));
                    }
                }
                c++;
            }

            string wonCheemsString = "";
            string wonItemString = "";
            if (wonCheems > 0)
            {
                wonCheemsString = $"\nYou won {wonCheems} cheems";
            }


            for (int j = 0; j < generatedItems.Count; j++)
            {
                if (generatedItems[j].Id != -1)
                {
                    wonItemString += $"\nYou won a {RarityString(generatedItems[j].Rarity)} {generatedItems[j].Name}!";

                }
                else
                {
                    wonItemString += $"\nYou would've gotten an item, but your inventory is full <:tf:946039048789688390>";
                }
            }

            string costString = slotTokensBuff?.Duration > 0
                ? $"Slot machine cost: 1 Slot token"
                : $"Slot machine cost: {amount} baps";


            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle($"{Context.User.Username}'s slot machine");
            builder.AddField("Slots", $"{replyString}");
            builder.AddField("Result", $"You won {won} baps{wonCheemsString}{wonItemString}\n{costString}");

            if (won > 0 || wonCheems > 0 || generatedItems.Count > 0)
            {
                builder.WithColor(Color.Green);
            }
            else
            {
                builder.WithColor(Color.Red);
            }

            if (wonCheems != 0)
                await Data.Data.AddUserCheems(Context.User.Id, wonCheems);

            if (slotTokensBuff?.Duration > 0)
            {
                await Data.Data.ReduceOrRemoveBuffAsync(slotTokensBuff.Id);
                if(won - amount > 0)
                    await Data.Data.SaveBalance(Context.User.Id, won, true, Context.Channel, amount);
            }
            else
            {
                await Data.Data.SaveBalance(Context.User.Id, won - amount, true, Context.Channel, amount);
            }


            //QUESTS


            List<int> QuestIndexes = new List<int>();
            #region assigment
            string hold = Data.Data.GetQuestIndexes(Context.User.Id);
            string[] values = hold.Split(',');
            for (int x = 0; x < values.Length; x++)
            {
                QuestIndexes.Add(int.Parse(values[x]));
            }
            #endregion

            List<int> WeeklyQuests = Data.Data.GetWeeklyQuest();

            if (won - amount < 0)
            {
                await Data.Data.SaveLostBapsMN(Context.User.Id, amount);
                await Data.Data.SaveLostBapsWeekly(Context.User.Id, amount);

                if (Data.Data.GetLostBapsMN(Context.User.Id) >= Program.Quests[1].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(1))
                {
                    await Program.CompleteQuest(1, QuestIndexes, Context.Channel, Context.User);
                }


                if (WeeklyQuests.Contains(1))
                {
                    Quest q = Program.WeeklyQuests[1];
                    int lostbapsw = Data.Data.GetLostBapsWeekly(Context.User.Id);
                    if (lostbapsw < q.GetCompleteReq(Context.User.Id) && (lostbapsw + amount) >= q.GetCompleteReq(Context.User.Id))
                    {
                        await Program.CompleteWeeklyQuest(1, Context.Channel, Context.User);
                    }
                }
                if (WeeklyQuests.Contains(3))
                {
                    Quest q = Program.WeeklyQuests[3];
                    int lostbapsw = Data.Data.GetLostBapsWeekly(Context.User.Id);
                    if (lostbapsw < q.GetCompleteReq(Context.User.Id) && (lostbapsw + amount) >= q.GetCompleteReq(Context.User.Id))
                    {
                        await Program.CompleteWeeklyQuest(3, Context.Channel, Context.User);
                    }
                }
            }
            else
            {

                await Data.Data.SaveWonBapsMN(Context.User.Id, won);
                await Data.Data.SaveWonBapsWeekly(Context.User.Id, won);

                if (Data.Data.GetWonBapsMN(Context.User.Id) >= Program.Quests[0].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(0))
                {
                    await Program.CompleteQuest(0, QuestIndexes, Context.Channel, Context.User);
                }

                if (WeeklyQuests.Contains(0))
                {
                    Quest q = Program.WeeklyQuests[0];
                    int wonbapsw = Data.Data.GetWonBapsWeekly(Context.User.Id);
                    if (wonbapsw < q.GetCompleteReq(Context.User.Id) && (wonbapsw + won) >= q.GetCompleteReq(Context.User.Id))
                    {
                        await Program.CompleteWeeklyQuest(0, Context.Channel, Context.User);
                    }
                }

                if (WeeklyQuests.Contains(2))
                {
                    Quest q = Program.WeeklyQuests[2];
                    int wonbapsw = Data.Data.GetWonBapsWeekly(Context.User.Id);
                    if (wonbapsw < q.GetCompleteReq(Context.User.Id) && (wonbapsw + won) >= q.GetCompleteReq(Context.User.Id))
                    {
                        await Program.CompleteWeeklyQuest(2, Context.Channel, Context.User);
                    }
                }
            }


            await ReplyAsync("", false, builder.Build());
        }

        string RarityString(int rarity)
        {
            if (rarity == 1)
                return "Common";
            else if (rarity == 2)
                return "Uncommon";
            else if (rarity == 3)
                return "Rare";
            else if (rarity == 4)
                return "Epic";
            else
                return "Legendary";

        }

        private Slot RollWithWeight(List<Slot> slots)
        {
            Random rnd = new Random();
            double roll = rnd.NextDouble();

            foreach (var item in slots)
            {
                if (roll < item.Weight)
                    return item;
            }

            return null;

        }

        private int Roll(List<Slot> slots, int baps, out List<Slot> picks)
        //private int Roll(List<Slot> slots, int baps)
        {

            Random rnd = new Random();

            picks = new List<Slot>();
            //List<Slot> picks = new List<Slot>();
            for (int i = 0; i < rowsCount * 3; i++)
            {
                picks.Add(RollWithWeight(slots));
            }
            List<bool> rowsWon = new List<bool>();

            for (int i = 0; i < rowsCount * 3; i += 3)
            {
                if (picks[i].emoji.Equals(picks[i + 1].emoji) && picks[i].emoji.Equals(picks[i + 2].emoji))
                {
                    rowsWon.Add(true);
                }

                else
                    rowsWon.Add(false);
            }
            return GenerateWinnings(baps, picks, rowsWon);

        }

        private int GenerateWinnings(int betAmount, List<Slot> picks, List<bool> rows)
        {
            double ret = 0;
            double prev = 0;

            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i] && picks[i * 3].rewardType == RewardType.BAPS)
                {
                    prev = picks[i * 3].rewardModifier * betAmount;
                }
                if (i == rows.Count / 2)
                {
                    prev *= 2;
                }


                ret += prev;
                prev = 0;
            }

            return (int)ret;
        }


        private double SimulateSlots()
        {
            int stack = 75000;
            int amount = 200;

            int won = 0;
            int lost = 0;

            int bapsWon = 0;
            int bapsLost = 0;

            int n = 1_000_000;
            for (int i = 0; i < n; i++)
            {
                List<Slot> picks;
                int win = Roll(emotes, amount, out picks);
                if (win > 0)
                {
                    won++;
                    bapsWon += win - amount;
                }
                else
                {
                    lost++;
                    bapsLost += amount;
                }

                stack += win;

                stack -= amount;
            }

            Console.WriteLine($"Slot cost: {amount}");
            Console.WriteLine($"spins: {n}");
            Console.WriteLine($"times won: {won}");
            Console.WriteLine($"times lost: {lost}");
            Console.WriteLine($"Total winnings: {bapsWon}");
            Console.WriteLine($"Total loss: {bapsLost}");
            Console.WriteLine($"Ratio: {(double)bapsWon / (double)bapsLost}");

            double chanceOfItem = 3 * (emotes[0].Weight * emotes[0].Weight * emotes[0].Weight);

            Console.WriteLine($"1 item every {(1 / chanceOfItem)} spins");
            Console.WriteLine($"1 item every {((1 / chanceOfItem) * amount) * (1f - (double)((double)bapsWon / (double)bapsLost))} baps");

            return (double)bapsWon / (double)bapsLost;

        }
    }
}
