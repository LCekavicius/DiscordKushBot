using System;
using System.Reflection;

namespace KushBot.DataClasses;

public class PlotFactory
{
    public Plot CreatePlot(Plot plot)
    {
        Plot instance = CreatePlotInstance(plot.Type, plot.AdditionalData);

        Type classType = plot.GetType();
        PropertyInfo[] properties = classType.GetProperties();

        foreach (PropertyInfo property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                object value = property.GetValue(plot);
                property.SetValue(instance, value);
            }
        }

        return instance;
    }

    private Plot CreatePlotInstance(PlotType type, string additionalData)
        => type switch
        {
            PlotType.Garden => new Garden(),
            PlotType.Quarry => new Quarry(),
            PlotType.Hatchery => new Hatchery(additionalData),
            _ => new Garden(),
        };
}
