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
    public class Gamble : ModuleBase<SocketCommandContext>
    {

        [Command("Flip",RunMode = RunMode.Async)]
        public async Task PingAsync(string ammount)
        {
            int amount = 0;

            if(ammount != "all")
            {
                amount = int.Parse(ammount);
            }

            int Baps = Data.Data.GetBalance(Context.User.Id);

            if(ammount == "all")
            {
                amount = Baps;
            }

            if (Baps < amount || amount <= 0)
            {
                await ReplyAsync($"{Context.User.Mention} Is too poor to bet <:hangg:945706193358295052>");
                return;
            }

            foreach (ulong IgnUser in Program.IgnoredUsers)
            {
                if(IgnUser == Context.User.Id)
                {
                    return;
                }
            }
            Program.IgnoredUsers.Add(Context.User.Id);
            

            Random rad = new Random();
            int temp = rad.Next(0, 2);

            if(Program.Test == Context.User.Id)
            {
                temp = 1;
                Program.Test = 0;
            }
            else if (Program.Fail == Context.User.Id)
            {
                temp = 0;
                Program.Fail = 0;
            }
            if(Program.NerfUser == Context.User.Id)
            {
                if(temp == 1)
                {
                    temp = rad.Next(0, 2);
                }
            }

            double extratoextra = rad.Next(30, 40 + Data.Data.GetPetLevel(Context.User.Id, 3));

            GardenAffectedSUser te = new GardenAffectedSUser();

            if (Program.GardenAffectedPlayers.Where(x => x.UserId == Context.User.Id).Count() >= 1)
            {
                 te = Program.GardenAffectedPlayers.Where(x => x.UserId == Context.User.Id).FirstOrDefault();
            }  

            if (temp == 1)
            {
                bool weed = false;

                if(te.Effect == "kush gym")
                {
                    if(rad.Next(1, 101) <= te.GetEffictivines())
                    {
                        weed = true;
                    }
                }
                if (weed)
                {
                    await ReplyAsync($"<:ipisa:945780033702752286> {Context.User.Mention} won {amount} Baps, his gym-plant transfused into some extra baps and got {amount} more baps!, he now has {Baps + amount * 2} <:stovi:945780098332774441>");
                    await WonBaps(amount * 2);
                }
                else
                {
                    await ReplyAsync($"<:ipisa:945780033702752286> {Context.User.Mention} won {amount} Baps, and now has {Baps + amount} <:stovi:945780098332774441>");
                    await WonBaps(amount);
                }
            }
            else
            {
                bool weed = false;

                if (te.Effect == "fishing rod")
                {
                    if (rad.Next(1, 101) <= te.GetEffictivines())
                    {
                        Console.WriteLine("HIT");
                        weed = true;
                    }
                }
                if (weed)
                {
                    await ReplyAsync($"<:zvejas:945703266078048328> {Context.User.Mention} would've lost his {amount} Baps, but he reeled them back in with his fishing rod <:fisher:945779965004247051>");
                }
                else
                {

                    await ReplyAsync($"<:zvejas:945703266078048328> {Context.User.Mention} Lost {amount} Baps, and now has {Baps - amount} <:egg:945783802867879987>");
                    await LostBaps(amount);
                }         
            }

            te.Duration -= 1;
            if(te.Duration == 0)
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
            await Data.Data.SaveLostFlipsMN(Context.User.Id, amount);
            await Data.Data.SaveWonFlipsChains(Context.User.Id, Data.Data.GetWonFlipsChain(Context.User.Id) * -1);


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
            acceptibleQs.Add(5);
            acceptibleQs.Add(7);

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
                   // await Data.Data.SaveLostBapsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(3, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(5))
            {
                Quest q = Program.WeeklyQuests[5];

                if (Data.Data.GetLostFlipsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetLostFlipsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    //await Data.Data.SaveLostFlipsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(5, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(7))
            {
                Quest q = Program.WeeklyQuests[7];

                if (Data.Data.GetLostFlipsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetLostFlipsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                    //await Data.Data.SaveLostFlipsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(7, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(1) || WeeklyQuests.Contains(3) || WeeklyQuests.Contains(5) || WeeklyQuests.Contains(7))
            {
                await Data.Data.SaveLostBapsWeekly(Context.User.Id, amount);
                await Data.Data.SaveLostFlipsWeekly(Context.User.Id, amount);
            }


            if (Data.Data.GetLostBapsMN(Context.User.Id) >= Program.Quests[1].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(1))
            {
                await Program.CompleteQuest(1, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetLostFlipsMN(Context.User.Id) >= Program.Quests[3].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(3))
            {
                await Program.CompleteQuest(3, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetBalance(Context.User.Id) >= Program.Quests[10].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(10))
            {
                await Program.CompleteQuest(10, QuestIndexes, Context.Channel, Context.User);
            }
            if (amount >= Program.Quests[14].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(14))
            {
                await Program.CompleteQuest(14, QuestIndexes, Context.Channel, Context.User);
            }


        }
        public async Task WonBaps(int amount)
        {
            await Data.Data.SaveBalance(Context.User.Id, amount, true);
            await Data.Data.SaveWonBapsMN(Context.User.Id, amount);
            await Data.Data.SaveWonFlipsMN(Context.User.Id, amount);

            int req = 60 + (int)(4 * Math.Pow(Program.GetTotalPetLvl(Context.User.Id), 1.08) * ((double)60 / 1400));

            if (amount >= req)
            {
                await Data.Data.SaveWonFlipsChains(Context.User.Id,1);
            }

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
            acceptibleQs.Add(4);
            acceptibleQs.Add(6);

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
                    //await Data.Data.SaveWonBapsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(2, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(4))
            {
                Quest q = Program.WeeklyQuests[4];

                if (Data.Data.GetWonFlipsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetWonFlipsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                   // await Data.Data.SaveWonFlipsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(4, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(6))
            {
                Quest q = Program.WeeklyQuests[6];

                if (Data.Data.GetWonFlipsWeekly(Context.User.Id) < q.GetCompleteReq(Context.User.Id) && (Data.Data.GetWonFlipsWeekly(Context.User.Id) + amount) >= q.GetCompleteReq(Context.User.Id))
                {
                   // await Data.Data.SaveWonFlipsWeekly(Context.User.Id, 50000);
                    await Program.CompleteWeeklyQuest(6, Context.Channel, Context.User);
                }
            }

            if (WeeklyQuests.Contains(0) || WeeklyQuests.Contains(2) || WeeklyQuests.Contains(4) || WeeklyQuests.Contains(6))
            {
                await Data.Data.SaveWonBapsWeekly(Context.User.Id, amount);
                await Data.Data.SaveWonFlipsWeekly(Context.User.Id, amount);
            }


            if (Data.Data.GetLostBapsMN(Context.User.Id) >= Program.Quests[1].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(1))
            {
                await Program.CompleteQuest(1, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetLostFlipsMN(Context.User.Id) >= Program.Quests[3].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(3))
            {
                await Program.CompleteQuest(3, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetBalance(Context.User.Id) >= Program.Quests[10].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(10))
            {
                await Program.CompleteQuest(10, QuestIndexes, Context.Channel, Context.User);
            }
            if (amount >= Program.Quests[14].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(14))
            {
                await Program.CompleteQuest(14, QuestIndexes, Context.Channel, Context.User);
            }

            if (Data.Data.GetWonBapsMN(Context.User.Id) >= Program.Quests[0].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(0))
            {
                await Program.CompleteQuest(0,QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetWonFlipsMN(Context.User.Id) >= Program.Quests[2].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(2))
            {
                await Program.CompleteQuest(2, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetWonFlipsMN(Context.User.Id) >= Program.Quests[22].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(22))
            {
                await Program.CompleteQuest(22, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetBalance(Context.User.Id) >= Program.Quests[10].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(10))
            {
                await Program.CompleteQuest(10, QuestIndexes, Context.Channel, Context.User);
            }
            if (amount >= Program.Quests[14].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(14))
            {
                await Program.CompleteQuest(14, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetWonFlipsChain(Context.User.Id) >= Program.Quests[17].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(17))
            {
                await Program.CompleteQuest(17, QuestIndexes, Context.Channel, Context.User);
            }
        }
        //public async Task CompleteQuest(int qIndex,List<int> QuestIndexes)
        //{

        //        await ReplyAsync($"{Context.User.Mention} Quest completed, rewarded: {Program.Quests[qIndex].Baps} baps");
        //        int delete = QuestIndexes.IndexOf(qIndex);
        //        QuestIndexes[delete] = -1;
        //        await Data.Data.SaveQuestIndexes(Context.User.Id, string.Join(',', QuestIndexes));

        //        await Data.Data.SaveBalance(Context.User.Id, Program.Quests[qIndex].Baps, false);
        //}
    }
}
