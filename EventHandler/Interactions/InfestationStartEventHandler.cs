using Discord.WebSocket;
using Discord;
using System;
using System.Threading.Tasks;

namespace KushBot.EventHandler.Interactions;

public class InfestationStartEventHandler : ComponentHandler
{
    public InfestationStartEventHandler(string customData, SocketInteraction interaction, SocketMessageComponent component, ulong userId)
        : base(customData, interaction, component, userId)
    {

    }

    public override async Task HandleClick()
    {
        if (((Program.InfestedChannelDate ?? DateTime.MinValue) + Program.InfestedChannelDuration) > DateTime.Now)
        {
            return;
        }

        Program.InfestedChannelDate = DateTime.Now;
        Program.InfestedChannelId = Interaction.ChannelId;

        await Component.Message.ModifyAsync(async o =>
        {
            o.Components = await BuildMessageComponent(true);
        });

        await Interaction.Channel.SendMessageAsync($"{Interaction.User.Mention} May have unleashed something upon <#{Interaction.ChannelId}>");
    }

    public override async Task<MessageComponent> BuildMessageComponent(bool isDisabled)
    {
        return new ComponentBuilder()
            .WithButton("Squeeze", customId: Program.InfestationEventComponentId,
                emote: Emote.Parse("<:p1:1224001339085029386>"),
                style: ButtonStyle.Danger,
                disabled: isDisabled)
            .Build();
    }

}
