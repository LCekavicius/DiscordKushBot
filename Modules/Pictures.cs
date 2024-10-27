using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp.Drawing.Processing;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using Discord.WebSocket;
using KushBot.Modules.Interactions;

namespace KushBot.Modules;

[Group("Icons")]
public class Pictures : ModuleBase<SocketCommandContext>
{
    private readonly PortraitManager _portraitManager;
    private readonly SqliteDbContext _context;
    private const string Path = @"Data/Pictures";

    private readonly DiscordSocketClient _client;

    public Pictures(PortraitManager portraitManager, SqliteDbContext context, DiscordSocketClient client)
    {
        _portraitManager = portraitManager;
        _context = context;
        _client = client;
    }

    [Command("Select")]
    public async Task SelectPicture(string picture)
    {
        var user = await _context.Users
            .Include(e => e.UserPictures)
            .Include(e => e.Items.Where(e => e.IsEquipped))
            .FirstOrDefaultAsync(e => e.Id == Context.User.Id);

        var picturesOwned = user.UserPictures;

        var picturePath = picture.StartsWith("gif") ? $"{picture}.gif" : $"{picture}.jpg";

        if (!picturesOwned.Any(e => e.Path.Equals(picturePath)))
        {
            await ReplyAsync($"{Context.User.Mention} You don't have that icon, obese");
            return;
        }

        user.SelectedPicture = picturePath;
        _portraitManager.GeneratePortrait(user);

        await ReplyAsync($"{Context.User.Mention} you updated your icon");
    }

    [Command("gifs")]
    public async Task ShowSpecials()
    {
        var picturesOwned = await _context.UserPictures
            .Where(e => e.OwnerId == Context.User.Id)
            .ToListAsync();

        var gifs = picturesOwned.Where(e => e.IsGif).ToList();

        if (gifs.Count == 0)
        {
            await ReplyAsync($"{Context.User.Mention} you knormal?");
            return;
        }

        // TODO make a paginated embed
        string print = $"{Context.User.Mention} you own these gif icons: {string.Join(", ", gifs.Select(e => System.IO.Path.GetFileNameWithoutExtension(e.Path)))}";

        print += "\nEquip them by typing 'kush icons select *icon*' (e.g. kush icons select gif3)";

        await ReplyAsync(print);
    }

    [Command("recent")]
    public async Task ShowRecent(IUser user = null)
    {
        user ??= Context.User;

        var icons = await _context.UserPictures
            .Where(e => e.OwnerId == user.Id)
            .ToListAsync();

        Embed embed = await GetRecentIconEmbed(user.Id, 0, icons.Count);

        if (NyaClaimGlobals.PaginatedEmbed.ContainsKey(Context.User.Id))
        {
            NyaClaimGlobals.PaginatedEmbed.Remove(Context.User.Id);
        }

        PaginatedEmbed paginatedEmbed = new PaginatedEmbed()
        {
            CurrentPage = 0,
            TotalPages = icons.Count,
            GetPageEmbedAsync = GetRecentIconEmbed,
            OwnerId = user.Id,
        };

        var msg = await ReplyAsync(embed: embed, components: ScrollEmbed.BuildMessageComponent(false));

        NyaClaimGlobals.PaginatedEmbed.Add(msg.Id, paginatedEmbed);
    }

    public async Task<Embed> GetRecentIconEmbed(ulong ownerId, int index, int totalPages)
    {
        using var context = new SqliteDbContext();

        var icons = await context.UserPictures
            .Where(e => e.OwnerId == ownerId)
            .ToListAsync();

        icons.Reverse();

        var dumpChannel = _client.GetChannel(DiscordBotService.DumpChannelId) as IMessageChannel;

        var attachmentName = $"{System.IO.Path.GetFileNameWithoutExtension(icons[index].Path)}{(icons[index].IsGif ? ".gif" : ".jpg")}";

        var uploadedFile = await dumpChannel.SendFileAsync($"Data/Pictures/{attachmentName}");

        EmbedBuilder builder = new EmbedBuilder();
        builder.WithImageUrl(uploadedFile.Attachments.FirstOrDefault()?.Url ?? "");
        builder.WithColor(Discord.Color.Green);
        builder.WithTitle($"#{System.IO.Path.GetFileNameWithoutExtension(icons[index].Path)}");
        builder.WithFooter($"Belongs to {_client.GetUser(ownerId).GlobalName} ~~ {index + 1} / {totalPages}", _client.GetUser(ownerId).GetAvatarUrl());

        return builder.Build();
    }


