using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace KushBot.Modules
{
    public class InfectionList : ModuleBase<SocketCommandContext>
    {
        [Command("parasites"), Alias("parasite")]
        public async Task ListInfections()
        {
            List<Infection> infections = await Data.Data.GetUserInfectionsAsync(Context.User.Id);

            if (!infections.Any())
            {
                await ReplyAsync($"{Context.User.Mention} you are not infected with parasites <:pepegladge:1224000106253127711>");
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
}
