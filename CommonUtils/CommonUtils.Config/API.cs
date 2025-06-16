using Exiled.API.Features;
using PlayerRoles;
using System.Collections.Generic;

namespace CommonUtils.Config.API;

public static class API
{
    // API methods for potential use by other plugins, not sure if anyone actually uses this
    public static float GetHealthOnKill(RoleTypeId role) =>
        MainPlugin.Configs.DamageHealthSettings.HealthGainedOnKill?.ContainsKey(role) ?? false
        ? MainPlugin.Configs.DamageHealthSettings.HealthGainedOnKill[role]
        : 0.0f;

    public static List<ItemType> GetStartItems(RoleTypeId role, Player player = null) => MainPlugin.Singleton.PlayerHandlers.GetStartingInventory(role, player);
}