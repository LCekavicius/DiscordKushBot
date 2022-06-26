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

            int minuteCut = (int)Math.Pow(Data.Data.GetPetTier(Context.User.Id, 1), 1.3) * 12;
            TimeSpan PetBuff = new TimeSpan(0,minuteCut,0);

            int petAbuseBonus = Data.Data.GetPetAbuseStrength(Context.User.Id, 1);
            TimeSpan tpab = new TimeSpan();
            TimeSpan pab = new TimeSpan();
            if (petAbuseBonus > 0)
            {
                tpab = (new TimeSpan(24, 0, 0) - PetBuff);
                pab = new TimeSpan(0, (int)(tpab.TotalMinutes / (1 + petAbuseBonus)), 0);
            }

            //CHECK THIS POTENTIAL BUG ni****
            if (lastDestroy.AddHours(24) - PetBuff - pab > DateTime.Now)
            {
                TimeSpan timeLeft = lastDestroy.AddHours(24) - DateTime.Now;
                await ReplyAsync($"<:egg:945783802867879987> {Context.User.Mention} Your Pinata is still growing, you still need to wait: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2} <:zltr:945780861662556180>");
                return;
            }

            Random rad = new Random();
            int sum = rad.Next(100,141);
            for(int i = 0; i < Data.Data.GetPetLevel(Context.User.Id,1); i++)
            {
                sum += rad.Next(18,28);
            }
            sum += Data.Data.GetPetLevel(Context.User.Id, 1) * 15;


            await Data.Data.SaveLastDestroy(Context.User.Id, DateTime.Now - PetBuff - pab);

            await ReplyAsync($"{Context.User.Mention} You destroyed your pinata and got {sum} Baps, the pinata starts growing again.");
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
