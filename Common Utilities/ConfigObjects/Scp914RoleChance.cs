using Common_Utilities.Interfaces;
using PlayerRoles;

namespace Common_Utilities.ConfigObjects;

public class Scp914RoleChance : IChanceObject
{
    public string Original { get; set; }

    public string New { get; set; } = RoleTypeId.Spectator.ToString();

    public double Chance { get; set; }

    public bool KeepInventory { get; set; } = true;

    public bool KeepHealth { get; set; } = true;

    public void Deconstruct(out string oldRole, out string newRole, out double chance, out bool keepInventory, out bool keepHealth)
    {
        oldRole = Original;
        newRole = New;
        chance = Chance;
        keepInventory = KeepInventory;
        keepHealth = KeepHealth;
    }
}