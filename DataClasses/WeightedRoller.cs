using System;

public class WeightedRoller
{
    private Random rnd => Random.Shared;

    public int Roll(int amplifier)
    {
        // Validate amplifier
        if (!IsValidAmplifier(amplifier))
            throw new ArgumentException("Amplifier must be one of: 1, 11, 21, 31, 41");

        // Calculate total weight for normalization
        double totalWeight = 0;
        for (int i = 1; i <= 50; i++)
        {
            totalWeight += CalculateWeight(i, amplifier);
        }

        // Generate random value
        double randomValue = rnd.NextDouble() * totalWeight;

        // Find the corresponding number
        double accumulatedWeight = 0;
        for (int i = 1; i <= 50; i++)
        {
            accumulatedWeight += CalculateWeight(i, amplifier);
            if (randomValue <= accumulatedWeight)
                return i;
        }

        return 50; // Fallback (shouldn't normally reach here)
    }

    private double CalculateWeight(int number, int amplifier)
    {
        // Base weight calculation - linear decrease from 5x to 1x for amplifier 1
        double baseWeight = 5.0 - ((number - 1) * 4.0 / 49.0);

        // If number is less than amplifier, reduce weight
        if (number < amplifier)
        {
            baseWeight *= (number / (double)amplifier);
        }

        return Math.Max(baseWeight, 0.1); // Ensure minimum weight
    }

    private bool IsValidAmplifier(int amplifier)
    {
        return amplifier == 1 || amplifier == 11 || amplifier == 21 ||
               amplifier == 31 || amplifier == 41;
    }
}