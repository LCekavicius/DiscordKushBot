//using Discord;
//using Discord.Commands;
//using KushBot.DataClasses;
//using KushBot.Global;
//using KushBot.Resources.Database;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace KushBot.Modules;

//[Group("redeem")]
//public class Redeem(SqliteDbContext dbContext, TutorialManager tutorialManager) : ModuleBase<SocketCommandContext>
//{
//    List<string> PrizeDesc = new List<string>();
//    List<int> Price = new List<int>();

//    int ascendedBaps = 2000000;
//    int RoleBaps = 150000; //inchia
//    int GaldikBaps = 15000; //galdikai
//    int FatBaps = 5000; //Silvijos

//    int AskedBaps = 250;
//    int IsnykBaps = 500;
//    int DegenerateBaps = 400;
//    int PakeiskBaps = 350;
//    int CringemineBaps = 800;
//    int PasitikrinkBaps = 1200;
//    int DinkBaps = 700;

//    int redeemCd = 2;

//    [Command("")]
//    public async Task RedeemStuff()
//    {
//        EmbedBuilder builder = new EmbedBuilder();

//        builder.WithTitle("Redeem")
//            .WithColor(Color.Blue)
//            .AddField("**Nupumpuoti**", $"Type 'kush redeem Nupumpuoti' to buy a color of your choosing for {ascendedBaps} Baps")
//            .AddField("**skibidi**", $"Type 'kush redeem skibidi' to buy a permanent role __**skibidi**__ for {RoleBaps} Baps")
//            .AddField("**gyat**", $"Type 'kush redeem gyat' to buy a temporary role __**gyat**__ for {GaldikBaps} Baps")
//            .AddField("**asked**", $"Type 'kush redeem asked @user' (e.g. kush redeem asked @tabobanda) to attach ':warning: KLAUSEM :warning:' bot replies to the user's messages (lasts for 15 unique messages) cost: {AskedBaps} Baps")
//            .AddField("**isnyk**", $"Type 'kush redeem isnyk @user' (e.g. kush redeem isnyk @tabobanda) to delete messages as the user types them (lasts for 15 unique messages) cost: {IsnykBaps} Baps")
//            .AddField("**pakeisk**", $"Type 'kush redeem pakeisk @user newName' (e.g. kush redeem pakeisk @tabobanda FAGGOT) to change the name of a user. cost: {PakeiskBaps} Baps")
//            .AddField("**degenerate**", $"Type 'kush redeem degenerate @user' (e.g. kush redeem degenerate @tabobanda) to attach a kush nya to the user's messages (lasts for 15 unique messages) {DegenerateBaps} Baps")
//            .AddField("**dink**", $"Type 'kush redeem dink @user' (e.g. kush redeem dink @tabobanda) to lock the user in <#945764667287031859> for 3 minutes. cost: {DinkBaps} Baps")
//            .AddField("**:)**", "More to come Soon!");

//        await ReplyAsync("", false, builder.Build());
//    }

//    [Command("dink", RunMode = RunMode.Async)]
//    public async Task dink(IGuildUser user)
//    {
//        var botUser = await dbContext.GetKushBotUserAsync(Context.User.Id);

//        if (botUser.Balance < DinkBaps)
//        {
//            await ReplyAsync($"{Context.User.Mention} Too poor for my liking.");
//            return;
//        }
//        if (botUser.RedeemDate.AddHours(redeemCd) > DateTime.Now)
//        {
//            await ReplyAsync($"{Context.User.Mention} Your redeem is on cooldown, twat");
//            return;
//        }

//        await tutorialManager.AttemptSubmitStepCompleteAsync(botUser, 5, 2, Context.Channel);

//        var target = await dbContext.GetKushBotUserAsync(user.Id);

//        await ReplyAsync($"{Context.User.Mention} you locked {user.Mention} in sad for 3 minutes {CustomEmojis.Gana}");
//        botUser.Balance -= DinkBaps;
//        botUser.RedeemDate = TimeHelper.Now;
        
//        await dbContext.SaveChangesAsync();

//        //var guild = DiscordBotService._client.GetGuild(337945443252305920);
//        //if (DiscordBotService.BotTesting)
//        //{
//        //    guild = DiscordBotService._client.GetGuild(490889121846263808);
//        //}

//        //SocketGuildUser usr = guild.GetUser(user.Id);
//        //SocketRole role = guild.GetRole(513478497885356041);
//        //if (DiscordBotService.BotTesting)
//        //{
//        //    role = guild.GetRole(641648331382325269);
//        //}

