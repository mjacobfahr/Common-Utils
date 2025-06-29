using Discord;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommonUtils.Core;

/// <summary>
/// Recommended usage:
/// - Add the following line to the very top of your main plugin cs file:
/// global using Log = CommonUtils.Core.Utils.Logger;
/// - In your plugin's OnEnabled() method, call EnableDebug() if Debug is active.
/// Reasons to use:
/// - LabApi logger doesn't auto-check the plugin's config to determine whether to print debug logs to console.
/// - LabApi logger uses weird colors by default so this class tries to match the EXILED logger.
/// - This logger is more customizable with colors.
/// - This logger does all of this without depending on EXILED or LabApi so it can be used in any project.
/// </summary>
public static class Logger
{
    public static HashSet<Assembly> DebugEnabled { get; set; } = new();

    // TODO: Need to come up with a way to automatically infer debug when an assembly uses this class..
    public static void EnableDebug()
    {
        DebugEnabled.Add(Assembly.GetCallingAssembly());
    }

    // TODO: Make the color-scheme configurable via a simple enum with a few schemes to choose from
    public static ConsoleColor DebugColor { get; set; } = ConsoleColor.Gray;            // Exiled: Green, LabApi: Gray
    public static ConsoleColor InfoColor { get; set; } = ConsoleColor.Cyan;             // Exiled: Cyan, LabApi: White
    public static ConsoleColor WarnColor { get; set; } = ConsoleColor.Magenta;          // Exiled: Magenta, LabApi: Yellow
    public static ConsoleColor ErrorColor { get; set; } = ConsoleColor.DarkRed;         // Exiled: DarkRed, LabApi: Red

    public static void Debug(object message)
    {
        if (DebugEnabled.Contains(Assembly.GetCallingAssembly()))
        {
            Send(message, LogLevel.Debug, DebugColor, assembly: Assembly.GetCallingAssembly());
        }
    }

    // Alternative that so callers can still provide a boolean at call-time
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
        return level.ToString().ToUpper();//.PadRight(5);  // pad right so all levels have the same width - however it makes all of these logs stick out next to other logs so maybe not
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