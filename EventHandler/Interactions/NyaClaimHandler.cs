using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KushBot.Global;

namespace KushBot.EventHandler.Interactions;

public class NyaClaimHandler : ComponentHandler
{
    public NyaClaimHandler(string customData, SocketInteraction interaction, SocketMessageComponent component, ulong userId)
        : base(customData, interaction, component, userId)
    {

    }

    private int GetMaxClaims(ulong userId)
    {
        return Program.BaseMaxNyaClaims + Data.Data.GetUserExtraClaims(userId);
    }

    public override async Task HandleClick()
    {
        List<string> componentData = Component.Data.CustomId.Split('_').ToList();
        Guid nyaClaimEvent = Guid.Parse(componentData[1]);

        if (!NyaClaimGlobals.NyaClaimEvents.TryGetValue(nyaClaimEvent, out var nyaClaim))
        {
            return;
        }

        if (nyaClaim.UserId != Interaction.User.Id && nyaClaim.TimeStamp.AddSeconds(5) > DateTime.Now)
        {
            return;
        }

        int maxAllowedClaims = GetMaxClaims(Interaction.User.Id);

        if (Data.Data.GetUserNyaClaims(Interaction.User.Id).Count >= maxAllowedClaims)
        {
            await Interaction.RespondAsync($"You can only have {maxAllowedClaims} claimed nya/vroom", ephemeral: true);
            return;
        }

        await Data.Data.SaveClaimAsync(Interaction.User.Id, nyaClaim);

        NyaClaimGlobals.NyaClaimEvents.Remove(nyaClaimEvent);

        await Component.Message.ModifyAsync(async e => e.Components = await BuildMessageComponent(false));
        await Interaction.Channel.SendMessageAsync($"{Interaction.User.Mention} successfully claimed a nya/vroom. See 'kush nya claims' or 'kush vroom claims'");
    }

    public override async Task<MessageComponent> BuildMessageComponent(bool isDisabled)
    {
        ComponentBuilder builder = new ComponentBuilder();
        return builder.WithButton("Claim", customId: $"{Program.NyaClaimComponentId}",
                emote: Emote.Parse("<:ima:945342040529567795>"),
                style: ButtonStyle.Secondary,
                disabled: true)
            .Build();

    }

}
