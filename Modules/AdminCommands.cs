﻿using Discord;
using Discord.Commands;
using KushBot.BackgroundJobs;
using KushBot.DataClasses;
using KushBot.DataClasses.enums;
using KushBot.DataClasses.Vendor;
using KushBot.Resources.Database;
using KushBot.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules;


public class AdminModule : ModuleBase<SocketCommandContext>
{
    private HashSet<ulong> Admins = new HashSet<ulong>();
    private readonly ISchedulerFactory schedulerFactory;
    private readonly VendorService vendorService;
    private readonly SqliteDbContext dbContext;

    public AdminModule(ISchedulerFactory schedulerFactory, VendorService vendorService, SqliteDbContext context)
    {
        this.schedulerFactory = schedulerFactory;
        this.vendorService = vendorService;
        dbContext = context;

        Admins.Add(192642414215692300);
        if (DiscordBotService.BotTesting)
        {
            Admins.Add(262629085858103296);
            Admins.Add(218087058223136778);
            Admins.Add(187483265865613312);
            Admins.Add(230743424263782400);
        }
    }

    [Command("archon", RunMode = RunMode.Async)]
    public async Task brea()
    {
        if (!Admins.Contains(Context.User.Id))
            return;

        //await Program.SpawnBoss(true, Context.User.Id);
    }

    [Command("spawn", RunMode = RunMode.Async)]
    public async Task bre()
    {
        if (!Admins.Contains(Context.User.Id))
            return;


        //await Program.SpawnBoss();
    }


    [Command("WeeklyReset")]
    public async Task ResetWeekly()
    {
        if (!Admins.Contains(Context.User.Id))
            return;
    }

    [Command("item")]
    public async Task CreateItemTemp(int rar)
    {
        if (!Admins.Contains(Context.User.Id))
            return;

        var rarity = (RarityType)rar;
        ItemManager manager = new();

        var user = await dbContext.GetKushBotUserAsync(Context.User.Id, UserDtoFeatures.Items);

        var item = manager.GenerateRandomItem(user, rarity);
        user.Items.Add(item);

        await dbContext.SaveChangesAsync();
    }

    [Command("infect")]
    public async Task Infect(IUser user)
    {
        if (!Admins.Contains(Context.User.Id))
            return;

        await dbContext.UserInfections.AddAsync(new()
        {
            OwnerId = user.Id,
            CreationDate = DateTime.Now,
            KillAttemptDate = DateTime.MinValue
        });

        await dbContext.SaveChangesAsync();
    }

    [Command("drop")]
    public async Task PingAsync(int amount, IUser user)
    {
        if (!Admins.Contains(Context.User.Id))
            return;

        await dbContext.Users.Where(e => e.Id == user.Id).ExecuteUpdateAsync(e => e.SetProperty(x => x.Balance, x => x.Balance + amount));
        await dbContext.SaveChangesAsync();
    }

    [Command("disable")]
    public async Task Disablebot()
    {
        if (!Admins.Contains(Context.User.Id))
            return;


        DiscordBotService.IsDisabled = true;
    }

    [Command("enable")]
    public async Task enablebot()
    {
        if (!Admins.Contains(Context.User.Id))
            return;


        DiscordBotService.IsDisabled = false;
    }


    [Command("clear")]
    public async Task clear(IUser user)
    {
        DiscordBotService.Test = 0;
        DiscordBotService.PetTest = 0;
        DiscordBotService.Fail = 0;
        DiscordBotService.NerfUser = 0;
    }

    [Command("check")]
    public async Task checke()
    {
        Console.WriteLine($"Test user: {DiscordBotService.Test}");
        Console.WriteLine($"Pet Test user: {DiscordBotService.PetTest}");
        Console.WriteLine($"Fail user: {DiscordBotService.Fail}");
        Console.WriteLine($"Nerf user: {DiscordBotService.NerfUser}");
    }

    [Command("test")]
    public async Task Tyst(ulong Id)
    {

        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        DiscordBotService.Test = Id;
        await Context.Message.DeleteAsync();

    }

    [Command("tier test")]
    public async Task tiertest(ulong Id)
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        DiscordBotService.TierTest = Id;
        await Context.Message.DeleteAsync();
    }



    [Command("fail")]
    public async Task Tyst2(ulong Id)
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        DiscordBotService.Fail = Id;
        await Context.Message.DeleteAsync();

    }

    [Command("pet test")]
    public async Task TystPet(ulong Id)
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        DiscordBotService.PetTest = Id;
        await Context.Message.DeleteAsync();

    }
    [Command("nerf")]
    public async Task nrf(ulong id)
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        DiscordBotService.NerfUser = id;
        await Context.Message.DeleteAsync();
    }

    [Command("provide quests")]
    public async Task AssignQuests()
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }

        var scheduler = await schedulerFactory.GetScheduler();
        var jobKey = JobKey.Create(nameof(ProvideQuestsJob), "DEFAULT");
        await scheduler.TriggerJob(jobKey);
    }

    [Command("forcew")]
    public async Task AssignQw()
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }

    }

    [Command("airdrop")]
    public async Task DropAirdrop()
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }

        var scheduler = await schedulerFactory.GetScheduler();
        var jobKey = JobKey.Create(nameof(AirDropJob));
        await scheduler.TriggerJob(jobKey);
    }

    [Command("set curse")]
    public async Task setcurse(IUser user, int length, string curse)
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        if (curse != "isnyk" && curse != "degenerate" && curse != "asked")
        {
            return;
        }

        CursedPlayer cp = new CursedPlayer(user.Id, curse, length);
        MessageHandler.CursedPlayers.Add(cp);
    }


    [Command("Nordi")]
    public async Task plot()
    {
        Random rad = new Random();

        switch (rad.Next(0, 4))
        {
            case 0:
                await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lto5o0");
                break;
            case 1:
                await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lu3wls");
                break;
            case 2:
                await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lu3ynh");
                break;
            case 3:
                await ReplyAsync($"{Context.User.Mention} http://prntscr.com/lx28iy");
                break;
        }
    }
    [Command("Excavatum")]
    public async Task excata()
    {

        EmbedBuilder builder = new EmbedBuilder();

        builder.WithImageUrl("https://cdn.discordapp.com/attachments/660888274427969537/806210221696483328/unknown.png");

        await ReplyAsync("", false, builder.Build());
    }

    [Command("Attach vendor")]
    public async Task AttachVendor()
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }

        await vendorService.GenerateVendorAsync();
    }

    [Command("Restock")]
    public async Task restockVendor()
    {
        if (Context.User.Id != 192642414215692300 && Context.User.Id != 187483265865613312 && Context.User.Id != 230743424263782400)
        {
            return;
        }

        if (vendorService.Properties == null)
        {
            await ReplyAsync("Vendor is detached");
            return;
        }

        var scheduler = await schedulerFactory.GetScheduler();
        var jobKey = JobKey.Create(nameof(RefreshVendorJob), "DEFAULT");
        await scheduler.TriggerJob(jobKey);
    }

    [Command("toggle prohibit")]
    public async Task ToggleProhibit()
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }

        DiscordBotService.IsBotUseProhibited = !DiscordBotService.IsBotUseProhibited;
        await ReplyAsync(DiscordBotService.IsBotUseProhibited.ToString());
    }

    [Command("amnesia")]
    public async Task Amnesia()
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }

        await dbContext.Users.Where(e => e.Id == Context.User.Id).ExecuteUpdateAsync(e => e.SetProperty(x => x.LastVendorPurchase, DateTime.MinValue));
        await dbContext.SaveChangesAsync();
    }
}
