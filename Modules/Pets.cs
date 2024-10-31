using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using KushBot.Global;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules;

[Group("pets"), Alias("Pet")]
[RequirePermissions(Permissions.Misc | Permissions.Core)]
public class PetsHelp : ModuleBase<SocketCommandContext>
{
    [Command("help"), Alias("")]
    public async Task Halp()
    {
        EmbedBuilder builder = new EmbedBuilder();
        builder.WithColor(Color.Green)
           .AddField("**Pets**", $"Pets play a major part in this server's bot so you should be informed. There are {Pets.All.Count} different pets in total " +
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
                $"**{Pets.SuperNed.Name}, {Pets.Pinata.Name}, {Pets.Maybich.Name}, {Pets.Goran.Name}, {Pets.Jew.Name}, {Pets.TylerJuan.Name}**");

        await ReplyAsync("", false, builder.Build());
    }

    [Command("")]
    public async Task ListPets(IUser User)
    {
        var pets = Data.Data.GetUserPets(Context.User.Id);

        if (!pets.Any())
        {
            await ReplyAsync($"{User.Mention} doesn't have any Pets");
        }

        string[] petLines = new string[Pets.All.Count];

        foreach (var petKvp in pets)
        {
            var pet = petKvp.Value;

            petLines[(int)pet.PetType] = $"[{pet.Tier}]" +
                    $"{pet.Name} - Level {pet.CombinedLevel}/99";
        }

        string text = string.Join("\n", petLines.Where(e => !string.IsNullOrEmpty(e)));

        EmbedBuilder builder = new EmbedBuilder();

        builder.WithColor(Color.Orange)
            .AddField($"{User.Username} Pets", $"{text}");

        await ReplyAsync("", false, builder.Build());

    }

    [Command("Progress")]
    public async Task Progress()
    {
        EmbedBuilder builder = new EmbedBuilder();
        builder.WithTitle($"{Context.User.Username}'s pet tier progress");
        builder.WithColor(Color.Orange);

        var pets = Data.Data.GetUserPets(Context.User.Id);

        foreach (var petKvp in pets)
        {
            var pet = petKvp.Value;

            builder.AddField($"{pet.Name}", $"Next tier progress: {pet.Dupes}/{Pets.GetNextPetTierReq(pet.Dupes)}");

        }

        await ReplyAsync($"", false, builder.Build());
    }

    [Command("SuperNed")]
    public async Task Ned()
    {
        EmbedBuilder builder = new EmbedBuilder();
        string desc = $"{Pets.SuperNed.Name} is a homeless professional, owning him lets you get extra baps when begging, Higher level = more baps per beg, Higher Tier = Lower cooldown";
        builder.WithColor(Color.LightGrey);
        builder.AddField($"{Pets.SuperNed.Name}", desc);
        await ReplyAsync("", false, builder.Build());
    }
    [Command("Baps pinata"), Alias("Pinata")]
    public async Task BapsPinata()
    {
        EmbedBuilder builder = new EmbedBuilder();
        string desc = $"{Pets.Pinata.Name} is an immortal object that grows baps inside it, it reaches adulthood after 22 hours. Owning this pet " +
            $"lets you use the command 'kush destroy' to destroy the pinata and get a lot of baps, don't worry the pinata cannot die and will simply start growing again. " +
            $"Higher level = More baps per destroy, Higher tier = Chance of instant growth after destruction";
        builder.WithColor(Color.LightGrey);
        builder.AddField($"{Pets.Pinata.Name}", desc);
        await ReplyAsync("", false, builder.Build());
    }
    [Command("Jew")]
    public async Task Zydas()
    {
        EmbedBuilder builder = new EmbedBuilder();
        string desc = $"{Pets.Jew.Name} is your average jew which survived the god's blessing called 'Holocaust' owning this pet lets you use 'kush yoink @player' " +
            $"(e.g. kush yoink @TaboBanda) to yoink some of the target's baps and put them into your own pocket. The Jew, on his way back, also yoinks some baps " +
            $"from random passengers around him. You can also use the jew's improvised tactics to steal from packages that are mid-way to someone. higher level = higher chance of yoinking someone, more baps on yoink and lower cooldown." +
            $" Higher Tier = Higher chance of the jew not going on cooldown";
        builder.WithColor(Color.Purple);
        builder.AddField($"{Pets.Jew.Name}", desc);
        await ReplyAsync("", false, builder.Build());
    }
    [Command("TylerJuan")]
    public async Task Tyler()
    {
        EmbedBuilder builder = new EmbedBuilder();
        string desc = $"{Pets.TylerJuan.Name} is a monstrosity that's been crossbred by your mom and autism itself. this odd-shaped egg is so ugly that simply " +
            $"looking at it makes you emotionally unstable and start destroying casino machines for some extra change when gambling, the extra change you find depends " +
            $"on how many baps you're using as well as the pet's level. Use this pet by typing 'kush rage'. \nHigher level = more baps per win, reduced cooldown " +
            $"\nHigher Tier = longer duration (+1 per tier)";
        builder.WithColor(Color.Purple);
        builder.AddField($"{Pets.TylerJuan.Name}", desc);


        builder.AddField($"Formulas", $"Rage baps are calculated based on the following formula:\n(((2 * gambledBaps - (gambledBaps ^ 2 / (5 + level + gambledBaps / 2))) * 0.9)/gambledBaps) * wonBaps\n" +
            $"Where *gambledBaps* = the amount inputted into a gamble and *wonbaps* = the amount received");
        await ReplyAsync("", false, builder.Build());
    }
    [Command("MayBich")]
    public async Task Maybach()
    {
        EmbedBuilder builder = new EmbedBuilder();
        string desc = $"{Pets.Maybich.Name} is a weird autist, after traversing difficult environments, succeeding in life-threatening missions he finally decided to " +
            $"settle down, thats when you found him in an egg, altough without consent, he still follow you and guides you with your quests, yet his annoying screeching " +
            $"still pisses you off. Gives more baps per quest completion and daily completion of all quests. Higher level = More baps. Higher Tier = More quests (e.g. tier 9 gives " +
            $"2 additional quests and 25% chance for a third one)";

        builder.WithColor(Color.Green);
        builder.AddField($"{Pets.Maybich.Name}", desc);
        await ReplyAsync("", false, builder.Build());
    }
    [Command("goran"), Alias("Goran Jelić", "Goran jelic")]
    public async Task Digger()
    {
        EmbedBuilder builder = new EmbedBuilder();
        string desc = $"{Pets.Goran.Name} aka Jamal, has been a slave laborer in somalia, mining coal since he was 6. Unfortunately he managed to escape, not long after " +
            $"that he ran into you, and now his true talents shine upon the world, pickaxe in one hand, KFC in the other, he swings away at the Baps mines. Type in 'kush dig' " +
            $"to force him to mine and use 'kush loot' when you want to take his earnings, the longer he stays in the mines the more baps you get, but the higher chance of him being knocked out, in this case " +
            $"you won't get any baps from him. By providing an optional parameter to dig ('kush dig *minutes*') you can limit the time that goran is digging. Higher level = lower chances of dying, more baps per minute, Higher tier = Chance to retrieve the baps he gained till his death";

        builder.WithColor(Color.Green);
        builder.AddField($"{Pets.Goran.Name}", desc);
        await ReplyAsync("", false, builder.Build());
    }

}
