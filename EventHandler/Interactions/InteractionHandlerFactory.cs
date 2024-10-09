using Discord.WebSocket;

namespace KushBot.EventHandler.Interactions;

public class InteractionHandlerFactory
{
    public static string InfestationEventComponentId = "infestation-start";
    public static string VendorComponentId = "vendor";
    public static string ParasiteComponentId = "kill";
    public static string NyaClaimComponentId = "nyaClaim";
    public static string PaginatedComponentId = "paginated";
    public static string AirDropClaimComponentId = "airDropLoot";

    public ComponentHandler GetComponentHandler(string customData,
        ulong userId = default,
        SocketInteraction interaction = default,
        SocketMessageComponent component = default)
    {
        if (customData.StartsWith(VendorComponentId))
            return new VendorComponentHandler(customData, interaction, component, userId);

        if (customData.StartsWith(InfestationEventComponentId))
            return new InfestationStartEventHandler(customData, interaction, component, userId);

        if (customData.StartsWith(ParasiteComponentId))
            return new ParasiteKillHandler(customData, interaction, component, userId);

        if (customData.StartsWith(NyaClaimComponentId))
            return new NyaClaimHandler(customData, interaction, component, userId);

        if (customData.StartsWith(PaginatedComponentId))
            return new PaginatedEmbedHandler(customData, interaction, component, userId);

        if (customData.StartsWith(AirDropClaimComponentId))
            return new AirDropLootHandler(customData, interaction, component, userId);

        return null;
    }
}