//        //await usr.AddRoleAsync(role);

//        //await Task.Delay(1000 * 60 * 3);

//        //await usr.RemoveRoleAsync(role);

//    }

//    [Command("pakeisk")]
//    public async Task dink(IGuildUser user,[Remainder]string name)
//    {
//        if (Data.Data.GetBalance(Context.User.Id) < PakeiskBaps)
//        {
//            await ReplyAsync($"{Context.User.Mention} Too poor for my liking.");
//            return;
//        }
//        if (Data.Data.GetRedeemDate(Context.User.Id).AddHours(redeemCd) > DateTime.Now)
//        {
//            await ReplyAsync($"{Context.User.Mention} Your redeem is on cooldown, twat");
//            return;
//        }
//        await Data.Data.SaveBalance(Context.User.Id, -1 * PakeiskBaps, false);
//        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 5, 2, Context.Channel);
//        try
//        {
//            await user.ModifyAsync(x =>
//            {
//                x.Nickname = name;
//            });

//            await Data.Data.SaveRedeemDate(Context.User.Id);
//            await ReplyAsync($"{Context.User.Mention} you changed {user.Mention} into {name} <:butybe:603614056233828352>");
//        }
//        catch
//        {
//            await ReplyAsync($"{Context.User.Mention} some shit has gone down, atleast you tried.\n\n\nnigger");
//        }
//    }

//    [Command("degenerate")]
//    public async Task degenerate(IGuildUser user)
//    {
//        if (Data.Data.GetBalance(Context.User.Id) < DegenerateBaps)
//        {
//            await ReplyAsync($"{Context.User.Mention} Too poor for my liking.");
//            return;
//        }
//        if (Data.Data.GetRedeemDate(Context.User.Id).AddHours(redeemCd) > DateTime.Now)
//        {
//            await ReplyAsync($"{Context.User.Mention} Your redeem is on cooldown, twat");
//            return;
//        }
//        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 5, 2, Context.Channel);
//        CursedPlayer temp = new CursedPlayer(user.Id, "degenerate", 15);
//        DiscordBotService.CursedPlayers.Add(temp);

//        await ReplyAsync($"{Context.User.Mention} you cursed {user.Mention} with degeneracy for 15 messages <:gana:627573211080425472>");
//        await Data.Data.SaveBalance(Context.User.Id, -1 * DegenerateBaps, false);
//        await Data.Data.SaveRedeemDate(Context.User.Id);

//    }

//    [Command("isnyk")]
//    public async Task isnyk(IGuildUser user)
//    {
//        if (Data.Data.GetBalance(Context.User.Id) < IsnykBaps)
//        {
//            await ReplyAsync($"{Context.User.Mention} Too poor for my liking.");
//            return;
//        }
//        if (Data.Data.GetRedeemDate(Context.User.Id).AddHours(redeemCd) > DateTime.Now)
//        {
//            await ReplyAsync($"{Context.User.Mention} Your redeem is on cooldown, twat");
//            return;
//        }
//        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 5, 2, Context.Channel);
//        CursedPlayer temp = new CursedPlayer(user.Id, "isnyk", 20);
//        DiscordBotService.CursedPlayers.Add(temp);

//        await ReplyAsync($"{Context.User.Mention} you cursed {user.Mention} with disappearance for 20 messages <:gana:627573211080425472>");
//        await Data.Data.SaveBalance(Context.User.Id, -1 * IsnykBaps, false);
//        await Data.Data.SaveRedeemDate(Context.User.Id);

//    }

//    [Command("asked")]
//    public async Task ASk(IGuildUser user)
//    {
//        if (Data.Data.GetBalance(Context.User.Id) < AskedBaps)
//        {
//            await ReplyAsync($"{Context.User.Mention} Too poor for my liking.");
//            return;
//        }
//        if (Data.Data.GetRedeemDate(Context.User.Id).AddHours(redeemCd) > DateTime.Now)
//        {
//            await ReplyAsync($"{Context.User.Mention} Your redeem is on cooldown, twat");
//            return;
//        }
//        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 5, 2, Context.Channel);
//        CursedPlayer temp = new CursedPlayer(user.Id, "asked", 20);
//        DiscordBotService.CursedPlayers.Add(temp);

//        await ReplyAsync($"{Context.User.Mention} you cursed {user.Mention} with ASKED for 20 messages :warning:");
//        await Data.Data.SaveBalance(Context.User.Id, -1 * AskedBaps, false);
//        await Data.Data.SaveRedeemDate(Context.User.Id);

