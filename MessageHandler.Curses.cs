using Discord.WebSocket;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KushBot;

public partial class MessageHandler
{
    public static List<CursedPlayer> CursedPlayers = [];

    //TODO Rewrite
    public async Task HandleCurseAsync(SocketUserMessage message)
    {
        CursedPlayer cp = CursedPlayers.Where(x => x.ID == message.Author.Id).FirstOrDefault();

        if (cp == null)
        {
            return;
        }

        if (cp.CurseName == "asked")
        {
            if (cp.Duration > 0)
            {
                await message.Channel.SendMessageAsync($"{message.Author.Mention} :warning: KLAUSEM :warning:");

                if (!cp.lastMessages.Contains(message.Content) && message.Content.Length > 2)
                {
                    cp.Duration -= 1;
                }

                if (cp.Duration <= 0)
                {
                    CursedPlayers.Remove(cp);
                    return;
                }

                cp.lastMessages.Add(message.Content);
            }
        }
        else if (cp.CurseName == "isnyk")
        {
            if (cp.Duration > 0)
            {
                await message.DeleteAsync();

                if (!cp.lastMessages.Contains(message.Content))
                {
                    cp.Duration -= 1;
                }

                if (cp.Duration == 0)
                {
                    CursedPlayers.Remove(cp);
                    return;
                }

                cp.lastMessages.Add(message.Content);
            }
        }
        else if (cp.CurseName == "degenerate")
        {
            if (cp.Duration > 0)
            {
                await message.Channel.SendFileAsync(DiscordBotService.WeebPaths[Random.Shared.Next(0, DiscordBotService.WeebPaths.Count)]);

                if (!cp.lastMessages.Contains(message.Content))
                {
                    cp.Duration -= 1;
                }

                if (cp.Duration == 0)
                {
                    CursedPlayers.Remove(cp);
                    return;
                }

                cp.lastMessages.Add(message.Content);
            }
        }
    }
}
