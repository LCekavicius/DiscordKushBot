using Discord;
using Discord.Commands;
using KushBot.DataClasses.Vendor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KushBot.Modules;


public class PlotasPasSima : ModuleBase<SocketCommandContext>
{
    private HashSet<ulong> Admins = new HashSet<ulong>();

    public PlotasPasSima()
    {
        Admins.Add(192642414215692300);
        if (Program.BotTesting)
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

        await Program.DropAirdrop();
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

        await Program.AssignWeeklyQuests();
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


        Program.AllowedKushBotChannels.Add(channel.Id);
    }

    [Command("channel remove")]
    public async Task chanelremove(IChannel channel)
    {
        if (!Admins.Contains(Context.User.Id))
            return;


        Program.AllowedKushBotChannels.Remove(channel.Id);
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


        Program.IsDisabled = true;
    }

    [Command("enable")]
    public async Task enablebot()
    {
        if (!Admins.Contains(Context.User.Id))
            return;


        Program.IsDisabled = false;
    }


    [Command("clear")]
    public async Task clear(IUser user)
    {
        Program.Test = 0;
        Program.PetTest = 0;
        Program.Fail = 0;
        Program.NerfUser = 0;
    }

    [Command("check")]
    public async Task checke()
    {
        Console.WriteLine($"Test user: {Program.Test}");
        Console.WriteLine($"Pet Test user: {Program.PetTest}");
        Console.WriteLine($"Fail user: {Program.Fail}");
        Console.WriteLine($"Nerf user: {Program.NerfUser}");
    }

    [Command("test")]
    public async Task Tyst(ulong Id)
    {

        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        Program.Test = Id;
        await Context.Message.DeleteAsync();

    }

    [Command("tier test")]
    public async Task tiertest(ulong Id)
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        Program.TierTest = Id;
        await Context.Message.DeleteAsync();
    }



    [Command("fail")]
    public async Task Tyst2(ulong Id)
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        Program.Fail = Id;
        await Context.Message.DeleteAsync();

    }

    [Command("pet test")]
    public async Task TystPet(ulong Id)
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        Program.PetTest = Id;
        await Context.Message.DeleteAsync();

    }
    [Command("nerf")]
    public async Task nrf(ulong id)
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        Program.NerfUser = id;
        await Context.Message.DeleteAsync();
    }

    [Command("force")]
    public async Task AssignQ()
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        await Program.AssignQuestsToPlayers();
    }

    [Command("forcew")]
    public async Task AssignQw()
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        await Program.AssignWeeklyQuests();
    }

    [Command("airdrop")]
    public async Task DropAirdROploeal()
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }

        await Program.DropAirdrop();
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
        Program.CursedPlayers.Add(cp);
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
        await Program._client.SetGameAsync(game);
    }


    [Command("Attach vendor")]
    public async Task AttachVendor()
    {
        if (!Admins.Contains(Context.User.Id))
        {
            return;
        }
        Program.VendorObj = new Vendor();
        Program.VendorObj.GenerateWares();

        if (Program.VendorObj.MessageId == default)
        {
            var channel = Program._client.GetChannel(Program.VendorChannelId) as IMessageChannel;
            var message = await channel.SendMessageAsync(embed: Program.VendorObj.BuildEmbed(), components: Program.VendorObj.BuildComponents());
            Program.VendorObj.MessageId = message.Id;
        }

        if (!File.Exists(Program.VendorJsonPath))
        {
            File.Create(Program.VendorJsonPath).Close();
        }

        File.WriteAllText(Program.VendorJsonPath, JsonConvert.SerializeObject(Program.VendorObj, Formatting.Indented, new JsonSerializerSettings
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

        if (Program.VendorObj == null)
        {
            await ReplyAsync("Vendor is detached");
            return;
        }

        await Program.VendorObj.RestockAsync();
    }

    [Command("reset")]
    public async Task Reset(IUser user)
    {
        if (!Program.BotTesting)
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

        Program.IsBotUseProhibited = !Program.IsBotUseProhibited;
        await ReplyAsync(Program.IsBotUseProhibited.ToString());
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
