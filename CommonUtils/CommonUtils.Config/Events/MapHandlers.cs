using CommonUtils.Config.ConfigObjects;
using CommonUtils.Core;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Scp914;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ExiledScp914 = Exiled.API.Features.Scp914;    // Scp914 conflicts with Scp914 from Assembly-CSharp

namespace CommonUtils.Config.Events;

public class MapHandlers
{
    private Config Configs => MainPlugin.Singleton.Config;

    public void OnUpgradingPickup(UpgradingPickupEventArgs ev)
    {
        var itemChances = Configs.Scp914ItemChances[ev.KnobSetting];
        if (itemChances.Count > 0)
        {
            List<Scp914ItemChance> chances = itemChances
                .Where(x =>
                    x.Original == ev.Pickup.Type.ToString()
                    || (CustomItem.TryGet(ev.Pickup, out CustomItem item) && x.Original == item.Name))
                .ToList();
            Log.Debug($"{nameof(OnUpgradingPickup)}: Found {chances.Count} matching {nameof(Scp914ItemChance)} config entries for {ev.KnobSetting}.");

            double rolledChance = ChanceHelper.RollChance(chances, MainPlugin.Configs.AdditiveProbabilities);
            foreach ((string sourceItem, string destinationItem, double chance, int count) in chances)
            {
                if (Configs.RollDebug)
                {
                    Log.Debug($"{nameof(OnUpgradingPickup)}: SCP-914 is trying to upgrade a {ev.Pickup.Type} pickup. {sourceItem} -> {destinationItem} (x{count}); {rolledChance} <= {chance} ({rolledChance <= chance})");
                }

                if (rolledChance <= chance)
                {
                    Log.Debug($"{nameof(OnUpgradingPickup)}: Randomly selected {nameof(Scp914ItemChance)}: {destinationItem} (x{count})");
                    if (Enum.TryParse(destinationItem, out ItemType itemType))
                    {
                        if (itemType is not ItemType.None)
                        {
                            UpgradePickup(ev.Pickup, ev.OutputPosition + Vector3.up, count, false, itemType: itemType);
                        }
                    }
                    else if (CustomItem.TryGet(destinationItem, out CustomItem customItem))
                    {
                        if (customItem is not null)
                        {
                            UpgradePickup(ev.Pickup, ev.OutputPosition + Vector3.up, count, true, customItem: customItem);
                        }
                    }

                    ev.Pickup.Destroy();
                    ev.IsAllowed = false;
                    break;
                }

                if (Configs.AdditiveProbabilities)
                {
                    rolledChance -= chance;
                }
            }
        }
    }

    public void OnUpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
    {
        var itemChances = Configs.Scp914ItemChances[ev.KnobSetting];
        if (itemChances.Count > 0)
        {
            List<Scp914ItemChance> chances = itemChances
                .Where(x =>
                    x.Original == ev.Item.Type.ToString()
                    || (CustomItem.TryGet(ev.Item, out CustomItem item) && x.Original == item.Name))
                .ToList();
            Log.Debug($"{nameof(OnUpgradingInventoryItem)}: Found {chances.Count} matching {nameof(Scp914ItemChance)} config entries for {ev.KnobSetting}.");

            double rolledChance = ChanceHelper.RollChance(chances, MainPlugin.Configs.AdditiveProbabilities);
            foreach ((string sourceItem, string destinationItem, double chance, int count) in chances)
            {
                if (Configs.RollDebug)
                {
                    Log.Debug($"{nameof(OnUpgradingInventoryItem)}: {ev.Player.Nickname} is trying to upgrade a {ev.Item.Type} in their inventory. {sourceItem} -> {destinationItem} (x{count}); {rolledChance} <= {chance} ({rolledChance <= chance})");
                }

                if (rolledChance <= chance)
                {
                    Log.Debug($"{nameof(OnUpgradingInventoryItem)}: Randomly selected {nameof(Scp914ItemChance)}: {destinationItem} (x{count})");
                    if (Enum.TryParse(destinationItem, out ItemType itemType))
                    {
                        if (itemType is not ItemType.None)
                        {
                            UpgradeInventoryItem(ev, count, false, itemType: itemType);
                        }
                    }
                    else if (CustomItem.TryGet(destinationItem, out CustomItem customItem))
                    {
                        if (customItem is not null)
                        {
                            UpgradeInventoryItem(ev, count, true, customItem: customItem);
                        }
                    }
                    break;
                }
                if (Configs.AdditiveProbabilities)
                {
                    rolledChance -= chance;
                }
            }
        }
    }

