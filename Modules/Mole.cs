using Discord.Commands;
using System;
using System.Threading.Tasks;
using KushBot.Data;
using KushBot.DataClasses;
using KushBot.Global;

namespace KushBot.Modules
{
    public class Mole : ModuleBase<SocketCommandContext>
    {

        [Command("dig"), Alias("set", "digger set")]
        public async Task SetDigger(int? digDuration = null)
        {
            if (digDuration.HasValue && digDuration < 1)
            {
                await ReplyAsync($"{Context.User.Mention} https://tenor.com/view/anime-cat-tail-wagging-cute-soft-gif-21253363");
                return;
            }

            var user = Data.Data.GetKushBotUser(Context.User.Id, UserDtoFeatures.Pets);

            if (!user.Pets.ContainsKey(PetType.Goran))
            {
                await ReplyAsync($"{Context.User.Mention} You don't have {Pets.Goran.Name}, dumb shit");
                return;
            }

            TimeSpan ts;
            ts = user.LootedDigger - DateTime.Now;

            if (user.DiggerState == 1)
            {
                await ReplyAsync($"{Context.User.Mention} Your {Pets.Goran.Name} is already digging and you don't have 2 of them, retard");
                return;
            }

            if (ts.Ticks > 0)
            {

                if (user.DiggerState == 0)
                {
                    await ReplyAsync($"{Context.User.Mention} However you look at it, your {Pets.Goran.Name} is too exhausted to dig, you need to wait {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
                    return;
                }
                if (user.DiggerState == -1)
                {
                    await ReplyAsync($"{Context.User.Mention} your {Pets.Goran.Name} is still unconscious, you should wait {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
                    return;
                }
                await ReplyAsync("Some shit's gone wrong, contact admin");
                return;
            }

            user.SetDigger = DateTime.Now;
            user.GoranMaxDigMinutes = digDuration;
            user.DiggerState = 1;

            await Data.Data.SaveKushBotUserAsync(user);

            await ReplyAsync($"{Context.User.Mention} You've forced your {Pets.Goran.Name} to dig {(digDuration.HasValue ? $"for {digDuration.Value} minutes" : "")}\n Use 'kush loot' when you want him to stop");

        }
        [Command("Loot"), Alias("digger loot")]
        public async Task LootDigger()
        {
            var user = Data.Data.GetKushBotUser(Context.User.Id, UserDtoFeatures.Pets);

            if (!user.Pets.ContainsKey(PetType.Goran))
            {
                await ReplyAsync($"{Context.User.Mention} You don't have {Pets.Goran.Name}, dumb shit");
                return;
            }
            if (user.DiggerState != 1)
            {
                await ReplyAsync($"{Context.User.Mention} Your {Pets.Goran.Name} isn't even digging... \n try using 'kush dig'");
                return;
            }

            var pet = user.Pets[PetType.Goran];

            TimeSpan length;
            var digSetDate = user.SetDigger;
            var maxDuration = user.GoranMaxDigMinutes;

            length = DateTime.Now - digSetDate;

            int minutes = (int)Math.Round(length.TotalMinutes);
            minutes++;

            minutes = Math.Min(minutes, maxDuration ?? int.MaxValue);

            double deathChance = 2 - (pet.CombinedLevel / (38 + (pet.CombinedLevel / 3.4)));

            double BapsPerMin = 0;
            double BapsGained = 0;

            BapsPerMin = (10 / Math.Pow(deathChance, 0.85));

            bool moleDead = false;

            Random rad = new Random();

            int petLvl = pet.CombinedLevel;

            int test = 0;

            for (int i = 0; i < minutes; i++)
            {
                if (deathChance * 100 > rad.Next(0, 10000))
                {
                    moleDead = true;
                    test = i;
                    BapsGained = (int)(BapsPerMin * i);
                    BapsGained = BapsGained + (BapsGained * (((double)petLvl) / 100));
                    break;
                }
            }

            if (moleDead)
            {
                Random rnd = new Random();

                double chanceToSave = pet.Tier * 2 + 3 * Math.Sqrt((double)pet.Tier);
                if (rnd.NextDouble() > chanceToSave / 100)
                {
                    await ReplyAsync($"{Context.User.Mention} Checking in after {minutes} minutes, you've found your {Pets.Goran.Name} unconscious");
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention} Checking in after {minutes} minutes, you've found your {Pets.Goran.Name} unconscious, but he seems to have left a pile" +
                        $" of {(int)BapsGained} baps");

                    user.Balance += (int)BapsGained;
                }
                user.LootedDigger = DateTime.Now.AddHours(1).AddMinutes(35 + -1 * pet.CombinedLevel + 1);
                user.DiggerState = -1;
            }
            else
            {
                BapsGained = minutes * BapsPerMin;
                BapsGained = BapsGained + (BapsGained * (((double)petLvl) / 100));

                BapsGained = Math.Round(BapsGained, 0);
                user.Balance += (int)BapsGained;
                await ReplyAsync($"{Context.User.Mention} After pulling your {Pets.Goran.Name} out of it's hole he hands you **{BapsGained}** Baps which he got in {minutes} minutes");
                user.LootedDigger = DateTime.Now.AddHours(1).AddMinutes(50 - (-1 * (pet.Level / 2) + 1));


                user.DiggerState = 0;
            }

            await Data.Data.SaveKushBotUserAsync(user);
        }
    }
}
