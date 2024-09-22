using Discord.WebSocket;

namespace KushBot.EventHandler.Interactions;

public class InteractionHandlerFactory
{
    public ComponentHandler GetComponentHandler(string customData,
        ulong userId = default,
        SocketInteraction interaction = default,
        SocketMessageComponent component = default)
    {
        if (customData.StartsWith(DiscordBotService.VendorComponentId))
            return new VendorComponentHandler(customData, interaction, component, userId);

        if (customData.StartsWith(DiscordBotService.InfestationEventComponentId))
            return new InfestationStartEventHandler(customData, interaction, component, userId);

        if (customData.StartsWith(DiscordBotService.ParasiteComponentId))
            return new ParasiteKillHandler(customData, interaction, component, userId);

        if (customData.StartsWith(DiscordBotService.NyaClaimComponentId))
            return new NyaClaimHandler(customData, interaction, component, userId);

        if (customData.StartsWith(DiscordBotService.PaginatedComponentId))
            return new PaginatedEmbedHandler(customData, interaction, component, userId);

        return null;
    }
}
