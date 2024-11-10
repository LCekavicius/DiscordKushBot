using Discord.Commands;
using KushBot.DataClasses;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules;

public class ChannelPermissionsModule(SqliteDbContext dbContext) : ModuleBase<SocketCommandContext>
{
    private ulong allowedId = 192642414215692300;

    [Command("permissions set")]
    public async Task SetChannelPermissions(string input)
    {
        if (Context.User.Id != allowedId)
        {
            return;
        }

        Permissions result = Permissions.None;
        var parts = input.Split(',');

        foreach (var part in parts)
        {
            var trimmedPart = part.Trim();

            if (Enum.TryParse(trimmedPart, ignoreCase: true, out Permissions parsedPermission))
            {
                result |= parsedPermission;
            }
            else
            {
                Console.WriteLine($"Warning: '{trimmedPart}' is not a valid permission.");
            }
        }

        var channelPerms = await dbContext.ChannelPerms.FirstOrDefaultAsync(e => e.Id == Context.Channel.Id);

        if (channelPerms != null)
        {
            channelPerms.Permissions = result;
            DiscordBotService.ChannelPerms.FirstOrDefault(e => e.Id == Context.Channel.Id).Permissions = result;
        }
        else
        {
            channelPerms = new ChannelPerms { Id = Context.Channel.Id, Permissions = result };
            DiscordBotService.ChannelPerms.Add(channelPerms);
            await dbContext.ChannelPerms.AddAsync(channelPerms);
        }

        await dbContext.SaveChangesAsync();
        await ReplyAsync($"{Context.User.Mention} Channel permissions updated");
    }

    [Command("permissions")]
    public async Task SetChannelPermissions()
    {
        List<string> strings = new();
        foreach (Permissions perm in Enum.GetValues(typeof(Permissions)))
        {
            strings.Add(perm.ToString());
        }

        await ReplyAsync($"Permitted permission values:\n {string.Join("\n", strings)}");
    }
}
