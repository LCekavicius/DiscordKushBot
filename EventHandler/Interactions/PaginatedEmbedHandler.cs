using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KushBot.Globals;

namespace KushBot.EventHandler.Interactions
{
    public class PaginatedEmbedHandler : ComponentHandler
    {
        public PaginatedEmbedHandler(string customData, SocketInteraction interaction, SocketMessageComponent component, ulong userId)
            : base(customData, interaction, component, userId)
        {

        }

        public override async Task HandleClick()
        {
            List<string> componentData = Component.Data.CustomId.Split('_').ToList();
            ulong ownerId = ulong.Parse(componentData[1]);
            string direction = componentData[2];

            if(!NyaClaimGlobals.PaginatedEmbed.TryGetValue(ownerId, out var paginatedEmbed))
            {
                return;
            }

            if(paginatedEmbed.TotalPages == 1)
            {
                return;
            }

            if(paginatedEmbed.GetPageEmbed != null)
            {
                await Component.Message.ModifyAsync(e =>
                {
                    e.Embed = paginatedEmbed.GetPageEmbed(ownerId, direction == "L" ? paginatedEmbed.PrevPage() : paginatedEmbed.NextPage(), paginatedEmbed.TotalPages);
                });
            }
            else if(paginatedEmbed.GetPageEmbedAsync != null)
            {
                Embed embed = await paginatedEmbed.GetPageEmbedAsync(ownerId, direction == "L" ? paginatedEmbed.PrevPage() : paginatedEmbed.NextPage(), paginatedEmbed.TotalPages);
                await Component.Message.ModifyAsync(e =>
                {
                    e.Embed = embed;
                });
            }
            

        }

        public override async Task<MessageComponent> BuildMessageComponent(bool isDisabled)
        {
            ComponentBuilder builder = new ComponentBuilder();
            return builder.WithButton("Claim", customId: $"",
                    disabled: true)
                .Build();

        }

    }
}
