using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules.Interactions;

public class LootAirdrop : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordSocketClient _client;
    private readonly SqliteDbContext _context;

    public LootAirdrop(DiscordSocketClient client, SqliteDbContext context)
    {
        _client = client;
        _context = context;
    }

    [ComponentInteraction(nameof(LootAirdrop))]
    public async Task Loot()
    {
        var interaction = Context.Interaction;

        var message = ((SocketMessageComponent)interaction).Message;
        var airdrop = AirDrops.Current.FirstOrDefault(e => e.Message.Id == message.Id);

        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 4, 1, Context.Channel);

        if (airdrop == null)
        {
            return;
        }

        var user = await _context.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Pets | Data.UserDtoFeatures.Items);

        airdrop.Loot(user);

        await _context.SaveChangesAsync();

        await message.ModifyAsync(e =>
        {
            e.Embed = airdrop.UpdateBuilder(_client).Build();
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
