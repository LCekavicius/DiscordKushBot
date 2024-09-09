using Discord;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.EventHandler.Interactions
{
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
        //public abstract Task HandleCreate(IMessageChannel channel);
        public abstract Task<MessageComponent> BuildMessageComponent(bool isDisabled = default);
    }
}
