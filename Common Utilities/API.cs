using Exiled.API.Features;
using PlayerRoles;
using System.Collections.Generic;

namespace Common_Utilities.API;

public static class API
{
    // API methods for potential use by other plugins, not sure if anyone actually uses this
    public static float GetHealthOnKill(RoleTypeId role) => Plugin.Configs.HealthOnKill?.ContainsKey(role) ?? false ? Plugin.Configs.HealthOnKill[role] : 0f;

    public static List<ItemType> GetStartItems(RoleTypeId role, Player player = null) => Plugin.Singleton.PlayerHandlers.GetStartingInventory(role, player);
}