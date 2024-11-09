using KushBot.DataClasses.enums;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Linq;
using System;
using KushBot.DataClasses;

namespace KushBot.Services;

public class PortraitManager
{
    public void GeneratePortrait(KushBotUser user)
    {
        string path = @"Data/";
        char seperator = '/';

        var selectedPic = user.SelectedPicture;
        string outputpath = $"{path}Portraits/{user.Id}.png";

        var isGif = selectedPic.StartsWith("gif");

        if (isGif)
        {
            outputpath = $"{path}Portraits/{user.Id}.gif";
        }

        var items = new UserItems(user.Items.OrderByDescending(e => e.IsEquipped).ThenBy(e => e.Name).ToList());

        List<Point> points = new List<Point>();

        if (isGif)
        {
            System.IO.File.Copy($@"{path}Pictures/{selectedPic}.gif",
                $@"{outputpath}", true);

            return;
        }

        string getPath(int slotIndex)
        {
            var item = items.Equipped[slotIndex];
            if (item is null)
            {
                return $"Data/Items/empty.png";
            }
            return $"{path}{(item?.Rarity == RarityType.Archon ? "ArchonItems" : "Items")}{seperator}{item?.Name}.png";
        }

        using (Image bg = Image.Load($"{path}Pictures{seperator}{selectedPic}"))
        using (Image sl1 = Image.Load(getPath(0)))
        using (Image sl2 = Image.Load(getPath(1)))
        using (Image sl3 = Image.Load(getPath(2)))
        using (Image sl4 = Image.Load(getPath(3)))
        using (Image outputImage = bg)
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

    public bool TryGetGif(KushBotUser user, UserPicture newPicture, out UserPicture gif)
    {
        if (!IsIconBlockCompleted(user, newPicture))
        {
            gif = null;
            return false;
        }

        var availableGifCount = DiscordBotService.PictureCount / 9;
        var allGifs = Enumerable.Range(1, availableGifCount).Select(e => $"gif{e}.gif");
        var ownedGifs = user.UserPictures.Where(e => e.IsGif).Select(e => e.Path);

        var availableGifs = allGifs.Except(ownedGifs);

        var newGif = availableGifs.Except(ownedGifs).ToList()[Random.Shared.Next(0, availableGifs.Count())];

        gif = new UserPicture(user.Id, newGif);
        return true;
    }

    public bool IsIconBlockCompleted(KushBotUser user, UserPicture newPicture)
    {
        int bracket = GetBracket(newPicture.Representation.Value);
        int TopId = bracket + 9;

        for (int i = 0; i < 9; i++)
        {
            if (!user.UserPictures.Select(e => e.Representation).Contains(bracket + i + 1))
            {
                return false;
            }
        }

        return true;
    }

    private int GetBracket(int chosen)
    {
        int num = (chosen - 1) / 9;
        return num * 9;
    }
}
