using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.Modules
{
    public class Hatch : ModuleBase<SocketCommandContext>
    {
        string potential = "Dupe count";
        [Command("hatch")]
        public async Task HatchEgg(int amount)
        {

            if (Data.Data.GetEgg(Context.User.Id) == false)
            {
                await ReplyAsync($"{Context.User.Mention} Niga, you don't even have an egg, type 'kush pets' if you're this dumb");
                return;
            }
            if(Data.Data.GetBalance(Context.User.Id) < amount)
            {
                await ReplyAsync($"{Context.User.Mention} Ever heard of math?");
                return;
            }

            if(amount < 1)
            {
                await ReplyAsync($"{Context.User.Mention} cringe");
                return;
            }

            Random rad = new Random();

            int HatchCost = rad.Next(400,700);

            float chance = (amount * 100) / HatchCost;

            float Roll = rad.Next(1,101);

            if(chance > Roll)
            {
                int petRarity = rad.Next(1, 101);
                if (Program.PetTest == Context.User.Id)
                {
                    petRarity = 99;
                }
                int temp = 0;
                EmbedBuilder builder = new EmbedBuilder();

                await Data.Data.SaveEgg(Context.User.Id, false);
                await Data.Data.SaveBalance(Context.User.Id, amount * -1, false);

                //int giveBackOnFail = rad.Next(150, 300);

                if (Program.PetTest == Context.User.Id)
                {
                    petRarity = rad.Next(90, 100);
                }
                if (petRarity <= 55)
                {
                    temp = rad.Next(0, 2);
                    builder.WithColor(Color.LightGrey);

                    if (Exists(Data.Data.GetPets(Context.User.Id), temp))
                    {
                        builder.AddField("Pet Hatching", $"{Context.User.Mention} Holy shit, You hatched your egg and got a **common** pet: **{Program.Pets[temp].Name}**, Since you already have it, it's {potential} increases by 1");
                        //await Data.Data.SaveBalance(Context.User.Id, giveBackOnFail, false);
                        await Data.Data.SavePetDupes(Context.User.Id, temp, Data.Data.GetPetDupe(Context.User.Id, temp) + 1);
                        await ReplyAsync("", false, builder.Build());
                        return;
                    }
                    else
                    {
                        builder.AddField("Pet Hatching", $"{Context.User.Mention} Holy shit, You hatched your egg and got a **common** pet: **{Program.Pets[temp].Name}** <:Pepejam:945806412049702972> ");
                    }
                }
                else if(petRarity <= 90)
                {
                    temp = rad.Next(2, 4);

                    builder.WithColor(Color.Blue);
           

                    if (Exists(Data.Data.GetPets(Context.User.Id), temp))
                    {
                        builder.AddField("Pet Hatching", $"{Context.User.Mention} Holy shit, You hatched your egg and got a **rare** pet: **{Program.Pets[temp].Name}**, Since you already have it, it's {potential} increases by 1");
                       // await Data.Data.SaveBalance(Context.User.Id, giveBackOnFail, false);
                        await Data.Data.SavePetDupes(Context.User.Id, temp, Data.Data.GetPetDupe(Context.User.Id, temp) + 1);

                        await ReplyAsync("", false, builder.Build());
                        return;
                    }
                    else
                    {
                        builder.AddField("Pet Hatching", $"{Context.User.Mention} Holy shit, You hatched your egg and got a **rare** pet: **{Program.Pets[temp].Name}** <:Pepejam:945806412049702972>");
                    }
                }
                else
                {
                    temp = rad.Next(4,6);
                    builder.WithColor(Color.Purple);

                    if(Program.PetTest == Context.User.Id)
                    {
                        temp = 4;
                    }

                    if (Data.Data.GetPets(Context.User.Id).Contains(temp.ToString()))
                    {
                        if(rad.NextDouble() < 0.33)
                        {
                            if (temp == 4) temp = 5;
                            else temp = 4;
                        }
                    }

                    if (Exists(Data.Data.GetPets(Context.User.Id), temp))
                    {
                        builder.AddField("Pet Hatching", $"{Context.User.Mention} Holy shit, You hatched your egg and got an **epic** pet: **{Program.Pets[temp].Name}**, Since you already have it, it's {potential} increases by 1");
                        //await Data.Data.SaveBalance(Context.User.Id, giveBackOnFail, false);
                        await Data.Data.SavePetDupes(Context.User.Id, temp, Data.Data.GetPetDupe(Context.User.Id, temp) + 1);

                        await ReplyAsync("", false, builder.Build());
                        Program.PetTest = 0;
                        return;
                    }
                    else
                    {
                        builder.AddField("Pet Hatching", $"{Context.User.Mention} Holy shit, You hatched your egg and got an **epic** pet: **{Program.Pets[temp].Name}** <:Pepejam:945806412049702972>");
                        Program.PetTest = 0;
                    }
                }
                await ReplyAsync("",false,builder.Build());

                await Data.Data.SavePetLevels(Context.User.Id, temp, 1, true);
                await Data.Data.SavePets(Context.User.Id, temp);

            }
            else
            {
                await ReplyAsync($"{Context.User.Mention} your egg seems displeased with ur lack of baps");
                await Data.Data.SaveBalance(Context.User.Id, amount * -1, false);
            }

        }
        public bool Exists(string text, int match)
        {
            for(int i = 0; i < text.Length; i++)
            {
                int temp = int.Parse(text[i].ToString());
                if(temp == match)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
