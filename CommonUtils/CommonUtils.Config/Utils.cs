using CommonUtils.Config.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace CommonUtils.Config;

public static class Utils
{
    public static int RollChance(IEnumerable<IChanceObject> chanceObjects)
    {
        double rolledChance = MainPlugin.Random.NextDouble();
        if (MainPlugin.Configs.AdditiveProbabilities)
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