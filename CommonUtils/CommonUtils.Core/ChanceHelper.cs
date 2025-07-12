using CommonUtils.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace CommonUtils.Core;

public static class ChanceHelper
{
    // TODO: Value type should be any numeric type
    public static int RollChanceFromChances(IEnumerable<int> chanceValues, bool additiveProbabilities = true)
    {
        int total = 100;
        if (additiveProbabilities)
        {
            total = chanceValues.Sum(x => x);
        }
        return Main.Random.Next(0, total);
    }

    public static int RollChance(IEnumerable<IChanceObject> chanceObjects, bool additiveProbabilities = true)
    {
        return InternalRollChance(chanceObjects, additiveProbabilities);
    }

    public static double RollChance(IEnumerable<IChanceObjectD> chanceObjects, bool additiveProbabilities = true)
    {
        return InternalRollChance(chanceObjects, additiveProbabilities);
    }

    // TODO: Figure out some fanciness to make this one method
    internal static int InternalRollChance(IEnumerable<IChanceObject> chanceObjects, bool additiveProbabilities)
    {
        int total = 100;
        if (additiveProbabilities)
        {
            total = chanceObjects.Sum(x => x.Chance);
        }
        return Main.Random.Next(0, total);
    }

    internal static double InternalRollChance(IEnumerable<IChanceObjectD> chanceObjects, bool additiveProbabilities)
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