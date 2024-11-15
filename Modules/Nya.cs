﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Rest;
using System.Linq;
using KushBot.Global;
using KushBot.Modules.Interactions;
using Discord.WebSocket;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using KushBot.DataClasses;
using System.IO;

namespace KushBot.Modules;

[RequirePermissions(Permissions.Nya, true)]
public class Nya(SqliteDbContext dbContext, DiscordSocketClient client) : ModuleBase<SocketCommandContext>
{
    public static List<string> WeebPaths = new List<string>();
    public static List<string> CarPaths = new List<string>();

    public static void ReadWeebPaths()
    {
        string path = "Data/Kemonos";
        string[] files = Directory.GetFiles(path);

        WeebPaths = files.ToList();
    }

    public static void ReadCarPaths()
    {
        string path = "Data/Cars";
        string[] files = Directory.GetFiles(path);

        CarPaths = files.ToList();
    }

    [Command("nya marry delay"), Alias("vroom marry delay")]
    public async Task DelayCd()
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(e => e.Id == Context.User.Id);

        var newMarryDate = user.LastNyaClaim > DateTime.Now ? user.LastNyaClaim : DateTime.Now;
        newMarryDate = newMarryDate.AddDays(1);

        var ts = newMarryDate.ToUniversalTime() - new DateTime(1970, 1, 1);

        user.LastNyaClaim = newMarryDate;
        await dbContext.SaveChangesAsync();

        await ReplyAsync($"{Context.User.Mention} you pushed back your next marry time by 1 day, Next nya marry: <t:{(int)ts.TotalSeconds}:R>");
    }

    [Command("nya marry"), Alias("vroom marry")]
    public async Task NyaMarry()
    {
        var lastNyaMarry = await dbContext.Users
            .Where(e => e.Id == Context.User.Id)
            .Select(e => e.NyaMarryDate)
            .FirstOrDefaultAsync();

        if (lastNyaMarry > DateTime.Now)
        {
            TimeSpan ts = lastNyaMarry - DateTime.Now;
            await ReplyAsync($"{CustomEmojis.PepeShy} {Context.User.Mention} You still need to wait {ts.Hours:d2}:{ts.Minutes:d2}:{ts.Seconds:d2} before you " +
                $"can remarry {CustomEmojis.PepeShy}");
            return;
        }

        if (NyaClaimGlobals.Engagements.Contains(Context.User.Id))
        {
            await ReplyAsync($"{Context.User.Mention} You dumb fucking shit fuck you dumb fuck retard bitch die adopted shit ape nigger");
            return;
        }

        NyaClaimGlobals.Engagements.Add(Context.User.Id);

        await ReplyAsync($"{CustomEmojis.PepeShy} {Context.User.Mention} you're prepared to engage. The next kush nya (or kush vroom {CustomEmojis.Pepew}) you " +
            $"roll will get to be on your stats page {CustomEmojis.PepeShy}");
    }

    public async Task HandleNyaClaim(List<string> paths)
    {
        var claimedImages = await dbContext.NyaClaims.Select(e => e.FileName).ToListAsync();

        string getPath()
        {
            int index = Random.Shared.Next(0, paths.Count());
            return paths.ElementAt(index).Replace("\\", "/");
        }

        string path = getPath();

        if (!claimedImages.Contains(path) && Random.Shared.NextDouble() > 0.5)
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
        var claim = await dbContext.NyaClaims.FirstOrDefaultAsync(e => e.FileName == path);
        if (claim is null)
        {
            await ReplyAsync($"{Context.User.Mention} aint it");
        }

        EmbedBuilder builder = new();
        builder.AddField(claim.OwnerId == Context.User.Id ? "+1 :key2:" : "\u200b", "Claim is still active, you can keep rolling!");
        builder.WithImageUrl(claim.Url);

        var user = client.GetUser(claim.OwnerId);

        builder.WithFooter($"Belongs to {user?.GlobalName ?? "someone else"}", user?.GetAvatarUrl());

        if (claim.OwnerId == Context.User.Id)
        {
            claim.Keys += 1;
            await dbContext.SaveChangesAsync();
        }
        await ReplyAsync(embed: builder.Build());
    }

    public async Task SendWithComponentAsync(string path)
    {
        NyaClaimGlobals.ClaimReadyUsers.Remove(Context.User.Id);

        var message = await Context.Channel.SendFileAsync(path, components: ClaimNya.BuildMessageComponent(false));

        NyaClaimGlobals.NyaClaimEvents.Add(message.Id, new()
        {
            UserId = Context.User.Id,
            ImageMessage = message,
            FileName = path,
            TimeStamp = DateTime.Now
        });
    }

    [Command("nya")]
    public async Task Throw()
    {
        if (NyaClaimGlobals.ClaimReadyUsers.Contains(Context.User.Id))
        {
            await HandleNyaClaim(WeebPaths);
            return;
        }

        int index = Random.Shared.Next(0, WeebPaths.Count);

        string send = WeebPaths[index];

        var picture = await Context.Channel.SendFileAsync($"{send}");

        if (NyaClaimGlobals.Engagements.Contains(Context.User.Id))
        {
            NyaClaimGlobals.Engagements.Remove(Context.User.Id);
            
            var user = await dbContext.GetKushBotUserAsync(Context.User.Id);
            user.NyaMarry = picture.Attachments.First().Url;
            user.NyaMarryDate = user.NyaMarryDate.AddHours(6);
            
            await ReplyAsync($"{Context.User.Mention} You succesfully married {CustomEmojis.Pog}{CustomEmojis.Pepew}{CustomEmojis.PepeShy}");
            await dbContext.SaveChangesAsync();
        }

        if (Random.Shared.NextDouble() <= 0.0005)
        {
            int baps = Random.Shared.Next(10, 25);
            await dbContext.Users
                .Where(e => e.Id == Context.User.Id)
                .ExecuteUpdateAsync(e => e.SetProperty(x => x.Balance, x => x.Balance + baps));

            await ReplyAsync($":3 {Context.User.Mention} OwO you uncovered {baps} baps >w<");
        }
    }

    [Command("vroom")]
    public async Task ThrowCar()
    {
        if (NyaClaimGlobals.ClaimReadyUsers.Contains(Context.User.Id))
        {
            await HandleNyaClaim(CarPaths);
            return;
        }

        int index = Random.Shared.Next(0, CarPaths.Count);

        var picture = await Context.Channel.SendFileAsync($"{CarPaths[index]}") as RestUserMessage;

        if (NyaClaimGlobals.Engagements.Contains(Context.User.Id))
        {
            NyaClaimGlobals.Engagements.Remove(Context.User.Id);
            var user = await dbContext.GetKushBotUserAsync(Context.User.Id);
            user.NyaMarry = picture.Attachments.First().Url;
            user.NyaMarryDate = user.NyaMarryDate.AddHours(6);

            await ReplyAsync($"{Context.User.Mention} You succesfully married {CustomEmojis.Pog}{CustomEmojis.Pepew}{CustomEmojis.PepeShy}");
            await dbContext.SaveChangesAsync();
        }

    }
}
