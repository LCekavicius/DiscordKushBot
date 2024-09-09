using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace KushBot.Modules
{
    enum PetRarity
    {
        Common, Rare, Epic
    }

    public class Hatch : ModuleBase<SocketCommandContext>
    {
        [Command("hatch")]
        public async Task HatchEgg(int amount)
        {

            if (Data.Data.GetEgg(Context.User.Id) == false)
            {
                await ReplyAsync($"{Context.User.Mention} Niga, you don't even have an egg, type 'kush pets' if you're this dumb");
                return;
            }
            if (Data.Data.GetBalance(Context.User.Id) < amount)
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

            Random rad = new Random();

            int HatchCost = rad.Next(400, 700);

            float chance = (amount * 100) / HatchCost;

            float Roll = rad.Next(1, 101);

            if (chance > Roll)
            {
                PetRarity rolledRarity = RollPetRarity();
                int petId = RollPetIdByRarity(rolledRarity);
                if(Program.PetTest == Context.User.Id)
                {
                    petId = 4;
                    Program.PetTest = default;
                }
                EmbedBuilder builder = new EmbedBuilder();

                await Data.Data.SaveEgg(Context.User.Id, false);
                await Data.Data.SaveBalance(Context.User.Id, amount * -1, false);

                string userPets = Data.Data.GetPets(Context.User.Id);
                int pity = Data.Data.GetUserPetPity(Context.User.Id);

                string dupeText = "";

                if (userPets.Contains(petId.ToString()))
                {
                    if (userPets.Length < 6 && pity > 5 && rad.Next(5, 13) < pity)
                    {
                        var petIds = new List<int>() { 0, 1, 2, 3, 4, 5 };
                        var availablePetIds = petIds.Except(userPets.Select(e => int.Parse(e.ToString())));

                        petId = availablePetIds.OrderBy(e => rad.NextDouble()).FirstOrDefault();
                        rolledRarity = GetRarityByPetId(petId);
                    }
                    else
                    {
                        dupeText = ". Since you already have it, it's dupe count increases by 1";
                    }

                }

                builder.WithColor(ColorByRarity(rolledRarity));


                builder.AddField("Pet Hatching", $"{Context.User.Mention} Holy shit, You hatched your egg and got a **{rolledRarity.ToString()}** pet: **{Program.Pets[petId].Name}** {dupeText}");


                await ReplyAsync("", false, builder.Build());

                if (string.IsNullOrEmpty(dupeText))
                {
                    await Data.Data.SavePetLevels(Context.User.Id, petId, 1, true);
                    await Data.Data.SavePets(Context.User.Id, petId);
                    await Data.Data.ResetUserPity(Context.User.Id);
                }
                else
                {
                    await Data.Data.IncrementPityAsync(Context.User.Id);
                    await Data.Data.SavePetDupes(Context.User.Id, petId, Data.Data.GetPetDupe(Context.User.Id, petId) + 1);
                }

            }
            else
            {
                await ReplyAsync($"{Context.User.Mention} your egg seems displeased with ur lack of baps");
                await Data.Data.SaveBalance(Context.User.Id, amount * -1, false);
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

        private PetRarity GetRarityByPetId(int id)
        {
            if (id < 2)
                return PetRarity.Common;
            else if (id < 2)
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


        public bool Exists(string text, int match)
        {
            for (int i = 0; i < text.Length; i++)
            {
                int temp = int.Parse(text[i].ToString());
                if (temp == match)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
