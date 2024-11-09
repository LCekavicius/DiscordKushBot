using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules;

public sealed record ExistingDuel(ulong Challenger, ulong Challenged, int Baps, DateTime DateTime);

[Group("duel")]
[RequirePermissions(Permissions.Core)]
public class Duel(SqliteDbContext dbContext, DiscordSocketClient client) : ModuleBase<SocketCommandContext>
{
    public static List<ExistingDuel> Duels = new();

    [Command("")]
    public async Task Challenge(int baps, IUser user)
    {
        int seconds = 10;
        if (Context.User.Id == user.Id)
        {
            await ReplyAsync($"{Context.User.Mention} can't duel yourself");
            return;
        }
        if (baps <= 0)
        {
            await ReplyAsync($"{Context.User.Mention} baps amount is too small L)");
            return;
        }

        var botUser = await dbContext.GetKushBotUserAsync(Context.User.Id);
        var target = await dbContext.GetKushBotUserAsync(user.Id);

        if (botUser.Balance < baps)
        {
            await ReplyAsync($"{Context.User.Mention} you don't even have that kind of cash smh...");
            return;
        }

        if (target.Balance < baps)
        {
            await ReplyAsync($"{user.Mention} doesn't even have that kind of cash smh...");
            return;
        }

        if (Duels.Any(x => x.Challenger == Context.User.Id))
        {
            await ReplyAsync($"{Context.User.Mention} You've already offered a duel... retard");
            return;
        }

        if (Duels.Any(x => x.Challenged == user.Id))
        {
            await ReplyAsync($"{user.Mention} already has a duel going on");
            return;
        }

        if (user.IsBot)
        {
            int stolen = (int)Math.Round((double)baps / Random.Shared.Next(8, 12));

            await ReplyAsync($"{Context.User.Mention}, {user.Mention} decided to rape you instead and stole **{stolen}** Baps");
            botUser.Balance -= stolen;
            await dbContext.SaveChangesAsync();
            return;
        }

        await ReplyAsync($"{CustomEmojis.Rieda} {Context.User.Mention} has challenged {user.Mention} into a coinflip duel for **{baps}** baps! {user.Mention} type 'kush duel accept' to accept the duel or " +
            $"'kush duel decline' to decline it. If not accepted in {seconds} seconds, it'll be automatically declined {CustomEmojis.Kuris}");

        var duel = new ExistingDuel(Context.User.Id, user.Id, baps, TimeHelper.Now);

        Duels.Add(duel);

        await Task.Delay(seconds * 1000);

        if (!Duels.Contains(duel))
        {
            return;
        }
        else
        {
            await ReplyAsync($"{user.Mention} has failed to accept in time");
            Duels.Remove(duel);
        }

    }

    [Command("Accept"), Alias("a")]
    public async Task DuelAccept()
    {
        var duel = Duels.FirstOrDefault(e => e.Challenged == Context.User.Id);
        if (duel is null)
        {
            await ReplyAsync($"{Context.User.Mention} you've not been challenged by anyone, loser");
            return;
        }

        if (duel.Challenged == Context.User.Id)
        {
            Random rnd = new Random();
            int temp = rnd.Next(0, 2);

            string message = $"**lost {duel.Baps}** baps. {CustomEmojis.Sadge} ";

            var challenger = await dbContext.GetKushBotUserAsync(duel.Challenger);
            var challenged = await dbContext.GetKushBotUserAsync(duel.Challenged);

            if (challenged.Balance < duel.Baps)
            {
                await ReplyAsync($"{Context.User.Mention} has managed to lose his cash before accepting {CustomEmojis.Rieda}");
                return;
            }

            if (challenged.Balance < duel.Baps)
            {
                await ReplyAsync($"{client.GetUser(duel.Challenger).Mention} has managed to lose his cash before accepting {CustomEmojis.Rieda}");
                return;
            }

            if (temp == 1)
            {
                message = $"**won {duel.Baps}** baps. {CustomEmojis.Pepehap}";
                challenged.Balance += duel.Baps;
                challenger.Balance -= duel.Baps;
            }
            else
            {
                challenger.Balance += duel.Baps;
                challenged.Balance -= duel.Baps;
            }

            await dbContext.SaveChangesAsync();

            await ReplyAsync($"{Context.User.Mention} has **accepted** the {client.GetUser(duel.Challenger).Mention}'s duel and {message}");
            Duels.Remove(duel);
        }
    }


    [Command("Decline"), Alias("d")]
    public async Task PingDecline()
    {

        foreach (ExistingDuel duel in Duels)
        {
            if (duel.Challenged == Context.User.Id)
            {
                await ReplyAsync($"{Context.User.Mention} has no balls whatsoever...");
                Duels.Remove(duel);
                return;
            }
        }
        await ReplyAsync($"{Context.User.Mention} you've not been challenged by anyone, loser");
    }
}
