using System;
using System.Globalization;
using System.Reflection;

namespace VK_UI3.Services;

public static class StaticService
{
    public static IServiceProvider Container { get; set; }
    public static string Version { get; } = typeof(StaticService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0";
    public static string BuildDate { get; } =
        typeof(StaticService).Assembly.GetCustomAttribute<BuildDateTimeAttribute>()?.Date.ToLocalTime()
            .ToString("d MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU")) ?? "23 СЃРµРЅС‚СЏР±СЂСЏ 2023";
}

[AttributeUsage(AttributeTargets.Assembly)]
public class BuildDateTimeAttribute : Attribute
{
    public DateTime Date { get; set; }
    public BuildDateTimeAttribute(string date)
    {
        Date = DateTime.Parse(date, CultureInfo.InvariantCulture);
    }
}

