using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules.Interactions;

public class KillParasite : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordSocketClient _client;
    private readonly SqliteDbContext _context;

    public KillParasite(DiscordSocketClient client, SqliteDbContext context)
    {
        _client = client;
        _context = context;
    }

    [ComponentInteraction($"{nameof(KillParasite)}_*")]
    public async Task Loot(string infectionId)
    {
        if (!Guid.TryParse(infectionId, out var id))
        {
            await FollowupAsync($"An error occured while trying to kill parasite #{id}", ephemeral: true);
            return;
        }

        var user = await _context.GetKushBotUserAsync(Context.User.Id, UserDtoFeatures.Pets | UserDtoFeatures.Infections);

        var infection = user.UserInfections.FirstOrDefault(e => e.Id == id);

        if (infection is null)
        {
            return;
        }

        var message = ((SocketMessageComponent)Context.Interaction).Message;

        var response = "";

        if (infection.State == InfectionState.AbyssalArchon)
        {
            response = HandleSummon(user, infection);
        }
        else
        {
            response = HandleKill(user, infection);
        }

        await _context.SaveChangesAsync();

        await message.ModifyAsync(e =>
        {
            e.Components = BuildMessageComponent(user);
        });
        
        await Context.Channel.SendMessageAsync(response);
    }

    private string HandleSummon(KushBotUser user, Infection infection)
    {
        //if (Program.ArchonObject != null)
        //    return;

        //Program.SpawnBoss(true, Interaction.User.Id);

        _context.UserInfections.Remove(infection);
        user.UserInfections.Remove(infection);

        return $"{Context.User.Mention} As you tug on a black protrusion on your neck, a godlike being emerges from your body, An Archon has appeared. <#{DiscordBotService.BossChannelId}>";
    }

    private string HandleKill(KushBotUser user, Infection infection)
    {
        (int? baps, bool isCd) = KillInfection(user, infection);

        if (isCd)
        {
            var remainingTime = infection.KillAttemptDate.AddHours(2) - TimeHelper.Now;
            return $"{Context.User.Mention} The parasite has hidden itself, it will be available for retry in: {remainingTime.ToString(@"hh\:mm\:ss")}";
        } 

        string stateName = EnumHelperV2Singleton.Instance.Helper.ToString<InfectionState>(infection.State);

        if (!baps.HasValue)
        {
            return $"{Context.User.Mention} You try your best to remove the {stateName} tier parasite, but it has " +
                $"burrowed deep into your veins, the only thing you're able to pull out is vast amounts of your own blood. You'll have to wait to try again.";
        }
        else
        {
            var response = $"{Context.User.Mention} ";
            response += GetInfectionResponseStringAsync(infection.State, baps > 0);
            response += $"{Math.Abs(baps ?? 0)} baps";
            response += baps > 0 ? $" {CustomEmojis.Gladge}" : $" {CustomEmojis.Sadge}";

            return response;
        }
    }

    public (int? baps, bool isCd) KillInfection(KushBotUser user, Infection infection)
    {
        if (infection == null)
            return (null, true);

        if (infection.KillAttemptDate.AddHours(2) > DateTime.Now)
            return (null, true);

        if (infection is null)
            return (null, false);

        bool isKilled = AttemptKillInfection(infection);

        if (!isKilled)
            return (null, false);

        int petLvl = user.Pets.TotalCombinedPetLevel;

        int bapsForKill = infection.GetBapsForKill(petLvl);

        user.Balance += bapsForKill;

        return (bapsForKill, false);
    }

    private bool AttemptKillInfection(Infection infection)
    {
        if (Random.Shared.NextDouble() > infection.GetInfectionKillChance())
        {
            infection.KillAttemptDate = DateTime.Now;
            _context.UserInfections.Update(infection);
            return false;
        }

        _context.UserInfections.Remove(infection);
        return true;
    }

    private string GetInfectionResponseStringAsync(InfectionState state, bool isProfit)
    {
        if (state == InfectionState.Hatchling)
        {
            if (!isProfit)
                return $"The hatchling violently squirms and wiggles as you are drawing it out from your veins. 40 centimeters of agony later you are drained of ";
            else
                return $"You tug on the parasite to flush it out from your blood vessels in a matter of seconds, you trade the 40 centimere rope into ";
        }
        else if (state == InfectionState.Juvenile)
        {
            if (!isProfit)
                return $"The juvenile parasite lacerates your insides on its way out leaving you bleeding and losing ";
            else
                return $"The juvenile parasite had turned domestic and leaves your body with ease you consume the meal and receive ";
        }
        else if (state == InfectionState.Tyrant)
        {
            if (!isProfit)
                return $"The Tyrant tier parasite mutilates your multiple body parts leaving you in a pool of blood and severed limbs, you have lost ";
            else
                return $"The Tyrant tier parasite has slashed and teared your insides yet it was a small price to pay for a mysterious traveler buying it off you for ";
        }
        else if (state == InfectionState.NecroticSovereign)
        {
            if (!isProfit)
                return $"The irreparable damage to your soul inflicted by the necrotic sovereign tier parasite has left you permanently scarred by losing ";
            else
                return $"The souls of the innocent the sovereign parasite had consumed while in control of your body provide you with ";
        }
        else if (state == InfectionState.EldritchPatriarch)
        {
            if (!isProfit)
                return $"The parasite in you had grown into an eldritch patriarch, removing it paralyzed you from neck-down, but its nothing compared to the emotional damage received from killing your beloved master, you are sucked out of ";
            else
                return $"The parasite in you had grown into an eldritch patriarch, you are able to fight against the mind control and even though you are left blind and paralyzed you are happy that the agony is finally over +";
        }

        return "After squishing the sticky egg it leaves behind some green, inconspicuous goo providing you with ";
    }

    public static MessageComponent BuildMessageComponent(KushBotUser user)
    {
        ComponentBuilder builder = new();

        var userInfections = user.UserInfections;

        foreach (var item in userInfections)
        {
            builder.WithButton(
                label: item.State == InfectionState.AbyssalArchon ? "Summon" : "Kill",
                customId: $"{nameof(KillParasite)}_{item.Id}",
                style: item.GetButtonStyle(),
                emote: item.GetEmote(),
                disabled: 
                    item.KillAttemptDate.AddHours(2) > DateTime.Now
                || (item.State == InfectionState.AbyssalArchon && user.Pets.TotalCombinedPetLevel < 20));
        }

        return builder.Build();
    }
}
