using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using KushBot.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules.Interactions;

public class BossParticipation(
    DiscordSocketClient client,
    SqliteDbContext dbContext,
    BossService bossService) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction($"{nameof(BossParticipation)}_Join")]
    public async Task Join() => await HandleAsync(true);

    [ComponentInteraction($"{nameof(BossParticipation)}_Leave")]
    public async Task Leave() => await HandleAsync(false);

    public async Task HandleAsync(bool isJoining)
    {
        var interaction = Context.Interaction;

        var message = ((SocketMessageComponent)interaction).Message;

        var boss = await dbContext.Bosses
            .Include(e => e.Participants)
            .Where(e => e.MessageId == message.Id)
            .FirstOrDefaultAsync();

        var participant = boss.Participants.FirstOrDefault(e => e.UserId == Context.User.Id);

        if (isJoining)
        {
            if (participant is not null)
            {
                return;
            }

            boss.Participants.Add(new BossParticipant
            {
                UserId = Context.User.Id,
                BossId = boss.Id,
            });

        }
        else
        {
            if (participant == null)
            {
                return;
            }

            boss.Participants.Remove(participant);
        }

        await dbContext.SaveChangesAsync();

        await message.ModifyAsync(async e =>
        {
            e.Embed = await bossService.GetBossEmbed(boss);
            e.Components = BuildMessageComponent(boss.Participants.Count >= boss.ParticipantSlots, boss.Participants.Count <= 0);
        });
    }

    public static MessageComponent BuildMessageComponent(bool isJoinDisabled, bool isLeaveDisabled)
    {
        var builder = new ComponentBuilder();
        builder.WithButton("Join", customId: $"{nameof(BossParticipation)}_Join",
                emote: Emote.Parse(CustomEmojis.Booba),
                style: ButtonStyle.Secondary,
                disabled: isJoinDisabled);

        builder.WithButton("Leave", $"{nameof(BossParticipation)}_Leave",
                emote: Emoji.Parse(":x:"),
                style: ButtonStyle.Secondary,
                disabled: isLeaveDisabled);

        return builder.Build();
    }
}
