global using Log = CommonUtils.Core.Utils.Logger;

using System;
using Random = System.Random;

namespace CommonUtils.Core;

public static class Main
{
    public static string Author { get; } = "DeadServer Team";

    public static string Name { get; } = "CommonUtils.Core";

    public static string Prefix { get; } = "CommonUtils.Core";

    public static Version Version { get; } = new(1, 0, 0);

    public static Random Random { get; } = new();
}