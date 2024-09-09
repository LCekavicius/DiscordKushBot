using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using KushBot.Global;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules;

enum PetRarity
{
    Common, Rare, Epic
}

public class Hatch : ModuleBase<SocketCommandContext>
{
    [Command("hatch")]
    public async Task HatchEgg(int amount)
    {
        var user = Data.Data.GetKushBotUser(Context.User.Id, Data.UserDtoFeatures.Pets);

        if (user.HasEgg == false)
        {
            await ReplyAsync($"{Context.User.Mention} Niga, you don't even have an egg, type 'kush pets' if you're this dumb");
            return;
        }

        if (user.Balance < amount)
        {
            await ReplyAsync($"{Context.User.Mention} Ever heard of math?");
            return;
        }

        if (amount < 1)
        {
            await ReplyAsync($"{Context.User.Mention} cringe");
            return;
        }

        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 3, 2, Context.Channel);

        Random rnd = new Random();

        int HatchCost = rnd.Next(400, 700);

        float chance = (amount * 100) / HatchCost;

        float Roll = rnd.Next(1, 101);

        if (chance > Roll)
        {
            PetRarity rolledRarity = RollPetRarity();
            PetType petType = (PetType)RollPetIdByRarity(rolledRarity);

            if (Program.PetTest == Context.User.Id)
            {
                petType = PetType.Jew;
                Program.PetTest = default;
            }
            EmbedBuilder builder = new EmbedBuilder();

            user.HasEgg = false;
            user.Balance -= amount;

            int pity = user.PetPity;

            string dupeText = "";

            if (user.Pets2.ContainsKey(petType))
            {
                if (user.Pets2.Count < 6 && pity > 5 && rnd.Next(5, 13) < pity)
                {
                    var petTypes = Global.Pets.All.Select(e => e.Type);
                    var availablePetIds = petTypes.Except(user.Pets2.Select(e => e.Value.PetType)).ToList();

                    petType = availablePetIds[rnd.Next(0, availablePetIds.Count)];
                    rolledRarity = GetRarityByPetId(petType);
                }
                else
                {
                    dupeText = ". Since you already have it, it's dupe count increases by 1";
                }

            }

            builder.WithColor(ColorByRarity(rolledRarity));

            builder.AddField("Pet Hatching", $"{Context.User.Mention} Holy shit, You hatched your egg and got a **{rolledRarity.ToString()}** pet: **{Global.Pets.All.FirstOrDefault(e => e.Type == petType).Name}** {dupeText}");

            await ReplyAsync("", false, builder.Build());

            if (string.IsNullOrEmpty(dupeText))
            {
                user.Pets2.Add(petType, new UserPet { Dupes = 0, Level = 1, PetType = petType, UserId = user.Id });
                user.PetPity = 0;
            }
            else
            {
                user.PetPity += 1;
                user.Pets2[petType].Dupes += 1;
            }

        }
        else
        {
            await ReplyAsync($"{Context.User.Mention} your egg seems displeased with ur lack of baps");
            user.Balance -= amount;
        }
        await Data.Data.SaveKushBotUserAsync(user, Data.UserDtoFeatures.Pets);
    }

    private Color ColorByRarity(PetRarity rarity) =>
        rarity switch
        {
            PetRarity.Epic => Color.Purple,
            PetRarity.Rare => Color.Blue,
            _ => Color.LightGrey
        };

    private PetRarity RollPetRarity()
    {
        Random rnd = new Random();
        if (rnd.NextDouble() < 0.15)
        {
            return PetRarity.Epic;
        }
        else if (rnd.NextDouble() < 0.475)
        {
            return PetRarity.Rare;
        }
        else
        {
            return PetRarity.Common;
        }
    }

    private PetRarity GetRarityByPetId(PetType id)
    {
        if ((int)id < 2)
            return PetRarity.Common;
        else if ((int)id < 2)
            return PetRarity.Rare;
        else
            return PetRarity.Epic;

    }

    private int RollPetIdByRarity(PetRarity rarity)
    {
        Random rnd = new();
        if (rarity == PetRarity.Common)
        {
            return rnd.Next(0, 2);
        }
        else if (rarity == PetRarity.Rare)
        {
            return rnd.Next(2, 4);
        }
        else
        {
            return rnd.Next(4, 6);
        }
    }
}
