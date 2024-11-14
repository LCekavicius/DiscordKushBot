using Discord;
using Discord.WebSocket;
using KushBot.DataClasses;
using KushBot.DataClasses.enums;
using KushBot.Global;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Services;

public class BossService(DiscordSocketClient client, SqliteDbContext dbContext)
{
    public async Task<Embed> GetBossEmbed(Boss boss)
    {
        var bossBase = BossBases.Bases[boss.BossBaseIndex];
        List<string> userNames = [];
        foreach (var item in boss.Participants)
        {
            var user = await client.GetUserAsync(item.UserId);
            userNames.Add(user.Username);
        }

        var abilities = boss.Abilities.GetEnabled();

        var abilityString = string.Join("\n", abilities.Select(e => $"{BossBases.AbilityEmojis[e]} {e}"));
        var footerString = string.Join("\n", abilities.Select(e => $"{e} - {BossBases.AbilityDescriptions[e]}"));

        return new EmbedBuilder()
            .WithTitle(bossBase.Name)
            .WithColor(boss.Rarity.GetRarityColor())
            .AddField("Level", $"{boss.Level} :level_slider:", inline: true)
            .AddField("Boss hp:", $"{boss.Health} :heart:", inline: true)
            .AddField($"Rarity:\n{boss.Rarity} :diamond_shape_with_a_dot_inside:", bossBase.Description)
            .AddField($"Participants ({boss.Participants.Count}/{boss.ParticipantSlots})",
                        boss.Participants.Any() ? string.Join("\n", userNames) : "---", inline: true)
            .AddField("Abilities", abilityString, inline: true)
            .AddField("Results", $"The battle will start <t:{((boss.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds).ToString().Split('.')[0]}:R>")
            .WithFooter(footerString)
            .Build();
    }

    public async Task HandleBossFightAsync(Boss boss)
    {
        var participants = await GetUsersAsync(boss);
        var labeled = await LabelUsersAsync(participants);

        var sim = new BossFightSim(boss, labeled.ToList());
        sim.Handle();
    }

    private async Task<ParticipatingUser[]> LabelUsersAsync(List<KushBotUser> users) =>
        await Task.WhenAll(users.Select(async item =>
        {
            var discordUser = await client.GetUserAsync(item.Id);
            return new ParticipatingUser(discordUser.Username, item);
        }));


    private async Task<List<KushBotUser>> GetUsersAsync(Boss boss)
    {
        var participantIds = boss.Participants.Select(e => e.UserId).ToList();
        var users = await dbContext.Users
            .Include(e => e.UserBuffs)
            .Include(e => e.Items)
                .ThenInclude(e => e.ItemStats)
            .Where(e => participantIds.Contains(e.Id))
            .ToListAsync();

        var pets = await dbContext.UserPets.Where(e => participantIds.Contains(e.UserId)).ToListAsync();

        foreach (var user in users)
        {
            user.Pets = new UserPets(pets.Where(e => e.UserId == user.Id).ToList());
        }

        return users;
    }
}
