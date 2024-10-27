using Discord.WebSocket;

namespace KushBot.EventHandler.Interactions;

public class InteractionHandlerFactory
{
    public static string ParasiteComponentId = "kill";

    public ComponentHandler GetComponentHandler(string customData,
        ulong userId = default,
        SocketInteraction interaction = default,
        SocketMessageComponent component = default)
    {
        if (customData.StartsWith(ParasiteComponentId))
            return new ParasiteKillHandler(customData, interaction, component, userId);

        return null;
    }
}
