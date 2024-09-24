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
                print += $"{i + 1}.{quest.GetQuestText()}, Reward: {quest.GetQuestReward()} baps";

                QuestRequirement chainReq = quest.Requirements.FirstOrDefault(e => e.Type == QuestRequirementType.Chain);
                QuestRequirement bapsXReq = quest.Requirements.FirstOrDefault(e => e.Type == QuestRequirementType.BapsX);
                QuestRequirement countReq = quest.Requirements.FirstOrDefault(e => e.Type == QuestRequirementType.Count);
                QuestRequirement progressReq = quest.Requirements.FirstOrDefault(e => e.Type == QuestRequirementType.Win || e.Type == QuestRequirementType.Lose);

                if (chainReq != null && bapsXReq != null && int.TryParse(bapsXReq.Value, out var requirement))
                {
                    print += $", {user.UserEvents.GetLongestSequence(quest.GetMatchingEventType() ?? UserEventType.None, requirement)}/{chainReq.Value}";
                } 
                else if (countReq != null && bapsXReq != null && int.TryParse(countReq.Value, out var countRequirement) && int.TryParse(bapsXReq.Value, out var bapsRequirement))
                {
                    print += $", {user.UserEvents.Count(e => quest.GetRelevantEventTypes().Contains(e.Type) && e.BapsInput >= bapsRequirement)}/{countReq.Value}";
                }
                else if (progressReq != null)
                {
                    print += $", {user.UserEvents.Where(e => quest.GetRelevantEventTypes().Contains(e.Type)).Sum(e => e.BapsChange)}/{progressReq.Value}";
                }
            }

            print += "\n";
        }

        TimeSpan NextMn = TimeHelper.MidnightIn;

        builder.AddField($"{Context.User.Username}'s Quests", $"{print}");


        if (!user.UserQuests.Any(e => !e.IsCompleted))
        {
            builder.AddField("Time till new quests:", $"{NextMn.Hours:D2}:{NextMn.Minutes:D2}:{NextMn.Seconds:D2} \n ~~--Upon completing **all** of today's quests you will get **{user.GetFullQuestCompleteReward()}** baps~~");
        }
        else
        {
            builder.AddField("Time till new quests:", $"{NextMn.Hours:D2}:{NextMn.Minutes:D2}:{NextMn.Seconds:D2} \n --Upon completing **all** of today's quests you will get **{user.GetFullQuestCompleteReward()}** baps");
        }

        string weeklyPrint = "";

        int petlvl = user.Pets.TotalCombinedPetLevel;
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

        //builder.AddField($"Weekly quests:", $"{weeklyPrint}\n--Upon completing weekly quests you will get a **boss ticket**{Reward}");
        //builder.AddField($"Race quest:", $"{race}");

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
