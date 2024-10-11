using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KushBot.DataClasses;

namespace KushBot.EventHandler.Interactions
{
    public class ParasiteKillHandler : ComponentHandler
    {
        public ParasiteKillHandler(string customData, SocketInteraction interaction, SocketMessageComponent component, ulong userId)
            : base(customData, interaction, component, userId)
        {

        }

        public override async Task HandleClick()
        {
            List<string> componentData = Component.Data.CustomId.Split('_').ToList();
            InfectionState state = (InfectionState)int.Parse(componentData[3]);

            if (ulong.Parse(componentData[1]) != Interaction.User.Id)
                return;

            if (state == InfectionState.AbyssalArchon)
            {
                //if (Program.ArchonObject != null)
                //    return;

                //Program.SpawnBoss(true, Interaction.User.Id);
                await Data.Data.RemoveInfection(Guid.Parse(componentData[2]));
                await Component.Channel.SendMessageAsync($"{Interaction.User.Mention} As you tug on a black protrusion on your neck, a godlike being emerges from your body, An Archon has appeared. <#{DiscordBotService.BossChannelId}>");
                await Component.Message.ModifyAsync(async e => e.Components = await BuildMessageComponent(false));
                
                return;
            }

            (int? baps, bool isCd) = await Data.Data.KillInfectionAsync(Guid.Parse(componentData[2]));

            if (isCd)
                return;
            
            string stateName = EnumHelperV2Singleton.Instance.Helper.ToString<InfectionState>(state);

            var userInfections = await Data.Data.GetUserInfectionsAsync(Interaction.User.Id);

            if (!baps.HasValue)
            {
                await Interaction.Channel.SendMessageAsync($"{Interaction.User.Mention} You try your best to remove the {stateName} tier parasite, but it has " +
                    $"burrowed deep into your veins, the only thing you're able to pull out is vast amounts of your own blood. You'll have to wait to try again.");

                await Component.Message.ModifyAsync(async e => e.Components = await BuildMessageComponent(false));
                return;
            }


            string response = $"{Interaction.User.Mention} ";
            response += GetInfectionResponseStringAsync(state, baps > 0);
            response += $"{Math.Abs(baps ?? 0)} baps";
            response += baps > 0 ? $" <:pepegladge:1224000106253127711>" : " <:sadge:945703001123848203>";


            await Component.Message.ModifyAsync(async e => e.Components = await BuildMessageComponent(false));

            await Interaction.Channel.SendMessageAsync(response);
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

        public override async Task<MessageComponent> BuildMessageComponent(bool isDisabled)
        {
            ComponentBuilder builder = new();

            var userInfections = await Data.Data.GetUserInfectionsAsync(UserId);

            foreach (var item in userInfections)
            {
                builder.WithButton(
                    item.State == InfectionState.AbyssalArchon ? "Summon" : "Kill",
                    $"kill_{UserId}_{item.Id}_{(int)item.State}",
                    item.GetButtonStyle(),
                    item.GetEmote(),
                    disabled: item.KillAttemptDate.AddHours(2) > DateTime.Now
                    || (item.State == InfectionState.AbyssalArchon
                        //&& (Program.ArchonObject != null || Program.GetTotalPetLvl(UserId) < 20)
                        ));
            }

            return builder.Build();
        }



    }
}
