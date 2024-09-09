using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using Discord.Rest;
using System.Linq;
using KushBot.DataClasses;
using KushBot.EventHandler.Interactions;

namespace KushBot.Modules
{

    public class Stats : ModuleBase<SocketCommandContext>
    {
        [Command("stats")]
        public async Task StatsTable(IUser user = null)
        {
            user ??= Context.User;

            if(Context.User.Id == user.Id)
                await TutorialManager.AttemptSubmitStepCompleteAsync(user.Id, 1, 0, Context.Channel);

            string HasEgg = "";
            if (Data.Data.GetEgg(user.Id))
            {
                HasEgg = "1/1";
            }
            else
            {
                HasEgg = "0/1";
            }
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle($"{user.Username}'s Statistics :");

            builder.WithColor(Discord.Color.Orange);
            builder.AddField($"Balance: :four_leaf_clover:", $"{Data.Data.GetBalance(user.Id)} baps", true);
            builder.AddField($"Yiked: :grimacing:", $"{Data.Data.GetTotalYikes(user.Id)} times\n{GetPrefix(Data.Data.GetTotalYikes(user.Id))}", true);
            //  .AddField("Pets", $"Jew - Level {Data.Data.GetPetLevel(user.Id,0)}/99");
            builder.AddField("Pets", $"{await PetDesc(user)}");
            builder.AddField($"Egg", $"{HasEgg}", true);
            builder.AddField("Boss tickets", $"{Data.Data.GetTicketCount(user.Id)}/3", true);

            builder.AddField("Next Procs", $"{Proc(user)}");

            bool hasBuffs = await Data.Data.UserHasBuffsAsync(user.Id);

            if (hasBuffs || Data.Data.GetRageDuration(user.Id) > 0)
            {
                builder.AddField("User has active buffs","See **'kush buffs'**");
            }


            // string path = @"D:\KushBot\Kush Bot\KushBot\KushBot\Data\Pictures";
            string path = @"Data/Portraits";

            builder.AddField("To-give baps remaining:", $"{Data.Data.GetRemainingDailyGiveBaps(user.Id)}/{Program.DailyGiveLimit}");

            IMessageChannel dump = Program._client.GetChannel(Program.DumpChannelId) as IMessageChannel;
            RestUserMessage picture;
            ulong selectedPicture = user.Id;
            int selectedPicactual = Data.Data.GetSelectedPicture(user.Id);
            try
            {
                if (selectedPicactual > 1000)
                {
                    //.gif
                    picture = await dump.SendFileAsync($@"{path}/{selectedPicture}.gif") as RestUserMessage;
                }
                else
                {
                    picture = await dump.SendFileAsync($@"{path}/{selectedPicture}.png") as RestUserMessage;
                }
            }
            catch
            {
                Inventory.GenerateNewPortrait(user.Id);
                picture = await dump.SendFileAsync($@"{path}/{selectedPicture}.png") as RestUserMessage;
            }
            string nyaUrl = Data.Data.GetNyaMarry(user.Id);
            if (nyaUrl != "")
                builder.WithThumbnailUrl(nyaUrl);

            string imgurl = picture.Attachments.First().Url;

            builder.WithImageUrl(imgurl);



            var userInfections = await Data.Data.GetUserInfectionsAsync(user.Id);

            InteractionHandlerFactory factory = new();
            var handler = factory.GetComponentHandler("kill", user.Id);

            await ReplyAsync("", false, builder.Build(), components: await handler.BuildMessageComponent());

        }

