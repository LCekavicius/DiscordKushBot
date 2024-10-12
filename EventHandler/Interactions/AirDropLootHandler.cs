using Discord.WebSocket;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KushBot.Global;
using System.Diagnostics;
using System;
using KushBot.DataClasses;
using static System.Collections.Specialized.BitVector32;

namespace KushBot.EventHandler.Interactions;

public class AirDropLootHandler : ComponentHandler
{
    public AirDropLootHandler(string customData, SocketInteraction interaction, SocketMessageComponent component, ulong userId)
        : base(customData, interaction, component, userId)
    {

    }

    public override async Task HandleClick()
    {
        if (!Interaction.HasResponded)
        {
            await Interaction.DeferAsync();
        }

        List<string> componentData = Component.Data.CustomId.Split('_').ToList();

        var airdrop = AirDrops.Current.FirstOrDefault(e => e.Message.Id == Component.Message.Id);

        await TutorialManager.AttemptSubmitStepCompleteAsync(Interaction.User.Id, 4, 1, Interaction.Channel);

        if (airdrop == null)
        {
            return;
        }

        await airdrop.Loot(Interaction.User.Id);

        await Component.Message.ModifyAsync(async e =>
            {
                e.Embed = airdrop.UpdateBuilder().Build();
                e.Components = await BuildMessageComponent(false);
            });
    }

    public override Task<MessageComponent> BuildMessageComponent(bool isDisabled)
    {
        ComponentBuilder builder = new ComponentBuilder();
        return Task.FromResult(builder.WithButton("Loot", customId: $"{InteractionHandlerFactory.AirDropClaimComponentId}",
                emote: Emote.Parse(CustomEmojis.Ima),
                style: ButtonStyle.Secondary,
                disabled: isDisabled)
            .Build());
    }

}
