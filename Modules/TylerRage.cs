﻿using Discord.Commands;
using KushBot.DataClasses;
using KushBot.Global;
using System;
using System.Threading.Tasks;

namespace KushBot.Modules;

[RequirePermissions(Permissions.Core)]
public class TylerRage : ModuleBase<SocketCommandContext>
{
    [Command("rage")]
    public async Task TylerJuanRage()
    {
        //var pets = Data.Data.GetUserPets(Context.User.Id);
        //if (!pets.ContainsKey(PetType.TylerJuan))
        //{
        //    await ReplyAsync($"{Context.User.Mention} you don't even have {Pets.TylerJuan.Name} pet, delusional shit");
        //    return;
        //}

        //DateTime lastRage = Data.Data.GetLastRage(Context.User.Id);
        //int petLevel = pets[PetType.TylerJuan].CombinedLevel;

        //if (lastRage.AddHours(4).AddSeconds(-1 * Math.Pow(petLevel, 1.5)) > DateTime.Now)
        //{
        //    TimeSpan timeLeft = (lastRage.AddHours(4).AddSeconds(-1 * Math.Pow(petLevel, 1.5)) - DateTime.Now);
        //    await ReplyAsync($"<:eggsleep:610494851557097532> {Context.User.Mention} You have scared TylerJuan and it ran away, it will come back in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2} <:walking2:606763665169055744>");
        //    return;
        //}

        //int CurrentRD = Data.Data.GetRageDuration(Context.User.Id);
        //if (CurrentRD != 0)
        //{
        //    await Data.Data.SaveRageDuration(Context.User.Id, -1 * CurrentRD);
        //}

        //double RageDurationDbl = 10 + 1 * pets[PetType.TylerJuan].Tier;

        //int RageDuration = (int)Math.Round(RageDurationDbl);

        //await ReplyAsync($"{Context.User.Mention} You get very angry because your pet {Pets.TylerJuan.Name} is too ugly to look at, You'll be raging for {RageDuration} Gambles, you'll get extra baps once you've calmed down");

        //await Data.Data.SaveLastRage(Context.User.Id, DateTime.Now.AddMinutes(-30));
        //await Data.Data.SaveRageDuration(Context.User.Id, RageDuration);

    }
}
