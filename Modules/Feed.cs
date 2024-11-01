using Discord.Commands;
using System;
using System.Threading.Tasks;
using KushBot.Data;
using KushBot.DataClasses;
using KushBot.Global;
using System.Linq;
using KushBot.DataClasses.Enums;
using KushBot.Resources.Database;

namespace KushBot.Modules;

public class Feed(SqliteDbContext dbContext, TutorialManager tutorialManager) : ModuleBase<SocketCommandContext>
{
    [Command("Feed")]
    [RequirePermissions(Permissions.Core)]
    public async Task Level([Remainder] string input)
    {
        var petType = PetType.SuperNed;

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

        var user = await dbContext.GetKushBotUserAsync(Context.User.Id, UserDtoFeatures.Pets | UserDtoFeatures.Quests);

        if ((int)petType < 0 || (int)petType > Global.Pets.All.Count)
        {
            await ReplyAsync($"{Context.User.Mention} No such pet + ur jeet");
            return;
        }

        if (!user.Pets.ContainsKey(petType))
        {
            await ReplyAsync($"{Context.User.Mention}, you don't have the {Global.Pets.Dictionary[petType].Name} pet, Dumb fuck...");
            return;
        }

        int petLevel = user.Pets[petType].Level;

        if (petLevel >= 99)
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

        user.Pets[petType].Level += 1;
        await tutorialManager.AttemptSubmitStepCompleteAsync(user, 4, 0, Context.Channel);

        user.Balance -= nextFeedCost;

        if (!user.UserEvents.Any(e => e.Type == UserEventType.Feed))
        {
            user.AddUserEvent(UserEventType.Feed);
            var result = user.AttemptCompleteQuests();
            await tutorialManager.AttemptCompleteQuestSteps(user, Context.Channel, result);
            await Context.CompleteQuestsAsync(result.freshCompleted, result.lastDailyCompleted, result.lastWeeklyCompleted);
        }

        await dbContext.SaveChangesAsync();
        await ReplyAsync($"{Context.User.Mention} You have fed your **{Global.Pets.Dictionary[petType].Name}** {nextFeedCost} baps and it's now level **{user.Pets[petType].Level}**");
    }

    int CalculateDifference(string str1, string str2)
    {
        int lengthDifference = Math.Abs(str1.Length - str2.Length);

        int charDifference = str1.Zip(str2, (c1, c2) => c1 != c2 ? 1 : 0).Sum();

        return charDifference + lengthDifference;
    }
}
