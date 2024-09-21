using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KushBot.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.Modules
{
    public class Risk : ModuleBase<SocketCommandContext>
    {
        [Command("simulate risk")]
        public async Task Simul()
        {

            int baps = 100000;
            int single = 20;
            int mod = 4;
            Random rad = new Random();
            Console.WriteLine("starting at: " + baps);
            for (int i = 0; i < 1_000_000; i++)
            {
                int temp = rad.Next(0, mod + 1);
                if (temp == mod)
                {
                    baps += single * mod;
                }
                else
                {
                    baps -= single;
                }
                if (baps <= 0)
                {
                    break;
                }
            }

            Console.WriteLine("Finishde at: " + baps);

        }

        //[Command("risk")]
        public async Task PingAsync(string ammount, int Mod)
        {

            if (Mod < 4)
            {
                await ReplyAsync($"{Context.User.Mention} Risk modifier of 4 is the minimum, bruh <:eggsleep:610494851557097532>");
                return;
            }

            int amount = 0;

            if (ammount != "all")
            {
                amount = int.Parse(ammount);
            }

            int Baps = Data.Data.GetBalance(Context.User.Id);

            if (ammount == "all")
            {
                amount = Baps;
            }
            if (Baps < amount || amount <= 0)
            {
                await ReplyAsync($"{Context.User.Mention} Is too poor to bet <:eggsleep:610494851557097532>");
                return;
            }

            if (Program.IgnoredUsers.ContainsKey(Context.User.Id))
            {
                return;
            }

            await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 2, 1, Context.Channel);

            Program.IgnoredUsers.Add(Context.User.Id, DateTime.Now.AddMilliseconds(Program.GambleDelay + 150));

            Random rad = new Random();

            int temp = rad.Next(0, Mod + 1);

            int winnings = (amount * Mod);

            if (Program.Fail == Context.User.Id)
            {
                temp = 0;
                Program.Fail = 0;
            }
            if (Program.Test == Context.User.Id && Mod <= 50)
            {
                temp = Mod;
                Program.Test = 0;
            }


            ConsumableBuff selectedBuff = null;

            if (temp == Mod)
            {
                selectedBuff = Data.Data.GetConsumableBuff(Context.User.Id, BuffType.KushGym);
                bool buffProc = selectedBuff != null && rad.Next(0, 100) < selectedBuff.Potency;

                if (buffProc)
                {
                    await ReplyAsync($"<:monkaw:725391691271635074> {Context.User.Mention} Risked and won {winnings} Baps, his gym-plant transfused into {winnings} more baps. and he now has {Baps + 2 * winnings} <:kitadimensija:603612585388146701>");
                    await WonBaps(2 * winnings, Mod);
                }
                else
                {
                    await ReplyAsync($"<:monkaw:725391691271635074>{Context.User.Mention} Risked and won {winnings} Baps, and now has {Baps + winnings} <:kitadimensija:603612585388146701>");
                    await WonBaps(winnings, Mod);
                }
            }
            else
            {
                selectedBuff = Data.Data.GetConsumableBuff(Context.User.Id, BuffType.FishingRod);
                bool buffProc = selectedBuff != null && rad.Next(0, 100) < selectedBuff.Potency;

                if (buffProc)
                {
                    await ReplyAsync($"<:zvej2:621802525812719646> {Context.User.Mention} would've lost his {amount} Baps, but he reeled them back in with his fishing rod <:zvej:603612521110175755>");

                }
                else
                {
                    await ReplyAsync($"<:zvej2:621802525812719646> {Context.User.Mention} Risked and Lost {amount} Baps, and now has {Baps - amount} <:zltr:945780861662556180>");
                    await LostBaps(amount);

                }

            }

            await Data.Data.ReduceOrRemoveBuffAsync(Context.User.Id, BuffType.KushGym);
            await Data.Data.ReduceOrRemoveBuffAsync(Context.User.Id, BuffType.FishingRod);

        }


        public async Task LostBaps(int amount)
        {
            await Data.Data.SaveBalance(Context.User.Id, amount * -1, true, Context.Channel);
            await Data.Data.SaveLostBapsMN(Context.User.Id, amount);
            await Data.Data.SaveLostRisksMN(Context.User.Id, amount);

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
            acceptibleQs.Add(13);
            acceptibleQs.Add(15);

            if (WeeklyQuests.Contains(1))
            {
                Quest q = Program.WeeklyQuests[1];

                if (Data.Data.GetLostBapsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetLostBapsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    // await Data.Data.SaveLostBapsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(1, Context.Channel, Context.User);
                }
            }
            if (WeeklyQuests.Contains(3))
            {
                Quest q = Program.WeeklyQuests[3];

                if (Data.Data.GetLostBapsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetLostBapsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    // await Data.Data.SaveLostBapsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(3, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(13))
            {
                Quest q = Program.WeeklyQuests[13];

                if (Data.Data.GetLostRisksWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetLostRisksWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    // await Data.Data.SaveLostRisksWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(13, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(15))
            {
                Quest q = Program.WeeklyQuests[15];

                if (Data.Data.GetLostRisksWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetLostRisksWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    //await Data.Data.SaveLostRisksWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(15, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(1) || WeeklyQuests.Contains(3) || WeeklyQuests.Contains(13) || WeeklyQuests.Contains(15))
            {
                await Data.Data.SaveLostBapsWeekly(Context.User.Id, amount);
                await Data.Data.SaveLostRisksWeekly(Context.User.Id, amount);
            }


            if (Data.Data.GetLostBapsMN(Context.User.Id) >= Program.Quests[1].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(1))
            {
                await Program.CompleteQuest(1, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetLostRisksMN(Context.User.Id) >= Program.Quests[7].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(7))
            {
                await Program.CompleteQuest(7, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetBalance(Context.User.Id) >= Program.Quests[10].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(10))
            {
                await Program.CompleteQuest(10, QuestIndexes, Context.Channel, Context.User);
            }
            if (amount >= Program.Quests[21].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(21))
            {
                await Program.CompleteQuest(21, QuestIndexes, Context.Channel, Context.User);
            }


        }
        public async Task WonBaps(int amount, int mod)
        {
            await Data.Data.SaveBalance(Context.User.Id, amount, true, Context.Channel, amount / mod);
            await Data.Data.SaveWonBapsMN(Context.User.Id, amount);
            await Data.Data.SaveWonRisksMN(Context.User.Id, amount);

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
            acceptibleQs.Add(12);
            acceptibleQs.Add(14);

            if (WeeklyQuests.Contains(0))
            {
                Quest q = Program.WeeklyQuests[0];

                if (Data.Data.GetWonBapsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetWonBapsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    // await Data.Data.SaveWonBapsWeekly(Context.User.Id, 50000);
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

            if (WeeklyQuests.Contains(12))
            {
                Quest q = Program.WeeklyQuests[12];

                if (Data.Data.GetWonRisksWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetWonRisksWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    // await Data.Data.SaveWonRisksWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(12, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(14))
            {
                Quest q = Program.WeeklyQuests[14];

                if (Data.Data.GetWonRisksWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetWonRisksWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    //  await Data.Data.SaveWonRisksWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(14, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(0) || WeeklyQuests.Contains(2) || WeeklyQuests.Contains(12) || WeeklyQuests.Contains(14))
            {
                await Data.Data.SaveWonBapsWeekly(Context.User.Id, amount);
                await Data.Data.SaveWonRisksWeekly(Context.User.Id, amount);
            }

            if (Data.Data.GetWonBapsMN(Context.User.Id) >= Program.Quests[0].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(0))
            {
                await Program.CompleteQuest(0, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetWonRisksMN(Context.User.Id) >= Program.Quests[6].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(6))
            {
                await Program.CompleteQuest(6, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetBalance(Context.User.Id) >= Program.Quests[10].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(10))
            {
                await Program.CompleteQuest(10, QuestIndexes, Context.Channel, Context.User);
            }
            if (amount / mod >= Program.Quests[19].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(19) && mod >= 8)
            {
                await Program.CompleteQuest(19, QuestIndexes, Context.Channel, Context.User);
            }
            if (amount / mod >= Program.Quests[21].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(21))
            {
                await Program.CompleteQuest(21, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetWonRisksMN(Context.User.Id) >= Program.Quests[24].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(24))
            {
                await Program.CompleteQuest(24, QuestIndexes, Context.Channel, Context.User);
            }
        }

    }


}
