using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KushBot.Data;
using KushBot.DataClasses;
using KushBot.Global;
using System.Linq;

namespace KushBot.Modules
{
    public class Feed : ModuleBase<SocketCommandContext>
    {
        [Command("Feed")]
        public async Task Level([Remainder] string input)
        {
            PetType petType = PetType.SuperNed;

            if (!Enum.TryParse(input, out petType))
            {
                var closestPet = Global.Pets.All
                    .Select(pet => new
                    {
                        Pet = pet,
                        Difference = CalculateDifference(pet.Name.ToLower(), input.ToLower())
                    })
                    .MinBy(e => e.Difference);

                PetType? type = (closestPet != null && closestPet.Difference <= 4) ? closestPet.Pet.Type : null;

                if (type is null)
                {
                    await ReplyAsync($"{Context.User.Mention} No such pet + ur black");
                    return;
                }
                else
                {
                    petType = type.Value;
                }
            }

            var user = Data.Data.GetKushBotUser(Context.User.Id, UserDtoFeatures.Pets);

            if((int)petType < 0 || (int)petType > Global.Pets.All.Count)
            {
                await ReplyAsync($"{Context.User.Mention} No such pet + ur jeet");
                return;
            }

            if (!user.Pets2.ContainsKey(petType))
            {
                await ReplyAsync($"{Context.User.Mention}, you don't have the {Global.Pets.Dictionary[petType].Name} pet, Dumb fuck...");
                return;
            }

            int petLevel = user.Pets2[petType].Level;
            int itemPetLevel = Data.Data.GetItemPetLevel(Context.User.Id, (int)petType);

            if (petLevel - itemPetLevel == 99)
            {
                await ReplyAsync($"{Context.User.Mention} Your pet is already level 99 {CustomEmojis.Gana}");
                return;
            }

            int nextFeedCost = Global.Pets.GetNextFeedCost(petLevel);

            if (nextFeedCost > user.Balance)
            {
                await ReplyAsync($"{Context.User.Mention} Can't even buy proper food for his pet, fucking loser");
                return;
            }

            user.Pets2[petType].Level += 1;
            await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 4, 0, Context.Channel);
            await ReplyAsync($"{Context.User.Mention} You have fed your **{Global.Pets.Dictionary[petType].Name}** {nextFeedCost} baps and it's now level **{user.Pets2[petType].Level}**");

            user.Balance -= nextFeedCost;

            await Data.Data.SaveKushBotUserAsync(user, UserDtoFeatures.Pets);

            List<int> QuestIndexes = new List<int>();
            #region Assignment
            string hold = Data.Data.GetQuestIndexes(Context.User.Id);
            string[] values = hold.Split(',');

            for (int i = 0; i < values.Length; i++)
            {
                QuestIndexes.Add(int.Parse(values[i]));
            }
            #endregion


            if (QuestIndexes.Contains(13))
            {
                await Program.CompleteQuest(13, QuestIndexes, Context.Channel, Context.User);
            }

        }

        int CalculateDifference(string str1, string str2)
        {
            int lengthDifference = Math.Abs(str1.Length - str2.Length);

            int charDifference = str1.Zip(str2, (c1, c2) => c1 != c2 ? 1 : 0).Sum();

            return charDifference + lengthDifference;
        }
    }
}
