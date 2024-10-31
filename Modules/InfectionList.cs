using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules;

public class InfectionList(SqliteDbContext dbContext) : ModuleBase<SocketCommandContext>
{
    [Command("parasites"), Alias("parasite")]
    [RequirePermissions(Permissions.Core)]
    public async Task ListInfections()
    {
        var infections = await dbContext.UserInfections.Where(e => e.OwnerId == Context.User.Id).ToListAsync();

        if (!infections.Any())
        {
            await ReplyAsync($"{Context.User.Mention} you are not infected with parasites {CustomEmojis.Gladge}");
            return;
        }

        EmbedBuilder builder = new EmbedBuilder();
        builder.WithColor(Color.Red);
        builder.WithTitle($"{Context.User.Username}'s afflictions:");
        foreach (var item in infections)
        {
            TimeSpan timeAlive = DateTime.Now - item.CreationDate;
            builder.AddField($"" +
                $"{item.GetEmote()}{EnumHelperV2Singleton.Instance.Helper.ToString<InfectionState>(item.State)}{item.GetEmote()}",
                item.State >= InfectionState.Tyrant
                ? $"Consumed baps:\n**{item.BapsDrained}/{NextTierCost(item.State)}**\nHours alive:\n{(int)timeAlive.TotalHours}/{item.GetRequiredHoursForGrowth()}"
                : $"Not consuming baps\nHours alive:\n{(int)timeAlive.TotalHours}/{item.GetRequiredHoursForGrowth()}",
                true);
        }
        builder.WithFooter("Abyssal archons require atleast 20 Total pet level to summon. Preferably more.");
        await ReplyAsync(embed: builder.Build());

    }

    private int NextTierCost(InfectionState state)
    {
        if (state == InfectionState.NecroticSovereign)
        {
            return 3000;
        }
        if (state == InfectionState.EldritchPatriarch || state == InfectionState.AbyssalArchon)
        {
            return 6000;
        }
        return 1000;
    }
}
