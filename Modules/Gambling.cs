using Discord.Commands;
using KushBot.DataClasses;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace KushBot.Modules;

[RequirePermissions(Permissions.Core)]
public class Gambling : ModuleBase<SocketCommandContext>
{
    [Command("flip")]
    public async Task Flip(string input)
    {
        var flip = new FlipGamble(Context);
        await flip.Start(input);
    }

    [Command("bet")]
    public async Task Bet(string input)
    {
        var bet = new BetGamble(Context);
        await bet.Start(input);
    }

    [Command("risk")]
    public async Task Risk(string input, int modifier)
    {
        var risk = new RiskGamble(Context, modifier);
        await risk.Start(input);
    }

    [Command("slots")]
    public async Task Slots(string input = "")
    {
        var slot = new SlotsGamble(Context);

        if (input.ToLower() == "all")
        {
            await slot.Start(input);
        }
        else
        {
            var userPets = Data.Data.GetUserPets(Context.User.Id);

            int amount = 40;
            int petLvlSum = userPets.Sum(e => e.Value.Level);

            if (userPets.Any())
            {
                amount += petLvlSum + 5 * (petLvlSum / userPets.Count);
            }

            await slot.Start(amount.ToString());
        }
    }

    [Command("sim slots")]
    public async Task SlotsSim()
    {
        if (Context.User.Id != 192642414215692300)
        {
            return;
        }

        List<double> ratios = new();

        for (int i = 0; i < 25; i++)
        {
            ratios.Add(SimSlot());
        }
        Console.WriteLine($"Average ratio: {ratios.Average()}");

    }

    public double SimSlot()
    {
        int count = 1_000_000;
        int amount = 200;
        int bapsWon = 0;
        int bapsLost = 0;

        int wonSpins = 0;
        int lostSpins = 0;

        Random rnd = new();

        for (int i = 0; i < count; i++)
        {
            var slot = new SlotsGamble(Context);
            var result = slot.Calculate();
            if (result.IsWin)
            {
                bapsWon += result.Baps;
                wonSpins++;
            }
            else
            {
                bapsLost += result.Baps;
                lostSpins++;
            }
        }

        Console.WriteLine($"Slot cost: {amount}");
        Console.WriteLine($"spins: {count}");
        Console.WriteLine($"Won spins: {wonSpins}");
        Console.WriteLine($"Lost spins: {lostSpins}");
        Console.WriteLine($"Win chance: {(double)wonSpins / (double)lostSpins}");
        Console.WriteLine($"Total winnings: {bapsWon}");
        Console.WriteLine($"Total loss: {bapsLost}");
        Console.WriteLine($"Ratio: {(double)bapsWon / (double)bapsLost}");

        Console.WriteLine("-------");

        return (double)bapsWon / (double)bapsLost;
    }
}
