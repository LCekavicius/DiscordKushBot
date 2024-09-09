using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.EventHandler.Interactions
{
    public class InteractionHandlerFactory
    {
        public ComponentHandler GetComponentHandler(string customData,
            ulong userId = default,
            SocketInteraction interaction = default,
            SocketMessageComponent component = default)
        {
            if (customData.StartsWith(Program.VendorComponentId))
                return new VendorComponentHandler(customData, interaction, component, userId);

            if (customData.StartsWith(Program.InfestationEventComponentId))
                return new InfestationStartEventHandler(customData, interaction, component, userId);

            if (customData.StartsWith(Program.ParasiteComponentId))
                return new ParasiteKillHandler(customData, interaction, component, userId);

            if (customData.StartsWith(Program.NyaClaimComponentId))
                return new NyaClaimHandler(customData, interaction, component, userId);

            if (customData.StartsWith(Program.PaginatedComponentId))
                return new PaginatedEmbedHandler(customData, interaction, component, userId);

            return null;
        }
    }
}
