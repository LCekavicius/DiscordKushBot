using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KushBot.Data;
//using SixLabors.ImageSharp.Processing.Dithering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.Primitives;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using Discord.Rest;
using System.Linq;

namespace KushBot.Modules
{

    public class Stats : ModuleBase<SocketCommandContext>
    {
        
        [Command("stats")]
        public async Task StatsTable()
        {
            string HasEgg = "";
            if (Data.Data.GetEgg(Context.User.Id))
            {
                HasEgg = "1/1";
            }
            else
            {
                HasEgg = "0/1";
            }
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle($"{Context.User.Username}'s Statistics :");

            builder.WithColor(Discord.Color.Orange);
            builder.AddInlineField($"Balance: :four_leaf_clover:", $"{Data.Data.GetBalance(Context.User.Id)} baps");
            builder.AddInlineField($"Yiked: :grimacing:", $"{Data.Data.GetTotalYikes(Context.User.Id)} times\n{GetPrefix(Data.Data.GetTotalYikes(Context.User.Id))}");
            //  .AddField("Pets", $"Jew - Level {Data.Data.GetPetLevel(Context.User.Id,0)}/99");
            builder.AddField("Pets", $"{PetDesc()}");
            builder.AddInlineField($"Egg", $"{HasEgg}");
            builder.AddInlineField("Boss tickets", $"{Data.Data.GetTicketCount(Context.User.Id)}/3");

            builder.AddField("Next Procs", $"{Proc()}");
            if(Data.Data.GetRageDuration(Context.User.Id) > 0)
            {
                builder.AddField("Raging",$"You'll be raging for {Data.Data.GetRageDuration(Context.User.Id)} more gambles.");
            }
           // string path = @"D:\KushBot\Kush Bot\KushBot\KushBot\Data\Pictures";
            string path = @"D:\KushBot\Kush Bot\KushBot\KushBot\Data\Portraits";

            if (!Program.BotTesting)
            {
                path = @"Data/Portraits";
            }

            builder.AddField("To-give baps remaining:", $"{Data.Data.GetRemainingDailyGiveBaps(Context.User.Id)}/{Program.DailyGiveLimit}");

            IMessageChannel dump = Program._client.GetChannel(Program.DumpChannelId) as IMessageChannel;
            RestUserMessage picture;
            ulong selectedPicture = Context.User.Id;
            int selectedPicactual = Data.Data.GetSelectedPicture(Context.User.Id);
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
                Inventory.GenerateNewPortrait(Context.User.Id);
                picture = await dump.SendFileAsync($@"{path}/{selectedPicture}.png") as RestUserMessage;
            }
            string nyaUrl = Data.Data.GetNyaMarry(Context.User.Id);
            if (nyaUrl != "")
                builder.WithThumbnailUrl(nyaUrl);

            string imgurl = picture.Attachments.First().Url;

            builder.WithImageUrl(imgurl);
            
            //if(Context.User.Id == 192642414215692300)
            //{
            //    builder.WithThumbnailUrl("https://images.anime-pictures.net/621/6215c1d47709394eb31f2860a8d90eb2.png?if=ANIME-PICTURES.NET_-_589062-913x1500-virtual+youtuber-hololive-shirakami+fubuki-reinama-single-long+hair.png");
            //}

            GardenAffectedSUser te = new GardenAffectedSUser();
            if(Program.GardenAffectedPlayers.Where(x => x.UserId == Context.User.Id).Count() > 0)
            {
                te = Program.GardenAffectedPlayers.Where(x => x.UserId == Context.User.Id).FirstOrDefault();
            }
            if (te.UserId == Context.User.Id)
            {
                builder.AddField("Affected By:", $"lvl {te.Level - 2} {te.Effect}, Duration: {te.Duration}");
            }

            await ReplyAsync("",false,builder.Build());

        }

