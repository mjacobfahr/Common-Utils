using Common_Utilities.ConfigObjects;
using Exiled.Events.EventArgs.Item;

namespace Common_Utilities.Events;

public class ItemHandlers
{
    private GameplaySettings GameplaySettings => MainPlugin.Configs.GameplaySettings;

    public void OnUsingRadioPickupBattery(UsingRadioPickupBatteryEventArgs ev)
    {
        ev.Drain *= GameplaySettings.RadioBatteryDrainMultiplier;
    }
}