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
using KushBot.DataClasses;
using System.Threading.Channels;
using KushBot.Global;

namespace KushBot.Modules
{

    public class Inventory : ModuleBase<SocketCommandContext>
    {

        public int GetUpgradeCost(Item item)
        {
            int ret = 0;

            ret += 200 + 22 * item.Rarity;

            for (int i = 0; i < item.Level; i++)
            {
                ret += 50 + (item.Level * 13);
            }

            return ret;
        }

        public string GetLevelSubString(Item item)
        {

            if (item.Level > 1)
            {
                return $"({item.Level})";
            }
            return "";
        }

        public string GetRarityEmote(int rarity)
        {
            switch (rarity)
            {
                case 2:
                    return ":green_square:";

                case 3:
                    return ":blue_square:";

                case 4:
                    return ":purple_square:";

                case 5:
                    return ":orange_square:";

                case 6:
                    return ":red_square:";

                default:
                    return ":white_large_square:";
            }
        }

        private List<Item> SortList(List<int> equiped, List<Item> items)
        {
            List<Item> newItems = new List<Item>();

            for (int i = 0; i < 4; i++)
            {
                if (equiped[i] != 0)
                {
                    newItems.Add(items.Where(x => x.Id == equiped[i]).FirstOrDefault());
                    items.Remove(newItems.Last());
                }
            }

            for (int i = 0; i < items.Count; i++)
            {
                newItems.Add(items[i]);
            }

            return newItems;
        }

        [Command("improve"), Alias("levelup", "upgrade")]
        public async Task UpgradeItem([Remainder] string input)
        {
            List<Item> items = Data.Data.GetUserItems(Context.User.Id);
            int id;
            if (int.TryParse(input, out id))
            {

            }
            else
            {
                if (items.Where(x => x.Name.ToLower() == input.ToLower()).Count() < 1)
                {
                    await ReplyAsync($"{Context.User.Mention} You dont have that item dumb fucking bitch");
                    return;
                }
                id = items.Where(x => x.Name.ToLower() == input.ToLower()).FirstOrDefault().Id;
            }
            if (items.Where(x => x.Name.ToLower() == input.ToLower()).Count() > 1)
            {
                await ReplyAsync($"{Context.User.Mention} You have 2 or more items of the same name, use the id instead");
                return;
            }

            if (items.Where(x => x.Id == id).Count() < 1)
            {
                await ReplyAsync($"{Context.User.Mention} You dont have that item dumb fucking bitch");
                return;
            }

            Item selectedItem = items.Where(x => x.Id == id).FirstOrDefault();

            if (Data.Data.GetUserCheems(Context.User.Id) < GetUpgradeCost(selectedItem))
            {
                await ReplyAsync($"{Context.User.Mention} Not enough cheems pepew");
                return;
            }
            int upgradeCost = GetUpgradeCost(selectedItem);

            await Data.Data.AddUserCheems(Context.User.Id, -1 * GetUpgradeCost(selectedItem));
            selectedItem.Level += 1;
            await Data.Data.UpgradeItem(selectedItem);




            await ReplyAsync($"{Context.User.Mention} You upgraded your {selectedItem.Name} for {upgradeCost} cheems");
        }

        [Command("Inventory"), Alias("Inv")]
        public async Task ShowInv()
        {
            List<Item> itemsUnsorted = Data.Data.GetUserItems(Context.User.Id);


            List<int> Equiped = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                Equiped.Add(Data.Data.GetEquipedItem(Context.User.Id, i + 1));
            }
            List<Item> items = SortList(Equiped, itemsUnsorted);

            EmbedBuilder builder = new EmbedBuilder();


            builder.WithTitle($"{Context.User.Username}'s Inventory: {items.Count}/{Program.ItemCap}");

            foreach (var item in items)
            {

                string equipText = "";
                string name = $"{GetLevelSubString(item)}{GetRarityEmote(item.Rarity)}{char.ToUpper(item.Name[0])}{item.Name.Substring(1)}";

                if (items.Where(x => x.Name.ToLower() == item.Name.ToLower()).Count() > 1)
                {
                    name += $" id:{item.Id}";
                }

                if (Equiped.Contains(item.Id))
                {
                    await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 3, 1, Context.Channel);
                    equipText += $"*Equiped* :shield: slot:{Equiped.IndexOf(item.Id) + 1}\n";
                }


                builder.AddField($"**{name}**", equipText + item.GetItemDescription() + $"Upgrade cost: **{GetUpgradeCost(item)}**", true);
            }

