using Common_Utilities.ConfigObjects;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Warhead;
using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common_Utilities.Events;

public class ServerHandlers
{
    private ServerSettings ServerSettings => MainPlugin.Configs.ServerSettings;
    private GameplaySettings GameplaySettings => MainPlugin.Configs.GameplaySettings;

    private bool friendlyFireDisable;

    public void OnWaitingForPlayers()
    {
        if (ServerSettings.AfkLimit > 0)
        {
            MainPlugin.AfkDict.Clear();
            MainPlugin.Coroutines.Add(Timing.RunCoroutine(AfkCheck()));
        }
        if (friendlyFireDisable)
        {
            Log.Debug($"{nameof(OnWaitingForPlayers)}: Disabling friendly fire.");
            Server.FriendlyFire = false;
            friendlyFireDisable = false;
        }
        if (ServerSettings.TimedBroadcastDelay > 0)
        {
            MainPlugin.Coroutines.Add(Timing.RunCoroutine(ServerBroadcast()));
        }

        Warhead.IsLocked = false;
    }

    public void OnRoundStarted()
    {
        if (GameplaySettings.AutonukeTime > -1)
        {
            MainPlugin.Coroutines.Add(Timing.CallDelayed(GameplaySettings.AutonukeTime, AutoNuke));
        }
        if (ServerSettings.CleanupRagdollDelay > 0)
        {
            MainPlugin.Coroutines.Add(Timing.RunCoroutine(RagdollCleanup()));
        }
        if (ServerSettings.CleanupItemDelay > 0)
        {
            MainPlugin.Coroutines.Add(Timing.RunCoroutine(ItemCleanup()));
        }
    }

    public void OnRoundEnded(RoundEndedEventArgs ev)
    {
        if (GameplaySettings.FriendlyFireOnRoundEnd && !Server.FriendlyFire)
        {
            Log.Debug($"{nameof(OnRoundEnded)}: Enabling friendly fire.");
            Server.FriendlyFire = true;
            friendlyFireDisable = true;
        }

        Timing.KillCoroutines(MainPlugin.Coroutines.ToArray());

        MainPlugin.Coroutines.Clear();
    }

    public void OnRestartingRound()
    {
        foreach (CoroutineHandle coroutine in MainPlugin.Coroutines)
        {
            Timing.KillCoroutines(coroutine);
        }

        MainPlugin.Coroutines.Clear();
    }

    public void OnWarheadStarting(StartingEventArgs _)
    {
        if (!GameplaySettings.WarheadChangeColor)
        {
            return;
        }

        foreach (Room room in Room.List)
        {
            room.Color = GameplaySettings.WarheadColor;
        }
    }

    public void OnWarheadStopping(StoppingEventArgs _)
    {
        if (!GameplaySettings.WarheadChangeColor || Warhead.IsLocked)
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
            yield return Timing.WaitForSeconds(ServerSettings.TimedBroadcastDelay);

            Map.Broadcast(ServerSettings.TimedBroadcastDuration, ServerSettings.TimedBroadcast);
        }
    }

    private IEnumerator<float> ItemCleanup()
    {
        while(true)
        {
            yield return Timing.WaitForSeconds(ServerSettings.CleanupItemDelay);

            foreach (Pickup pickup in Pickup.List)
            {
                if (!ServerSettings.CleanupItemOnlyPocket || pickup.Position.y < -1500f)
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
            yield return Timing.WaitForSeconds(ServerSettings.CleanupRagdollDelay);

            foreach (Ragdoll ragdoll in Ragdoll.List)
            {
                if (!ServerSettings.CleanupRagdollOnlyPocket || ragdoll.Position.y < -1500f)
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
            switch (GameplaySettings.AutonukeBroadcast.Duration)
            {
                case 0:
                    break;
                case 1:
                    Cassie.Message(GameplaySettings.AutonukeBroadcast.Content);
                    break;
                default:
                    Map.Broadcast(GameplaySettings.AutonukeBroadcast);
                    break;
            }

            Warhead.Start();
        }

        if (GameplaySettings.AutonukeLock)
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
                if (!MainPlugin.AfkDict.ContainsKey(player))
                {
                    MainPlugin.AfkDict.Add(player, new Tuple<int, Vector3>(0, player.Position));
                }

                if (player.Role.IsDead
                    || player.IsGodModeEnabled
                    || player.IsNoclipPermitted
                    || player.Role is FpcRole { IsGrounded: false }
                    || player.RemoteAdminPermissions.HasFlag(PlayerPermissions.AFKImmunity)
                    || ServerSettings.AfkIgnoredRoles.Contains(player.Role.Type))
                {
                    Log.Debug($"Player {player.Nickname} ({player.Role.Type}) is not a checkable player. NoClip: {player.IsNoclipPermitted} GodMode: {player.IsGodModeEnabled} " +
                        $"IsNotGrounded: {player.Role is FpcRole { IsGrounded: false }} AFKImunity: {player.RemoteAdminPermissions.HasFlag(PlayerPermissions.AFKImmunity)}");
                    continue;
                }

                if ((MainPlugin.AfkDict[player].Item2 - player.Position).sqrMagnitude > 2)
                {
                    Log.Debug($"Player {player.Nickname} has moved, resetting AFK timer.");
                    MainPlugin.AfkDict[player] = new Tuple<int, Vector3>(0, player.Position);
                }

                if (MainPlugin.AfkDict[player].Item1 >= ServerSettings.AfkLimit)
                {
                    MainPlugin.AfkDict.Remove(player);
                    Log.Debug($"Kicking {player.Nickname} for being AFK.");
                    player.Kick("You were kicked by CommonUtilities for being AFK.");
                }
                else if (MainPlugin.AfkDict[player].Item1 >= (ServerSettings.AfkLimit / 2))
                {
                    player.Broadcast(2, $"You have been AFK for {MainPlugin.AfkDict[player].Item1} seconds. You will be automatically kicked if you remain AFK for a total of {ServerSettings.AfkLimit} seconds.", shouldClearPrevious: true);
                }

                MainPlugin.AfkDict[player] = new Tuple<int, Vector3>(MainPlugin.AfkDict[player].Item1 + 1, MainPlugin.AfkDict[player].Item2);
            }
        }
    }
}