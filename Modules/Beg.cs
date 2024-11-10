using Discord.Commands;
using System;
using System.Threading.Tasks;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.DataClasses.Enums;
using KushBot.Resources.Database;
using System.Linq;
using KushBot.Services;

namespace KushBot.Modules;

public class Beg(SqliteDbContext dbContext, TutorialManager tutorialManager) : ModuleBase<SocketCommandContext>
{
    [Command("beg")]
    [RequirePermissions(Permissions.Core)]
    public async Task PingAsync()
    {
        var user = await dbContext.GetKushBotUserAsync(Context.User.Id, UserDtoFeatures.Pets | UserDtoFeatures.Quests);
        var lastBeg = user.LastBeg;

        var stepCompleted = await tutorialManager.AttemptSubmitStepCompleteAsync(user, 1, 1, Context.Channel);

        if (lastBeg.AddHours(1) > DateTime.Now)
        {
            if (stepCompleted)
            {
                await dbContext.SaveChangesAsync();
            }

            TimeSpan timeLeft = lastBeg.AddHours(1) - DateTime.Now;
            await ReplyAsync($"{CustomEmojis.Egg} {Context.User.Mention} " +
                $"You still Have to wait {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}" +
                $" to beg again, dipshit {CustomEmojis.Zltr}");
            return;
        }

        int charity = 0;

        int BegNum = Random.Shared.Next(30, 51);

        if (user.Pets.ContainsKey(PetType.SuperNed))
        {
            var pet = user.Pets[PetType.SuperNed];

            double diversity = (0.8 + (double)BegNum / 30);

            DateTime nextBeg = DateTime.Now.AddMinutes(-2 * pet.Tier);
            DateTime cappedBeg = DateTime.Now.AddMinutes(-59).AddSeconds(-1 * (5 * (pet.Tier - 18)));

            user.LastBeg = nextBeg > cappedBeg ? nextBeg : cappedBeg;

            int PetGain = (int)Math.Ceiling((1.55 * pet.CombinedLevel) * diversity)
                + (int)Math.Round(pet.CombinedLevel * 1.4);

            // % per level
            PetGain += (int)(PetGain * (((double)pet.CombinedLevel) / 100));

            charity = BegNum + PetGain;

            await ReplyAsync($"{Context.User.Mention} is so pathetic i had to give him {charity} baps, of which {PetGain} is because of his pet {Pets.SuperNed.Name} {CustomEmojis.Omega}");
            user.Balance += charity;
        }
        else
        {
            user.LastBeg = DateTime.Now;
            await ReplyAsync($"{Context.User.Mention} is so pathetic i had to give him {BegNum} baps {CustomEmojis.Omega}");
            user.Balance += BegNum;
        }

        user.AddUserEvent(UserEventType.Beg);
        var result = user.AttemptCompleteQuests();
        await tutorialManager.AttemptCompleteQuestSteps(user, Context.Channel, result);
        await Context.CompleteQuestsAsync(result.freshCompleted, result.lastDailyCompleted, result.lastWeeklyCompleted);

        await dbContext.SaveChangesAsync();
    }
}
