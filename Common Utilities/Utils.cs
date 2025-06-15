using Common_Utilities.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Common_Utilities;

public static class Utils
{
    public static int RollChance(IEnumerable<IChanceObject> chanceObjects)
    {
        double rolledChance = Plugin.Random.NextDouble();
        if (Plugin.Singleton.Config.AdditiveProbabilities)
        {
            rolledChance *= chanceObjects.Sum(x => x.Chance);
        }
        else
        {
            rolledChance *= 100;
        }
        return (int)rolledChance;
    }
}