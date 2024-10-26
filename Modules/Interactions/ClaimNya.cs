using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using KushBot.DataClasses;
using KushBot.EventHandler.Interactions;
using KushBot.Global;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules.Interactions;

public class ClaimNya : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordSocketClient _client;
    private readonly SqliteDbContext _context;

    public ClaimNya(DiscordSocketClient client, SqliteDbContext context)
    {
        _client = client;
        _context = context;
    }

    [ComponentInteraction(nameof(ClaimNya))]
    public async Task Claim()
    {
        var interaction = Context.Interaction;
        var message = ((SocketMessageComponent)interaction).Message;

        if (!NyaClaimGlobals.NyaClaimEvents.TryGetValue(message.Id, out var nyaClaim))
        {
            return;
        }

        if (nyaClaim.UserId != Context.User.Id && nyaClaim.TimeStamp.AddSeconds(5) > DateTime.Now)
        {
            return;
        }

        if (nyaClaim.TimeStamp.AddMinutes(2) < DateTime.Now)
        {
            await interaction.RespondAsync($"This roll was over 2 minutes ago and can't be claimed", ephemeral: true);
            return;
        }

        var claimsData = await _context.Users
            .Include(e => e.NyaClaims)
            .AsNoTracking()
            .Where(e => e.Id == interaction.User.Id)
            .Select(e => new
            {
                MaxAllowedClaims = e.ExtraClaimSlots + NyaClaimGlobals.BaseMaxNyaClaims,
                Claims = e.NyaClaims
            })
            .FirstOrDefaultAsync();

        if (claimsData.Claims.Count >= claimsData.MaxAllowedClaims)
        {
            await interaction.RespondAsync($"You can only have {claimsData.MaxAllowedClaims} claimed nya/vroom", ephemeral: true);
            return;
        }

        await _context.NyaClaims.AddAsync(new NyaClaim()
        {
            FileName = nyaClaim.FileName,
            OwnerId = Context.User.Id,
            Url = nyaClaim.ImageMessage.Attachments.FirstOrDefault()?.Url,
            SortIndex = claimsData.Claims.Any() ? claimsData.Claims.Max(e => e.SortIndex) + 1 : 0,
            ClaimDate = TimeHelper.Now,
        });


        NyaClaimGlobals.NyaClaimEvents.Remove(message.Id);
        await _context.SaveChangesAsync();

        await message.ModifyAsync(e => e.Components = BuildMessageComponent(true));
        await message.Channel.SendMessageAsync($"{Context.User.Mention} successfully claimed a nya/vroom. See 'kush nya claims' or 'kush vroom claims'");
    }

    public static MessageComponent BuildMessageComponent(bool isDisabled)
    {
        return new ComponentBuilder()
            .WithButton("Claim", customId: nameof(ClaimNya),
                emote: Emote.Parse(CustomEmojis.Ima),
                disabled: isDisabled,
                style: ButtonStyle.Secondary)
            .Build();
    }
}