    [Command("")]
    public async Task Show(int showPage = 1)
    {
        if (showPage > DiscordBotService.PictureCount / 9 || showPage <= 0)
        {
            await ReplyAsync($"{Context.User.Mention} that page doesnt exist");
            return;
        }

        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 1, 3, Context.Channel);

        int width = 576;
        int height = 576;

        int singleWidth = 192;
        int singleHeight = 192;

        string path = @"Data/Pictures";

        string fontFamily = "Arial";
        using (StreamReader reader = new StreamReader($"{path}/font.txt"))
        {
            fontFamily = reader.ReadLine();
        }
        var font = SixLabors.Fonts.SystemFonts.CreateFont($"{fontFamily}", 80);

        var userPictures = await _context.UserPictures.Where(e => e.OwnerId == Context.User.Id).ToListAsync();

        var ownedPictureRepresentations = userPictures.Where(e => !e.IsGif).Select(e => e.Representation.Value).ToList();

        using var portraitImage = new SixLabors.ImageSharp.Image<Rgba32>(width, height);

        for (int i = 0 + 9 * (showPage - 1); i < 9 + 9 * (showPage - 1); i++)
        {
            using var img = SixLabors.ImageSharp.Image.Load($@"{path}/{i + 1}.jpg");

            img.Mutate(x => x.Resize(singleWidth, singleHeight));

            if (!ownedPictureRepresentations.Contains(i + 1))
            {
                using var redImg = SixLabors.ImageSharp.Image.Load($@"{path}/Red.jpg");

                img.Mutate(x => x
                 .DrawImage(redImg, 0.65f)
                 .BokehBlur()
                );

            }

            Point p = GetPointByIndex(i, singleWidth);
            Point tp = new Point(p.X, p.Y + 7);
            portraitImage.Mutate(x => x
             .DrawImage(img, p, 1f)
             .DrawText((i + 1).ToString(), font, SixLabors.ImageSharp.Color.White, tp)
            );

        }
        portraitImage.Save($"{path}/{Context.User.Id}.png");



        await Context.Channel.SendFileAsync($"{path}/{Context.User.Id}.png", "Type 'kush icons pageNumber' (e.g. kush icons 3) to check other pages\n" +
            "Type 'kush icons select pictureId (e.g. kush icons select 17) to *equip* an icon\n" +
            "Type 'kush icons recent' to see recently acquired icons\n" +
            $"Type 'kush icons buy' to buy a random icon\nUpon completing a page of icons, you will get a special animated gif icon\n" +
            $"Price for next icon: **{325 + ownedPictureRepresentations.Count * 25}** baps");

    }

    [Command("buy")]
    public async Task BuyPicture()
    {
        var user = await _context.Users
            .Include(e => e.UserPictures)
            .Include(e => e.Items.Where(x => x.IsEquipped))
            .FirstOrDefaultAsync(e => e.Id == Context.User.Id);

        int price = 325 + user.UserPictures.Count * 25;

        if (user.Balance < price)
        {
            await ReplyAsync($"{Context.User.Mention} you are too poor for my wares fag");
            return;
        }

        if (user.UserPictures.Count >= DiscordBotService.PictureCount + DiscordBotService.PictureCount / 9)
        {
            await ReplyAsync($"{Context.User.Mention} this guy tried to purchase more images than he could own, a lot of people want their taxpayer money to go to this man");
            return;
        }

        var pictureRange = Enumerable.Range(1, DiscordBotService.PictureCount).Select(e => $"{e}.jpg");

        var available = pictureRange.Except(user.UserPictures.Select(e => e.Path)).ToList();

        var chosen = available[Random.Shared.Next(0, available.Count)];

        string path = @"Data/Pictures";

        await Context.Channel.SendFileAsync($"{path}/{chosen}", $"{Context.User.Mention} You got #{System.IO.Path.GetFileNameWithoutExtension(chosen)} icon at the cost of: **{price}** baps!");

        var chosenPicture = new UserPicture(user.Id, chosen);

        user.UserPictures.Add(chosenPicture);
        user.Balance -= price;

        if (_portraitManager.TryGetGif(user, chosenPicture, out var newGif))
        {
            user.UserPictures.Add(newGif);
        }

        await _context.SaveChangesAsync();
    }

    public SixLabors.ImageSharp.Point GetPointByIndex(int index, int OneSize)
    {
        while (index >= 9)
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
