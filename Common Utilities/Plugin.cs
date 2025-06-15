global using Scp914KnobSetting = Scp914.Scp914KnobSetting;
using Common_Utilities.ConfigObjects;
using Common_Utilities.EventHandlers;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using UnityEngine;
using PlayerEvents = Exiled.Events.Handlers.Player;
using Scp914Events = Exiled.Events.Handlers.Scp914;
using ServerEvents = Exiled.Events.Handlers.Server;
using WarheadEvents = Exiled.Events.Handlers.Warhead;
using Random = System.Random;

namespace Common_Utilities;

public class Plugin : Plugin<Config>
{
    public override string Author { get; } = "DeadServer Team";

    public override string Name { get; } = "Common Utils";

    public override string Prefix { get; } = "CommonUtils";

    public override Version Version { get; } = new(8, 0, 2);

    public override Version RequiredExiledVersion { get; } = new(9, 6, 1);

    public override PluginPriority Priority { get; } = PluginPriority.Higher;

    public static Plugin Singleton { get; private set; }

    public static Config Configs => Singleton.Config;

    public static Random Random { get; } = new();

    public PlayerHandlers PlayerHandlers { get; private set; } = new();

    public ServerHandlers ServerHandlers { get; private set; } = new();

    public MapHandlers MapHandlers { get; private set; } = new();

    public static List<CoroutineHandle> Coroutines { get; } = new();

    public static Dictionary<Exiled.API.Features.Player, Tuple<int, Vector3>> AfkDict { get; } = new();

    public override void OnEnabled()
    {
        if (Config.Debug)
        {
            DebugConfig();
        }

        Singleton = this;

        Log.Debug("Registering EventHandlers..");
        PlayerEvents.Hurting += PlayerHandlers.OnPlayerHurting;
        PlayerEvents.Verified += PlayerHandlers.OnPlayerVerified;
        PlayerEvents.Spawned += PlayerHandlers.OnSpawned;
        PlayerEvents.Escaping += PlayerHandlers.OnEscaping;
        if (Config.HealthOnKill != null)
        {
            PlayerEvents.Died += PlayerHandlers.OnPlayerDied;
        }
        if (Config.StartingInventories != null)
        {
            PlayerEvents.ChangingRole += PlayerHandlers.OnChangingRole;
        }
        if (Config.RadioBatteryDrainMultiplier is not 1)
        {
            PlayerEvents.UsingRadioBattery += PlayerHandlers.OnUsingRadioBattery;
        }
        if (Config.AfkLimit > 0)
        {
            PlayerEvents.Jumping += PlayerHandlers.AntiAfkEventHandler;
            PlayerEvents.Shooting += PlayerHandlers.AntiAfkEventHandler;
            PlayerEvents.UsingItem += PlayerHandlers.AntiAfkEventHandler;
            PlayerEvents.MakingNoise += PlayerHandlers.AntiAfkEventHandler;
            PlayerEvents.ReloadingWeapon += PlayerHandlers.AntiAfkEventHandler;
            PlayerEvents.ThrownProjectile += PlayerHandlers.AntiAfkEventHandler;
            PlayerEvents.ChangingMoveState += PlayerHandlers.AntiAfkEventHandler;
            PlayerEvents.InteractingDoor += PlayerHandlers.AntiAfkEventHandler;
            PlayerEvents.InteractingElevator += PlayerHandlers.AntiAfkEventHandler;
        }

        ServerEvents.RoundEnded += ServerHandlers.OnRoundEnded;
        ServerEvents.RoundStarted += ServerHandlers.OnRoundStarted;
        ServerEvents.RestartingRound += ServerHandlers.OnRestartingRound;
        ServerEvents.WaitingForPlayers += ServerHandlers.OnWaitingForPlayers;

        Scp914Events.UpgradingPlayer += MapHandlers.OnUpgradingPlayer;
        if (Config.Scp914ItemChanges != null)
        {
            Scp914Events.UpgradingPickup += MapHandlers.OnUpgradingPickup;
        }
        if (Config.Scp914ItemChanges != null)
        {
            Scp914Events.UpgradingInventoryItem += MapHandlers.OnUpgradingInventoryItem;
        }

        WarheadEvents.Starting += ServerHandlers.OnWarheadStarting;
        WarheadEvents.Stopping += ServerHandlers.OnWarheadStopping;

        Log.Debug("Registered EventHandlers");

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        PlayerEvents.Hurting -= PlayerHandlers.OnPlayerHurting;
        PlayerEvents.Verified -= PlayerHandlers.OnPlayerVerified;
        PlayerEvents.Spawned -= PlayerHandlers.OnSpawned;
        PlayerEvents.Escaping -= PlayerHandlers.OnEscaping;
        PlayerEvents.Died -= PlayerHandlers.OnPlayerDied;
        PlayerEvents.ChangingRole -= PlayerHandlers.OnChangingRole;
        PlayerEvents.UsingRadioBattery -= PlayerHandlers.OnUsingRadioBattery;
        PlayerEvents.Jumping -= PlayerHandlers.AntiAfkEventHandler;
        PlayerEvents.Shooting -= PlayerHandlers.AntiAfkEventHandler;
        PlayerEvents.UsingItem -= PlayerHandlers.AntiAfkEventHandler;
        PlayerEvents.MakingNoise -= PlayerHandlers.AntiAfkEventHandler;
        PlayerEvents.ReloadingWeapon -= PlayerHandlers.AntiAfkEventHandler;
        PlayerEvents.ThrownProjectile -= PlayerHandlers.AntiAfkEventHandler;
        PlayerEvents.ChangingMoveState -= PlayerHandlers.AntiAfkEventHandler;
        PlayerEvents.InteractingDoor -= PlayerHandlers.AntiAfkEventHandler;
        PlayerEvents.InteractingElevator -= PlayerHandlers.AntiAfkEventHandler;

        ServerEvents.RoundEnded -= ServerHandlers.OnRoundEnded;
        ServerEvents.RoundStarted -= ServerHandlers.OnRoundStarted;
        ServerEvents.RestartingRound -= ServerHandlers.OnRestartingRound;
        ServerEvents.WaitingForPlayers -= ServerHandlers.OnWaitingForPlayers;

        Scp914Events.UpgradingPlayer -= MapHandlers.OnUpgradingPlayer;
        Scp914Events.UpgradingPickup -= MapHandlers.OnUpgradingPickup;
        Scp914Events.UpgradingInventoryItem -= MapHandlers.OnUpgradingInventoryItem;

        WarheadEvents.Starting -= ServerHandlers.OnWarheadStarting;
        WarheadEvents.Stopping -= ServerHandlers.OnWarheadStopping;

        ServerHandlers = null;
        PlayerHandlers = null;
        MapHandlers = null;
        Singleton = null;
        base.OnDisabled();
    }

