using Discord;
using Discord.Commands;
using KushBot;
using KushBot.DataClasses;
using System;
using System.Linq;
using System.Threading.Tasks;

public class RequirePermissions(Permissions permissions, bool allowedInUndefined = false) : PreconditionAttribute
{
    public Permissions Permissions { get; } = permissions;
    public bool AllowedInUndefined { get; } = allowedInUndefined;

    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        if (context.User.IsBot)
        {
            return PreconditionResult.FromSuccess();
        }

        var channelPerms = DiscordBotService.ChannelPerms.FirstOrDefault(e => e.Id == context.Channel.Id);
        
        if (AllowedInUndefined && channelPerms is null)
        {
            return PreconditionResult.FromSuccess();
        }

        if(((channelPerms?.Permissions ?? Permissions.None) & Permissions) == Permissions)
        {
            return PreconditionResult.FromSuccess();
        }

        await context.Message.AddReactionAsync(Emoji.Parse("❌"));

        return PreconditionResult.FromError("Channel missing permissions for this command");
    }
}
