using CommonUtils.Config.ConfigObjects;
using Exiled.API.Enums;
using Exiled.API.Interfaces;
using PlayerRoles;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace CommonUtils.Config;

public class Config : IConfig
{
    [Description("If the plugin is enabled or not.")]
    public bool IsEnabled { get; set; } = true;

    [Description("Whether debug logs should be written to the console.")]
    public bool Debug { get; set; } = false;

    [Description("Whether debug logs relating to chance rolling should be written to the console.")]
    public bool RollDebug { get; set; } = false;

    [Description("Configure settings related to server maintenance/administration here.")]
    public ServerSettings ServerSettings { get; set; } = new();

    [Description("Configure settings related to gameplay here.")]
    public GameplaySettings GameplaySettings { get; set; } = new();

    [Description("Configure damage/health modifiers for weapons and roles here.")]
    public DamageHealthSettings DamageHealthSettings { get; set; } = new();

    [Description("Configure Disco Lights event that triggers when activating SCP-914.")]
    public Scp914Disco Scp914Disco { get; set; } = new();

    [Description("Configure audio sound-effects that trigger on certain events or otherwise.")]
    public AudioEffects AudioEffects { get; set; } = new();

    [Description("Whether or not probabilities should be additive (50 + 50 = 100) or not (50 + 50 = 2 seperate 50% chances)")]
    public bool AdditiveProbabilities { get; set; } = false;

    [Description("The list of starting items for roles. 'item_name' is the ItemType to give them, 'chance' is the percent chance of them spawning with it, " +
        "and 'group' allows you to restrict the item to only players with certain RA groups (Leave this as 'none' to allow all players to get the item). " +
        "You can specify the same item multiple times.")]
    public Dictionary<RoleTypeId, StartingInventory> StartingInventories { get; set; } = new()
    {
        {
            RoleTypeId.ClassD, new StartingInventory
            {
                Slot1 = new List<StartingItem>
                {
                    new()
                    {
                        ItemName = ItemType.KeycardJanitor.ToString(),
                        Chance = 10,
                        Group = "none",
                    },
                    new()
                    {
                        ItemName = ItemType.Coin.ToString(),
                        Chance = 100,
                        Group = "none",
                    },
                },
                Slot2 = new List<StartingItem>
                {
                    new()
                    {
                        ItemName = ItemType.Flashlight.ToString(),
                        Chance = 100,
                        Group = "none",
                    },
                },
                Ammo = new List<StartingAmmo>
                {
                    new()
                    {
                        Type = ItemType.Ammo556x45,
                        Amount = 200,
                        Group = "none",
                    },
                },
            }
        },
    };

    [Description("Whether or not SCPs are immune to effects gained from SCP-914.")]
    public bool ScpsImmuneToScp914Effects { get; set; } = true;

    [Description("Determines if SCP-914 effects are exclusive, meaning only one can be applied each time a player is upgraded.")]
    public bool Scp914EffectsExclusivity { get; set; } = false;

    [Description("The list of custom SCP-914 item upgrade recipes. 'original' is the ItemType being upgraded, " +
        "'new' is the ItemType to upgrade to, and 'chance' is the percent chance of the upgrade happening. " +
        "You can specify multiple upgrade choices for the same item. For custom items use the item's name.")]
    public Dictionary<Scp914KnobSetting, List<Scp914ItemChance>> Scp914ItemChances { get; set; } = new()
    {
        {
            Scp914KnobSetting.Rough, new List<Scp914ItemChance>
            {
                new()
                {
                    Original = ItemType.KeycardO5.ToString(),
                    New = ItemType.MicroHID.ToString(),
                    Chance = 50,
                },
            }
        },
    };

    [Description("A dictionary of random effects to apply to players when going through SCP-914 on certain settings.")]
    public Dictionary<Scp914KnobSetting, List<Scp914EffectChance>> Scp914EffectChances { get; set; } = new()
    {
        {
            Scp914KnobSetting.Rough, new List<Scp914EffectChance>
            {
                new()
                {
                    Effect = EffectType.Bleeding,
                    Chance = 100,
                },
            }
        },
    };

    [Description("The list of custom SCP-914 role change recipes for roles. Same rules as above, " +
        "except 'original' and 'new' should be RoleTypeId. For custom roles use the role's name.")]
    public Dictionary<Scp914KnobSetting, List<Scp914RoleChance>> Scp914RoleChances { get; set; } = new()
    {
        {
            Scp914KnobSetting.Rough, new List<Scp914RoleChance>
            {
                new()
                {
                    Original = RoleTypeId.ClassD.ToString(),
                    New = RoleTypeId.Spectator.ToString(),
                    Chance = 100,
                },
            }
        },
    };

    [Description("The list of SCP-914 teleport settings. When 'zone' is specified, a random room from the ZoneType " +
        "(excluding ignored_rooms) will be selected. When 'zone' is Unspecified, the specified 'room' (RoomType) " +
        "will be selected instead.")]
    public Dictionary<Scp914KnobSetting, List<Scp914TeleportChance>> Scp914TeleportChances { get; set; } = new()
    {
        {
            Scp914KnobSetting.Rough, new List<Scp914TeleportChance>
            {
                new()
                {
                    Room = RoomType.LczClassDSpawn,
                    Offset = Vector3.up,
                    Chance = 50,
                },
                new()
                {
                    Zone = ZoneType.LightContainment,
                    IgnoredRooms = new()
                    {
                        RoomType.Lcz173,
                    },
                    Chance = 100,
                },
            }
        },
    };
}