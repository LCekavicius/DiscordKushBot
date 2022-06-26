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
    public class PickPocket : ModuleBase<SocketCommandContext>
    {
        [Command("Yoink"), Alias("Pickpocket", "PP")]
        public async Task PickTarget()
        {
            double YoinkCd = 30 - (Data.Data.GetPetLevel(Context.User.Id, 4) / 3);

            if (Data.Data.GetLastYoink(Context.User.Id).AddHours(1).AddMinutes(YoinkCd) > DateTime.Now)
            {
                TimeSpan timeLeft = Data.Data.GetLastYoink(Context.User.Id).AddHours(1).AddMinutes(YoinkCd) - DateTime.Now;
                await ReplyAsync($"<:hangg:945706193358295052> {Context.User.Mention} You still Have to wait {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2} to yoink again, you sadistic jew <:gana:945781528699474053>");
                return;
            }
            else
            {
                await ReplyAsync($"{Context.User.Id} your PP is ready");
            }
        }

        [Command("Yoink"), Alias("Pickpocket", "PP")]
        public async Task PickTarget(IUser user)
        {
            
            if (Data.Data.GetPetLevel(Context.User.Id, 4) == 0)
            {
                await ReplyAsync($"{Context.User.Mention} You don't even have a pet {Program.Pets[4].Name}, Dumbass cuck");
                return;
            }
            double YoinKChance = 57 + (Data.Data.GetPetLevel(Context.User.Id, 4) / 3);
            Random rad = new Random();

            if(Program.Test == Context.User.Id)
            {
                YoinKChance = 100;
                Program.Test = 0;
            }

            double JewLevel = Data.Data.GetPetLevel(Context.User.Id, 4);

            double YoinkCd = 30 - (Data.Data.GetPetLevel(Context.User.Id, 4) / 3);

            if (Data.Data.GetLastYoink(Context.User.Id).AddHours(1).AddMinutes(YoinkCd) > DateTime.Now)
            {
                TimeSpan timeLeft = Data.Data.GetLastYoink(Context.User.Id).AddHours(1).AddMinutes(YoinkCd) - DateTime.Now;
                await ReplyAsync($"<:hangg:945706193358295052> {Context.User.Mention} You still Have to wait {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2} to yoink again, you sadistic jew <:gana:945781528699474053>");
                return;
            }


            if (Data.Data.GetBalance(user.Id) < 50 + (19 + (int)JewLevel) * (1.12 + JewLevel / 100))
            {
                await ReplyAsync($"{Context.User.Mention} you tried yoinking {user.Mention} but he's too poor to even bother to");
                return;
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

            int AbuseStrength = Data.Data.GetPetAbuseStrength(Context.User.Id, 4);

            if (rad.NextDouble() * 100 > YoinKChance)
            {
                await ReplyAsync($"{Context.User.Mention} your fat pet failed to Yoink the target's baps");
                await Data.Data.SaveLastYoink(Context.User.Id, DateTime.Now.AddMinutes(-30 - AbuseStrength * (-10)));
                await Data.Data.SaveFailedYoinks(Context.User.Id, 1);
                if (Data.Data.GetFailedYoinks(Context.User.Id) >= Program.Quests[16].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(16))
                {
                    await Program.CompleteQuest(16, QuestIndexes, Context.Channel, Context.User);
                }
                return;
            }


            //yoink chance = 50 + 0.25 * level ?

            double increment = rad.NextDouble();
            while (increment > 0.4)
            {
                increment = rad.NextDouble();
            }


            //  double yoinked = rad.Next(12, 18) + (Data.Data.GetPetLevel(Context.User.Id,4) * ((1.01 + Data.Data.GetPetLevel(Context.User.Id,4) / 500) + increment));
            int lowYoink = (int)Math.Round(JewLevel / 1.2);
            double yoinked = rad.Next(13 + lowYoink, 19 + (int)JewLevel) * (1.12 + JewLevel / 100);

            yoinked = Math.Round(yoinked);

            if (yoinked > Data.Data.GetBalance(user.Id))
            {
                yoinked = Data.Data.GetBalance(user.Id);
            }

            double extraYoink = rad.NextDouble();

            while (extraYoink < 0.65 || extraYoink > 0.85)
            {
                extraYoink = rad.NextDouble();
            }

            extraYoink = Math.Round((0.55 + extraYoink) * yoinked);

            int winnings = (int)extraYoink + (int)yoinked;


            await Data.Data.SaveBalance(user.Id, (int)yoinked * -1, false);
            await Data.Data.SaveBalance(Context.User.Id, winnings, false);

            int petTier = Data.Data.GetPetTier(Context.User.Id, 4);
            double TierBenefiteChance = (double)petTier * 1.1 + 1.75 * Math.Sqrt((double)petTier);

            string TierBenefit = "";
            double roll = rad.NextDouble();

            if(roll > TierBenefiteChance / 100)
            {
                await Data.Data.SaveLastYoink(Context.User.Id, DateTime.Now.AddMinutes(AbuseStrength * (-10)));
            }
            else
            {
                TierBenefit += "\nJew's tier reset his cooldown immediately. <:pepehap:945780175415689266>";
            }

            await Data.Data.SaveSuccessfulYoinks(Context.User.Id, 1);
            await Data.Data.SaveFailedYoinks(Context.User.Id, Data.Data.GetFailedYoinks(Context.User.Id) * -1);

            await ReplyAsync($"<:ima:945342040529567795> {Context.User.Mention} Yoinked {user.Mention} for {yoinked} Baps, on the way back he found some more and got **{winnings}** in total <:clueless:945702914641510450>{TierBenefit}");

            if (Data.Data.GetBalance(Context.User.Id) >= Program.Quests[10].GetCompleteReq(Context.User.Id) && QuestIndexes.Contains(10))
            {
                await Program.CompleteQuest(10, QuestIndexes, Context.Channel, Context.User);
            }
            if (Data.Data.GetSuccessfulYoinks(Context.User.Id) >= Program.Quests[15].CompleteReq && QuestIndexes.Contains(15))
            {
                await Program.CompleteQuest(15, QuestIndexes, Context.Channel, Context.User);
            }
        }
        [Command("Yoink"), Alias("Pickpocket", "PP")]
        public async Task PickTarget(string code)
        {
            if (Data.Data.GetPetLevel(Context.User.Id, 4) == 0)
            {
                await ReplyAsync($"{Context.User.Mention} You don't even have a pet {Program.Pets[4].Name}, Dumbass cuck");
                return;
            }

            bool exist = false;
            int index = -1;

            for(int i = 0; i < Program.GivePackages.Count; i++)
            {
                if(Program.GivePackages[i].Code == code)
                {
                    exist = true;
                    index = i;
                }
            }

            if(!exist)
            {
                await ReplyAsync($"{Context.User.Mention}, the {code} package either doesn't exist or has already bean yoinked");
                return;
            }

            if(Program.GivePackages[index].Author == Context.User.Id)
            {
                await ReplyAsync($"{Context.User.Mention} You can't yoink your own package, feeling smart?");
                return;
            }

            if (Program.GivePackages[index].Recipient == Context.User.Id)
            {
                await ReplyAsync($"{Context.User.Mention} You can't yoink a package addressed to you, feeling smart?");
                return;
            }

            Random rad = new Random();
            float StealMultiplier = rad.Next(23, 32 + Data.Data.GetPetLevel(Context.User.Id,4) / 3);
            StealMultiplier /= 100;
            //double StealMultiplier = 0.5;
            double stolen = Program.GivePackages[index].Baps * StealMultiplier;
            int _stolen = (int)Math.Round(stolen);

            await ReplyAsync($"{Context.User.Mention} has succesfully yoinked the package 'code **{code}**' and stole **{_stolen}/{Program.GivePackages[index].Baps}** baps!");

            Program.GivePackages[index].Baps -= _stolen;


            await Data.Data.SaveBalance(Context.User.Id, _stolen, false);

            Program.GivePackages.RemoveAt(index);

            await Data.Data.SaveLastYoink(Context.User.Id, Data.Data.GetLastYoink(Context.User.Id).AddMinutes(25));


        }
    }
}
