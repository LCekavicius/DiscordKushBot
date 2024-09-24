using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KushBot.Data;
using KushBot.DataClasses;
using KushBot.Global;

namespace KushBot.Modules;

public class Beg : ModuleBase<SocketCommandContext>
{

    [Command("beg")]
    public async Task PingAsync()
    {
        var user = Data.Data.GetKushBotUser(Context.User.Id, UserDtoFeatures.Pets);
        DateTime lastBeg = user.LastBeg;

        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 1, 1, Context.Channel);

        if (lastBeg.AddHours(1) > DateTime.Now)
        {
            TimeSpan timeLeft = lastBeg.AddHours(1) - DateTime.Now;
            await ReplyAsync($"{CustomEmojis.Egg} {Context.User.Mention} " +
                $"You still Have to wait {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}" +
                $" to beg again, dipshit {CustomEmojis.Zltr}");
            return;
        }

        int charity = 0;

        Random rnd = new Random();

        int BegNum = rnd.Next(30, 51);

        int petAbuseCdr = Data.Data.GetPetAbuseSupernedStrength(Context.User.Id, 0);

        if (int.TryParse("1", out var rate))
        //if (int.TryParse(DiscordBotService._configuration["rate"], out var rate))
        {
            BegNum *= rate;
        }

        if (user.Pets.ContainsKey(PetType.SuperNed))
        {
            var pet = user.Pets[PetType.SuperNed];

            double diversity = (0.8 + (double)BegNum / 30);

            DateTime nextBeg = DateTime.Now.AddMinutes(-1 * petAbuseCdr + (-2 * pet.Tier));
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

        await Data.Data.SaveKushBotUserAsync(user);

        //TODO handle quest
    }
}
