using System;
using Random = System.Random;

namespace CommonUtils.Core;

// TODO: I don't think this project actually needs a Plugin file/class

public class MainPlugin
{
    public string Author { get; } = "DeadServer Team";

    public string Name { get; } = "CommonUtils.Core";

    public string Prefix { get; } = "CommonUtils.Core";

    public Version Version { get; } = new(1, 0, 0);

    public static Random Random { get; } = new();
}