    private void DebugConfig()
    {
        if (Config.StartingInventories != null)
        {
            Log.Debug($"Starting Inventories: {Config.StartingInventories.Count}");
            foreach (KeyValuePair<RoleTypeId, RoleInventory> kvp in Config.StartingInventories)
            {
                for (int i = 0; i < kvp.Value.UsedSlots; i++)
                {
                    foreach (ItemChance chance in kvp.Value[i])
                    {
                        Log.Debug($"Inventory Config: {kvp.Key} - Slot{i + 1}: {chance.ItemName} ({chance.Chance})");
                    }
                }

                foreach ((ItemType type, ushort amount, string group) in kvp.Value.Ammo)
                {
                    Log.Debug($"Ammo Config: {kvp.Key} - {type} {amount} ({group})");
                }
            }
        }

        if (Config.Scp914ItemChanges != null)
        {
            Log.Debug($"{Config.Scp914ItemChanges.Count}");
            foreach (KeyValuePair<Scp914KnobSetting, List<ItemUpgradeChance>> upgrade in Config.Scp914ItemChanges)
            {
                foreach ((string oldItem, string newItem, double chance, int count) in upgrade.Value)
                    Log.Debug($"914 Item Config: {upgrade.Key}: {oldItem} -> {newItem}x({count}) - {chance}");
            }
        }

        if (Config.Scp914ClassChanges != null)
        {
            Log.Debug($"{Config.Scp914ClassChanges.Count}");
            foreach (KeyValuePair<Scp914KnobSetting, List<PlayerUpgradeChance>> upgrade in Config.Scp914ClassChanges)
            {
                foreach ((string oldRole, string newRole, double chance, bool keepInventory, bool keepHealth) in upgrade.Value)
                    Log.Debug($"914 Role Config: {upgrade.Key}: {oldRole} -> {newRole} - {chance} keepInventory: {keepInventory} keepHealth: {keepHealth}");
            }
        }

        if (Config.Scp914EffectChances != null)
        {
            Log.Debug($"{Config.Scp914EffectChances.Count}");
            foreach (KeyValuePair<Scp914KnobSetting, List<Scp914EffectChance>> upgrade in Config.Scp914EffectChances)
            {
                foreach ((EffectType effect, double chance, float duration) in upgrade.Value)
                    Log.Debug($"914 Effect Config: {upgrade.Key}: {effect} + {duration} - {chance}");
            }
        }

        if (Config.Scp914TeleportChances != null)
        {
            Log.Debug($"{Config.Scp914TeleportChances.Count}");
            foreach (KeyValuePair<Scp914KnobSetting, List<Scp914TeleportChance>> upgrade in Config.Scp914TeleportChances)
            {
                foreach ((RoomType room, List<RoomType> ignoredRooms, Vector3 offset, double chance, float damage, ZoneType zone) in upgrade.Value)
                {
                    Log.Debug($"914 Teleport Config: {upgrade.Key}: {room}/{zone} + {offset} - {chance} [{damage}]");
                    Log.Debug("Ignored rooms:");
                    if (ignoredRooms != null)
                    {
                        foreach (RoomType roomType in ignoredRooms)
                        {
                            Log.Debug(roomType);
                        }
                    }
                }
            }
        }
    }
}