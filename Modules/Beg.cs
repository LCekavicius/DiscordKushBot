using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KushBot.Data;

namespace KushBot.Modules
{
    public class Beg : ModuleBase<SocketCommandContext>
    {

        [Command("beg")]
        public async Task PingAsync()
        {
            DateTime lastbeg = Data.Data.GetLastBeg(Context.User.Id);

            if(lastbeg.AddHours(1) > DateTime.Now)
            {
                TimeSpan timeLeft = lastbeg.AddHours(1) - DateTime.Now;
                await ReplyAsync($"<:egg:945783802867879987> {Context.User.Mention} " +
                    $"You still Have to wait {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}" +
                    $" to beg again, dipshit <:zltr:945780861662556180>");
                return;
            }


            Random rad = new Random();

            int charity = 0;

            bool PetPowers = Exists(Data.Data.GetPets(Context.User.Id), 0);

            int BegNum = rad.Next(25, 41);

            int petAbuseCdr = Data.Data.GetPetAbuseSupernedStrength(Context.User.Id, 0);

            if (PetPowers)
            {
                double diversity = (0.8 + (double)BegNum / 30);
                await Data.Data.SaveLastBeg(Context.User.Id, DateTime.Now.AddMinutes(-1 * petAbuseCdr + (-1 * Data.Data.GetPetTier(Context.User.Id,0))));

                int PetGain = (int)Math.Ceiling((1.55 * Data.Data.GetPetLevel(Context.User.Id, 0)) * diversity)
                    + (int)Math.Round(Data.Data.GetPetLevel(Context.User.Id, 0) * 1.4);

                charity = BegNum + PetGain;

                await ReplyAsync($"{Context.User.Mention} is so pathetic i had to give him {charity} baps, of which {PetGain} is because of his pet {Program.Pets[0].Name} <:Omega:945781765899952199>");
                await Data.Data.SaveBalance(Context.User.Id, charity, false);

            }
            else
            {
                await Data.Data.SaveLastBeg(Context.User.Id, DateTime.Now);

                await ReplyAsync($"{Context.User.Mention} is so pathetic i had to give him {BegNum} baps <:Omega:945781765899952199>");
                await Data.Data.SaveBalance(Context.User.Id, BegNum, false);
            }

            List<int> QuestIndexes = new List<int>();

            string hold = Data.Data.GetQuestIndexes(Context.User.Id);
            string[] values = hold.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                QuestIndexes.Add(int.Parse(values[i]));
            }

            await Data.Data.SaveBegsMN(Context.User.Id, 1);
            await Data.Data.SaveBegsWeekly(Context.User.Id, 1);

            List<int> WeeklyQuests = Data.Data.GetWeeklyQuest();

            if (WeeklyQuests.Contains(16))
            {
                if(Data.Data.GetBegsWeekly(Context.User.Id) == Program.WeeklyQuests[16].GetCompleteReq(Context.User.Id))
                {
                    await Program.CompleteWeeklyQuest(16, Context.Channel, Context.User);
                }
            }

            if (QuestIndexes.Contains(8) && BegNum >= 32)
            {
                await Program.CompleteQuest(8, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetBalance(Context.User.Id) >= Program.Quests[10].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(10))
            {
                await Program.CompleteQuest(10, QuestIndexes, Context.Channel, Context.User);
            }
            if(Data.Data.GetBegsMN(Context.User.Id) >= Program.Quests[11].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(11))
            {
                await Program.CompleteQuest(11, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetBegsMN(Context.User.Id) >= Program.Quests[12].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(12))
            {
                await Program.CompleteQuest(12, QuestIndexes, Context.Channel, Context.User);
            }

        }

        public bool Exists(string text, int match)
        {
            for (int i = 0; i < text.Length; i++)
            {
                int temp = int.Parse(text[i].ToString());
                if (temp == match)
                {
                    return true;
                }
            }
            return false;
        }

    /*    public async Task CompleteQuest(int qIndex, List<int> QuestIndexes)
        {

            await ReplyAsync($"{Context.User.Mention} Quest completed, rewarded: {Program.Quests[qIndex].Baps} baps");
            int delete = QuestIndexes.IndexOf(qIndex);
            QuestIndexes[delete] = -1;
            await Data.Data.SaveQuestIndexes(Context.User.Id, string.Join(',', QuestIndexes));

            await Data.Data.SaveBalance(Context.User.Id, Program.Quests[qIndex].Baps, false);
        }*/

    }
}
