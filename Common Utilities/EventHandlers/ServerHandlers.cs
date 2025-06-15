using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Warhead;
using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common_Utilities.EventHandlers;

public class ServerHandlers
{
    private Config Configs => Plugin.Singleton.Config;

    private bool friendlyFireDisable;

    public void OnRoundStarted()
    {
        if (Configs.AutonukeTime > -1)
        {
            Plugin.Coroutines.Add(Timing.CallDelayed(Configs.AutonukeTime, AutoNuke));
        }
        if (Configs.RagdollCleanupDelay > 0)
        {
            Plugin.Coroutines.Add(Timing.RunCoroutine(RagdollCleanup()));
        }
        if (Configs.ItemCleanupDelay > 0)
        {
            Plugin.Coroutines.Add(Timing.RunCoroutine(ItemCleanup()));
        }
    }

    public void OnWaitingForPlayers()
    {
        if (Configs.AfkLimit > 0)
        {
            Plugin.AfkDict.Clear();
            Plugin.Coroutines.Add(Timing.RunCoroutine(AfkCheck()));
        }
        if (friendlyFireDisable)
        {
            Log.Debug($"{nameof(OnWaitingForPlayers)}: Disabling friendly fire.");
            Server.FriendlyFire = false;
            friendlyFireDisable = false;
        }
        if (Configs.TimedBroadcastDelay > 0)
        {
            Plugin.Coroutines.Add(Timing.RunCoroutine(ServerBroadcast()));
        }

        Warhead.IsLocked = false;
    }

    public void OnRoundEnded(RoundEndedEventArgs ev)
    {
        if (Configs.FriendlyFireOnRoundEnd && !Server.FriendlyFire)
        {
            Log.Debug($"{nameof(OnRoundEnded)}: Enabling friendly fire.");
            Server.FriendlyFire = true;
            friendlyFireDisable = true;
        }

        Timing.KillCoroutines(Plugin.Coroutines.ToArray());

        Plugin.Coroutines.Clear();
    }

    public void OnRestartingRound()
    {
        foreach (CoroutineHandle coroutine in Plugin.Coroutines)
        {
            Timing.KillCoroutines(coroutine);
        }

        Plugin.Coroutines.Clear();
    }

    public void OnWarheadStarting(StartingEventArgs _)
    {
        if (!Configs.ChangeWarheadColor)
        {
            return;
        }

        foreach (Room room in Room.List)
        {
            room.Color = Configs.WarheadColor;
        }
    }

    public void OnWarheadStopping(StoppingEventArgs _)
    {
        if (!Configs.ChangeWarheadColor || Warhead.IsLocked)
        {
            return;
        }

        foreach (Room room in Room.List)
        {
            room.ResetColor();
        }
    }

    private IEnumerator<float> ServerBroadcast()
    {
        while(true)
        {
            yield return Timing.WaitForSeconds(Configs.TimedBroadcastDelay);

            Map.Broadcast(Configs.TimedBroadcastDuration, Configs.TimedBroadcast);
        }
    }

    private IEnumerator<float> ItemCleanup()
    {
        while(true)
        {
            yield return Timing.WaitForSeconds(Configs.ItemCleanupDelay);

            foreach (Pickup pickup in Pickup.List)
            {
                if (!Configs.ItemCleanupOnlyPocket || pickup.Position.y < -1500f)
                {
                    pickup.Destroy();
                }
            }
        }
    }

    private IEnumerator<float> RagdollCleanup()
    {
        while(true)
        {
            yield return Timing.WaitForSeconds(Configs.RagdollCleanupDelay);

            foreach (Ragdoll ragdoll in Ragdoll.List)
            {
                if (!Configs.RagdollCleanupOnlyPocket || ragdoll.Position.y < -1500f)
                {
                    ragdoll.Destroy();
                }
            }
        }
    }

    private void AutoNuke()
    {
        if (!Warhead.IsInProgress)
        {
            switch (Configs.AutonukeBroadcast.Duration)
            {
                case 0:
                    break;
                case 1:
                    Cassie.Message(Configs.AutonukeBroadcast.Content);
                    break;
                default:
                    Map.Broadcast(Configs.AutonukeBroadcast);
                    break;
            }

            Warhead.Start();
        }

        if (Configs.AutonukeLock)
        {
            Warhead.IsLocked = true;
        }
    }

    private IEnumerator<float> AfkCheck()
    {
        while(true)
        {
            yield return Timing.WaitForSeconds(1f);

            foreach (Player player in Player.List)
            {
                if (!Plugin.AfkDict.ContainsKey(player))
                {
                    Plugin.AfkDict.Add(player, new Tuple<int, Vector3>(0, player.Position));
                }

                if (player.Role.IsDead
                    || player.IsGodModeEnabled
                    || player.IsNoclipPermitted
                    || player.Role is FpcRole { IsGrounded: false }
                    || player.RemoteAdminPermissions.HasFlag(PlayerPermissions.AFKImmunity)
                    || Configs.AfkIgnoredRoles.Contains(player.Role.Type))
                {
                    Log.Debug($"Player {player.Nickname} ({player.Role.Type}) is not a checkable player. NoClip: {player.IsNoclipPermitted} GodMode: {player.IsGodModeEnabled} IsNotGrounded: {player.Role is FpcRole { IsGrounded: false }} AFKImunity: {player.RemoteAdminPermissions.HasFlag(PlayerPermissions.AFKImmunity)}");
                    continue;
                }

                if ((Plugin.AfkDict[player].Item2 - player.Position).sqrMagnitude > 2)
                {
                    Log.Debug($"Player {player.Nickname} has moved, resetting AFK timer.");
                    Plugin.AfkDict[player] = new Tuple<int, Vector3>(0, player.Position);
                }

                if (Plugin.AfkDict[player].Item1 >= Configs.AfkLimit)
                {
                    Plugin.AfkDict.Remove(player);
                    Log.Debug($"Kicking {player.Nickname} for being AFK.");
                    player.Kick("You were kicked by CommonUtilities for being AFK.");
                }
                else if (Plugin.AfkDict[player].Item1 >= (Configs.AfkLimit / 2))
                {
                    player.Broadcast(2, $"You have been AFK for {Plugin.AfkDict[player].Item1} seconds. You will be automatically kicked if you remain AFK for a total of {Configs.AfkLimit} seconds.", shouldClearPrevious: true);
                }

                Plugin.AfkDict[player] = new Tuple<int, Vector3>(Plugin.AfkDict[player].Item1 + 1, Plugin.AfkDict[player].Item2);
            }
        }
    }
}