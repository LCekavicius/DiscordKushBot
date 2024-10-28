using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.DataClasses;

public class PlotsManager(KushBotUser user)
{
    public List<Plot> Plots { get => user.UserPlots; set => user.UserPlots = value; }

    public void UpdateState(List<Plot> plots = null)
    {
        plots ??= Plots.Where(e => e is Hatchery).ToList();
        plots.ForEach(e => ((Hatchery)e).UpdateState());
    }

    public int FillHatcheries(int maxFill, int? index = null)
    {
        List<Plot> plots = Plots
            .Where(e => e is Hatchery
               && (!index.HasValue || e.Id == Plots[index.Value].Id))
            .ToList();

        if (!plots.Any())
            return 0;

        int totalEggsFilled = 0;
        plots.ForEach(e => totalEggsFilled += maxFill > 0 ? ((Hatchery)e).Fill(ref maxFill) : 0);

        return totalEggsFilled;
    }

    public int NextPlotPrice()
    {
        if (Plots.Count == 0)
            return 1000;

        return 1000 + 500 * (int)Math.Pow(2, Plots.Count);
    }

    public string Collect(int plotIndex, UserPets userPets = null)
    {
        Plot plot = Plots[plotIndex];
        userPets ??= user.Pets;

        if (plot.IsReadyForCollecting())
        {
            return Plots[plotIndex].Collect(plotIndex, userPets);
        }

        return "That plot isn't ready to be collected, retard nigger";
    }

    public void ShiftTime(int minutes, int? plotIndex = null)
    {
        if (plotIndex != null)
            throw new NotImplementedException();

        foreach (var plot in Plots)
        {
            plot.ShiftTime(minutes);
        }
    }

    public string Collect(string input)
    {
        string amalgamation = "";
        int index = 0;
        var userPets = user.Pets; 

        foreach (var plot in Plots)
        {
            if ((input.ToLower() == "all"
                || (input.Length == 1 && plot.Type.ToString().StartsWith(char.ToUpper(input[0])))
                || plot.Type == EnumHelperV2Singleton.Instance.Helper.GetEnumByDescriptedValue<PlotType>(input, true))
                && plot.IsReadyForCollecting())
            {
                amalgamation += Collect(index, userPets) + (index + 1 == Plots.Count ? "" : "\n");
            }
            index++;
        }

        return amalgamation;
    }
}
