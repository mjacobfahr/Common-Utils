using CommonUtils.Config.ConfigObjects;
using CommonUtils.Core;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp049;
using PlayerRoles;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonUtils.Config.Events;

// These could be sorted elsewhere but they are all custom handlers added by me as opposed to the original version
public class AudioHandlers
{
    private AudioEffects AudioConfig => MainPlugin.Singleton.Config.AudioEffects;

    private void PlayAudioEffect(string effectName, string audioClip, GameObject audioAttacher)
    {
        // Create the audio player and attach speakers
        try
        {
            var audioPlayer = AudioHelper.QuickPlayClip(
                audioPlayerName: effectName,
                clipName: audioClip,
                parent: audioAttacher,
                speakerVolume: AudioConfig.SpeakerVolume,
                speakerCount: 2,
                minDistance: AudioConfig.MinDistance,
                maxDistance: AudioConfig.MaxDistance
            );
            if (audioPlayer is not null)
            {
                Log.Info($"PlayAudioEffect: Created audio player for effect: {effectName} - playing clip: {audioClip}");
            }
            else
            {
                Log.Error($"PlayAudioEffect: Failed to create audio player for effect: {effectName}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"PlayAudioEffect: Exception caused while trying to create audio player: {effectName}");
            Log.Debug($"Exception: {ex.Message}");
        }
    }

    public void OnDoctorFinishingRecall(FinishingRecallEventArgs ev)
    {
        // nothing here yet
    }

    public void OnDoctorSendingCall(SendingCallEventArgs ev)
    {
        string randomAudio = AudioConfig.DoctorCallAudio.GetRandomValue();
        PlayAudioEffect("DoctorCall", randomAudio, ev.Player.GameObject);
    }

    public void OnDied(DiedEventArgs ev)
    {
        if (ev.Player == null || ev.Attacker == null || ev.DamageHandler.Type == DamageType.Unknown)
        {
            return;
        }
        if (ev.Attacker.Role == RoleTypeId.Scp049 && !AudioConfig.DoctorKillAudio.IsEmpty())
        {
            string randomAudio = AudioConfig.DoctorKillAudio.GetRandomValue();
            PlayAudioEffect("DoctorKill", randomAudio, ev.Attacker.GameObject);
            return;
        }
        if (ev.Attacker.Role == RoleTypeId.Scp0492 && !AudioConfig.ZombieKillAudio.IsEmpty())
        {
            string randomAudio = AudioConfig.DoctorKillAudio.GetRandomValue();
            PlayAudioEffect("ZombieKill", randomAudio, ev.Attacker.GameObject);
            return;
        }
    }
}