namespace Common_Utilities;

using System.Collections.Generic;
using System.Linq;
using ConfigObjects;

public static class Utils
{
    public static int RollChance(IEnumerable<IChanceObject> scp914EffectChances)
    {
        double rolledChance = Plugin.Random.NextDouble();
        
        if (Plugin.Instance.Config.AdditiveProbabilities)
            rolledChance *= scp914EffectChances.Sum(x => x.Chance);
        else
            rolledChance *= 100;
        
        return (int)rolledChance;
    }
}