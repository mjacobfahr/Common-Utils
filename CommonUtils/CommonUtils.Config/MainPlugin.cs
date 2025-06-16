global using Scp914KnobSetting = Scp914.Scp914KnobSetting;

using CommonUtils.Config.ConfigObjects;
using CommonUtils.Config.Events;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using UnityEngine;
using ItemEvents = Exiled.Events.Handlers.Item;
using PlayerEvents = Exiled.Events.Handlers.Player;
using Scp914Events = Exiled.Events.Handlers.Scp914;
using ServerEvents = Exiled.Events.Handlers.Server;
using WarheadEvents = Exiled.Events.Handlers.Warhead;
using Random = System.Random;

namespace CommonUtils.Config;

// TODO: There is an excessive amount of null checks throughout the codebase.
//       The project has nullable disabled and most properties are default-constructed so this should not be necessary.
// TODO: The IChanceObject roll logic is duplicated all throughout the codebase, abstract it to a utility method.

public class MainPlugin : Plugin<Config>
{
    public override string Author { get; } = "DeadServer Team";

    public override string Name { get; } = "CommonUtils.Config";

    public override string Prefix { get; } = "CommonUtils.Config";

    public override Version Version { get; } = new(9, 0, 0);

    public override Version RequiredExiledVersion { get; } = new(9, 6, 1);

    public override PluginPriority Priority { get; } = PluginPriority.Medium;

    public static MainPlugin Singleton { get; private set; }

    public static Config Configs => Singleton.Config;

    public static Random Random { get; } = new();

    public ItemHandlers ItemHandlers { get; private set; } = new();

    public PlayerHandlers PlayerHandlers { get; private set; } = new();

    public ServerHandlers ServerHandlers { get; private set; } = new();

    public MapHandlers MapHandlers { get; private set; } = new();

    public static List<CoroutineHandle> Coroutines { get; } = new();

    public static Dictionary<Player, Tuple<int, Vector3>> AfkDict { get; } = new();

    public override void OnEnabled()
    {
        Singleton = this;

        if (Configs.Debug)
        {
            DebugConfig();
        }

        Log.Debug("Registering EventHandlers..");
        PlayerEvents.Hurting += PlayerHandlers.OnPlayerHurting;
        PlayerEvents.Verified += PlayerHandlers.OnPlayerVerified;
        PlayerEvents.Spawned += PlayerHandlers.OnSpawned;
        PlayerEvents.Escaping += PlayerHandlers.OnEscaping;
        if (Configs.DamageHealthSettings.HealthGainedOnKill is not null)
        {
            PlayerEvents.Died += PlayerHandlers.OnPlayerDied;
        }
        if (Configs.StartingInventories is not null)
        {
            PlayerEvents.ChangingRole += PlayerHandlers.OnChangingRole;
        }
        if (Configs.GameplaySettings.RadioBatteryDrainMultiplier != 1.0f)
        {
            PlayerEvents.UsingRadioBattery += PlayerHandlers.OnUsingRadioBattery;
            ItemEvents.UsingRadioPickupBattery += ItemHandlers.OnUsingRadioPickupBattery;
        }
        if (Configs.ServerSettings.AfkLimit > 0)
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

        WarheadEvents.Starting += ServerHandlers.OnWarheadStarting;
        WarheadEvents.Stopping += ServerHandlers.OnWarheadStopping;

        Scp914Events.UpgradingPlayer += MapHandlers.OnUpgradingPlayer;
        if (Configs.Scp914ItemChances is not null)
        {
            Scp914Events.UpgradingPickup += MapHandlers.OnUpgradingPickup;
        }
        if (Configs.Scp914ItemChances is not null)
        {
            Scp914Events.UpgradingInventoryItem += MapHandlers.OnUpgradingInventoryItem;
        }

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
        ItemEvents.UsingRadioPickupBattery -= ItemHandlers.OnUsingRadioPickupBattery;
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

        WarheadEvents.Starting -= ServerHandlers.OnWarheadStarting;
        WarheadEvents.Stopping -= ServerHandlers.OnWarheadStopping;

        Scp914Events.UpgradingPlayer -= MapHandlers.OnUpgradingPlayer;
        Scp914Events.UpgradingPickup -= MapHandlers.OnUpgradingPickup;
        Scp914Events.UpgradingInventoryItem -= MapHandlers.OnUpgradingInventoryItem;

        ItemHandlers = null;
        PlayerHandlers = null;
        ServerHandlers = null;
        MapHandlers = null;
        Singleton = null;
        base.OnDisabled();
    }

    private void DebugConfig()
    {
        if (Configs.StartingInventories is not null)
        {
            Log.Debug($"Starting Inventories: {Configs.StartingInventories.Count}");
            foreach (KeyValuePair<RoleTypeId, StartingInventory> kvp in Configs.StartingInventories)
            {
                for (int i = 0; i < kvp.Value.UsedSlots; i++)
                {
                    foreach (StartingItem chance in kvp.Value[i])
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

        if (Configs.Scp914ItemChances is not null)
        {
            Log.Debug($"{Configs.Scp914ItemChances.Count}");
            foreach (KeyValuePair<Scp914KnobSetting, List<Scp914ItemChance>> upgrade in Configs.Scp914ItemChances)
            {
                foreach ((string oldItem, string newItem, double chance, int count) in upgrade.Value)
                    Log.Debug($"914 Item Config: {upgrade.Key}: {oldItem} -> {newItem}x({count}) - {chance}");
            }
        }

        if (Configs.Scp914RoleChances is not null)
        {
            Log.Debug($"{Configs.Scp914RoleChances.Count}");
            foreach (KeyValuePair<Scp914KnobSetting, List<Scp914RoleChance>> upgrade in Configs.Scp914RoleChances)
            {
                foreach ((string oldRole, string newRole, double chance, bool keepInventory, bool keepHealth) in upgrade.Value)
                    Log.Debug($"914 Role Config: {upgrade.Key}: {oldRole} -> {newRole} - {chance} keepInventory: {keepInventory} keepHealth: {keepHealth}");
            }
        }

        if (Configs.Scp914EffectChances is not null)
        {
            Log.Debug($"{Configs.Scp914EffectChances.Count}");
            foreach (KeyValuePair<Scp914KnobSetting, List<Scp914EffectChance>> upgrade in Configs.Scp914EffectChances)
            {
                foreach ((EffectType effect, double chance, float duration) in upgrade.Value)
                    Log.Debug($"914 Effect Config: {upgrade.Key}: {effect} + {duration} - {chance}");
            }
        }

        if (Configs.Scp914TeleportChances is not null)
        {
            Log.Debug($"{Configs.Scp914TeleportChances.Count}");
            foreach (KeyValuePair<Scp914KnobSetting, List<Scp914TeleportChance>> upgrade in Configs.Scp914TeleportChances)
            {
                foreach ((RoomType room, List<RoomType> ignoredRooms, Vector3 offset, double chance, float damage, ZoneType zone) in upgrade.Value)
                {
                    Log.Debug($"914 Teleport Config: {upgrade.Key}: {room}/{zone} + {offset} - {chance} [{damage}]");
                    Log.Debug("Ignored rooms:");
                    if (ignoredRooms is not null)
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