using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KushBot.Data;
using SixLabors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp.Drawing.Processing;

namespace KushBot.Modules
{
    [Group("Icons")]
    public class Pictures : ModuleBase<SocketCommandContext>
    {
        static string testIcon = "-1";

        [Command("test")]
        public async Task SelectPicture(string picture)
        {
            testIcon = picture;
        }

        [Command("Select")]
        public async Task SelectPicture(int picture)
        {
            List<int> picturesOwned = Data.Data.GetPictures(Context.User.Id);

            if (!picturesOwned.Contains(picture))
            {
                await ReplyAsync($"{Context.User.Mention} You don't have that icon, obese");
                return;
            }

            await Data.Data.UpdateSelectedPicture(Context.User.Id, picture);
            Inventory.GenerateNewPortrait(Context.User.Id);
            await ReplyAsync($"{Context.User.Mention} you updated your icon");
        }

        [Command("specials")]
        public async Task ShowSpecials()
        {
            string path = @"C:\Users\Laurynas\Desktop\Kush Bot\KushBot\KushBot\Data\Pictures";

            List<int> picturesOwned = Data.Data.GetPictures(Context.User.Id);

            if (!Program.BotTesting)
            {
                path = @"Data/Pictures";
            }

            List<int> Printable = new List<int>();

            foreach (int item in picturesOwned)
            {
                if(item > 1000)
                {
                    Printable.Add(item);
                }
            }

            if(Printable.Count == 0)
            {
                await ReplyAsync($"{Context.User.Mention} you knormal?");
                return;
            }

            string print = $"{Context.User.Mention} you own special icons with the numbers of: ";
            foreach (int item in Printable)
            {
                print += $"{item}";
                if(Printable.Last() != item)
                {
                    print += ", ";
                }
            }

            print += "\nEquip them by typing 'kush icons select number' (e.g. kush icons select 1003)";

            await ReplyAsync(print);

        }

        [Command("")]
        public async Task Show(int showPage = 1)
        {
            if(showPage > Program.PictureCount / 9 || showPage <= 0)
            {
                await ReplyAsync($"{Context.User.Mention} that page doesnt exist");
                return;
            }

            int width = 576;
            int height = 576;

            int singleWidth = 192;
            int singleHeight = 192;

            string path = @"D:\KushBot\Kush Bot\KushBot\KushBot\Data\Pictures";

            if (!Program.BotTesting)
            {
                path = @"Data/Pictures";
            }
            string fontFamily = "Arial";
            using(StreamReader reader = new StreamReader($"{path}/font.txt"))
            {
                fontFamily = reader.ReadLine();
            }
            var font = SixLabors.Fonts.SystemFonts.CreateFont($"{fontFamily}", 80);

            List<int> picturesOwned = Data.Data.GetPictures(Context.User.Id);

            using (var image = new SixLabors.ImageSharp.Image<Rgba32>(width, height))
            {
                for (int i = 0 + 9 * (showPage - 1); i < 9 + 9 * (showPage - 1); i++)
                {
                    using (var img = SixLabors.ImageSharp.Image.Load($@"{path}/{i + 1}.jpg"))
                    {
                        img.Mutate(x => x.Resize(singleWidth, singleHeight));

                        if (!picturesOwned.Contains(i + 1))
                        {
                            using (var redImg = SixLabors.ImageSharp.Image.Load($@"{path}/Red.jpg"))
                            {
                                img.Mutate(x => x
                                 .DrawImage(redImg, 0.65f)
                                 .BokehBlur()
                                );
                            }
                        }

                        Point p = GetPointByIndex(i, singleWidth);
                        Point tp = new Point(p.X, p.Y + 7);
                        image.Mutate(x => x
                         .DrawImage(img, p, 1f)
                         .DrawText((i + 1).ToString(), font, SixLabors.ImageSharp.Color.White, tp)
                        );
                    }
                }
                image.Save($"{path}/{Context.User.Id}.png");
            }


            await Context.Channel.SendFileAsync($"{path}/{Context.User.Id}.png", "Type 'kush icons pageNumber' (e.g. kush icons 3) to check other pages\n" +
                "Type 'kush icons select pictureId (e.g. kush icons select 17) to *equip* an icons\n" +
                $"Type 'kush icons buy' to buy a random icons\nUpon completing a page of icons, you will get a special animated gif icon\n" +
                $"Price for another icons: **{350 + picturesOwned.Count * 50}** baps");

        }

        [Command("buy")]
        public async Task Buypictures()
        {
            List<int> allPictures = Data.Data.GetPictures(Context.User.Id);

            int price = 350 + allPictures.Count * 50;

            if(Data.Data.GetBalance(Context.User.Id) < price)
            {
                await ReplyAsync($"{Context.User.Mention} you are too poor for my wares fag");
                return;
            }
            if (allPictures.Count >= Program.PictureCount + Program.PictureCount / 9)
            {
                await ReplyAsync($"{Context.User.Mention} nigggggggggggeer");
                return;
            }
            
            Random rad = new Random();

            int chosen;

            List<int> temp = Enumerable.Range(1, Program.PictureCount).ToList();

            List<int> AvailableIcons = temp.Except(allPictures).ToList();

            chosen = AvailableIcons[rad.Next(0, AvailableIcons.Count)];

            if(Context.User.Id == 192642414215692300 && testIcon != "-1")
            {
                chosen = int.Parse(testIcon);
                testIcon = "-1";
            }

            //do
            //{
            //    chosen = rad.Next(4, Program.PictureCount + 1);
            //} while (allPictures.Contains(chosen));

            string path = @"D:\KushBot\Kush Bot\KushBot\KushBot\Data\Pictures";

            if (!Program.BotTesting)
            {
                path = @"Data/Pictures";
            }

            await Context.Channel.SendFileAsync($"{path}/{chosen}.jpg", $"{Context.User.Mention} You got #{chosen} icon at the cost of: **{price}** baps!");

            await Data.Data.SaveBalance(Context.User.Id, -1 * price, false);
            await Data.Data.UpdatePictures(Context.User.Id, chosen);

            if(Program.CompletedIconBlock(Context.User.Id, chosen))
            {
                List<int> newAllPictures = Data.Data.GetPictures(Context.User.Id);
                List<int> Specials = new List<int>();


                foreach (int item in newAllPictures)
                {
                    if(item > 1000)
                    {
                        Specials.Add(item);
                    }
                }
                int tmp;

                if (Specials.Count == 10)
                {
                    return;
                }

                do
                {
                    tmp = 1000 + rad.Next(1, 11);
                } while (Specials.Contains(tmp));

                await ReplyAsync($"Upon completing a full icon page, you got a special icon #{tmp}, type 'kush icons specials'");

                await Data.Data.UpdatePictures(Context.User.Id, tmp);
            }

        }

        public SixLabors.ImageSharp.Point GetPointByIndex(int index, int OneSize)
        {
            while(index >= 9)
            {
                index -= 9;
            }

            Point ret = new Point();

            int XFinder = index % 3;

            int YFinder = index / 3;

            ret.X = OneSize * XFinder;
            ret.Y = OneSize * YFinder;

            return ret;
        }

    }
}
