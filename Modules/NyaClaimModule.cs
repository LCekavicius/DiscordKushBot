using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KushBot.Global;
using System.Linq;
using System.Text.RegularExpressions;
using KushBot.Modules.Interactions;
using Discord.WebSocket;
using KushBot.Migrations;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using KushBot.Resources.Database;

namespace KushBot.Modules;

public class NyaClaimModule : ModuleBase<SocketCommandContext>
{
    private readonly DiscordSocketClient _client;
    private readonly SqliteDbContext _context;

    public NyaClaimModule(DiscordSocketClient client, SqliteDbContext context)
    {
        _client = client;
        _context = context;
    }


    [Command("nya sort"), Alias("vroom sort")]
    public async Task Sort([Remainder] string input = null)
    {
        if (string.IsNullOrEmpty(input))
        {
            await ReplyAsync($"{Context.User.Mention} input the sequence of current sort positions. E.g.: 'kush nya sort 3 1' would move the current claim on position 3, to position 1, and the first one to position 2.");
            return;
        }

        Regex regx = new Regex(@"(\S+)|(\s+(?=\S))");
        var matches = regx.Matches(input);
        var values = matches.OfType<Match>().Where(m => !string.IsNullOrWhiteSpace(m.Value));

        List<int> parsedValues = values
            .Select(e => int.TryParse(e.Value, out int parsed) ? parsed : -1)
            .ToList();

        if (parsedValues.Any(e => e == -1))
        {
            await ReplyAsync($"{Context.User.Mention} Bad format");
            return;
        }

        var userClaims = Data.Data.GetUserNyaClaims(Context.User.Id).Select(e => e.SortIndex).ToHashSet();

        if (parsedValues.Any(e => !userClaims.Contains(e - 1)))
        {
            await ReplyAsync($"{Context.User.Mention} Bad input");
            return;
        }

        if (parsedValues.Distinct().Count() != parsedValues.Count)
        {
            await ReplyAsync($"{Context.User.Mention} Bad input");
            return;
        }

        await Data.Data.SortClaims(Context.User.Id, parsedValues);
        await ReplyAsync($"{Context.User.Mention} Your claims were sorted");

    }

    [Command("nya trade cancel"), Alias("vroom trade cancel")]
    public async Task CancelTrade()
    {
        NyaClaimGlobals.NyaTrades.RemoveAll(e => e.Respondee.UserId == Context.User.Id || e.Suggester.UserId == Context.User.Id);
        await ReplyAsync($"{Context.User.Mention} You cancelled all your active nya trades");
    }

    [Command("nya trade"), Alias("vroom trade")]
    public async Task Trade(IUser tradeAgainst, [Remainder] string proposedTrade)
    {
        if (Context.User.Id == tradeAgainst.Id)
        {
            await ReplyAsync($"{Context.User.Mention} XD");
            return;
        }

        if (NyaClaimGlobals.NyaTrades.Any(e => e.Suggester.UserId == Context.User.Id || e.Respondee.UserId == Context.User.Id))
        {
            await ReplyAsync($"{Context.User.Mention} you already have active trade suggestions, remove all of them with 'kush nya trade cancel'");
            return;
        }

        if (NyaClaimGlobals.NyaTrades.Any(e => e.Suggester.UserId == tradeAgainst.Id || e.Respondee.UserId == tradeAgainst.Id))
        {
            await ReplyAsync($"{Context.User.Mention} {tradeAgainst.Username} already has active trade requests.");
            return;
        }

        try
        {
            var claim = NyaClaimGlobals.ParseTradeInput(proposedTrade);

            //if (claim == 0 && parsedResult.baps == 0)
            if (claim == 0)
            {
                await ReplyAsync($"{Context.User.Mention} Bad format, see 'kush nya trade'");
                return;
            }

            EmbedBuilder builder = new();
            builder.WithColor(Color.Magenta);
            builder.WithAuthor($"{Context.User.Username}'s trade proposal", Context.User.GetAvatarUrl());

            var nyaClaim = Data.Data.GetClaimBySortIndex(Context.User.Id, (claim ?? 5000) - 1);

            if (nyaClaim == null && claim != null)
            {
                await ReplyAsync($"{Context.User.Mention} Bad format, see 'kush nya trade'");
                return;
            }

            if (nyaClaim != null)
            {
                builder.WithImageUrl(nyaClaim.Url);
                builder.AddField("Keys", ":key2: (0)", true);
            }


            NyaClaimTrade trade = new();
            trade.DateTime = DateTime.Now;
            trade.Suggester = new()
            {
                //Baps = parsedResult.baps,
                NyaClaim = nyaClaim,
                UserId = Context.User.Id,
            };

            trade.Respondee = new()
            {
                UserId = tradeAgainst.Id,
            };

            NyaClaimGlobals.NyaTrades.Add(trade);

            await ReplyAsync($"{tradeAgainst.Mention} type the sort index of the character you want to trade against:", embed: builder.Build());
        }
        catch
        {
            await ReplyAsync($"{Context.User.Mention} Bad format, see 'kush nya trade'");
        }
    }

    [Command("nya trade"), Alias("vroom trade")]
    public async Task Trade()
    {
        await ReplyAsync($"{Context.User.Mention} Trade your claims (and baps) with the command 'kush nya trade #' '\n E.g." +
            $" **'kush nya trade @Tabo 3'** will propose a trade where you give away a claim at your sort index 3.\n Once " +
            $"the other party suggests his proposal, u'll be given an option to confirm or decline.\n You can only trade 1 claim per trade (for now)");
    }

