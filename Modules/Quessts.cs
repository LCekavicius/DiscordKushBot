using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using KushBot.DataClasses;
using System.Threading.Channels;

namespace KushBot.Modules
{
    public class Quests : ModuleBase<SocketCommandContext>
    {
        [Command("quests"), Alias("q", "qs")]
        public async Task DoQuests()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(Color.Gold);

            List<int> QuestsIndexes = new List<int>();

            string[] values;
            string temp;

            QuestsIndexes.Sort();

            temp = Data.Data.GetQuestIndexes(Context.User.Id);
            values = temp.Split(',');

            for (int i = 0; i < values.Count(); i++)
            {
                QuestsIndexes.Add(int.Parse(values[i]));
            }

            string print = "";

            int BapsFromPet;

            var userPets = Data.Data.GetUserPets(Context.User.Id);

            int petLvl = userPets[PetType.Maybich]?.Level ?? 0;

            double _BapsFromPet = Math.Pow(petLvl, 1.3) + petLvl * 3;

            BapsFromPet = (int)Math.Round(_BapsFromPet);

            //BapsFromPet += (int)(BapsFromPet * (((double)petLvl) / 100));

            //Items
            int bapsFlat = 0;
            double bapsPercent = 0;
            List<Item> items = Data.Data.GetUserItems(Context.User.Id);
            List<int> equiped = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                equiped.Add(Data.Data.GetEquipedItem(Context.User.Id, i + 1));
                if (equiped[i] != 0)
                {
                    Item item = items.Where(x => x.Id == equiped[i]).FirstOrDefault();
                    if (item.QuestBapsFlat != 0)
                    {
                        bapsFlat += item.QuestBapsFlat;
                    }
                    if (item.QuestBapsPercent != 0)
                    {
                        bapsPercent += item.QuestBapsPercent;
                    }
                }
            }

            for (int i = 0; i < QuestsIndexes.Count; i++)
            {

                if (QuestsIndexes[i] == -1)
                {
                    print += $"{i + 1} Quest completed! Wait for new quests\n";
                    continue;
                }

                print += i + 1 + "." + Program.Quests[QuestsIndexes[i]].GetDesc(Context.User.Id)
                    + $", Reward: {(int)((bapsPercent / 100 + 1) * (Program.Quests[QuestsIndexes[i]].Baps + bapsFlat + BapsFromPet + PercentageReward(Program.Quests[QuestsIndexes[i]].Baps, petLvl)))} baps  ";
                switch (QuestsIndexes[i])
                {
                    case 0:
                        //print += $", {Data.Data.GetWonBapsMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].CompleteReq}";
                        print += $", {Data.Data.GetWonBapsMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 1:
                        print += $", {Data.Data.GetLostBapsMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 2:
                        print += $", {Data.Data.GetWonFlipsMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 3:
                        print += $", {Data.Data.GetLostFlipsMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 4:
                        print += $", {Data.Data.GetWonBetsMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 5:
                        print += $", {Data.Data.GetLostBetsMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 6:
                        print += $", {Data.Data.GetWonRisksMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 7:
                        print += $", {Data.Data.GetLostRisksMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 10:
                        print += $", {Data.Data.GetBalance(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 11:
                        print += $", {Data.Data.GetBegsMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 12:
                        print += $", {Data.Data.GetBegsMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 15:
                        print += $", {Data.Data.GetSuccessfulYoinks(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 16:
                        print += $", {Data.Data.GetFailedYoinks(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 17:
                        print += $", {Data.Data.GetWonFlipsChain(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 22:
                        print += $", {Data.Data.GetWonFlipsMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 23:
                        print += $", {Data.Data.GetWonBetsMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 24:
                        print += $", {Data.Data.GetWonRisksMN(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 25:
                        print += $", {Data.Data.GetWonDuelsMn(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    case 26:
                        print += $", {Data.Data.GetWonDuelsMn(Context.User.Id)}/{Program.Quests[QuestsIndexes[i]].GetCompleteReq(Context.User.Id)}";
                        break;
                    default:
                        break;
                }
                print += "\n";
            }

