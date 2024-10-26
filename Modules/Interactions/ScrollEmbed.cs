using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using KushBot.EventHandler.Interactions;
using KushBot.Global;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KushBot.Modules.Interactions;

public class ScrollEmbed : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordSocketClient _client;

    public ScrollEmbed(DiscordSocketClient client)
    {
        _client = client;
    }

    [ComponentInteraction(nameof(ScrollEmbed) + "L")]
    public async Task ScrollLeft() => await Scroll(true);

    [ComponentInteraction(nameof(ScrollEmbed) + "R")]
    public async Task ScrollRight() => await Scroll(false);

    private async Task Scroll(bool left)
    {
        var interaction = Context.Interaction;
        var message = ((SocketMessageComponent)interaction).Message;

        // todo this causes a memory leak, handle endless paginated embeds
        if (!NyaClaimGlobals.PaginatedEmbed.TryGetValue(message.Id, out var paginatedEmbed))
        {
            return;
        }

        if (paginatedEmbed.TotalPages == 1)
        {
            return;
        }

        if (paginatedEmbed.GetPageEmbed != null)
        {
            await message.ModifyAsync(e =>
            {
                e.Embed = paginatedEmbed.GetPageEmbed(paginatedEmbed.OwnerId, left ? paginatedEmbed.PrevPage() : paginatedEmbed.NextPage(), paginatedEmbed.TotalPages);
            });
        }
        else if (paginatedEmbed.GetPageEmbedAsync != null)
        {
            Embed embed = await paginatedEmbed.GetPageEmbedAsync(paginatedEmbed.OwnerId, left ? paginatedEmbed.PrevPage() : paginatedEmbed.NextPage(), paginatedEmbed.TotalPages);
            await message.ModifyAsync(e =>
            {
                e.Embed = embed;
            });
        }
    }

    public static MessageComponent BuildMessageComponent(bool isDisabled)
    {
        return new ComponentBuilder()
            .WithButton(customId: nameof(ScrollEmbed) + "L",
                emote: Emoji.Parse(":arrow_left:"),
                style: ButtonStyle.Secondary,
                disabled: isDisabled)
            .WithButton(customId: nameof(ScrollEmbed) + "R",
                emote: Emoji.Parse(":arrow_right:"),
                style: ButtonStyle.Secondary,
                disabled: isDisabled)
            .Build();
    }
}
