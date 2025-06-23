using CommonUtils.Config.ConfigObjects;
using CommonUtils.Core;
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

namespace CommonUtils.Config.Events;

public class PlayerHandlers
{
    private ServerSettings ServerSettings => MainPlugin.Configs.ServerSettings;
    private GameplaySettings GameplaySettings => MainPlugin.Configs.GameplaySettings;
    private DamageHealthSettings DamageHealthSettings => MainPlugin.Configs.DamageHealthSettings;
    private Dictionary<RoleTypeId, StartingInventory> StartingInventories => MainPlugin.Configs.StartingInventories;

    public void OnPlayerVerified(VerifiedEventArgs ev)
    {
        if (ev.Player is null)
        {
            return;
        }

        string message = FormatJoinMessage(ev.Player);
        if (!string.IsNullOrEmpty(message))
        {
            ev.Player.Broadcast(ServerSettings.JoinMessageDuration, message);
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
        if (StartingInventories.ContainsKey(ev.NewRole) && !ev.ShouldPreserveInventory)
        {
            if (ev.Items == null)
            {
                Log.Warn("items is null");
                return;
            }

            ev.Items.Clear();
            ev.Items.AddRange(GetStartingInventory(ev.NewRole, ev.Player));

            if (StartingInventories[ev.NewRole].Ammo == null || StartingInventories[ev.NewRole].Ammo.Count <= 0)
            {
                return;
            }
            if (StartingInventories[ev.NewRole].Ammo.Any(s => string.IsNullOrEmpty(s.Group) || s.Group == "none" || s.Group == ev.Player.Group.Name))
            {
                ev.Ammo.Clear();
                foreach ((ItemType type, ushort amount, string group) in StartingInventories[ev.NewRole].Ammo)
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
        if (DamageHealthSettings.HealthStartingValues is not null && DamageHealthSettings.HealthStartingValues.TryGetValue(newRole, out int health))
        {
            ev.Player.MaxHealth = health;
            ev.Player.Health = health;
        }
        if (ev.Player.Role is FpcRole && GameplaySettings.PlayerHealthInfo)
        {
            ev.Player.CustomInfo = $"({ev.Player.Health}/{ev.Player.MaxHealth}) {(!string.IsNullOrEmpty(ev.Player.CustomInfo) ? ev.Player.CustomInfo.Substring(ev.Player.CustomInfo.LastIndexOf(')') + 1) : string.Empty)}";
        }

        if (ServerSettings.AfkIgnoredRoles is not null && ServerSettings.AfkIgnoredRoles.Contains(newRole) && MainPlugin.AfkDict.TryGetValue(ev.Player, out Tuple<int, Vector3> value))
        {
            MainPlugin.AfkDict[ev.Player] =
                new Tuple<int, Vector3>(newRole is RoleTypeId.Spectator ? value.Item1 : 0, ev.Player.Position);
        }
    }

    public void OnPlayerDied(DiedEventArgs ev)
    {
        if (ev.Attacker is not null && DamageHealthSettings.HealthGainedOnKill is not null && DamageHealthSettings.HealthGainedOnKill.ContainsKey(ev.Attacker.Role))
        {
            ev.Attacker.Heal(DamageHealthSettings.HealthGainedOnKill[ev.Attacker.Role]);
        }
    }

    public void OnPlayerHurting(HurtingEventArgs ev)
    {
        if (DamageHealthSettings.RoleDamageDealtMultipliers is not null && ev.Attacker is not null && DamageHealthSettings.RoleDamageDealtMultipliers.TryGetValue(ev.Attacker.Role, out var damageMultiplier))
        {
            ev.Amount *= damageMultiplier;
        }
        if (DamageHealthSettings.RoleDamageReceivedMultipliers is not null && DamageHealthSettings.RoleDamageReceivedMultipliers.TryGetValue(ev.Player.Role, out damageMultiplier))
        {
            ev.Amount *= damageMultiplier;
        }
        if (DamageHealthSettings.DamageMultipliers is not null && DamageHealthSettings.DamageMultipliers.TryGetValue(ev.DamageHandler.Type, out damageMultiplier))
        {
            ev.Amount *= damageMultiplier;
        }

        if (GameplaySettings.PlayerHealthInfo)
        {
            ev.Player.CustomInfo = $"({ev.Player.Health}/{ev.Player.MaxHealth}) {(!string.IsNullOrEmpty(ev.Player.CustomInfo) ? ev.Player.CustomInfo.Substring(ev.Player.CustomInfo.LastIndexOf(')') + 1) : string.Empty)}";
        }

        if (ev.Attacker is not null && MainPlugin.AfkDict.ContainsKey(ev.Attacker))
        {
            Log.Debug($"Resetting {ev.Attacker.Nickname} AFK timer.");
            MainPlugin.AfkDict[ev.Attacker] = new Tuple<int, Vector3>(0, ev.Attacker.Position);
        }
    }

    public void OnEscaping(EscapingEventArgs ev)
    {
        if (ev.Player.IsCuffed && GameplaySettings.DisarmedEscapeSwitchRole is not null && GameplaySettings.DisarmedEscapeSwitchRole.TryGetValue(ev.Player.Role, out RoleTypeId newRole))
        {
            ev.NewRole = newRole;
            ev.IsAllowed = newRole != RoleTypeId.None;
        }
    }

    public void OnUsingRadioBattery(UsingRadioBatteryEventArgs ev)
    {
        ev.Drain *= GameplaySettings.RadioBatteryDrainMultiplier;
    }

    public void AntiAfkEventHandler(IPlayerEvent ev)
    {
        if (ev.Player != null && MainPlugin.AfkDict.ContainsKey(ev.Player))
        {
            Log.Debug($"Resetting {ev.Player.Nickname} AFK timer.");
            MainPlugin.AfkDict[ev.Player] = new Tuple<int, Vector3>(0, ev.Player.Position);
        }
    }

    public List<ItemType> GetStartingInventory(RoleTypeId role, Player player = null)
    {
        List<ItemType> items = new();

        Log.Debug($"{nameof(GetStartingInventory)} Iterating through slots...");
        // iterate through slots
        for (int i = 0; i < StartingInventories[role].UsedSlots; i++)
        {
            Log.Debug($"\n{nameof(GetStartingInventory)} Iterating slot {i + 1}");
            Log.Debug($"{nameof(GetStartingInventory)} Checking groups...");

            // item chances for that slot
            List<StartingItem> itemChances = StartingInventories[role][i]
                .Where(x =>
                    player == null
                    || string.IsNullOrEmpty(x.Group)
                    || x.Group == "none"
                    || x.Group == player.Group.Name)
                .ToList();

            Log.Debug($"{nameof(GetStartingInventory)} Finished checking groups, found {itemChances.Count} valid itemChances.");

            double rolledChance = Chance.RollChance(itemChances, MainPlugin.Configs.AdditiveProbabilities);
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

                if (MainPlugin.Configs.AdditiveProbabilities)
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
        return string.IsNullOrEmpty(ServerSettings.JoinMessage)
            ? string.Empty
            : ServerSettings.JoinMessage
                .Replace("%player%", player.Nickname)
                .Replace("%server%", Server.Name)
                .Replace("%count%", $"{Player.Dictionary.Count}");
    }
}