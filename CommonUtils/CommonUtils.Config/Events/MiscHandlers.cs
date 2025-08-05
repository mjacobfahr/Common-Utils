using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Scp914;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ExiledScp914 = Exiled.API.Features.Scp914;    // Scp914 conflicts with Scp914 from Assembly-CSharp

namespace CommonUtils.Config.Events;

public class MiscHandlers
{
    private Config Configs => MainPlugin.Singleton.Config;

    private CoroutineHandle DiscoHandle { get; set; }

    

    public void OnActivating(ActivatingEventArgs ev)
    {
        Timing.KillCoroutines(DiscoHandle);
        if (Configs.Scp914Disco.Enabled)
        {
            Log.Info($"SCP914 activating: starting disco lights");

            DiscoHandle = Timing.RunCoroutine(DiscoCoroutine(ev.KnobSetting));
            Timing.WaitUntilDone(DiscoHandle);
        }
    }

    private IEnumerator<float> DiscoCoroutine(Scp914KnobSetting knobSetting)
    {
        Log.Debug($"DiscoCoroutine: starting loop - knobSetting: {knobSetting} - isWorking: {ExiledScp914.IsWorking}");
        float delay = Configs.Scp914Disco.DelayInitial;
        Room room914 = Room.Get(RoomType.Lcz914);

        while (ExiledScp914.IsWorking)
        {
            // don't set lights if lights have been disabled
            if (!room914.AreLightsOff)
            {
                Color newColor;
                if (knobSetting == Scp914KnobSetting.Rough)
                {
                    // TODO: Make this option configurable - maybe a dict of KnobSettings -> OverrideColors
                    if (room914.Color == Color.red)
                    {
                        newColor = new Color(0.5f, 0.0f, 0.0f);
                    }
                    else
                    {
                        newColor = Color.red;
                    }
                }
                else
                {
                    // TODO: Could be improved with a disco-themed color pack but meh this works
                    float h = (float)MainPlugin.Random.NextDouble();
                    float s = 1.0f;
                    float v = 1.0f;
                    newColor = Color.HSVToRGB(h, s, v);
                }
                Log.Debug($"DiscoCoroutine: setting room color to: {newColor}");
                room914.Color = newColor;
            }
            else
            {
                Log.Debug($"DiscoCoroutine: lights are off");
            }

            // adjust delay and wait
            delay += Configs.Scp914Disco.DelayChange;
            delay = delay >= Configs.Scp914Disco.DelayMinimum ? delay : Configs.Scp914Disco.DelayMinimum;
            delay = delay <= Configs.Scp914Disco.DelayMaximum ? delay : Configs.Scp914Disco.DelayMaximum;
            yield return Timing.WaitForSeconds(delay);
        }

        // restore color
        if (!room914.AreLightsOff)
        {
            room914.Color = Color.clear;
        }
    }
}