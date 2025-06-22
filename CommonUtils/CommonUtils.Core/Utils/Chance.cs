using CommonUtils.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonUtils.Core.Utils;

public static class Chance
{
    public static int RollChanceInt(IEnumerable<IChanceObject> chanceObjects, bool additiveProbabilities = true)
    {
        return (int)Math.Round(InternalRollChance(chanceObjects, additiveProbabilities));
    }

    public static double RollChance(IEnumerable<IChanceObject> chanceObjects, bool additiveProbabilities = true)
    {
        return InternalRollChance(chanceObjects, additiveProbabilities);
    }

    internal static double InternalRollChance(IEnumerable<IChanceObject> chanceObjects, bool additiveProbabilities = true)
    {
        double rolledChance = Main.Random.NextDouble();
        if (additiveProbabilities)
        {
            rolledChance *= chanceObjects.Sum(x => x.Chance);
        }
        else
        {
            rolledChance *= 100.0;
        }
        return rolledChance;
    }
}