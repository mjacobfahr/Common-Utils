using CommonUtils.Core.Interfaces;

namespace CommonUtils.Config.ConfigObjects;

public class StartingItem : IChanceObjectD
{
    public string ItemName { get; set; } = ItemType.None.ToString();

    public double Chance { get; set; }

    public string Group { get; set; } = "none";

    public void Deconstruct(out string name, out double i)
    {
        name = ItemName;
        i = Chance;
    }
}