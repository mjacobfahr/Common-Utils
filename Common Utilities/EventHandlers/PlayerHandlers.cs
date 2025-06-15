using Common_Utilities.ConfigObjects;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Player = Exiled.API.Features.Player;

namespace Common_Utilities.EventHandlers;

public class PlayerHandlers
{
    private Config Configs => Plugin.Singleton.Config;

    public void OnPlayerVerified(VerifiedEventArgs ev)
    {
        if (ev.Player is null)
        {
            return;
        }

        string message = FormatJoinMessage(ev.Player);
        if (!string.IsNullOrEmpty(message))
        {
            ev.Player.Broadcast(Configs.JoinMessageDuration, message);
        }
    }

    public void OnChangingRole(ChangingRoleEventArgs ev)
    {
        if (ev.Player == null)
        {
            Log.Warn($"{nameof(OnChangingRole)}: Triggering player is null.");
            return;
        }

        // no clue why this works while in ChangingRole instead of spawned but if it ain't broke don't fix it
        // answering my previous question, (obviously) it works because we're setting ev.Items which are yet to be given to the player
        if (Configs.StartingInventories.ContainsKey(ev.NewRole) && !ev.ShouldPreserveInventory)
        {
            if (ev.Items == null)
            {
                Log.Warn("items is null");
                return;
            }

            ev.Items.Clear();
            ev.Items.AddRange(GetStartingInventory(ev.NewRole, ev.Player));

            if (Configs.StartingInventories[ev.NewRole].Ammo == null || Configs.StartingInventories[ev.NewRole].Ammo.Count <= 0)
            {
                return;
            }
            if (Configs.StartingInventories[ev.NewRole].Ammo.Any(s => string.IsNullOrEmpty(s.Group) || s.Group == "none" || s.Group == ev.Player.Group.Name))
            {
                ev.Ammo.Clear();
                foreach ((ItemType type, ushort amount, string group) in Configs.StartingInventories[ev.NewRole].Ammo)
                {
                    if (string.IsNullOrEmpty(group) || group == "none" || group == ev.Player.Group.Name)
                    {
                        ev.Ammo.Add(type, amount);
                    }
                }
            }
        }
    }

    public void OnSpawned(SpawnedEventArgs ev)
    {
        if (ev.Player is null)
        {
            Log.Warn($"{nameof(OnSpawned)}: Triggering player is null.");
            return;
        }

        RoleTypeId newRole = ev.Player.Role.Type;
        if (Configs.HealthValues is not null && Configs.HealthValues.TryGetValue(newRole, out int health))
        {
            ev.Player.MaxHealth = health;
            ev.Player.Health = health;
        }
        if (ev.Player.Role is FpcRole && Configs.PlayerHealthInfo)
        {
            ev.Player.CustomInfo = $"({ev.Player.Health}/{ev.Player.MaxHealth}) {(!string.IsNullOrEmpty(ev.Player.CustomInfo) ? ev.Player.CustomInfo.Substring(ev.Player.CustomInfo.LastIndexOf(')') + 1) : string.Empty)}";
        }

        if (Configs.AfkIgnoredRoles is not null && Configs.AfkIgnoredRoles.Contains(newRole) && Plugin.AfkDict.TryGetValue(ev.Player, out Tuple<int, Vector3> value))
        {
            Plugin.AfkDict[ev.Player] =
                new Tuple<int, Vector3>(newRole is RoleTypeId.Spectator ? value.Item1 : 0, ev.Player.Position);
        }
    }

    public void OnPlayerDied(DiedEventArgs ev)
    {
        if (ev.Attacker is not null && Configs.HealthOnKill is not null && Configs.HealthOnKill.ContainsKey(ev.Attacker.Role))
        {
            ev.Attacker.Heal(Configs.HealthOnKill[ev.Attacker.Role]);
        }
    }

