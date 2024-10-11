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
    [Group("duel")]
    public class Duel : ModuleBase<SocketCommandContext>
    {
        [Command("", RunMode = RunMode.Async)]
        public async Task Challenge(int baps, IUser user)
        {
            int seconds = 12;
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
            if (Data.Data.GetBalance(Context.User.Id) < baps)
            {
                await ReplyAsync($"{Context.User.Mention} you don't even have that kind of cash smh...");
                return;
            }
            if (Data.Data.GetBalance(user.Id) < baps)
            {
                await ReplyAsync($"{user.Mention} doesn't even have that kind of cash smh...");
                return;
            }
            if (DiscordBotService.Duels.Any(x => x.Challenger == Context.User.Id))
            {
                await ReplyAsync($"{Context.User.Mention} You've already offered a duel... retard");
                return;
            }
            if (DiscordBotService.Duels.Any(x => x.Challenged == user.Id))
            {
                await ReplyAsync($"{user.Mention} already has a duel going on");
                return;
            }


            if (user.Id == 490888630542532619)
            {
                Random rad = new Random();
                int stolen = (int)Math.Round((double)baps / rad.Next(8, 12));

                await ReplyAsync($"{Context.User.Mention}, {user.Mention} decided to rape you instead and stole **{stolen}** Baps");
                await Data.Data.SaveBalance(Context.User.Id, -1 * stolen, false);
                return;
            }

            await ReplyAsync($"<:rieda:945781493291184168> {Context.User.Mention} has challenged {user.Mention} into a coinflip duel for **{baps}** baps! {user.Mention} type 'kush duel accept' to accept the duel or " +
                $"'kush duel decline' to decline it. If not accepted in {seconds} seconds, it'll be automatically declined <:kuris:626793247116886026>");

            ExistingDuel duel = new ExistingDuel(Context.User.Id, user.Id, baps);

            DiscordBotService.Duels.Add(duel);

            await Task.Delay(seconds * 1000);

            if (!DiscordBotService.Duels.Contains(duel))
            {
                return;
            }
            else
            {
                await ReplyAsync($"{user.Mention} has failed to accept in time");
                DiscordBotService.Duels.Remove(duel);
            }

        }
        [Command("", RunMode = RunMode.Async)]
        public async Task Challenge(string baps, IUser user)
        {
            if (baps.ToLower() == "all")
            {
                await Challenge(Data.Data.GetBalance(Context.User.Id), user);
            }
        }

        [Command("Accept", RunMode = RunMode.Async), Alias("a")]
        public async Task PingAsync()
        {

            foreach (ExistingDuel duel in DiscordBotService.Duels)
            {
                if (duel.Challenged == Context.User.Id)
                {
                    Random rad = new Random();
                    int temp = rad.Next(0, 2);

                    string message = $"**lost {duel.Baps}** baps. <:sadge:945703001123848203> ";

                    if (Data.Data.GetBalance(Context.User.Id) < duel.Baps)
                    {
                        await ReplyAsync($"{Context.User.Mention} has managed to lose his cash before accepting <:rieda:945781493291184168>");
                        return;
                    }

                    if (temp == 1)
                    {
                        message = $"**won {duel.Baps}** baps. <:pepehap:945780175415689266>";
                        await WonBaps(duel.Baps, duel.Challenged);
                        await LostBaps(duel.Baps, duel.Challenger);
                    }
                    else
                    {
                        await WonBaps(duel.Baps, duel.Challenger);
                        await LostBaps(duel.Baps, duel.Challenged);
                    }

                    await ReplyAsync($"{Context.User.Mention} has **accepted** the {DiscordBotService._client.GetUser(duel.Challenger).Mention}'s duel and {message}");
                    DiscordBotService.Duels.Remove(duel);
                    return;
                }
            }
            await ReplyAsync($"{Context.User.Mention} you've not been challenged by anyone, loser");
        }


        [Command("Decline", RunMode = RunMode.Async), Alias("d")]
        public async Task PingDecline()
        {

            foreach (ExistingDuel duel in DiscordBotService.Duels)
            {
                if (duel.Challenged == Context.User.Id)
                {
                    await ReplyAsync($"{Context.User.Mention} has no balls whatsoever...");
                    DiscordBotService.Duels.Remove(duel);
                    return;
                }
            }
            await ReplyAsync($"{Context.User.Mention} you've not been challenged by anyone, loser");
        }

        public async Task WonBaps(int amount, ulong userId)
        {
            await Data.Data.SaveBalance(userId, amount, false);
        }

        public async Task LostBaps(int amount, ulong userId)
        {
            await Data.Data.SaveBalance(userId, amount * -1, false);
        }
    }
}
