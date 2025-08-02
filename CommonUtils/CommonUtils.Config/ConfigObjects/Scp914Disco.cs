using System.ComponentModel;

namespace CommonUtils.Config.ConfigObjects;

public class Scp914Disco
{
    public bool Enabled { get; set; } = false;

    [Description("The initial time in seconds between disco lights changing. For reference, the total upgrade-process time is around 16 seconds.")]
    public float DelayInitial { get; set; } = 1.5f;

    [Description("The lowest value that the delay can be.")]
    public float DelayMinimum { get; set; } = 0.5f;

    [Description("The highest value that the delay can be.")]
    public float DelayMaximum { get; set; } = 3.0f;

    [Description("How much to modify the delay time after each disco lights change.")]
    public float DelayChange { get; set; } = -0.05f;
}
