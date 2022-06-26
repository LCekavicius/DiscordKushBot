using Discord;
using Discord.Commands;
using Discord.WebSocket;
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

            int baps = 10000;
            int single = 10;
            int mod = 4;
            Random rad = new Random();
            Console.WriteLine("starting at: " + baps);
            for (int i = 0; i < 100000; i++)
            {
                int temp = rad.Next(0, mod + 1);
                if(temp == mod)
                {
                    baps += single * mod;
                }
                else
                {
                    baps -= single;
                }
                if(baps <= 0)
                {
                    break;
                }
            }

            Console.WriteLine("Finishde at: " + baps);

        }

        [Command("risk", RunMode = RunMode.Async)]
        public async Task PingAsync(string ammount, int Mod)
        {
            
            if(Mod < 4)
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

            foreach (ulong IgnUser in Program.IgnoredUsers)
            {
                if (IgnUser == Context.User.Id)
                {
                    return;
                }
            }
            Program.IgnoredUsers.Add(Context.User.Id);

            Random rad = new Random();

            int temp2 = Mod;




            if (rad.Next(0,100) > 90)
            {
                temp2++;
            }

            double ExtraWinnings = 0;

            if (Program.CatchupMechanic.Any(x => x.userId == Context.User.Id)
                && rad.NextDouble() < (0.3 / Mod)
                && Program.CatchupMechanic.Where(x => x.userId == Context.User.Id).FirstOrDefault().remainingBuffs > 0)

            {
                Program.CatchupMechanic.Where(x => x.userId == Context.User.Id).FirstOrDefault().remainingBuffs -= 1;
                //Console.WriteLine("HIT");
                Program.Test = Context.User.Id;
            }


            if (Context.User.Id == Program.NerfUser)
            {
                temp2 += 2;
            }

            int temp = rad.Next(0, Mod+1);

            double winningsFloat = amount * Mod;

            winningsFloat = Math.Round(winningsFloat + ExtraWinnings);

            int winnings = Convert.ToInt32(winningsFloat);

            double extratoextra = rad.Next(30, 40 + Data.Data.GetPetLevel(Context.User.Id, 3));

            if(Program.Fail == Context.User.Id)
            {
                temp = 0;
                Program.Fail = 0;
            }
            if (Program.Test == Context.User.Id && Mod <= 100)
            {
                temp = Mod;
                Program.Test = 0;
            }
            GardenAffectedSUser te = new GardenAffectedSUser();

            if (Program.GardenAffectedPlayers.Where(x => x.UserId == Context.User.Id).Count() >= 1)
            {
                te = Program.GardenAffectedPlayers.Where(x => x.UserId == Context.User.Id).FirstOrDefault();
            }

            if (temp == Mod)
            {
                bool weed = false;

                if (te.Effect == "kush gym")
                {
                    if (rad.Next(1, 101) <= te.GetEffictivines())
                    {
                        weed = true;
                    }
                }
                if (weed)
                {
                    await ReplyAsync($"<:monkaw:725391691271635074> {Context.User.Mention} Risked and won {winnings} Baps, his gym-plant transfused into {amount} more baps. and he now has {Baps + winnings + amount} <:kitadimensija:603612585388146701>");
                    await WonBaps(winnings + amount, Mod);
                }
                else
                {
                    await ReplyAsync($"<:monkaw:725391691271635074>{Context.User.Mention} Risked and won {winnings} Baps, and now has {Baps + winnings} <:kitadimensija:603612585388146701>");
                    await WonBaps(winnings, Mod);
                }


            }
            else
            {
                bool weed = false;

                if (te.Effect == "fishing rod")
                {
                    if (rad.Next(1, 101) <= te.GetEffictivines())
                    {
                        weed = true;
                    }
                }

                if (weed)
                {
                    await ReplyAsync($"<:zvej2:621802525812719646> {Context.User.Mention} would've lost his {amount} Baps, but he reeled them back in with his fishing rod <:zvej:603612521110175755>");

                }
                else
                {
                    await ReplyAsync($"<:zvej2:621802525812719646> {Context.User.Mention} Risked and Lost {amount} Baps, and now has {Baps - amount} <:zltr:945780861662556180>");
                    await LostBaps(amount);

                }

            }

            te.Duration -= 1;
            if (te.Duration == 0)
            {
                Program.GardenAffectedPlayers.Remove(te);
            }


            await Task.Delay(Program.GambleDelay);
            Program.IgnoredUsers.Remove(Context.User.Id);
        }


        public async Task LostBaps(int amount)
        {
            await Data.Data.SaveBalance(Context.User.Id, amount * -1, true);
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
        public async Task WonBaps(int amount,int mod)
        {
            await Data.Data.SaveBalance(Context.User.Id, amount, true);
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
            if(amount / mod >= Program.Quests[21].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(21))
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
