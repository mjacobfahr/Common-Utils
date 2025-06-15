using PlayerRoles;
using System.Collections.Generic;
using System.ComponentModel;

namespace Common_Utilities.ConfigObjects;

public class ServerSettings
{
    [Description("Whether the server should auto-lock the lobby while waiting for players before a new round.")]
    public bool AutoLobbyLock { get; set; } = false;

    [Description("The text displayed at the timed interval specified below.")]
    public string TimedBroadcast { get; set; } = "<color=#bfff00>This server is running </color><color=red>EXILED Common-Utilities</color><color=#bfff00>, enjoy your stay!</color>";

    [Description("The time each timed broadcast will be displayed.")]
    public ushort TimedBroadcastDuration { get; set; } = 5;

    [Description("The delay between each timed broadcast. To disable timed broadcasts, set this to 0")]
    public float TimedBroadcastDelay { get; set; } = 0;

    [Description("The message displayed to the player when they first join the server. Setting this to empty will disable these broadcasts.")]
    public string JoinMessage { get; set; } = string.Empty;

    [Description("The amount of time (in seconds) the join message is displayed.")]
    public ushort JoinMessageDuration { get; set; } = 5;

    [Description("The maximum time, in seconds, that a player can be AFK before being kicked. Set to -1 to disable AFK system.")]
    public int AfkLimit { get; set; } = -1;

    [Description("The roles that are ignored by the AFK system.")]
    public List<RoleTypeId> AfkIgnoredRoles { get; set; } = new()
    {
        RoleTypeId.Scp079,
        RoleTypeId.Spectator,
        RoleTypeId.Tutorial,
        RoleTypeId.Filmmaker,
        RoleTypeId.Overwatch,
    };

    [Description("The frequency (in seconds) between ragdoll cleanups. Set to 0 to disable.")]
    public float CleanupRagdollDelay { get; set; } = 0.0f;

    [Description("If ragdoll cleanup should only happen in the Pocket Dimension or not.")]
    public bool CleanupRagdollOnlyPocket { get; set; } = false;

    [Description("The frequency (in seconds) between item cleanups. Set to 0 to disable.")]
    public float CleanupItemDelay { get; set; } = 0.0f;

    [Description("If item cleanup should only happen in the Pocket Dimension or not.")]
    public bool CleanupItemOnlyPocket { get; set; } = false;
}