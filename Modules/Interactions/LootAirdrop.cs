using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using KushBot.Global;
using KushBot.Resources.Database;
using KushBot.Services;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules.Interactions;

public class LootAirdrop(DiscordSocketClient client, SqliteDbContext dbContext, TutorialManager tutorialManager) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction(nameof(LootAirdrop))]
    public async Task Loot()
    {
        var interaction = Context.Interaction;

        var message = ((SocketMessageComponent)interaction).Message;
        var airdrop = AirDrops.Current.FirstOrDefault(e => e.Message.Id == message.Id);

        var user = await dbContext.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Pets | Data.UserDtoFeatures.Items);
        bool stepCompleted = await tutorialManager.AttemptSubmitStepCompleteAsync(user, 4, 1, Context.Channel);

        if (airdrop == null)
        {
            if (stepCompleted)
            {
                await dbContext.SaveChangesAsync();
            }

            return;
        }

        airdrop.Loot(user);

        await dbContext.SaveChangesAsync();

        await message.ModifyAsync(e =>
        {
            e.Embed = airdrop.UpdateBuilder(client).Build();
            e.Components = BuildMessageComponent(false);
        });
    }

    public static MessageComponent BuildMessageComponent(bool isDisabled)
    {
        return new ComponentBuilder()
            .WithButton("Loot", customId: nameof(LootAirdrop),
                emote: Emote.Parse(CustomEmojis.Ima),
                style: ButtonStyle.Secondary,
                disabled: isDisabled)
            .Build();
    }
}