//    }

//    [Command("Nupumpuoti")]
//    public async Task Roleascend()
//    {
//        if (Data.Data.GetBalance(Context.User.Id) < ascendedBaps)
//        {
//            await ReplyAsync($"{Context.User.Mention} POOR");
//            return;
//        }
//        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 5, 2, Context.Channel);
//        await Data.Data.SaveBalance(Context.User.Id, -1 * ascendedBaps, false);


//        await ReplyAsync($"<:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>{Context.User.Mention} You've redeemed a color! PM an admin with a color of your choosing to receive it <:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>");
//    }

//    [Command("gyat")]
//    public async Task RoleGald()
//    {
//        //if (Data.Data.GetBalance(Context.User.Id) < GaldikBaps)
//        //{
//        //    await ReplyAsync($"{Context.User.Mention} POOR");
//        //    return;
//        //}
//        //await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 5, 2, Context.Channel);
//        //await Data.Data.SaveBalance(Context.User.Id, -1 * GaldikBaps, false);

//        //var guild = DiscordBotService._client.GetGuild(337945443252305920);

//        //SocketGuildUser user = guild.GetUser(Context.User.Id);
//        //SocketRole role = guild.GetRole(1225482619697893537);


//        //await user.AddRoleAsync(role);

//        //await ReplyAsync($"<:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>{Context.User.Mention} You've redeemed a role! <:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>");
//    }

//    //[Command("kappai")]
//    //public async Task RoleFat()
//    //{
//    //    if (Data.Data.GetBalance(Context.User.Id) < FatBaps)
//    //    {
//    //        await ReplyAsync($"{Context.User.Mention} POOR");
//    //        return;
//    //    }
//    //    await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 5, 2, Context.Channel);
//    //    await Data.Data.SaveBalance(Context.User.Id, -1 * FatBaps, false);

//    //    var guild = Program._client.GetGuild(337945443252305920);


//    //    SocketGuildUser user = guild.GetUser(Context.User.Id);
//    //    SocketRole role = guild.GetRole(945782644241760427);


//    //    await user.AddRoleAsync(role);

//    //    await ReplyAsync($"<:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>{Context.User.Mention} You've redeemed a role! <:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>");
//    //}

//    [Command("skibidi")]
//    public async Task Role()
//    {
//        //if(Data.Data.GetBalance(Context.User.Id) < RoleBaps)
//        //{
//        //    await ReplyAsync($"{Context.User.Mention} POOR");
//        //    return;
//        //}
//        //await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 5, 2, Context.Channel);
//        //await Data.Data.SaveBalance(Context.User.Id, -1 * RoleBaps, false);

//        //var guild = DiscordBotService._client.GetGuild(337945443252305920);

//        //SocketGuildUser user = guild.GetUser(Context.User.Id);
//        //SocketRole role = guild.GetRole(945785365292285963);

//        //await user.AddRoleAsync(role);

//        //await ReplyAsync($"<:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>{Context.User.Mention} You've redeemed a role! <:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>");
//    }


//    public async Task RedeemPrize(int index)
//    {
//        string PrizeChannel = "<#491605808254156802>";

//        if (DiscordBotService.BotTesting)
//        {
//            PrizeChannel = "<#494199544582766610>";
//        }

//        if(Data.Data.GetBalance(Context.User.Id) < Price[index - 1])
//        {
//            await ReplyAsync($"{Context.User.Mention} You don't have {Price[index - 1]} baps, dumbass 👋");
//            return;
//        }
//        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 5, 2, Context.Channel);
//        await ReplyAsync($"{Context.User.Mention} Has redeemed prize {index} --> {PrizeChannel} 👋");
//        if (index < 4)
//        {
//            await DiscordBotService.RedeemMessage(Context.User.Mention, Context.Guild.EveryoneRole.Mention, PrizeDesc[index - 1], Context.Channel.Id);
//        }
//        else
//        {
//            await DiscordBotService.RedeemMessage(Context.User.Mention, "", PrizeDesc[index - 1], Context.Channel.Id);
//        }

//        await Data.Data.SaveBalance(Context.User.Id, (Price[index - 1] * -1), false);

//    }

//    public void SetLists()
//    {
//        Price.Add(250000); //Inchiai
//        Price.Add(750); //Asked
//        Price.Add(1000); //Isnyk
//        Price.Add(1100); //Pakeisk
//        Price.Add(800); //Cringemine
//        Price.Add(1200); //Pasitikrink


//        string pogId = "<:pog:497471636094844928>";
//    }
//}
