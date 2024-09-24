using Discord;
using Discord.Commands;
using KushBot.BackgroundJobs;
using KushBot.DataClasses.Vendor;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KushBot.Modules;


public class AdminModule : ModuleBase<SocketCommandContext>
{
    private HashSet<ulong> Admins = new HashSet<ulong>();
    private readonly ISchedulerFactory _schedulerFactory;

    public AdminModule(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
        Admins.Add(192642414215692300);
        if (DiscordBotService.BotTesting)
        {
            Admins.Add(187483265865613312);
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

    [Command("airdrop", RunMode = RunMode.Async)]
    public async Task bres()
    {
        if (!Admins.Contains(Context.User.Id))
            return;

        await DiscordBotService.DropAirdrop();
    }

    [Command("ticket add")]
    public async Task GiveTicketToMan(IUser user)
    {
        if (!Admins.Contains(Context.User.Id))
            return;

        await Data.Data.SaveTicket(user.Id, true);
    }
    [Command("ticket remove")]
    public async Task RemoveTickerFromMan(IUser user)
    {
        if (!Admins.Contains(Context.User.Id))
            return;

        await Data.Data.SaveTicket(user.Id, false);
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

        Data.Data.GenerateItem(Context.User.Id, rar);
    }

    [Command("channel add")]
    public async Task Channeladd(IChannel channel)
    {
        if (!Admins.Contains(Context.User.Id))
            return;


        DiscordBotService.AllowedKushBotChannels.Add(channel.Id);
    }

    [Command("channel remove")]
    public async Task chanelremove(IChannel channel)
    {
        if (!Admins.Contains(Context.User.Id))
            return;


        DiscordBotService.AllowedKushBotChannels.Remove(channel.Id);
    }


    [Command("infect")]
    public async Task Infect(IUser user)
    {
        if (!Admins.Contains(Context.User.Id))
            return;

        await Data.Data.InfestUserAsync(user.Id);
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

        var scheduler = await _schedulerFactory.GetScheduler();
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
    public async Task DropAirdROploeal()
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }

        await DiscordBotService.DropAirdrop();
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
        DiscordBotService.CursedPlayers.Add(cp);
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

    [Command("set game")]
    public async Task teasdasd([Remainder] string game)
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        await DiscordBotService._client.SetGameAsync(game);
    }


    [Command("Attach vendor")]
    public async Task AttachVendor()
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        DiscordBotService.VendorObj = new Vendor();
        DiscordBotService.VendorObj.GenerateWares();

        if (DiscordBotService.VendorObj.MessageId == default)
        {
            var channel = DiscordBotService._client.GetChannel(DiscordBotService.VendorChannelId) as IMessageChannel;
            var message = await channel.SendMessageAsync(embed: DiscordBotService.VendorObj.BuildEmbed(), components: DiscordBotService.VendorObj.BuildComponents());
            DiscordBotService.VendorObj.MessageId = message.Id;
        }

        if (!File.Exists(DiscordBotService.VendorJsonPath))
        {
            File.Create(DiscordBotService.VendorJsonPath).Close();
        }

        File.WriteAllText(DiscordBotService.VendorJsonPath, JsonConvert.SerializeObject(DiscordBotService.VendorObj, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        }));
    }

    [Command("Restock")]
    public async Task restockVendor()
    {
        if (Context.User.Id != 192642414215692300 && Context.User.Id != 187483265865613312 && Context.User.Id != 230743424263782400)
        {
            return;
        }

        if (DiscordBotService.VendorObj == null)
        {
            await ReplyAsync("Vendor is detached");
            return;
        }

        await DiscordBotService.VendorObj.RestockAsync();
    }

    [Command("reset")]
    public async Task Reset(IUser user)
    {
        if (!DiscordBotService.BotTesting)
            return;

        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }

        await Data.Data.DeleteUser(user.Id);

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
    public async Task amnesia()
    {
        if (Context.User.Id != 192642414215692300 && Context.User.Id != 187483265865613312 && Context.User.Id != 230743424263782400)
        {
            return;
        }

        await Data.Data.RefreshLastVendorPurchaseAsync(Context.User.Id);

    }



}
