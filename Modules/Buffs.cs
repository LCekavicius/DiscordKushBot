using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using KushBot.DataClasses;
using System.Linq;

namespace KushBot.Modules
{
    public class Buffs : ModuleBase<SocketCommandContext>
    {
        [Command("Buffs")]
        [RequirePermissions(Permissions.Core)]
        public async Task PingAsync(IUser user = null)
        {
            user ??= Context.User;
            EmbedBuilder builder = new();
            List<ConsumableBuff> buffs = Data.Data.GetConsumableBuffList(user.Id);
            int rageDuration = Data.Data.GetRageDuration(user.Id);

            int buffCount = buffs.Count + (rageDuration > 0 ? 1 : 0);

            builder.WithAuthor($"{user.Username}'s buffs {buffCount} / 15", user.GetAvatarUrl());
            builder.WithColor(Color.Red);


            if (rageDuration > 0)
            {
                builder.AddField("Tyler rage <:fear:1231718238031712316>", $"Raging for {rageDuration} more gamble\nGenerate rage baps, paid out when the rage ends");
            }


            if (!buffs.Any() && rageDuration == 0)
            {
                builder.AddField("\u200b", "​​​​\u200b", true);
                builder.AddField("\u200b", $"​​​​\n\n\n\n\n\n<:fear:1231718238031712316>", true);
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
