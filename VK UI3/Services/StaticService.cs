using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace MusicX.Services;

public static class StaticService
{
    public static IServiceProvider Container { get; set; }
    public static string Version { get; } = typeof(StaticService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0";
    public static string BuildDate { get; } =
        typeof(StaticService).Assembly.GetCustomAttribute<BuildDateTimeAttribute>()?.Date.ToLocalTime()
            .ToString("d MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU")) ?? "23 сентября 2023";
    
    public static Version MinimumOsVersion { get; } = new(10, 0, 19041, 0);
    public static Version CurrentOsVersion { get; } = Environment.OSVersion.Version;

    public static DirectoryInfo UserDataFolder { get; } =
        new(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MusicM"));
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