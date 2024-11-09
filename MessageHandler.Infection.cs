using Discord.WebSocket;
using KushBot.DataClasses;
using KushBot.Modules.Interactions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot;

public partial class MessageHandler
{
    private static Dictionary<ulong, DateTime> IgnoredUsers = [];
    public static ulong? InfestedChannelId = null;
    public static DateTime InfestedChannelDate = DateTime.MinValue;
    public static TimeSpan InfestedChannelDuration = TimeSpan.FromHours(1);

    public static string InfectionMessageText => "An odd looking egg appears from the ground. Best leave it be.";

    private async Task HandleInfestationEventAsync(SocketMessage message)
    {
        if (IgnoredUsers.ContainsKey(message.Author.Id))
        {
            if (IgnoredUsers[message.Author.Id] < DateTime.Now)
            {
                IgnoredUsers.Remove(message.Author.Id);
            }
            else
            {
                return;
            }
        }

        if (InfestedChannelId != null && (InfestedChannelDate + InfestedChannelDuration) > DateTime.Now)
        {
            if (message.Channel.Id != InfestedChannelId)
                return;

            if (Random.Shared.NextDouble() > 0.9935)
            {
                IgnoredUsers.Add(message.Author.Id, DateTime.Now.AddHours(8));

                await dbContext.UserInfections.AddAsync(new()
                {
                    OwnerId = message.Author.Id,
                    CreationDate = DateTime.Now,
                    KillAttemptDate = DateTime.MinValue
                });

                await dbContext.SaveChangesAsync();
            }
        }
        else
        {
            if (Random.Shared.NextDouble() > 0.995)
            {
                var perms = await dbContext.ChannelPerms.Where(e => e.Id == message.Channel.Id).Select(e => e.Permissions).FirstOrDefaultAsync();
                if (!perms.HasFlag(Permissions.Core))
                {
                    return;
                }
                var component = InfestationStart.BuildMessageComponent(false);
                await message.Channel.SendMessageAsync(InfectionMessageText, components: component);
            }
        }
    }

    private async Task HandleInfectionBapsConsumeAsync(SocketUserMessage message)
    {
        if (Random.Shared.NextDouble() < 0.97)
            return;

        var userData = await dbContext.Users
            .Include(e => e.UserInfections)
            .Where(e => e.Id == message.Author.Id)
            .FirstOrDefaultAsync();

        var qualifiedInfections = userData.UserInfections
            .Where(e => (e.State == InfectionState.Tyrant || e.State == InfectionState.NecroticSovereign || e.State == InfectionState.EldritchPatriarch)
                        && e.BapsDrained <= 6000)
            .ToList();

        if (!qualifiedInfections.Any())
            return;

        int bapsConsumed = 0;

        var infection = qualifiedInfections[Random.Shared.Next(0, qualifiedInfections.Count)];

        switch (infection.State)
        {
            case InfectionState.Tyrant:
                bapsConsumed += Random.Shared.Next(80, 141);
                break;
            case InfectionState.NecroticSovereign:
                bapsConsumed += Random.Shared.Next(180, 281);
                break;
            case InfectionState.EldritchPatriarch:
                bapsConsumed += Random.Shared.Next(300, 561);
                break;
            default:
                break;
        }


        bapsConsumed = Math.Min(bapsConsumed, userData.Balance);
        bapsConsumed = bapsConsumed < 0 ? 0 : bapsConsumed;

        if (bapsConsumed == 0)
            return;

        infection.BapsDrained += bapsConsumed;
        userData.Balance -= bapsConsumed;

        await dbContext.SaveChangesAsync();

        await message.Channel.SendMessageAsync($"{message.Author.Mention} A parasite eats away at your flesh, draining you out of {bapsConsumed} baps");
    }
}
