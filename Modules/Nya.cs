using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Rest;
using System.Linq;
using KushBot.Global;
using KushBot.EventHandler.Interactions;

namespace KushBot.Modules;

public class Nya : ModuleBase<SocketCommandContext>
{
    public static bool test = false;
    public static string testString = "lhYgEDF";

    [Command("nya test")]
    public async Task nyaTest()
    {
        if (Context.User.Id == 192642414215692300)
            test = true;
    }

    [Command("nya marry delay", RunMode = RunMode.Async), Alias("vroom marry delay")]
    public async Task DelayCd()
    {
        var lastMarry = Data.Data.GetNyaMarryDate(Context.User.Id);
        DateTime newMarryDate = lastMarry > DateTime.Now ? lastMarry : DateTime.Now;
        newMarryDate = newMarryDate.AddDays(1);

        TimeSpan ts = newMarryDate.ToUniversalTime() - new DateTime(1970, 1, 1);
        await Data.Data.SaveNyaMarryDate(Context.User.Id, newMarryDate);
        await ReplyAsync($"{Context.User.Mention} you pushed back your next marry time by 1 day, Next nya marry: <t:{(int)ts.TotalSeconds}:R>");
    }

    [Command("nya marry", RunMode = RunMode.Async), Alias("vroom marry")]
    public async Task NyaMarry()
    {
        DateTime lastNyaMarry = Data.Data.GetNyaMarryDate(Context.User.Id);
        if (lastNyaMarry > DateTime.Now)
        {
            TimeSpan ts = lastNyaMarry - DateTime.Now;
            await ReplyAsync($"<:pepeshy:948015871199182858> {Context.User.Mention} You still need to wait {ts.Hours:d2}:{ts.Minutes:d2}:{ts.Seconds:d2} before you " +
                $"can remarry <:pepeshy:948015871199182858>");
            return;
        }

        if (DiscordBotService.Engagements.Contains(Context.User.Id))
        {
            await ReplyAsync($"{Context.User.Mention} You dumb fucking shit fuck you dumb fuck retard bitch die adopted shit ape nigger");
            return;
        }


        DiscordBotService.Engagements.Add(Context.User.Id);

        await ReplyAsync($"<:pepeshy:948015871199182858> {Context.User.Mention} you're prepared to engage. The next kush nya (or kush vroom <:Pepew:945806849406566401>) you " +
            $"roll will get to be on your stats page <:pepeshy:948015871199182858>");
    }

    public async Task HandleNyaClaim(List<string> paths)
    {
        Random rnd = new Random();
        var claimedImages = Data.Data.GetClaimedImgPaths();

        string getPath()
        {
            int index = rnd.Next(0, paths.Count());
            return paths.ElementAt(index).Replace("\\", "/");
        }

        string path = getPath();

        if (!claimedImages.Contains(path) && rnd.NextDouble() > 0.5)
        {
            path = getPath();
        }

        if (claimedImages.Contains(path))
        {
            await SendInEmbedAsync(path);
        }
        else
        {
            await SendWithComponentAsync(path);
        }
    }

    public async Task SendInEmbedAsync(string path)
    {
        var claim = Data.Data.GetClaimByPath(path);
        if (claim is null)
        {
            await ReplyAsync($"{Context.User.Mention} aint it");
        }

        EmbedBuilder builder = new();
        builder.AddField(claim.OwnerId == Context.User.Id ? "+1 :key2:" : "\u200b", "Claim is still active, you can keep rolling!");
        builder.WithImageUrl(claim.Url);

        var user = DiscordBotService._client.GetUser(claim.OwnerId);

        builder.WithFooter($"Belongs to {user?.GlobalName ?? "someone else"}", user?.GetAvatarUrl());

        if (claim.OwnerId == Context.User.Id)
        {
            await Data.Data.IncrementKeysForClaimAsync(claim.Id);
        }
        await ReplyAsync(embed: builder.Build());
    }

    public async Task SendWithComponentAsync(string path)
    {
        ComponentBuilder builder = new ComponentBuilder();
        Guid guid = Guid.NewGuid();
        NyaClaimGlobals.ClaimReadyUsers.Remove(Context.User.Id);

        builder.WithButton("Claim", customId: $"{InteractionHandlerFactory.NyaClaimComponentId}_{guid}",
                emote: Emote.Parse(CustomEmojis.Ima),
                style: ButtonStyle.Secondary);

        var message = await Context.Channel.SendFileAsync(path, components: builder.Build());

        NyaClaimGlobals.NyaClaimEvents.Add(guid, new NyaClaimEvent()
        {
            UserId = Context.User.Id,
            ImageMessage = message,
            FileName = path,
            TimeStamp = DateTime.Now
        });


    }

    [Command("nya", RunMode = RunMode.Async)]
    public async Task Throw()
    {
        if (NyaClaimGlobals.ClaimReadyUsers.Contains(Context.User.Id))
        {
            await HandleNyaClaim(DiscordBotService.WeebPaths);
            return;
        }

        Random rnd = new Random();

        int index = rnd.Next(0, DiscordBotService.WeebPaths.Count);

        string send = DiscordBotService.WeebPaths[index];

        EmbedBuilder builder = new();

        if (test && Context.User.Id == 192642414215692300)
        {
            send = DiscordBotService.WeebPaths.Where(x => x.Contains(testString)).FirstOrDefault();
            test = false;
        }
        var picture = await Context.Channel.SendFileAsync($"{send}") as RestUserMessage;

        if (DiscordBotService.Engagements.Contains(Context.User.Id))
        {
            await Data.Data.SaveNyaMarry(Context.User.Id, picture.Attachments.First().Url);
            DiscordBotService.Engagements.Remove(Context.User.Id);
            await Data.Data.AddToNyaMarryDate(Context.User.Id, 6);
            await ReplyAsync($"{Context.User.Mention} You succesfully married <:Pog:948018159665938462><:Pepew:945806849406566401><:pepeshy:948015871199182858>");
        }

        if (rnd.NextDouble() <= 0.0005)
        {
            int baps = rnd.Next(10, 25);
            await Data.Data.SaveBalance(Context.User.Id, baps, false);
            await ReplyAsync($":3 {Context.User.Mention} OwO you uncovered {baps} baps >w<");
        }
    }

    [Command("vroom", RunMode = RunMode.Async)]
    public async Task ThrowCar()
    {
        if (NyaClaimGlobals.ClaimReadyUsers.Contains(Context.User.Id))
        {
            await HandleNyaClaim(DiscordBotService.CarPaths);
            return;
        }

        Random rnd = new Random();

        int index = rnd.Next(0, DiscordBotService.CarPaths.Count);

        var picture = await Context.Channel.SendFileAsync($"{DiscordBotService.CarPaths[index]}") as RestUserMessage;

        if (DiscordBotService.Engagements.Contains(Context.User.Id))
        {
            await Data.Data.SaveNyaMarry(Context.User.Id, picture.Attachments.First().Url);
            DiscordBotService.Engagements.Remove(Context.User.Id);
            await Data.Data.AddToNyaMarryDate(Context.User.Id, 6);
            await ReplyAsync($"{Context.User.Mention} You succesfully married <:Pog:948018159665938462><:Pepew:945806849406566401><:pepeshy:948015871199182858>");
        }

    }
}
