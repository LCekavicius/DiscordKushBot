using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KushBot.Data;

namespace KushBot.Modules
{
    public class Destroy : ModuleBase<SocketCommandContext>
    {
        [Command("destroy"), Alias("pinata","d")]
        public async Task DestroyPinatac()
        {
            if (!exists(Data.Data.GetPets(Context.User.Id), 1))
            {
                await ReplyAsync($"{Context.User.Mention} dipshit doesnt even have a pinata pet");
                return;
            }

            DateTime lastDestroy = Data.Data.GetLastDestroy(Context.User.Id);

            int petAbuseBonus = Data.Data.GetPetAbuseStrength(Context.User.Id, 1);
            TimeSpan tpab = new TimeSpan();
            TimeSpan pab = new TimeSpan();
            if (petAbuseBonus > 0)
            {
                tpab = (new TimeSpan(22, 0, 0));
                pab = new TimeSpan(0, (int)(tpab.TotalMinutes / (1 + petAbuseBonus)), 0);
            }

            //CHECK THIS POTENTIAL BUG ni****
            if (lastDestroy.AddHours(22) - pab > DateTime.Now)
            {
                TimeSpan timeLeft = lastDestroy.AddHours(22) - DateTime.Now;
                await ReplyAsync($"<:egg:945783802867879987> {Context.User.Mention} Your Pinata is still growing, you still need to wait: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2} <:zltr:945780861662556180>");
                return;
            }

            bool isCdReset = false;

            int petTier = Data.Data.GetPetTier(Context.User.Id, 1);
            double tierFloat = (double)petTier;

            Random rad = new Random();

            if(rad.NextDouble() < (tierFloat * 2) / 100)
            {
                isCdReset = true;
            }

            int sum = rad.Next(100,141);
            int petLvl = Data.Data.GetPetLevel(Context.User.Id, 1);

            for (int i = 0; i < petLvl; i++)
            {
                sum += rad.Next(18,29);
            }
            
            sum += petLvl * 14;

            sum += (int)(sum * (((double)petLvl) / 100));

            if (!isCdReset)
            {
                await Data.Data.SaveLastDestroy(Context.User.Id, DateTime.Now - pab);
            }

            string cdResetText = isCdReset ? "\nWtf? The tier bonus proc'ed and the pinata restored itself immediately!" : "";

            await ReplyAsync($"{Context.User.Mention} You destroyed your pinata and got {sum} Baps, the pinata starts growing again.{cdResetText}");
            await Data.Data.SaveBalance(Context.User.Id,sum, false);

            List<int> QuestIndexes = new List<int>();
            #region assignment
            string hold = Data.Data.GetQuestIndexes(Context.User.Id);
            string[] values = hold.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                QuestIndexes.Add(int.Parse(values[i]));
            }
            #endregion

            if (Data.Data.GetBalance(Context.User.Id) >= Program.Quests[10].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(10))
            {
                await Program.CompleteQuest(10, QuestIndexes, Context.Channel, Context.User);
            }


        }

        public bool exists(string text, int match)
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
    }
}
