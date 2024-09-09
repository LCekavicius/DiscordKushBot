using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.Modules
{
    [Group("pets"),Alias("Pet")]
    public class Pets : ModuleBase<SocketCommandContext>
    {
        [Command("help"), Alias("")]
        public async Task Halp()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(Color.Green)
           .AddField("**Pets**", $"Pets play a major part in this server's bot so you should be informed. There are {Program.Pets.Count} different pets in total " +
           $"for you to obtain. Each of them has their own attributes that you can use for your own self-gain.")
           .AddField("Getting a pet", "To get a pet you first need to buy an egg from the god himself, type 'kush egg' to buy an egg for 350 baps.")
           .AddField("Hatching the Egg", "To hatch the egg you need to use the command 'kush hatch n' (e.g. kush hatch 500) the egg will either hatch " +
           "or ignore you depending on RNG and the baps you used to hatch the egg, more baps = higher chance of hatching, at 700 baps the chance becomes 100%")
           .AddField("Leveling your pet", "Each pet can be leveled to level 99 starting from level 1, to do so use the command 'kush feed petname' (e.g. " +
           "kush feed Pinata) this costs baps, the higher the pet's level the more baps it costs but the greater the pet's effects become")
           .AddField("Stalking on people", "You can use the command 'kush pets @user' (e.g. kush pets @taboBanda) to see someone's pets and their levels")
           .AddField("Rarity", "Pets are grouped into 3 rarity categories: common, Rare, Epic. Chances of obtaining are as follows: 55% 35% 10%")
           .AddField($"Dupes & Tiers", "Pets have tiers, by getting a certain number of duplicate pets, their Tier goes up, improving their effectivines," +
           " the tier is indicated between the [] in stats window. Check your tier progress by typing 'kush pets progress'")
           .AddField("Info on each pet", $"Type 'kush pets petName' (e.g. kush pets SuperNed) for info on a specific pet, pet names are: " +
           $"**{Program.Pets[0].Name}, Pinata, {Program.Pets[3].Name}, Goran, {Program.Pets[4].Name}, {Program.Pets[5].Name}**");

            await ReplyAsync("", false, builder.Build());
        }
        [Command("")]
        public async Task Halmp(IUser User)
        {
            string _Pets = Data.Data.GetPets(User.Id);

            if (_Pets == "")
            {
                await ReplyAsync ($"{User.Mention} doesn't have any Pets");
            }
            string[] petDescs = new string[Program.Pets.Count];

            for (int i = 0; i < _Pets.Length; i++)
            {
                double negate = 0;
                if (Data.Data.GetPetLevel(User.Id, int.Parse(_Pets[i].ToString())) < 15)
                {
                    negate = (double)Data.Data.GetPetLevel(Context.User.Id, int.Parse(_Pets[i].ToString())) / 100;
                }
                else
                {
                    negate = 0.14;
                }

                int BapsFed = 0;
                if (Data.Data.GetPetLevel(User.Id, int.Parse(_Pets[i].ToString())) == 1)
                {
                    BapsFed = 100;
                }
                else
                {
                    double _BapsFed = Math.Pow(Data.Data.GetPetLevel(User.Id, int.Parse(_Pets[i].ToString())), 1.13 - negate) * 85;

                    BapsFed = (int)Math.Round(_BapsFed);
                }

                if (Data.Data.GetPetLevel(Context.User.Id, int.Parse(_Pets[i].ToString())) < 99)
                {
                    petDescs[i] = $"[{Data.Data.GetPetTier(User.Id, int.Parse(_Pets[i].ToString()))}]" +
                        $"{Program.GetPetName(int.Parse(_Pets[i].ToString()))} - Level {Data.Data.GetPetLevel(User.Id, int.Parse(_Pets[i].ToString()))}/99";
                }
                else
                {
                    petDescs[i] = $"[{Data.Data.GetPetTier(User.Id, int.Parse(_Pets[i].ToString()))}]" +
                        $"{Program.GetPetName(int.Parse(_Pets[i].ToString()))} - Level {Data.Data.GetPetLevel(User.Id, int.Parse(_Pets[i].ToString()))}/99";
                }
            }
            
            string TargetsPets = string.Join("\n", petDescs);

            EmbedBuilder builder = new EmbedBuilder();

            builder.WithColor(Color.Orange)
                .AddField($"{User.Username} Pets",$"{TargetsPets}");

            await ReplyAsync("", false, builder.Build());

        }

        [Command("Progress")]
        public async Task Progressp()
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle($"{Context.User.Username}'s pet tier progress");
            builder.WithColor(Color.Orange);
            string pets = Data.Data.GetPets(Context.User.Id);
            for (int i = 0; i < Program.Pets.Count; i++)
            {
                if (pets.Contains(i.ToString()))
                {
                    builder.AddField($"{Program.Pets[i].Name}", $"Next tier progress: {Data.Data.GetPetDupe(Context.User.Id,i)}/{Data.Data.GetNextPetTierReq(Context.User.Id,i)}");
                }
            }
            await ReplyAsync($"", false, builder.Build());
        }

        [Command("SuperNed")]
        public async Task Ned()
        {
            EmbedBuilder builder = new EmbedBuilder();
            string desc = $"{Program.Pets[0].Name} is a homeless professional, owning him lets you get extra baps when begging, Higher level = more baps per beg, Higher Tier = Lower cooldown";
            builder.WithColor(Color.LightGrey);
            builder.AddField($"{Program.Pets[0].Name}", desc);
            await ReplyAsync("", false, builder.Build());
        }
        [Command("Baps pinata"),Alias("Pinata")]
        public async Task BapsPinata()
        {
            EmbedBuilder builder = new EmbedBuilder();
            string desc = $"{Program.Pets[1].Name} is an immortal object that grows baps inside it, it reaches adulthood after 22 hours. Owning this pet " +
                $"lets you use the command 'kush destroy' to destroy the pinata and get a lot of baps, don't worry the pinata cannot die and will simply start growing again. " +
                $"Higher level = More baps per destroy, Higher tier = Chance of instant growth after destruction";
            builder.WithColor(Color.LightGrey);
            builder.AddField($"{Program.Pets[1].Name}", desc);
            await ReplyAsync("", false, builder.Build());
        }
        [Command("Jew")]
        public async Task Zydas()
        {
            EmbedBuilder builder = new EmbedBuilder();
            string desc = $"{Program.Pets[4].Name} is your average jew which survived the god's blessing called 'Holocaust' owning this pet lets you use 'kush yoink @player' " +
                $"(e.g. kush yoink @TaboBanda) to yoink some of the target's baps and put them into your own pocket. The Jew, on his way back, also yoinks some baps " +
                $"from random passengers around him. You can also use the jew's improvised tactics to steal from packages that are mid-way to someone. higher level = higher chance of yoinking someone, more baps on yoink and lower cooldown." +
                $" Higher Tier = Higher chance of the jew not going on cooldown";
            builder.WithColor(Color.Purple);
            builder.AddField($"{Program.Pets[4].Name}", desc);
            await ReplyAsync("", false, builder.Build());
        }
        [Command("TylerJuan")]
        public async Task Tyler()
        {
            EmbedBuilder builder = new EmbedBuilder();
            string desc = $"{Program.Pets[5].Name} is a monstrosity that's been crossbred by your mom and autism itself. this odd-shaped egg is so ugly that simply " +
                $"looking at it makes you emotionally unstable and start destroying casino machines for some extra change when gambling, the extra change you find depends " +
                $"on how many baps you're using as well as the pet's level. Use this pet by typing 'kush rage'. \nHigher level = more baps per win, reduced cooldown " +
                $"\nHigher Tier = longer duration (+1 per tier)";
            builder.WithColor(Color.Purple);
            builder.AddField($"{Program.Pets[5].Name}", desc);


            builder.AddField($"Formulas", $"Rage baps are calculated based on the following formula:\n(((2 * gambledBaps - (gambledBaps ^ 2 / (5 + level + gambledBaps / 2))) * 0.9)/gambledBaps) * wonBaps\n" +
                $"Where *gambledBaps* = the amount inputted into a gamble and *wonbaps* = the amount received");
            await ReplyAsync("", false, builder.Build());
        }
        [Command("MayBich")]
        public async Task Maybach()
        {
            EmbedBuilder builder = new EmbedBuilder();
            string desc = $"{Program.Pets[3].Name} is a weird autist, after traversing difficult environments, succeeding in life-threatening missions he finally decided to " +
                $"settle down, thats when you found him in an egg, altough without consent, he still follow you and guides you with your quests, yet his annoying screeching " +
                $"still pisses you off. Gives more baps per quest completion and daily completion of all quests. Higher level = More baps. Higher Tier = More quests (e.g. tier 9 gives " +
                $"2 additional quests and 25% chance for a third one)";

            builder.WithColor(Color.Green);
            builder.AddField($"{Program.Pets[3].Name}", desc);
            await ReplyAsync("", false, builder.Build());
        }
        [Command("goran"),Alias("Goran Jelić", "Goran jelic")]
        public async Task Digger()
        {
            EmbedBuilder builder = new EmbedBuilder();
            string desc = $"{Program.Pets[2].Name} aka Jamal, has been a slave laborer in somalia, mining coal since he was 6. Unfortunately he managed to escape, not long after " +
                $"that he ran into you, and now his true talents shine upon the world, pickaxe in one hand, KFC in the other, he swings away at the Baps mines. Type in 'kush dig' " +
                $"to force him to mine and use 'kush loot' when you want to take his earnings, the longer he stays in the mines the more baps you get, but the higher chance of him being knocked out, in this case " +
                $"you won't get any baps from him. By providing an optional parameter to dig ('kush dig *minutes*') you can limit the time that goran is digging. Higher level = lower chances of dying, more baps per minute, Higher tier = Chance to retrieve the baps he gained till his death";

            builder.WithColor(Color.Green);
            builder.AddField($"{Program.Pets[2].Name}", desc);
            await ReplyAsync("", false, builder.Build());
        }

    }
}