    public void OnUpgradingPlayer(UpgradingPlayerEventArgs ev)
    {
        var effectChances = Configs.Scp914EffectChances[ev.KnobSetting];
        if (effectChances.Count > 0 && (ev.Player.Role.Side != Side.Scp || !Configs.ScpsImmuneToScp914Effects))
        {
            List<Scp914EffectChance> chances = effectChances;
            Log.Debug($"{nameof(OnUpgradingPlayer)}: Found {chances.Count} matching {nameof(Scp914EffectChance)} config entries for {ev.KnobSetting}.");

            double rolledChance = ChanceHelper.RollChance(chances, MainPlugin.Configs.AdditiveProbabilities);
            foreach ((EffectType effect, double chance, float duration) in chances)
            {
                if (Configs.RollDebug)
                {
                    Log.Debug($"{nameof(OnUpgradingPlayer)}: {ev.Player.Nickname} is trying to get a {effect} effect via upgrade. duration: {duration}s; {rolledChance} <= {chance} ({rolledChance <= chance})");
                }

                if (rolledChance <= chance)
                {
                    Log.Debug($"{nameof(OnUpgradingPlayer)}: Randomly selected {nameof(Scp914EffectChance)}: {effect} ({duration}s)");
                    ev.Player.EnableEffect(effect, duration);
                    if (Configs.Scp914EffectsExclusivity)
                    {
                        break;
                    }
                }
                if (Configs.AdditiveProbabilities)
                {
                    rolledChance -= chance;
                }
            }
        }

        var roleChances = Configs.Scp914RoleChances[ev.KnobSetting];
        if (roleChances.Count > 0)
        {
            List<Scp914RoleChance> chances = roleChances
                .Where(x =>
                    x.Original == ev.Player.Role.Type.ToString()
                    || (CustomRole.TryGet(ev.Player, out IReadOnlyCollection<CustomRole> customRoles) && customRoles.Select(r => r.Name).Contains(x.Original)))
                .ToList();
            Log.Debug($"{nameof(OnUpgradingPlayer)}: Found {chances.Count} matching {nameof(Scp914RoleChance)} config entries for {ev.KnobSetting}.");

            double rolledChance = ChanceHelper.RollChance(chances, MainPlugin.Configs.AdditiveProbabilities);
            foreach ((string sourceRole, string destinationRole, double chance, bool keepInventory, bool keepHealth) in chances)
            {
                if (Configs.RollDebug)
                {
                    Log.Debug($"{nameof(OnUpgradingPlayer)}: {ev.Player.Nickname} is trying to change their {ev.Player.Role.Type} role via upgrade. {sourceRole} -> {destinationRole}; keepInventory: {keepInventory}; keepHealth: {keepHealth}; {rolledChance} <= {chance} ({rolledChance <= chance})");
                }

                if (rolledChance <= chance)
                {
                    Log.Debug($"{nameof(OnUpgradingPlayer)}: Randomly selected {nameof(Scp914RoleChance)}: {destinationRole}");
                    float originalHealth = ev.Player.Health;
                    var originalItems = ev.Player.Items;
                    var originalAmmo = ev.Player.Ammo;

                    if (Enum.TryParse(destinationRole,  out RoleTypeId roleType))
                    {
                        ev.Player.Role.Set(roleType, SpawnReason.Respawn, RoleSpawnFlags.None);
                    }
                    else if (CustomRole.TryGet(destinationRole, out CustomRole customRole))
                    {
                        if (customRole is not null)
                        {
                            customRole.AddRole(ev.Player);
                            Timing.CallDelayed(0.5f, () => ev.Player.Teleport(ev.OutputPosition));
                        }
                    }

                    if (ev.Player.Role.IsDead)
                    {
                        return;
                    }
                    if (keepHealth)
                    {
                        ev.Player.Health = originalHealth;
                    }
                    if (keepInventory)
                    {
                        foreach (var item in originalItems)
                        {
                            ev.Player.AddItem(item);
                        }
                        foreach (var kvp in originalAmmo)
                        {
                            ev.Player.SetAmmo(kvp.Key.GetAmmoType(), kvp.Value);
                        }
                    }

                    ev.Player.Position = ev.OutputPosition;
                    break;
                }
                if (Configs.AdditiveProbabilities)
                {
                    rolledChance -= chance;
                }
            }
        }

        var teleportChances = Configs.Scp914TeleportChances[ev.KnobSetting];
        if (teleportChances.Count > 0)
        {
            List<Scp914TeleportChance> chances = teleportChances;
            Log.Debug($"{nameof(OnUpgradingPlayer)}: Found {chances.Count} matching {nameof(Scp914TeleportChance)} config entries for {ev.KnobSetting}.");

            double rolledChance = ChanceHelper.RollChance(chances, MainPlugin.Configs.AdditiveProbabilities);
            foreach ((RoomType roomType, List<RoomType> ignoredRooms, Vector3 offset, double chance, float damage, ZoneType zone) in chances)
            {
                if (Configs.RollDebug)
                {
                    Log.Debug($"{nameof(OnUpgradingPlayer)}: {ev.Player.Nickname} is trying to teleport via upgrade. {roomType} + {offset}; {zone} + {ignoredRooms?.Count} ignored rooms; damage: {damage}; {rolledChance} <= {chance} ({rolledChance <= chance})");
                }

                if (rolledChance <= chance)
                {
                    Log.Debug($"{nameof(OnUpgradingPlayer)}: Randomly selected {nameof(Scp914TeleportChance)}: {roomType} ({zone})");
                    ev.OutputPosition = ChoosePosition(zone, ignoredRooms, offset, roomType);
                    DealDamage(ev.Player, damage);
                    break;
                }
                if (Configs.AdditiveProbabilities)
                {
                    rolledChance -= chance;
                }
            }
        }
    }

