using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonUtils.Core;

public static class Extensions
{
    /////////////////////////////////////////////////////////
    // Enum Extensions
    /////////////////////////////////////////////////////////

    // Not an extension but...
    public static List<Enum> GetAllValues(Type type)
    {
        return Enum.GetValues(type).Cast<Enum>().ToList();
    }

    public static List<Enum> GetAllValues(this Enum value)
    {
        return Enum.GetValues(value.GetType()).Cast<Enum>().ToList();
    }

    public static IEnumerable<Enum> GetFlags(this Enum value)
    {
        return Enum.GetValues(value.GetType()).Cast<Enum>().ToArray();
    }

    public static IEnumerable<Enum> GetActiveFlags(this Enum value)
    {
        Enum[] flags = new Enum[0];
        foreach (var flag in value.GetFlags())
        {
            if (value.HasFlag(flag))
            {
                flags = flags.Append(flag).ToArray();
            }
        }
        return flags;
    }

    public static string GetActiveFlagNames(this Enum value)
    {
        return string.Join(", ", value.GetActiveFlags());
    }
}
