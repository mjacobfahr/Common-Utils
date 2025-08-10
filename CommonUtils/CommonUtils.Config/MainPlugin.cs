global using Log = CommonUtils.Core.Logger;
global using Scp914KnobSetting = Scp914.Scp914KnobSetting;

using CommonUtils.Config.ConfigObjects;
using CommonUtils.Config.Events;
using CommonUtils.Core;
using CommonUtils.Core.Interfaces;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ItemEvents = Exiled.Events.Handlers.Item;
using PlayerEvents = Exiled.Events.Handlers.Player;
using Scp914Events = Exiled.Events.Handlers.Scp914;
using Scp049Events = Exiled.Events.Handlers.Scp049;
using ServerEvents = Exiled.Events.Handlers.Server;
using WarheadEvents = Exiled.Events.Handlers.Warhead;
using Random = System.Random;

namespace CommonUtils.Config;

// TODO: There is an excessive amount of null checks throughout the codebase.
//       The project has nullable disabled and most properties are default-constructed so this should not be necessary.
// TODO: The IChanceObject roll logic is duplicated all throughout the codebase, abstract it to a utility method.
//       - Make a routine in Core that handles the additiveProb logic of reducing roll each time, pass a OnSelected handler to the method
//       - To help, IChanceObject should also require a Debug() method that logs its properties
//       - Update rolling methods in MapHandlers.cs/PlayerHandlers.cs

public class MainPlugin : Plugin<Config>
{
    public override string Author { get; } = "DeadServer Team";

    public override string Name { get; } = "CommonUtils.Config";

    public override string Prefix { get; } = "CommonUtils.Config";

    public override Version Version { get; } = new(1, 0, 0);

    public override Version RequiredExiledVersion { get; } = new(9, 6, 1);

    public override PluginPriority Priority { get; } = PluginPriority.Medium;

    public static MainPlugin Singleton { get; private set; }

    public static Config Configs => Singleton.Config;

    public static Random Random { get; } = new();

    public ItemHandlers ItemHandlers { get; private set; } = new();

    public PlayerHandlers PlayerHandlers { get; private set; } = new();

    public ServerHandlers ServerHandlers { get; private set; } = new();

    public MapHandlers MapHandlers { get; private set; } = new();

    public MiscHandlers MiscHandlers { get; private set; } = new();

    public AudioHandlers AudioHandlers { get; private set; } = new();

    public static List<CoroutineHandle> Coroutines { get; } = new();

    public static Dictionary<Player, Tuple<int, Vector3>> AfkDict { get; } = new();

