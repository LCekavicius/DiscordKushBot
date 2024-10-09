using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace KushBot.EventHandler.Interactions;

public abstract class ComponentHandler
{
    public string CustomData { get; set; }
    public SocketInteraction Interaction { get; set; }
    public SocketMessageComponent Component { get; set; }
    public ulong UserId { get; set; }

    public ComponentHandler(string customData, SocketInteraction interaction, SocketMessageComponent component, ulong userId)
    {
        CustomData = customData;
        Interaction = interaction;
        Component = component;
        UserId = userId;
    }

    public abstract Task HandleClick();

    public abstract Task<MessageComponent> BuildMessageComponent(bool isDisabled = false);
}
