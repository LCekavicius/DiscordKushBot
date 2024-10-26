using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace KushBot.Modules.Interactions;

public class InfestationStart : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordSocketClient _client;

    public InfestationStart(DiscordSocketClient client)
    {
        _client = client;
    }

    [ComponentInteraction(nameof(InfestationStart))]
    public async Task Infest()
    {
        if (((DiscordBotService.InfestedChannelDate ?? DateTime.MinValue) + DiscordBotService.InfestedChannelDuration) > DateTime.Now)
        {
            return;
        }

        var interaction = Context.Interaction;

        var message = ((SocketMessageComponent)interaction).Message;

        DiscordBotService.InfestedChannelDate = DateTime.Now;
        DiscordBotService.InfestedChannelId = interaction.ChannelId;

        await message.ModifyAsync(o =>
        {
            o.Components = BuildMessageComponent(true);
        });

        await interaction.Channel.SendMessageAsync($"{Context.User.Mention} May have unleashed something upon <#{interaction.ChannelId}>");
    }

    public static MessageComponent BuildMessageComponent(bool isDisabled)
    {
        return new ComponentBuilder()
            .WithButton("Squeeze", customId: nameof(InfestationStart),
                emote: Emote.Parse("<:p1:1224001339085029386>"),
                style: ButtonStyle.Danger,
                disabled: isDisabled)
            .Build();
    }
}
