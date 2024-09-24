using System;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.DataClasses.Enums;

namespace KushBot.Modules;

public class Quests : ModuleBase<SocketCommandContext>
{
    [Command("quests"), Alias("q", "qs")]
    public async Task ShowQuests()
    {
        var user = Data.Data.GetKushBotUser(Context.User.Id, Data.UserDtoFeatures.Quests | Data.UserDtoFeatures.Pets | Data.UserDtoFeatures.Items);

        EmbedBuilder builder = new EmbedBuilder();
        builder.WithColor(Color.Gold);

        string print = "";

        int petLvl = user.Pets[PetType.Maybich]?.CombinedLevel ?? 0;

        double _BapsFromPet = Math.Pow(petLvl, 1.3) + petLvl * 3;

        int BapsFromPet = (int)Math.Round(_BapsFromPet);

        int i = 0;

        foreach (var quest in user.UserQuests)
        {
            if (quest.IsCompleted)
            {
                print += $"{i + 1} Quest completed! Wait for new quests";
            }
            else
            {
                print += $"{i + 1}.{quest.GetQuestText()}, Reward: {quest.GetQuestReward(user)} baps";

                QuestRequirement chainReq = quest.Requirements.FirstOrDefault(e => e.Type == QuestRequirementType.Chain);
                QuestRequirement progressReq = quest.Requirements.FirstOrDefault(e => e.Type == QuestRequirementType.Win || e.Type == QuestRequirementType.Lose);

                if (chainReq != null && progressReq != null && int.TryParse(progressReq.Value, out var requirement))
                {
                    print += $", {user.UserEvents.GetLongestSequence(quest.GetMatchingEventType() ?? UserEventType.None, requirement)}/{chainReq.Value}";
                }
                else
                {
                    if (progressReq != null)
                    {
                        print += $", {user.UserEvents.Where(e => quest.GetRelevantEventTypes().Contains(e.Type)).Sum(e => e.Amount)}/{progressReq.Value}";
                    }
                }
            }

            //TODO add progress display (x/y)
            print += "\n";
        }

        TimeSpan NextMn = TimeHelper.MidnightIn;

        builder.AddField($"{Context.User.Username}'s Quests", $"{print}");


        if (!user.UserQuests.Any(e => !e.IsCompleted))
        {
            builder.AddField("Time till new quests:", $"{NextMn.Hours:D2}:{NextMn.Minutes:D2}:{NextMn.Seconds:D2} \n ~~--Upon completing **all** of today's quests you will get **{DiscordBotService.RewardForFullQuests + (int)Math.Round(_BapsFromPet * 1.9)}** baps~~");
        }
        else
        {
            builder.AddField("Time till new quests:", $"{NextMn.Hours:D2}:{NextMn.Minutes:D2}:{NextMn.Seconds:D2} \n --Upon completing **all** of today's quests you will get **{DiscordBotService.RewardForFullQuests + (int)Math.Round(_BapsFromPet * 1.9)}** baps");
        }

        string weeklyPrint = "";

        List<int> weeklyQuests = Data.Data.GetWeeklyQuest();

        //string tempSS = $"1.{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetDesc(Context.User.Id)}," +
        //    $" Reward: {(int)((bapsPercent / 200 + 1) * (DiscordBotService.WeeklyQuests[weeklyQuests[0]].Baps + bapsFlat + WeeklyQPetBonus(Context.User.Id, 0)))} baps";
        //string weeklyPrintFirst = "";

        //switch (weeklyQuests[0])
        //{
        //    case 0:
        //        weeklyPrintFirst += $", {Data.Data.GetWonBapsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 1:
        //        weeklyPrintFirst += $", {Data.Data.GetLostBapsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 2:
        //        weeklyPrintFirst += $", {Data.Data.GetWonBapsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 3:
        //        weeklyPrintFirst += $", {Data.Data.GetLostBapsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 4:
        //        weeklyPrintFirst += $", {Data.Data.GetWonFlipsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 5:
        //        weeklyPrintFirst += $", {Data.Data.GetLostFlipsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 6:
        //        weeklyPrintFirst += $", {Data.Data.GetWonFlipsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 7:
        //        weeklyPrintFirst += $", {Data.Data.GetLostFlipsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 8:
        //        weeklyPrintFirst += $", {Data.Data.GetWonBetsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 9:
        //        weeklyPrintFirst += $", {Data.Data.GetLostBetsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 10:
        //        weeklyPrintFirst += $", {Data.Data.GetWonBetsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 11:
        //        weeklyPrintFirst += $", {Data.Data.GetLostBetsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 12:
        //        weeklyPrintFirst += $", {Data.Data.GetWonRisksWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 13:
        //        weeklyPrintFirst += $", {Data.Data.GetLostRisksWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 14:
        //        weeklyPrintFirst += $", {Data.Data.GetWonRisksWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 15:
        //        weeklyPrintFirst += $", {Data.Data.GetLostRisksWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 16:
        //        weeklyPrintFirst += $", {Data.Data.GetBegsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[0]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //}

        bool isFirstWeeklyFinished = false;
        bool isSecondWeeklyFinished = false;

        if (isFirstWeeklyFinished)
        {
            weeklyPrint += "1. Quest completed! Wait for new quests\n";
        }
        else
        {
            //weeklyPrint += tempSS + weeklyPrintFirst + "\n";
        }

        string tempSSS = $"2.{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetDesc(Context.User.Id)}, Reward: {DiscordBotService.WeeklyQuests[weeklyQuests[1]].Baps + WeeklyQPetBonus(Context.User.Id, 1)} baps";
        string weeklyPrintSecond = "";


        //switch (weeklyQuests[1])
        //{
        //    case 0:
        //        weeklyPrintSecond += $", {Data.Data.GetWonBapsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 1:
        //        weeklyPrintSecond += $", {Data.Data.GetLostBapsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 2:
        //        weeklyPrintSecond += $", {Data.Data.GetWonBapsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 3:
        //        weeklyPrintSecond += $", {Data.Data.GetLostBapsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 4:
        //        weeklyPrintSecond += $", {Data.Data.GetWonFlipsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 5:
        //        weeklyPrintSecond += $", {Data.Data.GetLostFlipsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 6:
        //        weeklyPrintSecond += $", {Data.Data.GetWonFlipsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 7:
        //        weeklyPrintSecond += $", {Data.Data.GetLostFlipsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 8:
        //        weeklyPrintSecond += $", {Data.Data.GetWonBetsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 9:
        //        weeklyPrintSecond += $", {Data.Data.GetLostBetsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 10:
        //        weeklyPrintSecond += $", {Data.Data.GetWonBetsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 11:
        //        weeklyPrintSecond += $", {Data.Data.GetLostBetsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 12:
        //        weeklyPrintSecond += $", {Data.Data.GetWonRisksWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 13:
        //        weeklyPrintSecond += $", {Data.Data.GetLostRisksWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 14:
        //        weeklyPrintSecond += $", {Data.Data.GetWonRisksWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 15:
        //        weeklyPrintSecond += $", {Data.Data.GetLostRisksWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //    case 16:
        //        weeklyPrintSecond += $", {Data.Data.GetBegsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[1]].GetCompleteReq(Context.User.Id)}";
        //        break;
        //}

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

        string race = "s";

        if (weeklyQuests[2] == -1)
        {
            if (DiscordBotService.RaceFinisher == 0)
            {
                race += $"Someone has finished the race quest before you :)";
            }
            else
            {
                race += $"Race quest finished by: {DiscordBotService._client.GetUser(DiscordBotService.RaceFinisher).Username}";
            }
        }
        else
        {
            //race += $"{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetDesc(Context.User.Id)}," +
            //    $" Reward: {(int)((bapsPercent / 200 + 1) * (bapsFlat + DiscordBotService.WeeklyQuests[weeklyQuests[2]].Baps + WeeklyQPetBonus(Context.User.Id, 2)))} baps";
            //switch (weeklyQuests[2])
            //{
            //    case 0:
            //        race += $", {Data.Data.GetWonBapsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 1:
            //        race += $", {Data.Data.GetLostBapsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 2:
            //        race += $", {Data.Data.GetWonBapsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 3:
            //        race += $", {Data.Data.GetLostBapsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 4:
            //        race += $", {Data.Data.GetWonFlipsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 5:
            //        race += $", {Data.Data.GetLostFlipsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 6:
            //        race += $", {Data.Data.GetWonFlipsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 7:
            //        race += $", {Data.Data.GetLostFlipsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 8:
            //        race += $", {Data.Data.GetWonBetsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 9:
            //        race += $", {Data.Data.GetLostBetsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 10:
            //        race += $", {Data.Data.GetWonBetsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 11:
            //        race += $", {Data.Data.GetLostBetsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 12:
            //        race += $", {Data.Data.GetWonRisksWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 13:
            //        race += $", {Data.Data.GetLostRisksWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 14:
            //        race += $", {Data.Data.GetWonRisksWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 15:
            //        race += $", {Data.Data.GetLostRisksWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //    case 16:
            //        race += $", {Data.Data.GetBegsWeekly(Context.User.Id)}/{DiscordBotService.WeeklyQuests[weeklyQuests[2]].GetCompleteReq(Context.User.Id)}\n";
            //        break;
            //}
        }

        int petlvl = DiscordBotService.GetTotalPetLvl(Context.User.Id);
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

    int WeeklyQPetBonus(ulong userId, int qIndex)
    {
        List<int> weeklyQuests = Data.Data.GetWeeklyQuest();

        int BapsFromPet = 0;

        var pet = Data.Data.GetUserPets(userId)[PetType.Maybich];

        if (pet != null)
        {
            double _BapsFromPet = (Math.Pow(pet.CombinedLevel, 1.3) + pet.CombinedLevel * 3) + (DiscordBotService.WeeklyQuests[weeklyQuests[qIndex]].Baps / 100 * pet.CombinedLevel);
            BapsFromPet = (int)Math.Round(_BapsFromPet);
        }
        else
        {
            BapsFromPet = 0;
        }

        return BapsFromPet;
    }

    //string AppendQuestString(List<int> qsIndexes, int i)
    //{
    //    string temp = "";

    //    temp += $"{Data.Data.GetWonBapsMN(Context.User.Id)}/{DiscordBotService.Quests[qsIndexes[i]].GetCompleteReq(Context.User.Id)}";

    //    return temp;
    //}
    int PercentageReward(int reward, int petLvl)
    {
        double temp = reward / 100;
        temp *= petLvl;
        int hold = (int)Math.Round(temp);

        return hold;
    }

}
