using Discord.WebSocket;
using Discord;
using KushBot.Global;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot;

public partial class MessageHandler
{
    public async Task HandleNyaTradeAsync(SocketMessage message)
    {
        if (!NyaClaimGlobals.NyaTrades.Any(e => e.Respondee.UserId == message.Author.Id || e.Suggester.UserId == message.Author.Id))
            return;

        NyaClaimGlobals.NyaTrades.RemoveAll(e => (DateTime.Now - e.DateTime).TotalSeconds > 90 && (e.Respondee.UserId == message.Author.Id || e.Suggester.UserId == message.Author.Id));

        try
        {
            if (NyaClaimGlobals.NyaTrades.Any(e => e.Respondee.UserId == message.Author.Id))
            {
                var trade = NyaClaimGlobals.NyaTrades.FirstOrDefault(e => e.Respondee.UserId == message.Author.Id);
                if (message.Content.Any(e => char.IsDigit(e)) && !message.Content.ToLower().Contains("kush"))
                {
                    var claim = NyaClaimGlobals.ParseTradeInput(message.Content);

                    if (claim == null)
                        return;

                    EmbedBuilder builder = new();
                    builder.WithColor(Color.Magenta);
                    builder.WithAuthor($"{message.Author.Username}'s trade response", message.Author.GetAvatarUrl());

                    var nyaClaim = Data.Data.GetClaimBySortIndex(message.Author.Id, (claim ?? 5000) - 1);

                    if (nyaClaim == null && claim != null)
                        return;

                    if (nyaClaim != null)
                    {
                        builder.WithImageUrl(nyaClaim.Url);
                        builder.AddField("Keys", ":key2: (0)", true);
                    }

                    trade.Respondee.NyaClaim = nyaClaim;

                    await message.Channel.SendMessageAsync($"{client.GetUser(trade.Suggester.UserId).Mention} {message.Author.Username} has responded, type 'confirm' in chat if you wish to confirm", embed: builder.Build());
                }
            }

            if (NyaClaimGlobals.NyaTrades.Any(e => e.Suggester.UserId == message.Author.Id && e.Respondee.HasResponded))
            {
                var trade = NyaClaimGlobals.NyaTrades.FirstOrDefault(e => e.Suggester.UserId == message.Author.Id && e.Respondee.HasResponded);
                if (trade != null && message.Content.ToLower() == "confirm")
                {
                    await message.Channel.SendMessageAsync($"{message.Author.Mention} :handshake: {client.GetUser(trade.Respondee.UserId).Mention} The trade has concluded.");
                    await Data.Data.ConcludeNyaTradeAsync(trade);
                }
            }
        }
        catch { }
    }
}