            DateTime now = DateTime.Now;
            DateTime tomorrow = now.AddDays(1);
            DateTime nextMidnight = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 0, 0, 0);
            TimeSpan NextMn = nextMidnight - now;

            builder.AddField($"{Context.User.Username}'s Quests", $"{print}");


            bool fullComplete = true;
            foreach (int quest in QuestsIndexes)
            {
                if (quest != -1)
                {
                    fullComplete = false;
                }
            }

            if (!fullComplete)
                builder.AddField("Time till new quests:", $"{NextMn.Hours:D2}:{NextMn.Minutes:D2}:{NextMn.Seconds:D2} \n --Upon completing **all** of today's quests you will get **{Program.RewardForFullQuests + (int)Math.Round(_BapsFromPet * 1.9)}** baps");
            else
                builder.AddField("Time till new quests:", $"{NextMn.Hours:D2}:{NextMn.Minutes:D2}:{NextMn.Seconds:D2} \n ~~--Upon completing **all** of today's quests you will get **{Program.RewardForFullQuests + (int)Math.Round(_BapsFromPet * 1.9)}** baps~~");

            string weeklyPrint = "";

            List<int> weeklyQuests = Data.Data.GetWeeklyQuest();

            string tempSS = $"1.{Program.WeeklyQuests[weeklyQuests[0]].GetDesc(Context.User.Id)}," +
                $" Reward: {(int)((bapsPercent / 200 + 1) * (Program.WeeklyQuests[weeklyQuests[0]].Baps + bapsFlat + WeeklyQPetBonus(Context.User.Id, 0)))} baps";
            string weeklyPrintFirst = "";

            switch (weeklyQuests[0])
            {
                case 0:
                    weeklyPrintFirst += $", {Data.Data.GetWonBapsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 1:
                    weeklyPrintFirst += $", {Data.Data.GetLostBapsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 2:
                    weeklyPrintFirst += $", {Data.Data.GetWonBapsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 3:
                    weeklyPrintFirst += $", {Data.Data.GetLostBapsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 4:
                    weeklyPrintFirst += $", {Data.Data.GetWonFlipsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 5:
                    weeklyPrintFirst += $", {Data.Data.GetLostFlipsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 6:
                    weeklyPrintFirst += $", {Data.Data.GetWonFlipsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 7:
                    weeklyPrintFirst += $", {Data.Data.GetLostFlipsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 8:
                    weeklyPrintFirst += $", {Data.Data.GetWonBetsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 9:
                    weeklyPrintFirst += $", {Data.Data.GetLostBetsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 10:
                    weeklyPrintFirst += $", {Data.Data.GetWonBetsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 11:
                    weeklyPrintFirst += $", {Data.Data.GetLostBetsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 12:
                    weeklyPrintFirst += $", {Data.Data.GetWonRisksWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 13:
                    weeklyPrintFirst += $", {Data.Data.GetLostRisksWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 14:
                    weeklyPrintFirst += $", {Data.Data.GetWonRisksWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 15:
                    weeklyPrintFirst += $", {Data.Data.GetLostRisksWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 16:
                    weeklyPrintFirst += $", {Data.Data.GetBegsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
                    break;
            }

            bool isFirstWeeklyFinished = FinishedWeekly(0);
            bool isSecondWeeklyFinished = FinishedWeekly(1);

            if (isFirstWeeklyFinished)
            {
                weeklyPrint += "1. Quest completed! Wait for new quests\n";
            }
            else
            {
                weeklyPrint += tempSS + weeklyPrintFirst + "\n";
            }

            string tempSSS = $"2.{Program.WeeklyQuests[weeklyQuests[1]].GetDesc(Context.User.Id)}, Reward: {Program.WeeklyQuests[weeklyQuests[1]].Baps + WeeklyQPetBonus(Context.User.Id, 1)} baps";
            string weeklyPrintSecond = "";


            switch (weeklyQuests[1])
            {
                case 0:
                    weeklyPrintSecond += $", {Data.Data.GetWonBapsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 1:
                    weeklyPrintSecond += $", {Data.Data.GetLostBapsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 2:
                    weeklyPrintSecond += $", {Data.Data.GetWonBapsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 3:
                    weeklyPrintSecond += $", {Data.Data.GetLostBapsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 4:
                    weeklyPrintSecond += $", {Data.Data.GetWonFlipsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 5:
                    weeklyPrintSecond += $", {Data.Data.GetLostFlipsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 6:
                    weeklyPrintSecond += $", {Data.Data.GetWonFlipsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 7:
                    weeklyPrintSecond += $", {Data.Data.GetLostFlipsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 8:
                    weeklyPrintSecond += $", {Data.Data.GetWonBetsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 9:
                    weeklyPrintSecond += $", {Data.Data.GetLostBetsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 10:
                    weeklyPrintSecond += $", {Data.Data.GetWonBetsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 11:
                    weeklyPrintSecond += $", {Data.Data.GetLostBetsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 12:
                    weeklyPrintSecond += $", {Data.Data.GetWonRisksWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 13:
                    weeklyPrintSecond += $", {Data.Data.GetLostRisksWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 14:
                    weeklyPrintSecond += $", {Data.Data.GetWonRisksWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 15:
                    weeklyPrintSecond += $", {Data.Data.GetLostRisksWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
                case 16:
                    weeklyPrintSecond += $", {Data.Data.GetBegsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
                    break;
            }

            if (isSecondWeeklyFinished)
            {
                weeklyPrint += "2. Quest completed! Wait for new quests\n";
            }
            else
            {
                weeklyPrint += tempSSS + weeklyPrintSecond + "\n";
            }

            if (isFirstWeeklyFinished && isSecondWeeklyFinished)
            {
                await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 3, 0, Context.Channel);
            }

            string race = "";

            if (weeklyQuests[2] == -1)
            {
                if (Program.RaceFinisher == 0)
                {
                    race += $"Someone has finished the race quest before you :)";
                }
                else
                {
                    race += $"Race quest finished by: {Program._client.GetUser(Program.RaceFinisher).Username}";
                }
            }
            else
            {
                race += $"{Program.WeeklyQuests[weeklyQuests[2]].GetDesc(Context.User.Id)}," +
                    $" Reward: {(int)((bapsPercent / 200 + 1) * (bapsFlat + Program.WeeklyQuests[weeklyQuests[2]].Baps + WeeklyQPetBonus(Context.User.Id, 2)))} baps";
                switch (weeklyQuests[2])
                {
                    case 0:
                        race += $", {Data.Data.GetWonBapsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 1:
                        race += $", {Data.Data.GetLostBapsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 2:
                        race += $", {Data.Data.GetWonBapsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 3:
                        race += $", {Data.Data.GetLostBapsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 4:
                        race += $", {Data.Data.GetWonFlipsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 5:
                        race += $", {Data.Data.GetLostFlipsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 6:
                        race += $", {Data.Data.GetWonFlipsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 7:
                        race += $", {Data.Data.GetLostFlipsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 8:
                        race += $", {Data.Data.GetWonBetsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 9:
                        race += $", {Data.Data.GetLostBetsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 10:
                        race += $", {Data.Data.GetWonBetsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 11:
                        race += $", {Data.Data.GetLostBetsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 12:
                        race += $", {Data.Data.GetWonRisksWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 13:
                        race += $", {Data.Data.GetLostRisksWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 14:
                        race += $", {Data.Data.GetWonRisksWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 15:
                        race += $", {Data.Data.GetLostRisksWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                    case 16:
                        race += $", {Data.Data.GetBegsWeekly(Context.User.Id)}/{Program.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
                        break;
                }
            }

            int petlvl = Program.GetTotalPetLvl(Context.User.Id);
            int rarity = 1;

            if (petlvl > 240)
                rarity = 5;
            else if (petlvl > 180)
                rarity = 4;
            else if (petlvl > 120)
                rarity = 3;
            else if (petlvl > 60)
                rarity = 2;

            string rarityString;
            switch (rarity)
            {
                case 5:
                    rarityString = "Legendary";
                    break;
                case 4:
                    rarityString = "Epic";
                    break;
                case 3:
                    rarityString = "Rare";
                    break;
                case 2:
                    rarityString = "Uncommon";
                    break;
                default:
                    rarityString = "Common";
                    break;
            }

            string Reward = $"\nas well as a {rarityString} item.";

            builder.AddField($"Weekly quests:", $"{weeklyPrint}\n--Upon completing weekly quests you will get a **boss ticket**{Reward}");
            builder.AddField($"Race quest:", $"{race}");

            await ReplyAsync("", false, builder.Build());
        }


        bool FinishedWeekly(int id)
        {
            if (Data.Data.GetCompletedWeekly(Context.User.Id, id) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        int WeeklyQPetBonus(ulong userId, int qIndex)
        {
            List<int> weeklyQuests = Data.Data.GetWeeklyQuest();

            int BapsFromPet = 0;

            var pet = Data.Data.GetUserPets(userId)[PetType.Maybich];

            if (pet != null)
            {
                double _BapsFromPet = (Math.Pow(pet.CombinedLevel, 1.3) + pet.CombinedLevel * 3) + (Program.WeeklyQuests[weeklyQuests[qIndex]].Baps / 100 * pet.CombinedLevel);
                BapsFromPet = (int)Math.Round(_BapsFromPet);
            }
            else
            {
                BapsFromPet = 0;
            }

            return BapsFromPet;
        }

        string AppendQuestString(List<int> qsIndexes, int i)
        {
            string temp = "";

            temp += $"{Data.Data.GetWonBapsMN(Context.User.Id)}/{Program.Quests[qsIndexes[i]].GetCompleteReq(Context.User.Id)}";

            return temp;
        }
        int PercentageReward(int reward, int petLvl)
        {
            double temp = reward / 100;
            temp *= petLvl;
            int hold = (int)Math.Round(temp);

            return hold;
        }

    }
}
