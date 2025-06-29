﻿using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace CommonUtils.Core;

public static class AudioHelper
{
    /// <summary>
    /// Loads an audio clip (by filename `audioFile`) from directory path `audioDir`.
    /// </summary>
    /// <returns>True if clip was loaded successfully.</returns>
    public static bool LoadAudioClip(string audioDir, string audioFile, bool log = false)
    {
        Log.Debug($"-- loading audio clip: {audioFile}", print: log);

        string filepath = Path.Combine(audioDir, audioFile);
        string name = audioFile.Replace(".ogg", "");
        return AudioClipStorage.LoadClip(filepath, name);
    }

    /// <summary>
    /// Loads all clips (by filename) in `audioFiles` from directory path `audioDir`.
    /// </summary>
    /// <returns>A list of all files that failed to load.</returns>
    public static List<string> LoadAudioClips(string audioDir, List<string> audioFiles)
    {
        List<string> failedClips = new();
        foreach (string file in audioFiles)
        {
            if (!LoadAudioClip(audioDir, file))
            {
                failedClips.Add(file);
            }
        }
        return failedClips;
    }

    /// <summary>
    ///  Gets or creates AudioPlayer by name. If creating and `parent` is provided, creates and attaches spatial speakers to the GameObject (See AttachAudioPlayer).
    /// </summary>
    /// <returns>The AudioPlayer.</returns>
    public static AudioPlayer GetAudioPlayer(string audioPlayerName, GameObject parent = null, float speakerVolume = 1.0f, int speakerCount = 1, float minDistance = 5.0f, float maxDistance = 5.0f)
    {
        return AudioPlayer.CreateOrGet(audioPlayerName, onIntialCreation: (audioPlayer) =>
        {
            if (parent is not null)
            {
                AttachAudioPlayer(audioPlayer, parent, speakerVolume, speakerCount, minDistance, maxDistance);
            }
        });
    }

    /// <summary>
    /// Creates spatial speaker(s) for a given AudioPlayer and binds them to a GameObject's transform.
    /// </summary>
    /// <param name="audioPlayer">The AudioPlayer, use GetAudioPlayer() to create/access by name.</param>
    /// <param name="parent">The GameObjet to attach speaker(s) to.</param>
    /// <param name="speakerVolume">Volume of each speaker, must be in range [0.0, 1.0]. Defaults to max volume.</param>
    /// <param name="speakerCount">How many speakers to add, use this to increase volume above max. Defaults to 1.</param>
    /// <param name="minDistance">Volume will always be at full speakerVolume within this distance. Defaults to 5.0.</param>
    /// <param name="maxDistance">Volume will decrease with distance until it disappears at this distance. Defaults to 5.0.</param>
    /// <returns>The created Speaker object. If many speakers are created, the first is returned.</returns>
    public static Speaker AttachAudioPlayer(AudioPlayer audioPlayer, GameObject parent, float speakerVolume = 1.0f, int speakerCount = 1, float minDistance = 5.0f, float maxDistance = 5.0f, bool log = false)
    {
        try
        {
            // Attach created audio player to the gameObject's transform
            audioPlayer.transform.SetParent(parent.transform);

            Speaker outSpeaker = null;
            for (int i = 0; i < speakerCount; i++)
            {
                string speakerName = $"{audioPlayer.Name}-Main-{i}";

                // This created speaker will be in 3D space.
                Speaker speaker = audioPlayer.GetOrAddSpeaker(speakerName, isSpatial: true, minDistance: minDistance, maxDistance: maxDistance);

                // Attach created speaker to gameObject.
                speaker.transform.SetParent(parent.transform);

                // Set local positino to zero to make sure that speaker is in the gameObject.
                speaker.transform.localPosition = Vector3.zero;

                // Set volume on the speaker
                speaker.Volume = speakerVolume;

                if (i == 0)
                {
                    outSpeaker = speaker;
                }
            }

            Log.Debug($"Setting audio player speaker to position: {audioPlayer.transform.position}", print: log);
            return outSpeaker;
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during SetAudioPlayerParent(): {ex.Message}");
            Log.Debug($"-- stacktrace: {ex.StackTrace}", print: log);
            return null;
        }
    }
}