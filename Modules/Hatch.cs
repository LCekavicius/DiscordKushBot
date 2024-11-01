using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules;

enum PetRarity
{
    Common, Rare, Epic
}

public class Hatch(SqliteDbContext dbContext, TutorialManager tutorialManager) : ModuleBase<SocketCommandContext>
{
    [Command("hatch")]
    [RequirePermissions(Permissions.Core)]
    public async Task HatchEgg(int amount)
    {
        var user = await dbContext.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Pets);

        if (user.Eggs <= 0)
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

        await tutorialManager.AttemptSubmitStepCompleteAsync(user, 3, 2, Context.Channel);

        int HatchCost = Random.Shared.Next(400, 700);

        float chance = (amount * 100) / HatchCost;

        float Roll = Random.Shared.Next(1, 101);

        if (chance > Roll)
        {
            PetRarity rolledRarity = RollPetRarity();
            PetType petType = (PetType)RollPetIdByRarity(rolledRarity);

            if (DiscordBotService.PetTest == Context.User.Id)
            {
                petType = PetType.Jew;
                DiscordBotService.PetTest = default;
            }
            EmbedBuilder builder = new EmbedBuilder();

            user.Eggs -= 1;
            user.Balance -= amount;

            int pity = user.PetPity;

            string dupeText = "";

            if (user.Pets.ContainsKey(petType))
            {
                if (user.Pets.Count < 6 && pity > 5 && Random.Shared.Next(5, 13) < pity)
                {
                    var petTypes = Global.Pets.All.Select(e => e.Type);
                    var availablePetIds = petTypes.Except(user.Pets.Select(e => e.Value.PetType)).ToList();

                    petType = availablePetIds[Random.Shared.Next(0, availablePetIds.Count)];
                    rolledRarity = GetRarityByPetId(petType);
                }
                else
                {
                    dupeText = ". Since you already have it, it's dupe count increases by 1";
                }

            }

            builder.WithColor(ColorByRarity(rolledRarity));

            builder.AddField("Pet Hatching", $"{Context.User.Mention} Holy shit, You hatched your egg and got a **{rolledRarity.ToString()}** pet: **{Global.Pets.All.FirstOrDefault(e => e.Type == petType).Name}** {dupeText}");


            if (string.IsNullOrEmpty(dupeText))
            {
                user.Pets.Add(petType, new UserPet { Dupes = 0, Level = 1, PetType = petType, UserId = user.Id });
                user.PetPity = 0;
            }
            else
            {
                user.PetPity += 1;
                user.Pets[petType].Dupes += 1;
                await tutorialManager.AttemptSubmitStepCompleteAsync(user, 4, 2, Context.Channel);
            }

            await dbContext.SaveChangesAsync();
            await ReplyAsync("", false, builder.Build());
        }
        else
        {
            user.Balance -= amount;
            await dbContext.SaveChangesAsync();
            await ReplyAsync($"{Context.User.Mention} your egg seems displeased with ur lack of baps");
        }
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
        var value = Random.Shared.NextDouble();
        return value switch
        {
            < 0.15 => PetRarity.Epic,
            < 0.475 => PetRarity.Rare,
            _ => PetRarity.Common
        };
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