    [Command("nya expand"), Alias("vroom expand")]
    public async Task ExpandClaimSlots()
    {
        int extraClaimSlots = Data.Data.GetUserExtraClaims(Context.User.Id);
        int bapsCost = 2500 + (500 * extraClaimSlots);

        if (Data.Data.GetBalance(Context.User.Id) < bapsCost)
        {
            await ReplyAsync($"POOR. +1 claim slot costs {bapsCost} for you.");
            return;
        }

        await Data.Data.SaveBalance(Context.User.Id, -1 * bapsCost, false);
        await Data.Data.SaveUserExtraClaimsAsync(Context.User.Id);
        await ReplyAsync($"{Context.User.Mention} You expanded your nya claim slots for {bapsCost} baps, you can now own up to {NyaClaimGlobals.BaseMaxNyaClaims + extraClaimSlots + 1} claims");
    }


    [Command("nya reset")]
    public async Task TestRemoveLaterPls()
    {
        if (Context.User.Id != 192642414215692300)
        {
            return;
        }
        await Data.Data.SaveLastClaimDate(Context.User.Id, DateTime.MinValue);
    }

    [Command("nya claim"), Alias("vroom claim")]
    public async Task Claim()
    {
        DateTime lastClaim = Data.Data.GetLastClaimDate(Context.User.Id);

        int maxAllowedClaims = NyaClaimGlobals.BaseMaxNyaClaims + Data.Data.GetUserExtraClaims(Context.User.Id);

        if (lastClaim.AddHours(3) > DateTime.Now)
        {
            TimeSpan timeLeft = lastClaim.AddHours(3) - DateTime.Now;
            await ReplyAsync($"{Context.User.Mention} Your claim is on cooldown, you can use it in {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}");
            return;
        }

        var nyaClaims = Data.Data.GetUserNyaClaims(Context.User.Id);

        if (nyaClaims.Count >= maxAllowedClaims)
        {
            await ReplyAsync($"{Context.User.Mention} You already have {maxAllowedClaims} claims and cant have more (for now), u can dismiss them with the command 'kush nya dismiss #' where # is the sort index (see 'kush nya claimed'). Alternatively you can expand your claim slots with 'kush nya expand' (cost: **{2500 + (maxAllowedClaims - NyaClaimGlobals.BaseMaxNyaClaims) * 500}** baps)");
            return;
        }

        int gracePeriod = 5;

        await Data.Data.SaveLastClaimDate(Context.User.Id);

        NyaClaimGlobals.ClaimReadyUsers.Add(Context.User.Id);

        await ReplyAsync($"<:Pepejam:945806412049702972> {Context.User.Mention} You are claim-ready, the next kush nya or kush vroom will have a button attached," +
            $" Click it to claim the roll. Claim button stays protected for {gracePeriod} seconds <:Pepejam:945806412049702972>");

    }

    [Command("nya dismiss"), Alias("vroom dismiss")]
    public async Task Dismiss(int index)
    {
        var nyaClaims = Data.Data.GetUserNyaClaims(Context.User.Id);

        var claim = nyaClaims.FirstOrDefault(e => e.SortIndex == index - 1);

        if (claim == null)
        {
            await ReplyAsync($"{Context.User.Mention} ?XD");
            return;
        }


        NyaClaimGlobals.PaginatedEmbed.Remove(Context.User.Id);
        await Data.Data.DismissNyaClaims(Context.User.Id, index - 1);
        await ReplyAsync($"{Context.User.Mention} You successfully dismissed your claim at sort position {index}");
    }

    [Command("nya claims"), Alias("vroom claims", "nya claimed", "vroom claimed")]
    public async Task List(IUser user = null)
    {

        user ??= Context.User;

        var nyaClaims = Data.Data.GetUserNyaClaims(user.Id);

        if (!nyaClaims.Any())
        {
            await ReplyAsync($"{user.Mention} You got no claims arab");
            return;
        }

        Embed embed = await GetNyaEmbedByPage(user.Id, 0, nyaClaims.Count);

        if (NyaClaimGlobals.PaginatedEmbed.ContainsKey(user.Id))
        {
            NyaClaimGlobals.PaginatedEmbed.Remove(user.Id);
        }

        PaginatedEmbed paginatedEmbed = new PaginatedEmbed()
        {
            CurrentPage = 0,
            TotalPages = nyaClaims.Count,
            GetPageEmbedAsync = GetNyaEmbedByPage,
            OwnerId = user.Id
        };

        var msg = await ReplyAsync(embed: embed, components: ScrollEmbed.BuildMessageComponent(false));

        NyaClaimGlobals.PaginatedEmbed.Add(msg.Id, paginatedEmbed);
    }

    public async Task<Embed> GetNyaEmbedByPage(ulong ownerId, int index, int totalPages)
    {
        using var context = new SqliteDbContext();
        var claim = await context.NyaClaims.FirstOrDefaultAsync(e => e.OwnerId == ownerId && e.SortIndex == index);
        if (claim == null)
            return null;

        EmbedBuilder builder = new EmbedBuilder();
        builder.WithImageUrl(claim.Url);
        builder.WithColor(Color.Blue);
        builder.WithTitle($"{index + 1}");
        builder.AddField("Claimed on", $"{claim.ClaimDate.ToString("yyyy-MM-dd")}", true);
        builder.AddField("Keys", $":key2: ({claim.Keys})", true);
        builder.WithFooter($"Belongs to {_client.GetUser(ownerId).GlobalName} ~~ {index + 1} / {totalPages}", _client.GetUser(ownerId).GetAvatarUrl());


        return builder.Build();
    }

}
