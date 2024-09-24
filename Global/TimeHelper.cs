using System;

namespace KushBot.Global;

public static class TimeHelper
{
    public static DateTime Now => DateTime.Now;
    public static DateTime Tomorrow => Now.AddDays(1);
    public static DateTime LastMidnight => new DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0);
    public static DateTime NextMidnight => new DateTime(Tomorrow.Year, Tomorrow.Month, Tomorrow.Day, 0, 0, 0);
    public static TimeSpan MidnightIn => NextMidnight - Now;
}
