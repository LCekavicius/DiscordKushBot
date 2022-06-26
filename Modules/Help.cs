using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpMe()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("This is our custom bot's help field, i'll guide you through some commands")
                .WithColor(Color.Green)
                .AddField("**Baps**", "this is the server's currecny")
                .AddField("**__Pets__**", "Pets play a **HUGE** role in this bot, use 'kush pets help' for more information")
                .AddField("**Quests**","kush quests - see your quests which you can do for rewards")
                .AddField("Sell", "kush sell - You can sell your egg for some baps.")
                .AddField("**Balance**", "kush balance - check how many baps u've got ")
                .AddField("**Stats**", "kush stats - better version of kush balance, shows Baps, pets aswell as your cooldowns")
                .AddField("**Top**", "kush top - see the leaderboards!")
                .AddField("**Give**","kush give n @user (e.g. kush give 25 @TaboBanda) - send a package, containing baps, to a user (as of season 2, some of the pacakage's baps can be yoinked)")
                .AddField("**Flip**", "kush flip n -(e.g. kush flip 15) flip a coin and lose or double n baps")
                .AddField("**Risk**", "kush risk n mod -(e.g. kush risk 100 4) take a risk for high reward, the higher the mod the greater reward but less chance of winning(min mod - 4)")
                .AddField("**Bet**", "kush bet n - (e.g. kush bet 200) multiply n baps by a random modifier to lose or win some baps (min bet - 100 baps)")
                .AddField("**Slots**", "kush slots spin the slot machine (price determined by your total pet level) for a chance to win baps, cheems and or items." +
                " Type 'kush slots help' for more info regarding rates/rewards.")
                .AddField("**Duel**", "kush duel n @user - (e.g. kush duel 200 @TaboBanda) do a coinflip duel with someone for baps.")
                .AddField("**Beg**", "kush beg - Beg the gods for some baps (hourly)")
                .AddField("**Redeem**", "kush redeem - See what you can spend your baps on")
                .AddField("**Icons**", "type 'kush Icons' to edit your profile's Icon")
                .AddField("**Plots**", "'kush plots help' to learn more")
                .AddField("**Bosses**", "'kush bosses' to learn more")
                .AddField("**Items**", "'kush items' to learn more")
                .AddField("**Nya marry**", "'kush nya marry' uwu");


            await ReplyAsync("", false, builder.Build());
        }

        [Command("slots help")]
        public async Task HelpMeSlots()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Slots");
            builder.WithColor(Color.Green);

            builder.AddField("Rewards", "You can get either baps, cheems or items.");
            builder.AddField("Odds", "No idea what the exact odds are but the likeliness of emojis are as follows (Rarest to most common): " +
                "<:ima:945342040529567795><:Booba:944937036702441554><:kitadimensija:945779895164895252><:Cheems:945704650378707015><:stovi:945780098332774441><:rieda:945781493291184168><:Pepejam:945806412049702972><:widepep:945703091876020245><:Omega:945781765899952199> " +
                "Generally, you lose more baps than you gain, but you have a chance of getting items and cheems.");
            builder.AddField($"Rewards", "<:ima:945342040529567795> Grants items, <:Cheems:945704650378707015> grants cheems. Everything else grants baps, " +
                "the amount is determined by the rarity of the emoji.");
            builder.AddField($"Winning", "A row with the same emojis will count as a win. All rows award a win. Winning on the middle row awards" +
                " double the rewards (items get a higher chance for better rarity)");
            builder.AddField("Cost", $"The cost of slots is determined by your total pet level following this equation: *40 + TotalPetLevel + 4*AveragePetLevel*. " +
                $"Your current slots cost is **{40 + Program.GetTotalPetLvl(Context.User.Id) + 5 * Program.GetAveragePetLvl(Context.User.Id)}** baps");


            await ReplyAsync("", false, builder.Build());
        }

        [Command("items"), Alias("items help")]
        public async Task HelpMeItems()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Items");
            builder.WithColor(Color.Gold);

            builder.AddField($"Item", "Items are designed to give you various permanent passive effects/buffs");
            builder.AddField("Effects", "Some of the effects include: Extra levels to a specific pet (in this case you can level your pet over 99" +
                "(P.s. feed cost is not impacted by pet level gained from items)), boss dmg, more baps from airdrops/quests, more quest slots, etc.");


            builder.AddField("Obtaining items", "Getting items require you to fight bosses," +
                " you'll have a chance to get an item of the same rarity as the boss. You also have a chance to get items from " +
                "weekly quests as well as the slot machine.");
            builder.AddField($"Rarities", "There are 5 base rarities for items: common, uncommon, rare, epic, legendary");
            builder.AddField($"Inventory", "Items you gain go to your inventory, you can see your inventory by typing 'kush inv', you're gonna see" +
                " your items alongside their stats");
            builder.AddField($"Equiping", "Having an item is not enough, you have to equip it. to do so type 'kush equip *itemName*' (e.g. " +
                "'kush equip caonima'. However, since item graphics are tied to their name, you might have multiple items of the same name, Meaning " +
                "you'll have to use the item id (Shown next to the name, in the inventory, if you need to use it), in this case use 'kush equip id'." +
                " You have **4** slots to equip your items on. Equiped items will be displayed visually on your picture in the stats window");
            builder.AddField($"Unequip", "You cant unequip items, but you can destroy them, more info under \"obtaining cheems\"");
            builder.AddField("Cheems", "Cheems are the material you use to improve items. Every time you improve an item, its level " +
                "goes up and it receives a bonus from the stat pool");
            builder.AddField("Obtaining cheems", "To obtain cheems you need to destroy your items by using the command 'kush destroy *itemName*' " +
                "Similarly to equiping, in case you have 2 items of the same name you'll need to use the id provided next to the item name. " +
                "Additionaly, you can get cheems from the slot machine.");
            builder.AddField("Improving items","To improve an item, use the comand 'kush improve *itemName*', similarly to equiping, " +
                "you'll have to use the id, if you have 2 items of the same name. There's no limit to the level of an item but the cost required " +
                "grows exponentially.");


            await ReplyAsync("", false, builder.Build());
        }

        [Command("bosses")]
        public async Task HelpMeBosses()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Bosses");
            builder.WithColor(Color.DarkRed);

            builder.AddField($"Bosses", $"In the designated <#{Program.BossChannelId}> channel, bosses will appear at pre-determined times of the day");
            builder.AddField("Raiding", "If you have a boss ticket (and atleast 2 pets), you can use it, by" +
                " clicking on an emoji under the boss's embed to enter a boss fight to either fail miserably or get some rewards. If clicking the emojis do not work (fuck discord API)" +
                ", you can use theese commands: 'kush boss join', 'kush boss leave'. There's a 30 minute window " +
                "before the boss is defeated or leaves the channel, at the end of this timer, the boss fight results will appear");
            builder.AddField("Tickets", "Tickets can be gained by completing both of your weekly quests. Completing all of your daily quests " +
                "has a 28.57% chance to net you a ticket. **You can only have at most 3 tickets**");
            builder.AddField($"Combat", "During the bossfight you attack with 2 randomly selected pets, dealing damage equal to the selected pets' levels + the pets' tier," +
                " Meaning your APL (average " +
                "pet level) determines your average damage per single hit");

            await ReplyAsync("", false, builder.Build());
        }

        [Command("boss join")]
        public async Task Walas()
        {
            if(Data.Data.GetTicketCount(Context.User.Id) < 1)
            {
                await ReplyAsync($"{Context.User.Mention} you don't have a ticket ydyot");
                return;
            }
            await Program.BossObject.SignUp(Context.User.Id);
        }
        [Command("boss leave")]
        public async Task Walas2()
        {
            await Program.BossObject.SignOff(Context.User.Id);
        }
    }
}
