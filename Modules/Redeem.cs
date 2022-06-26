using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KushBot.Modules
{
    [Group("redeem")]
    public class Redeem : ModuleBase<SocketCommandContext>
    {
        List<string> PrizeDesc = new List<string>();
        List<int> Price = new List<int>();

        int ascendedBaps = 2000000;
        int RoleBaps = 250000; //inchia
        int GaldikBaps = 25000; //galdikai
        int FatBaps = 5000; //Silvijos

        int AskedBaps = 250;
        int IsnykBaps = 500;
        int DegenerateBaps = 400;
        int PakeiskBaps = 350;
        int CringemineBaps = 800;
        int PasitikrinkBaps = 1200;
        int DinkBaps = 700;

        int redeemCd = 2;

        [Command("")]
        public async Task RedeemStuff()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Use (kush redeem n) n - number of the redeemable prize")
                .WithColor(Color.Blue)
                .AddField("**Nupumpuoti**", $"Type 'kush redeem Nupumpuoti' to buy a color of your choosing for {ascendedBaps} Baps")
                .AddField("**Trolai**", $"Type 'kush redeem trolai' to buy a role __**Trolai**__ for {RoleBaps} Baps")
                .AddField("**Pogai**", $"Type 'kush redeem pogai' to buy a role __**Pogai**__ for {GaldikBaps} Baps")
                .AddField("**Kappai**", $"Type 'kush redeem kapai' to buy a role __**Kapai**__ for {FatBaps} Baps")
                .AddField("**asked**", $"Type 'kush redeem asked @user' (e.g. kush redeem asked @tabobanda) to ***ask*** a user. cost: {AskedBaps} Baps")
                .AddField("**isnyk**", $"Type 'kush redeem isnyk @user' (e.g. kush redeem isnyk @tabobanda) to hide the user. cost: {IsnykBaps} Baps")
                .AddField("**pakeisk**", $"Type 'kush redeem pakeisk @user newName' (e.g. kush redeem pakeisk @tabobanda FAGGOT) to change the name of a user. cost: {PakeiskBaps} Baps")
                .AddField("**degenerate**", $"Type 'kush redeem degenerate @user' (e.g. kush redeem degenerate @tabobanda) to degenerate them {DegenerateBaps} Baps")
                .AddField("**dink**", $"Type 'kush redeem dink @user' (e.g. kush redeem dink @tabobanda) to lock the user in sad for 3 minutes. cost: {DinkBaps} Baps")
                .AddField("**:)**", "More to come Soon!");



            await ReplyAsync("", false, builder.Build());

        }

        [Command("dink", RunMode = RunMode.Async)]
        public async Task dink(IGuildUser user)
        {
            if (Data.Data.GetBalance(Context.User.Id) < DinkBaps)
            {
                await ReplyAsync($"{Context.User.Mention} Too poor for my liking.");
                return;
            }
            if (Data.Data.GetRedeemDate(Context.User.Id).AddHours(redeemCd) > DateTime.Now)
            {
                await ReplyAsync($"{Context.User.Mention} Your redeem is on cooldown, twat");
                return;
            }

            await ReplyAsync($"{Context.User.Mention} you locked {user.Mention} in sad for 3 minutes <:gana:627573211080425472>");
            await Data.Data.SaveBalance(Context.User.Id, -1 * DinkBaps, false);
            await Data.Data.SaveRedeemDate(Context.User.Id);

            var guild = Program._client.GetGuild(337945443252305920);
            if (Program.BotTesting)
            {
                guild = Program._client.GetGuild(490889121846263808);
            }

            SocketGuildUser usr = guild.GetUser(user.Id);
            SocketRole role = guild.GetRole(513478497885356041);
            if (Program.BotTesting)
            {
                role = guild.GetRole(641648331382325269);
            }

            await usr.AddRoleAsync(role);

            await Task.Delay(1000 * 60 * 3);

            await usr.RemoveRoleAsync(role);

        }

        [Command("pakeisk")]
        public async Task dink(IGuildUser user,[Remainder]string name)
        {
            if (Data.Data.GetBalance(Context.User.Id) < PakeiskBaps)
            {
                await ReplyAsync($"{Context.User.Mention} Too poor for my liking.");
                return;
            }
            if (Data.Data.GetRedeemDate(Context.User.Id).AddHours(redeemCd) > DateTime.Now)
            {
                await ReplyAsync($"{Context.User.Mention} Your redeem is on cooldown, twat");
                return;
            }
            await Data.Data.SaveBalance(Context.User.Id, -1 * PakeiskBaps, false);

            try
            {
                await user.ModifyAsync(x =>
                {
                    x.Nickname = name;
                });

                await Data.Data.SaveRedeemDate(Context.User.Id);
                await ReplyAsync($"{Context.User.Mention} you changed {user.Mention} into {name} <:butybe:603614056233828352>");
            }
            catch
            {
                await ReplyAsync($"{Context.User.Mention} some shit has gone down, atleast you tried.\n\n\nnigger");
            }
        }

        [Command("degenerate")]
        public async Task degenerate(IGuildUser user)
        {
            if (Data.Data.GetBalance(Context.User.Id) < DegenerateBaps)
            {
                await ReplyAsync($"{Context.User.Mention} Too poor for my liking.");
                return;
            }
            if (Data.Data.GetRedeemDate(Context.User.Id).AddHours(redeemCd) > DateTime.Now)
            {
                await ReplyAsync($"{Context.User.Mention} Your redeem is on cooldown, twat");
                return;
            }

            CursedPlayer temp = new CursedPlayer(user.Id, "degenerate", 15);
            Program.CursedPlayers.Add(temp);

            await ReplyAsync($"{Context.User.Mention} you cursed {user.Mention} with degeneracy for 15 messages <:gana:627573211080425472>");
            await Data.Data.SaveBalance(Context.User.Id, -1 * DegenerateBaps, false);
            await Data.Data.SaveRedeemDate(Context.User.Id);

        }

        [Command("isnyk")]
        public async Task isnyk(IGuildUser user)
        {
            if (Data.Data.GetBalance(Context.User.Id) < IsnykBaps)
            {
                await ReplyAsync($"{Context.User.Mention} Too poor for my liking.");
                return;
            }
            if (Data.Data.GetRedeemDate(Context.User.Id).AddHours(redeemCd) > DateTime.Now)
            {
                await ReplyAsync($"{Context.User.Mention} Your redeem is on cooldown, twat");
                return;
            }

            CursedPlayer temp = new CursedPlayer(user.Id, "isnyk", 20);
            Program.CursedPlayers.Add(temp);

            await ReplyAsync($"{Context.User.Mention} you cursed {user.Mention} with disappearance for 20 messages <:gana:627573211080425472>");
            await Data.Data.SaveBalance(Context.User.Id, -1 * IsnykBaps, false);
            await Data.Data.SaveRedeemDate(Context.User.Id);

        }

        [Command("asked")]
        public async Task ASk(IGuildUser user)
        {
            if (Data.Data.GetBalance(Context.User.Id) < AskedBaps)
            {
                await ReplyAsync($"{Context.User.Mention} Too poor for my liking.");
                return;
            }
            if (Data.Data.GetRedeemDate(Context.User.Id).AddHours(redeemCd) > DateTime.Now)
            {
                await ReplyAsync($"{Context.User.Mention} Your redeem is on cooldown, twat");
                return;
            }

            CursedPlayer temp = new CursedPlayer(user.Id, "asked", 20);
            Program.CursedPlayers.Add(temp);

            await ReplyAsync($"{Context.User.Mention} you cursed {user.Mention} with ASKED for 20 messages :warning:");
            await Data.Data.SaveBalance(Context.User.Id, -1 * AskedBaps, false);
            await Data.Data.SaveRedeemDate(Context.User.Id);

        }

        [Command("Nupumpuoti")]
        public async Task Roleascend()
        {
            if (Data.Data.GetBalance(Context.User.Id) < ascendedBaps)
            {
                await ReplyAsync($"{Context.User.Mention} POOR");
                return;
            }
            await Data.Data.SaveBalance(Context.User.Id, -1 * ascendedBaps, false);


            await ReplyAsync($"<:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>{Context.User.Mention} You've redeemed a color! PM an admin with a color of your choosing to receive it <:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>");
        }

        [Command("pogai")]
        public async Task RoleGald()
        {
            if (Data.Data.GetBalance(Context.User.Id) < GaldikBaps)
            {
                await ReplyAsync($"{Context.User.Mention} POOR");
                return;
            }

            await Data.Data.SaveBalance(Context.User.Id, -1 * GaldikBaps, false);

            var guild = Program._client.GetGuild(337945443252305920);

            SocketGuildUser user = guild.GetUser(Context.User.Id);
            SocketRole role = guild.GetRole(945785173553848390);


            await user.AddRoleAsync(role);

            await ReplyAsync($"<:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>{Context.User.Mention} You've redeemed a role! <:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>");
        }

        [Command("kappai")]
        public async Task RoleFat()
        {
            if (Data.Data.GetBalance(Context.User.Id) < FatBaps)
            {
                await ReplyAsync($"{Context.User.Mention} POOR");
                return;
            }

            await Data.Data.SaveBalance(Context.User.Id, -1 * FatBaps, false);

            var guild = Program._client.GetGuild(337945443252305920);


            SocketGuildUser user = guild.GetUser(Context.User.Id);
            SocketRole role = guild.GetRole(945782644241760427);
 

            await user.AddRoleAsync(role);

            await ReplyAsync($"<:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>{Context.User.Mention} You've redeemed a role! <:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>");
        }

        [Command("trolai")]
        public async Task Role()
        {
            if(Data.Data.GetBalance(Context.User.Id) < RoleBaps)
            {
                await ReplyAsync($"{Context.User.Mention} POOR");
                return;
            }

            await Data.Data.SaveBalance(Context.User.Id, -1 * RoleBaps, false);

            var guild = Program._client.GetGuild(337945443252305920);

            SocketGuildUser user = guild.GetUser(Context.User.Id);
            SocketRole role = guild.GetRole(945785365292285963);

            await user.AddRoleAsync(role);

            await ReplyAsync($"<:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>{Context.User.Mention} You've redeemed a role! <:kitadimensija:603612585388146701><:kitadimensija:603612585388146701><:kitadimensija:603612585388146701>");
        }


        public async Task RedeemPrize(int index)
        {
            string PrizeChannel = "<#491605808254156802>";

            if (Program.BotTesting)
            {
                PrizeChannel = "<#494199544582766610>";
            }

            if(Data.Data.GetBalance(Context.User.Id) < Price[index - 1])
            {
                await ReplyAsync($"{Context.User.Mention} You don't have {Price[index - 1]} baps, dumbass 👋");
                return;
            }
            await ReplyAsync($"{Context.User.Mention} Has redeemed prize {index} --> {PrizeChannel} 👋");

            if(index < 4)
            {
                await Program.RedeemMessage(Context.User.Mention, Context.Guild.EveryoneRole.Mention, PrizeDesc[index - 1], Context.Channel.Id);
            }
            else
            {
                await Program.RedeemMessage(Context.User.Mention, "", PrizeDesc[index - 1], Context.Channel.Id);
            }

            await Data.Data.SaveBalance(Context.User.Id, (Price[index - 1] * -1), false);

        }
        public async Task SetMoreQuests(ulong userId, int extraQ)
        {
            List<int> QuestIndexes = new List<int>();
            #region assignment
            string hold = Data.Data.GetQuestIndexes(Context.User.Id);
            string[] values = hold.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                QuestIndexes.Add(int.Parse(values[i]));
            }
            #endregion

            Random rad = new Random();
            for(int i = 0; i < extraQ; i++)
            {
                int temp = rad.Next(0, Program.Quests.Count);

                while (QuestIndexes.Contains(temp))
                {
                    temp = rad.Next(0, Program.Quests.Count);
                }

                QuestIndexes.Add(temp);

                await Data.Data.SaveQuestIndexes(userId, string.Join(',', QuestIndexes));

                #region hardcode bullshit Cx
                switch (temp)
                {
                    case 0:
                        if(Data.Data.GetWonBapsMN(Context.User.Id) >= Program.Quests[temp].CompleteReq) { await Program.CompleteQuest(temp, QuestIndexes, Context.Channel, Context.User); }
                        break;
                    case 1:
                        if (Data.Data.GetLostBapsMN(Context.User.Id) >= Program.Quests[temp].CompleteReq) { await Program.CompleteQuest(temp, QuestIndexes, Context.Channel, Context.User); }
                        break;
                    case 2:
                        if (Data.Data.GetWonFlipsMN(Context.User.Id) >= Program.Quests[temp].CompleteReq) { await Program.CompleteQuest(temp, QuestIndexes, Context.Channel, Context.User); }
                        break;
                    case 3:
                        if (Data.Data.GetLostFlipsMN(Context.User.Id) >= Program.Quests[temp].CompleteReq) { await Program.CompleteQuest(temp, QuestIndexes, Context.Channel, Context.User); }
                        break;
                    case 4:
                        if (Data.Data.GetWonBetsMN(Context.User.Id) >= Program.Quests[temp].CompleteReq) { await Program.CompleteQuest(temp, QuestIndexes, Context.Channel, Context.User); }
                        break;
                    case 5:
                        if (Data.Data.GetLostBetsMN(Context.User.Id) >= Program.Quests[temp].CompleteReq) { await Program.CompleteQuest(temp, QuestIndexes, Context.Channel, Context.User); }
                        break;
                    case 6:
                        if (Data.Data.GetWonRisksMN(Context.User.Id) >= Program.Quests[temp].CompleteReq) { await Program.CompleteQuest(temp, QuestIndexes, Context.Channel, Context.User); }
                        break;
                    case 7:
                        if (Data.Data.GetLostRisksMN(Context.User.Id) >= Program.Quests[temp].CompleteReq) { await Program.CompleteQuest(temp, QuestIndexes, Context.Channel, Context.User); }
                        break;
                    case 10:
                        if (Data.Data.GetBalance(Context.User.Id) >= Program.Quests[temp].CompleteReq) { await Program.CompleteQuest(temp, QuestIndexes, Context.Channel, Context.User); }
                        break;
                    case 11:
                        if (Data.Data.GetBegsMN(Context.User.Id) >= Program.Quests[temp].CompleteReq) { await Program.CompleteQuest(temp, QuestIndexes, Context.Channel, Context.User); }
                        break;
                    case 12:
                        if (Data.Data.GetBegsMN(Context.User.Id) >= Program.Quests[temp].CompleteReq) { await Program.CompleteQuest(temp, QuestIndexes, Context.Channel, Context.User); }
                        break;
                    case 15:
                        if (Data.Data.GetSuccessfulYoinks(Context.User.Id) >= Program.Quests[temp].CompleteReq) { await Program.CompleteQuest(temp, QuestIndexes, Context.Channel, Context.User); }
                        break;
                    default:
                        break;
                }
                #endregion
                
            }

        }

        public void SetLists()
        {
            Price.Add(250000); //Inchiai
            Price.Add(750); //Asked
            Price.Add(1000); //Isnyk
            Price.Add(1100); //Pakeisk
            Price.Add(800); //Cringemine
            Price.Add(1200); //Pasitikrink


            string pogId = "<:pog:497471636094844928>";

            //PrizeDesc.Add($"Role <@&491304890925056010> for {Price[0]} Baps!!! {pogId}");
            //PrizeDesc.Add($"Role <@&491306573642203136> for {Price[1]} Baps!!! {pogId}");
            //PrizeDesc.Add($"Role <@&491305882894860298> for {Price[2]} Baps!!! {pogId}");
            //PrizeDesc.Add($"<@189771487715000340> to sing a nightcore song on stream for {Price[3]} Baps!!! {pogId}");
            //PrizeDesc.Add($"<@247318508751290368> to sing a nightcore song on stream for {Price[4]} Baps!!! {pogId}");         
            //PrizeDesc.Add($"<@189771487715000340> to dance to a classic **Senas geras gabalas** for {Price[5]} Baps!!! {pogId}");
            //PrizeDesc.Add($"<@247318508751290368> 's private selfie for {Price[6]} Baps!!! {pogId}");
            //PrizeDesc.Add($"<@189771487715000340> 's private selfie for {Price[7]} Baps!!! {pogId}");
            //PrizeDesc.Add($"has redeemed Video teaser for {Price[8]} Baps!!! {pogId}");

            //<@247318508751290368> - Laimonini
            //<@189771487715000340> - Panda
        }
    }
}
