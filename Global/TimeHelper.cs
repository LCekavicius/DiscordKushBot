using System;

namespace KushBot.Global;

public static class TimeHelper
{
    public static DateTime Now => DateTime.Now;
    public static DateTime Tomorrow => Now.AddDays(1);
    public static DateTime LastMidnight => new DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0);
    public static DateTime NextMidnight => new DateTime(Tomorrow.Year, Tomorrow.Month, Tomorrow.Day, 0, 0, 0);
    public static TimeSpan MidnightIn => NextMidnight - Now;

    public static DateTime NextMondayMidnight => DateTime.Today.AddDays(((int)DayOfWeek.Monday - (int)DateTime.Today.DayOfWeek + 7) % 7 == 0 ? 7 : ((int)DayOfWeek.Monday - (int)DateTime.Today.DayOfWeek + 7) % 7).Date;
    public static TimeSpan MondayIn => NextMondayMidnight - Now;

}