        public string Proc(IUser user, int skip = 0, int take = 500)
        {
            TimeSpan timeLeft;
            List<string> Procs = new List<string>();

            string Beg;
            string Pinata;
            string Digger;
            string Yoink;
            string Tyler = "Next rage in:";

            timeLeft = Data.Data.GetLastBeg(user.Id).AddHours(1) - DateTime.Now;

            if (timeLeft.Ticks > 0)
            {
                Beg = $"Next Beg in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
            else
            {
                Beg = "Next Beg in: **Ready**";
            }
            Procs.Add(Beg);

            if (Data.Data.GetPetLevel(user.Id, 1) - Data.Data.GetItemPetLevel(user.Id, 1) != 0)
            {
                timeLeft = (Data.Data.GetLastDestroy(user.Id).AddHours(22) - DateTime.Now);
                if (timeLeft.Ticks > 0)
                {
                    Pinata = $"Next Pinata in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
                }
                else
                {
                    Pinata = "Next Pinata in: **Ready**";
                }
                Procs.Add(Pinata);
            }
            if (Data.Data.GetPetLevel(user.Id, 2) - Data.Data.GetItemPetLevel(user.Id, 2) != 0)
            {
                timeLeft = Data.Data.GetLootedDigger(user.Id) - DateTime.Now;
                if ((Data.Data.GetDiggerState(user.Id) == 0 || Data.Data.GetDiggerState(user.Id) == -1) && timeLeft.Ticks > 0)
                {
                    Digger = $"Next Digger in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
                }
                else if (Data.Data.GetDiggerState(user.Id) == 1)
                {
                    var diggerData = Data.Data.GetSetDigger(user.Id);
                    TimeSpan ts = DateTime.Now - diggerData.digSetDate;
                    int minutes = (int)Math.Round(ts.TotalMinutes);
                    minutes++;
                    minutes = Math.Min(minutes, diggerData.maxDuration ?? int.MaxValue);
                    Digger = $"Next Digger in: **Digging, {minutes} minutes in**";
                }
                else
                {
                    Digger = "Next Digger in: **Ready**";
                }
                Procs.Add(Digger);
            }

            if (Data.Data.GetPetLevel(user.Id, 4) - Data.Data.GetItemPetLevel(user.Id, 4) != 0)
            {
                timeLeft = Data.Data.GetLastYoink(user.Id).AddHours(1).AddMinutes(30 - (Data.Data.GetPetLevel(user.Id, 4) / 3)) - DateTime.Now;
                if (timeLeft.Ticks > 0)
                {
                    Yoink = $"Next Yoink in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
                }
                else
                {
                    Yoink = "Next Yoink in: **Ready**";
                }
                Procs.Add(Yoink);
            }
            if (Data.Data.GetPetLevel(user.Id, 5) - Data.Data.GetItemPetLevel(user.Id, 5) != 0)
            {
                timeLeft = Data.Data.GetLastRage(user.Id).AddHours(4).AddSeconds(-1 * Math.Pow((Data.Data.GetPetLevel(user.Id, 5)), 1.5)) - DateTime.Now;
                if (timeLeft.Ticks > 0)
                {
                    Tyler = $"Next Rage in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
                }
                else
                {
                    Tyler = "Next Rage in: **Ready**";
                }
                Procs.Add(Tyler);
            }

            if (Data.Data.GetYikeDate(user.Id).AddHours(2) > DateTime.Now)
            {
                TimeSpan ts = Data.Data.GetYikeDate(user.Id).AddHours(2) - DateTime.Now;
                Procs.Add($"Next Yike in: {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
            }
            else
            {
                Procs.Add($"Next Yike in: **Ready**");
            }

            if (Data.Data.GetRedeemDate(user.Id).AddHours(3) > DateTime.Now)
            {
                TimeSpan ts = Data.Data.GetRedeemDate(user.Id).AddHours(3) - DateTime.Now;
                Procs.Add($"Next Redeem in: {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
            }
            else
            {
                Procs.Add($"Next Redeem in: **Ready**");
            }

            string procs = string.Join("\n", Procs.Skip(skip).Take(take));

            return procs;
        }     

        public async Task<string> PetDesc(IUser user)
        {
            string _Pets = Data.Data.GetPets(user.Id);



            if (_Pets == "")
            {
                return "No Pets";
            }

            bool attemptedSubmit = false;

            string[] petDescs = new string[Program.Pets.Count];

            for (int i = 0; i < _Pets.Length; i++)
            {
                int itemPetLevel = Data.Data.GetItemPetLevel(user.Id, int.Parse(_Pets[i].ToString()));
                int petLevel = Data.Data.GetPetLevel(user.Id, int.Parse(_Pets[i].ToString()));
                int petTier = Data.Data.GetPetTier(user.Id, int.Parse(_Pets[i].ToString()));

                if (petTier >= 1 && !attemptedSubmit)
                {
                    attemptedSubmit = true;
                    await TutorialManager.AttemptSubmitStepCompleteAsync(user.Id, 4, 2, Context.Channel);
                }

                double negate = 0;
                if (petLevel - itemPetLevel < 15)
                {
                    negate = ((double)petLevel - itemPetLevel) / 100;
                }
                else
                {
                    negate = 0.14;
                }



                int BapsFed = 0;
                if (petLevel - itemPetLevel == 1)
                {
                    BapsFed = 100;
                }
                else
                {

                    double _BapsFed = Math.Pow(petLevel - itemPetLevel, 1.14 - negate) * (70 + ((petLevel - itemPetLevel) / 1.25));
                    BapsFed = (int)Math.Round(_BapsFed);
                }

                string realPetLvl = itemPetLevel == 0 ? "" : $"({petLevel - itemPetLevel})";

                if (petLevel < 99)
                {
                    petDescs[i] = $"[{petTier}]{Program.GetPetName(int.Parse(_Pets[i].ToString()))}";



                    string nyaUrl = Data.Data.GetNyaMarry(user.Id);
                    petDescs[i] += $" - Level {petLevel}/" + $"{99 + itemPetLevel} NLC: {BapsFed}";
                    //if (nyaUrl != "")
                    //{
                    //    petDescs[i] += $" - Level {petLevel}/" + $"{99 + itemPetLevel} NLC: {BapsFed}";

                    //}
                    //else
                    //{
                    //    petDescs[i] += $" - Level {petLevel}/" + $"{99 + itemPetLevel} Level up cost: {BapsFed}";
                    //}

                }
                else
                {
                    petDescs[i] = $"[{petTier}]{Program.GetPetName(int.Parse(_Pets[i].ToString()))}" +
                        $" - Level {petLevel}/" +
                        $"{99 + itemPetLevel}";
                }
            }
            return string.Join("\n", petDescs);
        }

        public string GetPrefix(int yikes)
        {
            if (yikes >= 300)
            {
                return "Astro cringe";
            }
            else if (yikes >= 275)
            {
                return "Turbo cringe";
            }
            else if (yikes >= 250)
            {
                return "Nuclear cringe";
            }
            else if (yikes >= 225)
            {
                return "Astoundingly cringe";
            }
            else if (yikes >= 200)
            {
                return "Excessively cringe";
            }
            else if (yikes >= 175)
            {
                return "WAY too cringe";
            }
            else if (yikes >= 150)
            {
                return "Too cringe";
            }
            else if (yikes >= 125)
            {
                return "Very cringe";
            }
            else if (yikes >= 100)
            {
                return "Cringe+";
            }
            else if (yikes >= 75)
            {
                return "Cringe";
            }
            else if (yikes >= 50)
            {
                return "Kinda cringe";
            }
            else if (yikes >= 25)
            {
                return "a bit cringe";
            }
            else
            {
                return "";
            }

        }
    }
}
