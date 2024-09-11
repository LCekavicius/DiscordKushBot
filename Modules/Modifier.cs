using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.Modules
{
    public class Modifier : ModuleBase<SocketCommandContext>
    {

        [Command("Simulate Bet", RunMode = RunMode.Async)]
        public async Task simul()
        {
            int amount = 200;
            int loseChance = 633;

            int lost = 0;
            int won = 0;

            Random rad = new Random();


            int n = 100000;
            for (int i = 0; i < n; i++)
            {
                if (i % 2500 == 0)
                    Console.WriteLine($"{i} done");

                int chance = rad.Next(0, 1000);

                if (Program.NerfUser == Context.User.Id)
                {
                    if (chance > loseChance)
                    {
                        if (rad.Next(0, 2) == 0)
                        {
                            loseChance = rad.Next(0, 1000);
                        }
                    }
                }

                float modifier;


                if (chance < loseChance)
                {
                    modifier = rad.Next(0, 100);
                }
                else if (chance < 915)
                {
                    modifier = rad.Next(100, 200);
                }
                else if (chance < 999)
                {
                    modifier = rad.Next(200, 401);
                }
                else
                {
                    modifier = rad.Next(1000, 1500);
                }

                int transfusion = (int)Math.Round((modifier * amount) / 100);

                if (modifier < 100)
                {
                    lost += amount - transfusion;
                }
                else
                {
                    won += transfusion - amount;
                }
            }


            Console.WriteLine($"Rolls {n}");
            Console.WriteLine($"Won {won} baps");
            Console.WriteLine($"lost {lost} baps");
            Console.WriteLine($"Ratio {(double)((double)won / (double)lost)}");
        }


        [Command("Bet")]
        public async Task PingAsync(string ammount)
        {
            int amount;

            if (Program.IgnoredUsers.ContainsKey(Context.User.Id))
            {
                return;
            }


            if (ammount == "all")
            {
                amount = Data.Data.GetBalance(Context.User.Id);
            }
            else
            {
                amount = int.Parse(ammount);
            }

            if (amount < 100)
            {
                await ReplyAsync($"{Context.User.Mention} 100 baps is the minimum amount to bet you eyesore");
                return;
            }

            int userBalance = Data.Data.GetBalance(Context.User.Id);

            if (userBalance < amount)
            {
                await ReplyAsync($"{Context.User.Mention} Is too Poor to bet");
                return;
            }


            await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 2, 2, Context.Channel);

            Program.IgnoredUsers.Add(Context.User.Id, DateTime.Now.AddMilliseconds(Program.GambleDelay + 150));

            List<int> QuestIndexes = new List<int>();
            #region assigment
            string hold = Data.Data.GetQuestIndexes(Context.User.Id);
            string[] values = hold.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                QuestIndexes.Add(int.Parse(values[i]));
            }
            #endregion

            if (amount >= Program.Quests[20].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(20))
            {
                await Program.CompleteQuest(20, QuestIndexes, Context.Channel, Context.User);
            }

            Random rad = new Random();

            int chance = rad.Next(0, 1000);

            if (Program.Fail == Context.User.Id)
            {
                chance = 0;
                Program.Fail = 0;
            }
            if (Program.Test == Context.User.Id)
            {
                chance = rad.Next(624, 1000);
                Program.Test = 0;
            }

            //int loseChance = 630;
            int loseChance = 633;

            if (Program.NerfUser == Context.User.Id)
            {
                if (chance > loseChance)
                {
                    if (rad.Next(0, 2) == 0)
                    {
                        loseChance = rad.Next(0, 1000);
                    }
                }
            }

            float modifier;


            if (chance < loseChance)
            {
                modifier = rad.Next(0, 100);
            }
            else if (chance < 914)//910
            {
                modifier = rad.Next(100, 200);
            }
            else if (chance < 999)
            {
                modifier = rad.Next(200, 401);
            }
            else
            {
                modifier = rad.Next(1000, 1500);
            }


            int transfusion = (int)Math.Round((modifier * amount) / 100);

            userBalance -= (amount - transfusion);

            ConsumableBuff selectedBuff = null;

            if (modifier / 100 >= 1)
            {
                selectedBuff = Data.Data.GetConsumableBuff(Context.User.Id, BuffType.KushGym);
                bool weed = selectedBuff != null && rad.Next(0, 100) < selectedBuff.Potency;

                if (weed)
                {
                    await ReplyAsync($"{Context.User.Mention} bet **{amount}** Baps which transfused into **{transfusion}** with **{modifier / 100}** as a multiplier, gym weed" +
                        $" netted extra {transfusion - amount}, and now has {userBalance + (transfusion - amount)} <:Pepew:945806849406566401>");
                    await WonBaps(2 * transfusion - amount, modifier / 100, amount);

                }
                else
                {
                    await WonBaps(transfusion - amount, modifier / 100, amount);
                    await ReplyAsync($"{Context.User.Mention} bet **{amount}** Baps which transfused into **{transfusion}** with **{modifier / 100}** as a multiplier, and now has {userBalance} <:Pepew:945806849406566401>");
                }

            }
            else
            {
                selectedBuff = Data.Data.GetConsumableBuff(Context.User.Id, BuffType.FishingRod);
                bool weed = selectedBuff != null && rad.Next(0, 100) < selectedBuff.Potency;

                if (weed)
                {
                    await ReplyAsync($"{Context.User.Mention} bet **{amount}** Baps transfused into **{transfusion}** with **{modifier / 100}** as a multiplier but you pulled the money out with ur fishing rod <:Pepew:945806849406566401>");
                }
                else
                {
                    await LostBaps((transfusion - amount) * -1);
                    await ReplyAsync($"{Context.User.Mention} bet **{amount}** Baps transfused into **{transfusion}** with **{modifier / 100}** as a multiplier, and now has {userBalance} <:Pepew:945806849406566401>");
                }
            }

            await Data.Data.ReduceOrRemoveBuffAsync(Context.User.Id, BuffType.KushGym);
            await Data.Data.ReduceOrRemoveBuffAsync(Context.User.Id, BuffType.FishingRod);
        }
        public async Task LostBaps(int amount)
        {
            await Data.Data.SaveBalance(Context.User.Id, amount * -1, true, Context.Channel);
            await Data.Data.SaveLostBapsMN(Context.User.Id, amount);
            await Data.Data.SaveLostBetsMN(Context.User.Id, amount);


            List<int> QuestIndexes = new List<int>();
            #region assigment
            string hold = Data.Data.GetQuestIndexes(Context.User.Id);
            string[] values = hold.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                QuestIndexes.Add(int.Parse(values[i]));
            }
            #endregion

            List<int> WeeklyQuests = Data.Data.GetWeeklyQuest();
            List<int> acceptibleQs = new List<int>();
            acceptibleQs.Add(1);
            acceptibleQs.Add(3);
            acceptibleQs.Add(9);
            acceptibleQs.Add(11);

            if (WeeklyQuests.Contains(1))
            {
                Quest q = Program.WeeklyQuests[1];

                if (Data.Data.GetLostBapsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetLostBapsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    //await Data.Data.SaveLostBapsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(1, Context.Channel, Context.User);
                }
            }
            if (WeeklyQuests.Contains(3))
            {
                Quest q = Program.WeeklyQuests[3];

                if (Data.Data.GetLostBapsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetLostBapsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    //await Data.Data.SaveLostBapsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(3, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(9))
            {
                Quest q = Program.WeeklyQuests[9];

                if (Data.Data.GetLostBetsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetLostBetsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    //await Data.Data.SaveLostBetsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(9, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(11))
            {
                Quest q = Program.WeeklyQuests[11];

                if (Data.Data.GetLostBetsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetLostBetsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    //await Data.Data.SaveLostBetsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(11, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(1) || WeeklyQuests.Contains(3) || WeeklyQuests.Contains(9) || WeeklyQuests.Contains(11))
            {
                await Data.Data.SaveLostBapsWeekly(Context.User.Id, amount);
                await Data.Data.SaveLostBetsWeekly(Context.User.Id, amount);
            }

            if (Data.Data.GetLostBapsMN(Context.User.Id) >= Program.Quests[1].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(1))
            {
                await Program.CompleteQuest(1, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetLostBetsMN(Context.User.Id) >= Program.Quests[5].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(5))
            {
                await Program.CompleteQuest(5, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetBalance(Context.User.Id) >= Program.Quests[10].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(10))
            {
                await Program.CompleteQuest(10, QuestIndexes, Context.Channel, Context.User);
            }

        }
        public async Task WonBaps(int amount, double mod, int inputBaps)
        {
            await Data.Data.SaveBalance(Context.User.Id, amount, true, Context.Channel, inputBaps);
            await Data.Data.SaveWonBapsMN(Context.User.Id, amount);
            await Data.Data.SaveWonBetsMN(Context.User.Id, amount);

            List<int> QuestIndexes = new List<int>();
            #region assignment
            string hold = Data.Data.GetQuestIndexes(Context.User.Id);
            string[] values = hold.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                QuestIndexes.Add(int.Parse(values[i]));
            }
            #endregion

            List<int> WeeklyQuests = Data.Data.GetWeeklyQuest();
            List<int> acceptibleQs = new List<int>();
            acceptibleQs.Add(0);
            acceptibleQs.Add(2);
            acceptibleQs.Add(8);
            acceptibleQs.Add(10);

            if (WeeklyQuests.Contains(0))
            {
                Quest q = Program.WeeklyQuests[0];

                if (Data.Data.GetWonBapsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetWonBapsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    //await Data.Data.SaveWonBapsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(0, Context.Channel, Context.User);
                }
            }
            if (WeeklyQuests.Contains(2))
            {
                Quest q = Program.WeeklyQuests[2];

                if (Data.Data.GetWonBapsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetWonBapsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    // await Data.Data.SaveWonBapsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(2, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(8))
            {
                Quest q = Program.WeeklyQuests[8];

                if (Data.Data.GetWonBetsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetWonBetsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    //await Data.Data.SaveWonBetsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(8, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(10))
            {
                Quest q = Program.WeeklyQuests[10];

                if (Data.Data.GetWonBetsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetWonBetsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    //await Data.Data.SaveWonBetsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(10, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(0) || WeeklyQuests.Contains(2) || WeeklyQuests.Contains(8) || WeeklyQuests.Contains(10))
            {
                await Data.Data.SaveWonBapsWeekly(Context.User.Id, amount);
                await Data.Data.SaveWonBetsWeekly(Context.User.Id, amount);
            }

            if (Data.Data.GetWonBapsMN(Context.User.Id) >= Program.Quests[0].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(0))
            {
                await Program.CompleteQuest(0, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetWonBetsMN(Context.User.Id) >= Program.Quests[4].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(4))
            {
                await Program.CompleteQuest(4, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetWonBetsMN(Context.User.Id) >= Program.Quests[23].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(23))
            {
                await Program.CompleteQuest(23, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetBalance(Context.User.Id) >= Program.Quests[10].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(10))
            {
                await Program.CompleteQuest(10, QuestIndexes, Context.Channel, Context.User);
            }
            if (mod >= Program.Quests[18].CompleteReq && QuestIndexes.Contains(18))
            {
                await Program.CompleteQuest(18, QuestIndexes, Context.Channel, Context.User);
            }
        }

        /*public async Task CompleteQuest(int qIndex, List<int> QuestIndexes)
        {

            await ReplyAsync($"{Context.User.Mention} Quest completed, rewarded: {Program.Quests[qIndex].Baps} baps");
            int delete = QuestIndexes.IndexOf(qIndex);
            QuestIndexes[delete] = -1;
            await Data.Data.SaveQuestIndexes(Context.User.Id, string.Join(',', QuestIndexes));

            await Data.Data.SaveBalance(Context.User.Id, Program.Quests[qIndex].Baps, false);
        }*/
    }
}
