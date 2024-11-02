using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using KushBot.DataClasses;
using System.Linq;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using KushBot.Global;

namespace KushBot.Modules
{
    public class Buffs(SqliteDbContext dbContext) : ModuleBase<SocketCommandContext>
    {
        [Command("Buffs")]
        [RequirePermissions(Permissions.Core)]
        public async Task PingAsync(IUser user = null)
        {
            user ??= Context.User;
            EmbedBuilder builder = new();
            List<ConsumableBuff> buffs = await dbContext.ConsumableBuffs.Where(e => e.OwnerId == user.Id).ToListAsync();
            //int rageDuration = Data.Data.GetRageDuration(user.Id);

            //int buffCount = buffs.Count + (rageDuration > 0 ? 1 : 0);
            int buffCount = buffs.Count;

            builder.WithAuthor($"{user.Username}'s buffs {buffCount} / 15", user.GetAvatarUrl());
            builder.WithColor(Color.Red);


            //if (rageDuration > 0)
            //{
            //    builder.AddField($"Tyler rage {CustomEmojis.Fear}", $"Raging for {rageDuration} more gamble\nGenerate rage baps, paid out when the rage ends");
            //}


            //if (!buffs.Any() && rageDuration == 0)
            if (!buffs.Any())
            {
                builder.AddField("\u200b", "​​​​\u200b", true);
                builder.AddField("\u200b", $"​​​​\n\n\n\n\n\n{CustomEmojis.Fear}", true);
                builder.AddField("\u200b", "​​​​\u200b", true);
                builder.AddField("\u200b", "​​​​​​​​\n\n\n\n\n\n\n\n\n\n");
            }

            foreach (var buff in buffs)
            {
                builder.AddField(buff.DisplayName, buff.GetDescriptionByType());
            }

            await ReplyAsync(embed: builder.Build());
        }
    }
}