        public string Proc()
        {
            TimeSpan timeLeft;
            List<string> Procs = new List<string>();

            string Beg;
            string Pinata;
            string Digger;
            string Yoink;
            string Tyler = "Next rage in:";

            timeLeft = Data.Data.GetLastBeg(Context.User.Id).AddHours(1) - DateTime.Now;

            if (timeLeft.Ticks > 0)
            {
                Beg = $"Next Beg in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
            else
            {
                Beg = "Next Beg in: **Ready**";
            }
            Procs.Add(Beg);

            if(Data.Data.GetPetLevel(Context.User.Id, 1) - Data.Data.GetItemPetLevel(Context.User.Id, 1) != 0)
            {
                timeLeft = (Data.Data.GetLastDestroy(Context.User.Id).AddHours(24) - DateTime.Now);
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
            if (Data.Data.GetPetLevel(Context.User.Id, 2) - Data.Data.GetItemPetLevel(Context.User.Id, 2) != 0)
            {
                timeLeft = Data.Data.GetLootedDigger(Context.User.Id) - DateTime.Now;
                if ((Data.Data.GetDiggerState(Context.User.Id) == 0 || Data.Data.GetDiggerState(Context.User.Id) == -1) && timeLeft.Ticks > 0)
                {
                    Digger = $"Next Digger in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
                }
                else if(Data.Data.GetDiggerState(Context.User.Id) == 1)
                {
                    TimeSpan ts = DateTime.Now - Data.Data.GetSetDigger(Context.User.Id);
                    int minutes = (int)Math.Round(ts.TotalMinutes);
                    minutes++;
                    Digger = $"Next Digger in: **Digging, {minutes} minutes in**";
                }
                else
                {
                    Digger = "Next Digger in: **Ready**";
                }
                Procs.Add(Digger);
            }

            if (Data.Data.GetPetLevel(Context.User.Id,4) - Data.Data.GetItemPetLevel(Context.User.Id, 4) != 0)
            {
                timeLeft = Data.Data.GetLastYoink(Context.User.Id).AddHours(1).AddMinutes(30 - (Data.Data.GetPetLevel(Context.User.Id, 4) / 3)) - DateTime.Now;
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
            if (Data.Data.GetPetLevel(Context.User.Id, 5) - Data.Data.GetItemPetLevel(Context.User.Id, 5) != 0)
            {
                timeLeft = Data.Data.GetLastRage(Context.User.Id).AddHours(4).AddSeconds(-1 * Math.Pow((Data.Data.GetPetLevel(Context.User.Id, 5)), 1.5)) - DateTime.Now;
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
            
            if(Data.Data.GetYikeDate(Context.User.Id).AddHours(2) > DateTime.Now)
            {
                TimeSpan ts = Data.Data.GetYikeDate(Context.User.Id).AddHours(2) - DateTime.Now;
                Procs.Add($"Next Yike in: {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
            }
            else
            {
                Procs.Add($"Next Yike in: **Ready**");
            }

            if (Data.Data.GetRedeemDate(Context.User.Id).AddHours(3) > DateTime.Now)
            {
                TimeSpan ts = Data.Data.GetRedeemDate(Context.User.Id).AddHours(3) - DateTime.Now;
                Procs.Add($"Next Redeem in: {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
            }
            else
            {
                Procs.Add($"Next Redeem in: **Ready**");
            }

            string procs = string.Join("\n",Procs);

            return procs;
        }

        [Command("stats")]
        public async Task StatsTable(IUser User)
        {
            await Data.Data.MakeRowForJew(User.Id);

            string HasEgg = "";
            if (Data.Data.GetEgg(User.Id))
            {
                HasEgg = "1/1";
            }
            else
            {
                HasEgg = "0/1";
            }
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle($"{User.Username}'s Statistics :");

            builder.WithColor(Discord.Color.Orange);
            builder.AddInlineField($"Balance: :four_leaf_clover:", $"{Data.Data.GetBalance(User.Id)} baps");
            builder.AddInlineField($"Yiked: :grimacing:", $"{Data.Data.GetTotalYikes(User.Id)} times\n{GetPrefix(Data.Data.GetTotalYikes(User.Id))}");
            builder.AddField($"Egg", $"{HasEgg}");
            builder.AddInlineField("Boss tickets", $"{Data.Data.GetTicketCount(User.Id)}/3");

            string path = @"D:\KushBot\Kush Bot\KushBot\KushBot\Data\Portraits";

            //if (User.Id == 192642414215692300)
            //    builder.WithThumbnailUrl("https://images.anime-pictures.net/621/6215c1d47709394eb31f2860a8d90eb2.png?if=ANIME-PICTURES.NET_-_589062-913x1500-virtual+youtuber-hololive-shirakami+fubuki-reinama-single-long+hair.png");

            string nyaUrl = Data.Data.GetNyaMarry(User.Id);
            if (nyaUrl != "")
                builder.WithThumbnailUrl(nyaUrl);

            if (!Program.BotTesting)
            {
                path = @"Data/Portraits";
            }

            IMessageChannel dump = Program._client.GetChannel(Program.DumpChannelId) as IMessageChannel;

            RestUserMessage picture;

            ulong selectedPicture = User.Id;
            int selectedPicactual = Data.Data.GetSelectedPicture(Context.User.Id);
            try
            {
                if (selectedPicactual > 1000)
                {
                    picture = await dump.SendFileAsync($@"{path}/{selectedPicture}.gif") as RestUserMessage;
                }
                else
                {
                    picture = await dump.SendFileAsync($@"{path}/{selectedPicture}.jpg") as RestUserMessage;
                }
            }
            catch
            {
                    Inventory.GenerateNewPortrait(Context.User.Id);
                    picture = await dump.SendFileAsync($@"{path}/{selectedPicture}.png") as RestUserMessage;
            }
            

            string imgurl = picture.Attachments.First().Url;

            builder.WithImageUrl(imgurl);

            await ReplyAsync("", false, builder.Build());
        }

        public string PetDesc()
        {
            string _Pets = Data.Data.GetPets(Context.User.Id);



            if(_Pets == "")
            {
                return "No Pets";
            }
            string[] petDescs = new string[Program.Pets.Count];

            for (int i = 0; i < _Pets.Length;i++)
            {
                int itemPetLevel = Data.Data.GetItemPetLevel(Context.User.Id, int.Parse(_Pets[i].ToString()));
                int petLevel = Data.Data.GetPetLevel(Context.User.Id, int.Parse(_Pets[i].ToString()));
                int petTier = Data.Data.GetPetTier(Context.User.Id, int.Parse(_Pets[i].ToString()));
                double negate = 0;
                if(petLevel - itemPetLevel < 15)
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
                    //double _BapsFed = Math.Pow(Data.Data.GetPetLevel(Context.User.Id, int.Parse(_Pets[i].ToString())), 1.14 - negate) * (75 + (Data.Data.GetPetLevel(Context.User.Id, _Pets[i])));
                    //double _BapsFed = Math.Pow(Data.Data.GetPetLevel(Context.User.Id, int.Parse(_Pets[i].ToString())), 1.14 - negate) * (75 + (Data.Data.GetPetLevel(Context.User.Id, int.Parse(_Pets[i].ToString()))));
                    BapsFed = (int)Math.Round(_BapsFed);

                }

                if (petLevel < 99)
                {
                    petDescs[i] = $"[{petTier}]{Program.GetPetName(int.Parse(_Pets[i].ToString()))}";

                    string nyaUrl = Data.Data.GetNyaMarry(Context.User.Id);
                    if (nyaUrl != "")
                    {
                        petDescs[i] += $" - Lvl {petLevel}/" + $"{99 + itemPetLevel} NLC: {BapsFed}";

                    }
                    else
                    {
                        petDescs[i] += $" - Lvl {petLevel}/" + $"{99 + itemPetLevel} Next Level Cost: {BapsFed}";
                    }

                }
                else
                {
                    petDescs[i] = $"[{petTier}]{Program.GetPetName(int.Parse(_Pets[i].ToString()))}" +
                        $" - Level {petLevel}/" +
                        $"{99 + itemPetLevel}";
                }
            }
            return string.Join("\n",petDescs);          
        }

        public string GetPrefix(int yikes)
        {
            if (yikes >= 250)
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
            else if(yikes >= 175)
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