    public override void OnEnabled()
    {
        Singleton = this;
        if (Configs.Debug)
        {
            Log.EnableDebug();
        }
        ValidateConfig();

        if (Configs.AudioEffects.Enabled)
        {
            // Load audio files
            Log.Info($"Loading audio clips from directory: {Configs.AudioEffects.AudioPath}");
            Configs.AudioEffects.DoctorResurrectAudio = AudioHelper.LoadAudioClips(Configs.AudioEffects.AudioPath, Configs.AudioEffects.DoctorResurrectAudio, returnSuccesses: true, log: true);
            Configs.AudioEffects.DoctorCallAudio = AudioHelper.LoadAudioClips(Configs.AudioEffects.AudioPath, Configs.AudioEffects.DoctorCallAudio, returnSuccesses: true, log: true);
            Configs.AudioEffects.DoctorKillAudio = AudioHelper.LoadAudioClips(Configs.AudioEffects.AudioPath, Configs.AudioEffects.DoctorKillAudio, returnSuccesses: true, log: true);
            Configs.AudioEffects.ZombieKillAudio = AudioHelper.LoadAudioClips(Configs.AudioEffects.AudioPath, Configs.AudioEffects.ZombieKillAudio, returnSuccesses: true, log: true);
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

        Scp914Events.Activating += MiscHandlers.OnActivating;

        if (Configs.AudioEffects.Enabled)
        {
            Scp049Events.FinishingRecall += AudioHandlers.OnDoctorFinishingRecall;
            Scp049Events.SendingCall += AudioHandlers.OnDoctorSendingCall;
            PlayerEvents.Died += AudioHandlers.OnDied;
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

        Scp914Events.Activating -= MiscHandlers.OnActivating;

        Scp049Events.FinishingRecall -= AudioHandlers.OnDoctorFinishingRecall;
        Scp049Events.SendingCall -= AudioHandlers.OnDoctorSendingCall;
        PlayerEvents.Died -= AudioHandlers.OnDied;

        ItemHandlers = null;
        PlayerHandlers = null;
        ServerHandlers = null;
        MapHandlers = null;
        MiscHandlers = null;
        AudioHandlers = null;
        Singleton = null;
        base.OnDisabled();
    }

    private void ValidateConfig()
    {
        // First validate and make sure config objects (that did not get default) are not null
        if (Configs.AudioEffects is null)
        {
            Configs.AudioEffects = new AudioEffects()
            {
                Enabled = false,
            };
        }
        if (!Directory.Exists(Configs.AudioEffects.AudioPath))
        {
            Configs.AudioEffects.Enabled = false;
            Log.Warn($"Disabling AudioEffects: AudioPath directory does not exist: {Configs.AudioEffects.AudioPath}");
        }

        // Check all chance objects and do one log if any are found
        bool foundNulls = false;
        if (Configs.StartingInventories is null)
        {
            foundNulls = true;
            Configs.StartingInventories = new();
            // TODO: Add a default entry for each RoleTypeId - ideally if its null we just use the default config value
        }

        if (Configs.Scp914ItemChances is null)
        {
            foundNulls = true;
            Configs.Scp914ItemChances = CreateDefaultUpgradeChances<Scp914ItemChance>();
        }
        else
        {
            Configs.Scp914ItemChances = AddDefaultUpgradeChances(Configs.Scp914ItemChances);
        }
        if (Configs.Scp914EffectChances is null)
        {
            foundNulls = true;
            Configs.Scp914EffectChances = CreateDefaultUpgradeChances<Scp914EffectChance>();
        }
        else
        {
            Configs.Scp914EffectChances = AddDefaultUpgradeChances(Configs.Scp914EffectChances);
        }
        if (Configs.Scp914RoleChances is null)
        {
            foundNulls = true;
            Configs.Scp914RoleChances = CreateDefaultUpgradeChances<Scp914RoleChance>();
        }
        else
        {
            Configs.Scp914RoleChances = AddDefaultUpgradeChances(Configs.Scp914RoleChances);
        }
        if (Configs.Scp914TeleportChances is null)
        {
            foundNulls = true;
            Configs.Scp914TeleportChances = CreateDefaultUpgradeChances<Scp914TeleportChance>();
        }
        else
        {
            Configs.Scp914TeleportChances = AddDefaultUpgradeChances(Configs.Scp914TeleportChances);
        }

        if (foundNulls)
        {
            Log.Debug($"Some ChanceObjects lists in the config were null - they will be auto-initialized");
        }

        // TODO: Get rid of need for Unspecified/Unknown (zone vs room)

        // Now debug the config if debug is enabled
        if (Configs.Debug)
        {
            try
            {
                Log.Debug($"StartingInventories: {Configs.StartingInventories.Count}");
                foreach (KeyValuePair<RoleTypeId, StartingInventory> kvp in Configs.StartingInventories)
                {
                    for (int i = 0; i < kvp.Value.UsedSlots; i++)
                    {
                        foreach (StartingItem chance in kvp.Value[i])
                        {
                            Log.Debug($"-- {kvp.Key}-Slot{i + 1}: ({chance.Chance}) {chance.ItemName}");
                        }
                    }

                    foreach ((ItemType type, ushort amount, string group) in kvp.Value.Ammo)
                    {
                        Log.Debug($"-- {kvp.Key}-{type}: {amount} - group: {group}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"StartingInventories exception: {e.Message}");
            }

            try
            {
                Log.Debug($"Scp914ItemChances: {Configs.Scp914ItemChances.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<Scp914ItemChance>> upgrade in Configs.Scp914ItemChances)
                {
                    foreach ((string oldItem, string newItem, double chance, int count) in upgrade.Value)
                    {
                        Log.Debug($"-- {upgrade.Key}: ({chance}) {oldItem} -> {newItem}x({count})");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Scp914ItemChances exception: {e.Message}");
            }

            try
            {
                Log.Debug($"Scp914EffectChances: {Configs.Scp914EffectChances.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<Scp914EffectChance>> upgrade in Configs.Scp914EffectChances)
                {
                    foreach ((EffectType effect, double chance, float duration) in upgrade.Value)
                    {
                        Log.Debug($"-- {upgrade.Key}: ({chance}) {effect} - duration: {duration}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Scp914EffectChances exception: {e.Message}");
            }

            try
            {
                Log.Debug($"Scp914RoleChances: {Configs.Scp914RoleChances.Count}");
                foreach (KeyValuePair<Scp914KnobSetting, List<Scp914RoleChance>> upgrade in Configs.Scp914RoleChances)
                {
                    foreach ((string oldRole, string newRole, double chance, bool keepInventory, bool keepHealth) in upgrade.Value)
                    {
                        Log.Debug($"-- {upgrade.Key}: ({chance}) {oldRole} -> {newRole} - keepInventory: {keepInventory} keepHealth: {keepHealth}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Scp914RoleChances exception: {e.Message}");
            }

            try
            {
                if (Configs.Scp914TeleportChances is not null)
                {
                    Log.Debug($"Scp914TeleportChances: {Configs.Scp914TeleportChances.Count}");
                    foreach (KeyValuePair<Scp914KnobSetting, List<Scp914TeleportChance>> upgrade in Configs.Scp914TeleportChances)
                    {
                        foreach ((RoomType room, List<RoomType> ignoredRooms, Vector3 offset, double chance, float damage, ZoneType zone) in upgrade.Value)
                        {
                            Log.Debug($"-- {upgrade.Key}: ({chance}) {room}/{zone} + {offset} - damage: {damage} ignored rooms: [{string.Join(", ", ignoredRooms.Select(x => x.ToString()))}]");
                        }
                    }
                }
                else
                {
                    Log.Warn($"Scp914TeleportChances is NULL");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Scp914TeleportChances exception: {e.Message}");
            }
        }
    }

    private Dictionary<Scp914KnobSetting, List<T>> CreateDefaultUpgradeChances<T>()
        where T : IChanceObjectD
    {
        return new Dictionary<Scp914KnobSetting, List<T>>()
        {
            { Scp914KnobSetting.Rough, new List<T>() },
            { Scp914KnobSetting.Coarse, new List<T>() },
            { Scp914KnobSetting.OneToOne, new List<T>() },
            { Scp914KnobSetting.Fine, new List<T>() },
            { Scp914KnobSetting.VeryFine, new List<T>() },
        };
    }

    private Dictionary<Scp914KnobSetting, List<T>> AddDefaultUpgradeChances<T>(Dictionary<Scp914KnobSetting, List<T>> configValue)
        where T : IChanceObjectD
    {
        foreach (var item in Scp914KnobSetting.Rough.GetAllValues())
        {
            Scp914KnobSetting setting = (Scp914KnobSetting)item;
            if (configValue.TryGetValue(setting, out List<T> list) && list is not null)
            {
                foreach (var listItem in list)
                {
                    if (listItem is Scp914TeleportChance teleportChance)
                    {
                        teleportChance.IgnoredRooms = teleportChance.IgnoredRooms ??= new();
                    }
                }
            }
            else
            {
                configValue[setting] = new List<T>();
            }
        }
        return configValue;
    }
}