            builder.WithColor(Discord.Color.Gold);
            builder.AddField("Cheems <:Cheems:945704650378707015>", $"{Data.Data.GetUserCheems(Context.User.Id)} cheems");

            await ReplyAsync("", false, builder.Build());

        }

        [Command("Inventory"), Alias("Inv")]
        public async Task ShowInv(IUser User)
        {
            List<Item> itemsUnsorted = Data.Data.GetUserItems(User.Id);


            List<int> Equiped = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                Equiped.Add(Data.Data.GetEquipedItem(User.Id, i + 1));
            }
            List<Item> items = SortList(Equiped, itemsUnsorted);

            EmbedBuilder builder = new EmbedBuilder();


            builder.WithTitle($"{User.Username}'s Inventory: {items.Count}/{Program.ItemCap}");
            foreach (var item in items)
            {

                string equipText = "";
                string name = $"{GetLevelSubString(item)}{GetRarityEmote(item.Rarity)}{char.ToUpper(item.Name[0])}{item.Name.Substring(1)}";

                if (items.Where(x => x.Name == item.Name).Count() > 1)
                {
                    name += $" id:{item.Id}";
                }

                if (Equiped.Contains(item.Id))
                    equipText += $"*Equiped* :shield: slot:{Equiped.IndexOf(item.Id) + 1}\n";


                builder.AddField($"**{name}**", equipText + item.GetItemDescription(), true);

            }

            builder.WithColor(Discord.Color.Gold);

            builder.AddField("Cheems <:Cheems:945704650378707015>", $"{Data.Data.GetUserCheems(User.Id)} cheems");

            await ReplyAsync("", false, builder.Build());

        }

        [Command("destroy"), Alias("unequip")]
        public async Task destroyItem([Remainder] string input)
        {
            List<Item> items = Data.Data.GetUserItems(Context.User.Id);

            int id;
            if (int.TryParse(input, out id))
            {

            }
            else
            {
                if (items.Where(x => x.Name.ToLower() == input.ToLower()).Count() < 1)
                {
                    await ReplyAsync($"{Context.User.Mention} You dont have that item dumb fucking bitch");
                    return;
                }
                id = items.Where(x => x.Name.ToLower() == input.ToLower()).FirstOrDefault().Id;
            }

            if (items.Where(x => x.Name.ToLower() == input.ToLower()).Count() > 1)
            {
                await ReplyAsync($"{Context.User.Mention} You have 2 or more items of the same name, use the id instead");
                return;
            }

            if (items.Where(x => x.Id == id).Count() < 1)
            {
                await ReplyAsync($"{Context.User.Mention} You dont have that item dumb fucking bitch");
                return;
            }

            Item selectedItem = items.Where(x => x.Id == id).FirstOrDefault();

            List<int> Equiped = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                Equiped.Add(Data.Data.GetEquipedItem(Context.User.Id, i + 1));
                if (Equiped[i] == selectedItem.Id)
                {
                    await Data.Data.SaveEquipedItem(Context.User.Id, i + 1, 0);
                }
            }



            Random rad = new Random();

            int cheems = 0;

            cheems += selectedItem.Rarity * 40;

            for (int i = 0; i < selectedItem.Rarity + 1; i++)
            {
                cheems += rad.Next(15, 36);
            }

            cheems += (selectedItem.Level - 1) * rad.Next(65, 75);

            cheems += GetUpgradeCost(selectedItem) / 4;

            await Data.Data.AddUserCheems(Context.User.Id, cheems);

            await Data.Data.DestroyItem(selectedItem.Id);

            await ReplyAsync($"{Context.User.Mention} You destroyed {selectedItem.Name} and got {cheems} cheems! <:Cheems:945704650378707015>");
        }


        [Command("equip")]
        public async Task EquipItem([Remainder] string input)
        {
            var user = Data.Data.GetKushBotUser(Context.User.Id, UserDtoFeatures.Items);

            var items = user.Items.GetItemsByString(input);

            if (items == null || !items.Any())
            {
                await ReplyAsync($"{Context.User.Mention} You dont have that item dumb fucking bitch");
                return;
            }

            if (items.Count > 1)
            {
                await ReplyAsync($"{Context.User.Mention} You have 2 or more items of the same name, use the id instead");
                return;
            }

            var item = items[0];
            await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 3, 1, Context.Channel);

            if (user.Items.Equipped.Any(e => e.Id == item.Id))
            {
                await ReplyAsync($"{Context.User.Mention} You already have that equipped. dubm fbyhudsadaslkjn");
                return;
            }

            // ????? double check this, unequip is no longer a command iirc
            if (user.Items.Equipped.Count >= Items.EquipLimit)
            {
                await ReplyAsync($"{Context.User.Mention} All of your item slots are equipped." +
                    $" type 'kush unequip *slotNumber*' to unequip. Slot numbers E [1;4] **UNEQUIPPED ITEMS WILL BE DESTROYED**");
                return;
            }

            GenerateNewPortrait(Context.User.Id);

            await ReplyAsync($"{Context.User.Mention} Successfully equipped {item.Name}");
            item.IsEquipped = true;

            await Data.Data.SaveKushBotUserAsync(user, UserDtoFeatures.Items);
        }

        public static void GenerateNewPortrait(ulong userId)
        {
            //TODO Temporary, remove later
            var user = Data.Data.GetKushBotUser(userId);

            string path = @"Data/";
            char seperator = '/';

            int selectedPic = user.SelectedPicture;
            string outputpath = $"{path}Portraits/{userId}.png";

            if (selectedPic > 1000)
                outputpath = $"{path}Portraits/{userId}.gif";



            List<Item> items = Data.Data.GetUserItems(userId);
            items.Add(new Item(userId, "empty"));
            items.Last().Id = 0;

            List<int> equiped = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                equiped.Add(Data.Data.GetEquipedItem(userId, i + 1));
            }

            List<Point> points = new List<Point>();


            if (selectedPic > 1000)
            {
                System.IO.File.Copy($@"{path}Pictures/{selectedPic}.gif",
                    $@"{outputpath}", true);

                return;
            }

            var a = Directory.GetFiles($"{path}{(items.FirstOrDefault(x => x.Id == equiped[0]).Rarity == 6 ? "ArchonItems" : "Items")}");

            using (SixLabors.ImageSharp.Image bg = SixLabors.ImageSharp.Image.Load($"{path}Pictures{seperator}{selectedPic}.jpg"))
            using (SixLabors.ImageSharp.Image sl1 = SixLabors.ImageSharp.Image.Load($"{path}{(items.FirstOrDefault(x => x.Id == equiped[0]).Rarity == 6 ? "ArchonItems" : "Items")}{seperator}{items.FirstOrDefault(x => x.Id == equiped[0]).Name}.png"))

            using (SixLabors.ImageSharp.Image sl2 = SixLabors.ImageSharp.Image.Load($"{path}{(items.FirstOrDefault(x => x.Id == equiped[1]).Rarity == 6 ? "ArchonItems" : "Items")}{seperator}{items.Where(x => x.Id == equiped[1]).FirstOrDefault().Name}.png"))
            using (SixLabors.ImageSharp.Image sl3 = SixLabors.ImageSharp.Image.Load($"{path}{(items.FirstOrDefault(x => x.Id == equiped[2]).Rarity == 6 ? "ArchonItems" : "Items")}{seperator}{items.Where(x => x.Id == equiped[2]).FirstOrDefault().Name}.png"))
            using (SixLabors.ImageSharp.Image sl4 = SixLabors.ImageSharp.Image.Load($"{path}{(items.FirstOrDefault(x => x.Id == equiped[3]).Rarity == 6 ? "ArchonItems" : "Items")}{seperator}{items.Where(x => x.Id == equiped[3]).FirstOrDefault().Name}.png"))
            using (SixLabors.ImageSharp.Image outputImage = bg)
            {
                sl1.Mutate(x => x.Resize(sl1.Width / 3, sl1.Height / 3));
                sl2.Mutate(x => x.Resize(sl2.Width / 3, sl2.Height / 3));
                sl3.Mutate(x => x.Resize(sl3.Width / 3, sl3.Height / 3));
                sl4.Mutate(x => x.Resize(sl4.Width / 3, sl4.Height / 3));

                points.Add(new Point(0, 430));
                points.Add(new Point(144, 430));
                points.Add(new Point(288, 430));
                points.Add(new Point(144 * 3, 430));
                outputImage.Mutate(x => x
                .DrawImage(sl1, points[0], 1f)
                .DrawImage(sl2, points[1], 1f)
                .DrawImage(sl3, points[2], 1f)
                .DrawImage(sl4, points[3], 1f)
                );

                outputImage.Save(outputpath);
            }
        }

    }
}