    private Vector3 ChoosePosition(ZoneType zone, List<RoomType> ignoredRooms, Vector3 offset, RoomType roomType)
    {
        // without this offset, players will always fall through the floor on teleport
        Vector3 floorOffset = Vector3.up * 1.5f;

        Room room;
        if (zone == ZoneType.Unspecified)
        {
            room = Room.Get(roomType);
        }
        else
        {
            room = Room.List.Where(x => x.Zone == zone && !ignoredRooms.Contains(x.Type)).GetRandomValue();
        }
        return (room?.Position ?? Vector3.zero) + floorOffset + (zone == ZoneType.Unspecified ? offset : Vector3.zero);
    }

    private void DealDamage(Player player, float damage)
    {
        if (damage > 0f)
        {
            float amount = player.MaxHealth * damage;
            if (damage > 1f)
            {
                amount = damage;
            }

            Log.Debug($"{nameof(OnUpgradingPlayer)}: {player.Nickname} is being damaged for {amount}. -- {player.Health} * {damage}");
            player.Hurt(amount, "SCP-914 Teleport", "SCP-914");
        }
    }

    private void UpgradePickup(Pickup oldItem, Vector3 outputPos, int count, bool isCustomItem, ItemType itemType = ItemType.None, CustomItem customItem = null)
    {
        if (!isCustomItem)
        {
            Quaternion quaternion = oldItem.Rotation;
            Player previousOwner = oldItem.PreviousOwner;
            for (int i = 0; i < count; i++)
            {
                Pickup.CreateAndSpawn(itemType, outputPos, quaternion, previousOwner);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                customItem!.Spawn(outputPos, oldItem.PreviousOwner);
            }
        }
    }

    private void UpgradeInventoryItem(UpgradingInventoryItemEventArgs ev, int count, bool isCustomItem, ItemType itemType = ItemType.None, CustomItem customItem = null)
    {
        ev.Player.RemoveItem(ev.Item);
        if (!isCustomItem)
        {
            for (int i = 0; i < count; i++)
            {
                if (!ev.Player.IsInventoryFull)
                {
                    ev.Player.AddItem(itemType);
                }
                else
                {
                    Pickup.CreateAndSpawn(itemType, ExiledScp914.OutputPosition, ev.Player.Rotation, ev.Player);
                }
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                if (!ev.Player.IsInventoryFull)
                {
                    customItem!.Give(ev.Player);
                }
                else
                {
                    customItem!.Spawn(ExiledScp914.OutputPosition, ev.Player);
                }
            }
        }
    }
}