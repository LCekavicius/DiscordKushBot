using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.Modules
{
    public class TylerRage : ModuleBase<SocketCommandContext>
    {
        [Command("rage")]
        public async Task TylerJuanRage()
        {
            string pets = Data.Data.GetPets(Context.User.Id);
            if (!pets.Contains('5'))
            {
                await ReplyAsync($"{Context.User.Mention} you don't even have {Program.Pets[5].Name} pet, delusional shit");
                return;
            }

            DateTime lastRage = Data.Data.GetLastRage(Context.User.Id);
            int petLevel = Data.Data.GetPetLevel(Context.User.Id, 5);

            if (lastRage.AddHours(4).AddSeconds(-1 * Math.Pow(petLevel, 1.5)) > DateTime.Now)
            {
                TimeSpan timeLeft = (lastRage.AddHours(4).AddSeconds(-1 * Math.Pow(petLevel, 1.5)) - DateTime.Now);
                await ReplyAsync($"<:eggsleep:610494851557097532> {Context.User.Mention} You have scared TylerJuan and it ran away, it will come back in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2} <:walking2:606763665169055744>");
                return;
            }

            int CurrentRD = Data.Data.GetRageDuration(Context.User.Id);
            if (CurrentRD != 0)
            {
                await Data.Data.SaveRageDuration(Context.User.Id, -1 * CurrentRD);
            }

            double RageDurationDbl = 10 + 1 * Data.Data.GetPetTier(Context.User.Id, 5);

            int abuseStrength = Data.Data.GetPetAbuseStrength(Context.User.Id, 5);

            //RageDurationDbl += abuseStrength * 2;

            int RageDuration = (int)Math.Round(RageDurationDbl);


            await ReplyAsync($"{Context.User.Mention} You get very angry because your pet {Program.Pets[5].Name} is too ugly to look at, You'll be raging for {RageDuration} Gambles, you'll get extra baps once you've calmed down");

            await Data.Data.SaveLastRage(Context.User.Id, DateTime.Now.AddMinutes(-30 * abuseStrength));
            await Data.Data.SaveRageDuration(Context.User.Id, RageDuration);

        }
    }

}
