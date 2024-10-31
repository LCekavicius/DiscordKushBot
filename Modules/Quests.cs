using System;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.DataClasses.enums;

namespace KushBot.Modules;

[RequirePermissions(Permissions.Core)]
public class Quests : ModuleBase<SocketCommandContext>
{
    [Command("quests"), Alias("q", "qs")]
    public async Task ShowQuests()
    {
        var user = Data.Data.GetKushBotUser(Context.User.Id, Data.UserDtoFeatures.Quests | Data.UserDtoFeatures.Pets | Data.UserDtoFeatures.Items);

        EmbedBuilder builder = new EmbedBuilder();
        builder.WithColor(Color.Gold);

        TimeSpan midnightIn = TimeHelper.MidnightIn;

        builder.AddField($"{Context.User.Username}'s Quests", GetFieldValue(user, user.UserQuests.Where(e => e.IsDaily)));

        if (!user.UserQuests.Where(e => e.IsDaily).Any(e => !e.IsCompleted))
        {
            builder.AddField("Time till new quests:", $"{midnightIn.Hours:D2}:{midnightIn.Minutes:D2}:{midnightIn.Seconds:D2} \n ~~-Upon completing **all** of today's quests you will get **{user.GetDailiesCompleteReward()}** baps~~");
        }
        else
        {
            builder.AddField("Time till new quests:", $"{midnightIn.Hours:D2}:{midnightIn.Minutes:D2}:{midnightIn.Seconds:D2} \n -Upon completing **all** of today's quests you will get **{user.GetDailiesCompleteReward()}** baps");
        }

        builder.AddField($"Weekly Quests", GetFieldValue(user, user.UserQuests.Where(e => !e.IsDaily)));

        int petlvl = user.Pets.TotalRawPetLevel;
        RarityType rarity = (RarityType)(1 + petlvl / 60);

        string itemReward = $"\nas well as a {rarity} item.";

        var mondayIn = TimeHelper.MondayIn;

        if (!user.UserQuests.Where(e => e.IsDaily).Any(e => !e.IsCompleted))
        {
            builder.AddField("Time till new weeklies:", $"{mondayIn.Days}d {midnightIn.Hours:D2}:{midnightIn.Minutes:D2}:{midnightIn.Seconds:D2} \n ~~-Upon completing weekly quests you will get a **boss ticket**{itemReward}~~");
        }
        else
        {
            builder.AddField("Time till new weeklies:", $"{mondayIn.Days}d {midnightIn.Hours:D2}:{midnightIn.Minutes:D2}:{midnightIn.Seconds:D2} \n -Upon completing weekly quests you will get a **boss ticket**{itemReward}");
        }

        //builder.AddField($"Race quest:", $"{race}");

        await ReplyAsync("", false, builder.Build());
    }

    private string GetFieldValue(KushBotUser user, IEnumerable<Quest> quests)
    {
        string print = "";
        int i = 0;

        foreach (var quest in quests)
        {
            var text = quest.IsCompleted ? $"~~{quest.GetQuestText().Replace("*", "")}~~" : quest.GetQuestText();

            print += $"{i + 1}.{text}";

            if (!quest.IsCompleted)
            {
                print += $", Reward: {quest.GetQuestReward()} baps";

                QuestRequirement chainReq = quest.Requirements.FirstOrDefault(e => e.Type == QuestRequirementType.Chain);
                QuestRequirement bapsXReq = quest.Requirements.FirstOrDefault(e => e.Type == QuestRequirementType.BapsX);
                QuestRequirement modifierXReq = quest.Requirements.FirstOrDefault(e => e.Type == QuestRequirementType.ModifierX);
                QuestRequirement countReq = quest.Requirements.FirstOrDefault(e => e.Type == QuestRequirementType.Count);
                QuestRequirement progressReq = quest.Requirements.FirstOrDefault(e => e.Type == QuestRequirementType.Win || e.Type == QuestRequirementType.Lose);

                if (chainReq != null && bapsXReq != null && int.TryParse(bapsXReq.Value, out var requirement))
                {
                    print += $", {user.UserEvents.GetChainLength(quest)}/{chainReq.Value}";
                }
                else if (countReq != null && int.TryParse(countReq.Value, out var countRequirement) && countRequirement != 1)
                {
                    int.TryParse(bapsXReq?.Value ?? "0", out var bapsRequirement);
                    int.TryParse(modifierXReq?.Value ?? "0", out var modifierRequirement);
                    var count = user.UserEvents.Count(e => quest.GetRelevantEventTypes().Contains(e.Type) && e.BapsInput >= bapsRequirement && e.Modifier >= modifierRequirement);
                    print += $", {count}/{countReq.Value}";
                }
                else if (progressReq != null)
                {
                    print += $", {user.UserEvents.Where(e => quest.GetRelevantEventTypes().Contains(e.Type)).Sum(e => (long)e.BapsChange)}/{progressReq.Value}";
                }
                i++;
            }
            else
            {
                print += ". **Quest completed**";
            }


            print += "\n";
        }

        return print;
    }
}
