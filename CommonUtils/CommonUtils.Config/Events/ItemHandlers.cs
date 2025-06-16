using CommonUtils.Config.ConfigObjects;
using Exiled.Events.EventArgs.Item;

namespace CommonUtils.Config.Events;

public class ItemHandlers
{
    private GameplaySettings GameplaySettings => MainPlugin.Configs.GameplaySettings;

    public void OnUsingRadioPickupBattery(UsingRadioPickupBatteryEventArgs ev)
    {
        ev.Drain *= GameplaySettings.RadioBatteryDrainMultiplier;
    }
}