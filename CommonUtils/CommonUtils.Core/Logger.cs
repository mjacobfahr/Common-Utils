using Discord;
using System;
using System.Reflection;

namespace CommonUtils.Core;

/// <summary>
/// Recommended usage:
/// - Add the following line to the very top of your main plugin cs file:
/// global using Log = CommonUtils.Core.Utils.Logger;
/// - In your plugin's OnEnabled() method, set the PrintDebug property based on your plugin's config.
/// Reasons to use:
/// - LabApi logger doesn't auto-check the plugin's config to determine whether to print debug logs to console.
/// - LabApi logger uses weird colors by default so this class tries to match the EXILED logger.
/// - This logger is more customizable with colors.
/// - This logger does all of this without depending on EXILED or LabApi so it can be used in any project.
/// </summary>
public static class Logger
{
    // TODO: Might need to make this a HashSet<Assembly> or something like Exiled
    public static bool PrintDebug { get; set; } = false;

    // TODO: Make the color-scheme configurable via a simple enum with a few schemes to choose from
    //       These settable properties will work for now
    public static ConsoleColor DebugColor { get; set; } = ConsoleColor.Gray;        // Exiled: Green, LabApi: Gray
    public static ConsoleColor InfoColor { get; set; } = ConsoleColor.Cyan;         // Exiled: Cyan, LabApi: White
    public static ConsoleColor WarnColor { get; set; } = ConsoleColor.Magenta;      // Exiled: Magenta, LabApi: Yellow
    public static ConsoleColor ErrorColor { get; set; } = ConsoleColor.DarkRed;     // Exiled: DarkRed, LabApi: Red

    // ----- Log methods -----

    // Automatically uses the PrintDebug flag
    public static void Debug(object message)
    {
        if (PrintDebug)
        {
            Send(message, LogLevel.Debug, DebugColor, assembly: Assembly.GetCallingAssembly());
        }
    }

    // Alternatively can still provide a boolean at call-time
    public static void Debug(object message, bool print)
    {
        if (print)
        {
            Send(message, LogLevel.Debug, DebugColor, assembly: Assembly.GetCallingAssembly());
        }
    }

    public static void Info(object message)
    {
        Send(message, LogLevel.Info, InfoColor, assembly: Assembly.GetCallingAssembly());
    }

    public static void Warn(object message)
    {
        Send(message, LogLevel.Warn, WarnColor, assembly: Assembly.GetCallingAssembly());
    }

    public static void Error(object message)
    {
        Send(message, LogLevel.Error, ErrorColor, assembly: Assembly.GetCallingAssembly());
    }

    // ----- Utility methods -----

    private static string FormatAssembly(Assembly assembly = null)
    {
        if (assembly is not null)
        {
            return assembly.GetName().Name;
        }
        else
        {
            return Assembly.GetCallingAssembly().GetName().Name;    // if assembly is not provided then this will always show "CommonUtils.Core"
        }
    }

    private static string FormatLevel(LogLevel level)
    {
        return level.ToString().ToUpper().PadRight(5);  // pad right so all levels have the same width
    }

    private static string FormatLog(object message, LogLevel level, Assembly assembly = null)
    {
        return $"[{FormatLevel(level)}] [{FormatAssembly(assembly)}] {message}";
    }

    public static void Send(object message, LogLevel level, ConsoleColor color = ConsoleColor.Gray, Assembly assembly = null)
    {
        SendRaw(FormatLog(message, level, assembly), color);
    }

    public static void SendRaw(string message, ConsoleColor color)
    {
        ServerConsole.AddLog(message, color);
    }
}