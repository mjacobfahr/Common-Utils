using CommonUtils.Core.Interfaces;

namespace CommonUtils.Config.ConfigObjects;

public class Scp914ItemChance : IChanceObjectD
{
    public string Original { get; set; }

    public string New { get; set; }

    public double Chance { get; set; }

    public int Count { get; set; } = 1;

    public void Deconstruct(out string originalItem, out string destinationItem, out double chance, out int count)
    {
        originalItem = Original;
        destinationItem = New;
        chance = Chance;
        count = Count;
    }
}