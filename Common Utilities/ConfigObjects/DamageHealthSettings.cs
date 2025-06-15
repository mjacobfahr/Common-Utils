using Exiled.API.Enums;
using PlayerRoles;
using System.Collections.Generic;
using System.ComponentModel;
namespace Common_Utilities.ConfigObjects;

public class DamageHealthSettings
{
    [Description("A dictionary of all roles and their damage dealt modifiers. The number here is a multiplier, not a raw damage amount. " +
        "Thus, setting it to 1 = normal damage, 1.5 = 50% more damage, and 0.5 = 50% less damage.")]
    public Dictionary<RoleTypeId, float> RoleDamageDealtMultipliers { get; set; } = new()
    {
        {
            RoleTypeId.Scp173, 1.0f
        },
    };

    [Description("Dictionary of roles and their damage received multipliers. Multipliers are same as above.")]
    public Dictionary<RoleTypeId, float> RoleDamageReceivedMultipliers { get; set; } = new()
    {
        {
            RoleTypeId.Scp173, 1.0f
        },
    };

    [Description("A dictionary of all Weapons and their damage modifiers. Multipliers are same as above.")]
    public Dictionary<DamageType, float> DamageMultipliers { get; set; } = new()
    {
        {
            DamageType.E11Sr, 1.0f
        },
    };

    [Description("A dictionary of roles and how much health they should be given when they kill someone.")]
    public Dictionary<RoleTypeId, float> HealthGainedOnKill { get; set; } = new()
    {
        {
            RoleTypeId.Scp173, 0
        },
    };

    [Description("A dictionary of roles and what their default starting health should be.")]
    public Dictionary<RoleTypeId, int> HealthStartingValues { get; set; } = new()
    {
        {
            RoleTypeId.Scp173, 3200
        },
    };
}