    public void OnPlayerHurting(HurtingEventArgs ev)
    {
        if (Configs.RoleDamageDealtMultipliers is not null && ev.Attacker is not null && Configs.RoleDamageDealtMultipliers.TryGetValue(ev.Attacker.Role, out var damageMultiplier))
        {
            ev.Amount *= damageMultiplier;
        }
        if (Configs.RoleDamageReceivedMultipliers is not null && Configs.RoleDamageReceivedMultipliers.TryGetValue(ev.Player.Role, out damageMultiplier))
        {
            ev.Amount *= damageMultiplier;
        }
        if (Configs.DamageMultipliers is not null && Configs.DamageMultipliers.TryGetValue(ev.DamageHandler.Type, out damageMultiplier))
        {
            ev.Amount *= damageMultiplier;
        }

        if (Configs.PlayerHealthInfo)
        {
            ev.Player.CustomInfo = $"({ev.Player.Health}/{ev.Player.MaxHealth}) {(!string.IsNullOrEmpty(ev.Player.CustomInfo) ? ev.Player.CustomInfo.Substring(ev.Player.CustomInfo.LastIndexOf(')') + 1) : string.Empty)}";
        }

        if (ev.Attacker is not null && Plugin.AfkDict.ContainsKey(ev.Attacker))
        {
            Log.Debug($"Resetting {ev.Attacker.Nickname} AFK timer.");
            Plugin.AfkDict[ev.Attacker] = new Tuple<int, Vector3>(0, ev.Attacker.Position);
        }
    }

    public void OnEscaping(EscapingEventArgs ev)
    {
        if (ev.Player.IsCuffed && Configs.DisarmedEscapeSwitchRole is not null && Configs.DisarmedEscapeSwitchRole.TryGetValue(ev.Player.Role, out RoleTypeId newRole))
        {
            ev.NewRole = newRole;
            ev.IsAllowed = newRole != RoleTypeId.None;
        }
    }

    public void OnUsingRadioBattery(UsingRadioBatteryEventArgs ev)
    {
        ev.Drain *= Configs.RadioBatteryDrainMultiplier;
    }

    public void AntiAfkEventHandler(IPlayerEvent ev)
    {
        if (ev.Player != null && Plugin.AfkDict.ContainsKey(ev.Player))
        {
            Log.Debug($"Resetting {ev.Player.Nickname} AFK timer.");
            Plugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(0, ev.Player.Position);
        }
    }

    public List<ItemType> GetStartingInventory(RoleTypeId role, Player player = null)
    {
        List<ItemType> items = new();

        Log.Debug($"{nameof(GetStartingInventory)} Iterating through slots...");
        // iterate through slots
        for (int i = 0; i < Configs.StartingInventories[role].UsedSlots; i++)
        {
            Log.Debug($"\n{nameof(GetStartingInventory)} Iterating slot {i + 1}");
            Log.Debug($"{nameof(GetStartingInventory)} Checking groups...");

            // item chances for that slot
            List<ItemChance> itemChances = Configs.StartingInventories[role][i]
                .Where(x => 
                    player == null 
                    || string.IsNullOrEmpty(x.Group) 
                    || x.Group == "none" 
                    || x.Group == player.Group.Name)
                .ToList();

            Log.Debug($"{nameof(GetStartingInventory)} Finished checking groups, found {itemChances.Count} valid itemChances.");

            double rolledChance = Utils.RollChance(itemChances);

            foreach ((string item, double chance) in itemChances)
            {
                Log.Debug($"{nameof(GetStartingInventory)} Trying to assign to slot {i + 1} for {role}; item: {item}; {rolledChance} <= {chance} ({rolledChance <= chance}).");

                if (rolledChance <= chance)
                {
                    if (Enum.TryParse(item, true, out ItemType type))
                    {
                        items.Add(type);
                        break;
                    }

                    if (CustomItem.TryGet(item, out CustomItem customItem))
                    {
                        if (player != null)
                        {
                            customItem!.Give(player);
                        }
                        else
                        {
                            Log.Warn($"{nameof(GetStartingInventory)} Tried to give {customItem!.Name} to a null player.");
                        }
                        break;
                    }

                    Log.Warn($"{nameof(GetStartingInventory)} Skipping {item} as it is not a valid ItemType or CustomItem!");
                }

                if (Configs.AdditiveProbabilities)
                {
                    rolledChance -= chance;
                }
            }
        }

        Log.Debug($"{nameof(GetStartingInventory)} Finished iterating slots.");
        return items;
    }

    private string FormatJoinMessage(Player player)
    {
        return string.IsNullOrEmpty(Configs.JoinMessage)
            ? string.Empty
            : Configs.JoinMessage
                .Replace("%player%", player.Nickname)
                .Replace("%server%", Server.Name)
                .Replace("%count%", $"{Player.Dictionary.Count}");
    }
}