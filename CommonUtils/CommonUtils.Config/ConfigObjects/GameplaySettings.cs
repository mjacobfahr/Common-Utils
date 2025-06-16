using PlayerRoles;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using ExiledBroadcast = Exiled.API.Features.Broadcast;  // conflicts with Broadcast from Assembly-CSharp

namespace CommonUtils.Config.ConfigObjects;

public class GameplaySettings
{
    [Description("The amount of time (in seconds) after the round starts, before the facilities auto-nuke will start. Set to -1 to disable.")]
    public float AutonukeTime { get; set; } = -1;

    [Description("Wether or not the nuke should be unable to be disabled during the auto-nuke countdown.")]
    public bool AutonukeLock { get; set; } = true;

    [Description("The message given to all players when the auto-nuke is triggered. A duration of 2 or more will be a text message on-screen. " +
        "A duration of 1 makes it a cassie announcement. A duration of 0 disables it.")]
    public ExiledBroadcast AutonukeBroadcast { get; set; } = new()
    {
        Content = "The auto nuke has been activated.",
        Duration = 10,
        Show = true,
        Type = Broadcast.BroadcastFlags.Normal,
    };

    [Description("Roles that when cuffed in the escape area will change into the target one.")]
    public Dictionary<RoleTypeId, RoleTypeId> DisarmedEscapeSwitchRole { get; set; } = new()
    {
        {
            RoleTypeId.NtfCaptain, RoleTypeId.ChaosMarauder
        },
        {
            RoleTypeId.ChaosMarauder, RoleTypeId.NtfCaptain
        },
    };

    [Description("Whether or not friendly fire should be enabled during rounds.")]
    public bool FriendlyFireEnabled { get; set; } = false;

    [Description("Whether or not friendly fire should automatically turn on when a round ends. " +
        "It will turn itself back off before the next round starts. Does not apply if FriendlyFireEnabled is on.")]
    public bool FriendlyFireOnRoundEnd { get; set; } = false;

    [Description("Whether or not to show player's health under their name when you look at them.")]
    public bool PlayerHealthInfo { get; set; } = false;

    [Description("The multiplier applied to radio battery usage. Set to 0 to disable radio battery drain.")]
    public float RadioBatteryDrainMultiplier { get; set; } = 1.0f;

    [Description("Whether to change the color of lights while warhead is active.")]
    public bool WarheadChangeColor { get; set; } = false;

    [Description("The color to use for lights while the warhead is active. In the RGBA format using values between 0 and 1. Ignored if ChangeWarheadColor is set to false.")]
    public Color WarheadColor { get; set; } = new(1.0f, 0.2f, 0.2f, 1);
}