using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CommonUtils.Core;

/// <summary>
/// Helper class that provides convenience methods for AudioPlayerApi routines.
/// </summary>
public static class AudioHelper
{
    /// <summary>
    /// Loads all clips (by filename) in `audioFiles` from directory path `audioDir`.
    /// </summary>
    /// <param name="audioDir"><inheritdoc cref="LoadAudioClip(string, string, bool, Assembly)" path="/param[@name='audioDir']"/></param>
    /// <param name="audioFiles">A list of all audio filenames that should be loaded from the directory.</param>
    /// <param name="log"><inheritdoc cref="LoadAudioClip(string, string, bool, Assembly)" path="/param[@name='log']"/></param>
    /// <returns>A list of all files that failed to load.</returns>
    public static List<string> LoadAudioClips(string audioDir, List<string> audioFiles, bool log = false)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        List<string> failedClips = new();
        foreach (string file in audioFiles)
        {
            if (!LoadAudioClip(audioDir, file, log, assembly: callingAssembly))
            {
                failedClips.Add(file);
            }
        }
        return failedClips;
    }

    /// <summary>
    /// Loads an audio clip (by filename `audioFile`) from directory path `audioDir`.
    /// </summary>
    /// <param name="audioDir">Path to the directory that audio files are in.</param>
    /// <param name="audioFile">Name of audio file within audioDir to load. The loaded clip will be named as the file name minus the extension.</param>
    /// <param name="log">Whether debug messages should be logged. Defaults to false.</param>
    /// <param name="assembly"><inheritdoc cref="AttachAudioPlayer(AudioPlayer, GameObject, float, int, float, float, bool, Assembly)" path="/param[@name='assembly']"/></param>
    /// <returns>True if clip was loaded successfully.</returns>
    public static bool LoadAudioClip(string audioDir, string audioFile, bool log = false, Assembly assembly = null)
    {
        Assembly callingAssembly = assembly ?? Assembly.GetCallingAssembly();
        Log.Debug($"-- loading audio clip: {audioFile}", print: log, assembly: callingAssembly);

        string filepath = Path.Combine(audioDir, audioFile);
        string name = audioFile.Replace(".ogg", "");
        return AudioClipStorage.LoadClip(filepath, name);
    }

    /// <summary>
    ///  Gets or creates AudioPlayer by name. If creating and `parent` is provided, creates and attaches spatial speakers to the GameObject (See AttachAudioPlayer).
    /// </summary>
    /// <param name="audioPlayerName">The name of the AudioPlayer to get or create.</param>
    /// <param name="parent"><inheritdoc cref="AttachAudioPlayer(AudioPlayer, GameObject, float, int, float, float, bool, Assembly)" path="/param[@name='parent']"/></param>
    /// <param name="speakerVolume"><inheritdoc cref="AttachAudioPlayer(AudioPlayer, GameObject, float, int, float, float, bool, Assembly)" path="/param[@name='speakerVolume']"/></param>
    /// <param name="speakerCount"><inheritdoc cref="AttachAudioPlayer(AudioPlayer, GameObject, float, int, float, float, bool, Assembly)" path="/param[@name='speakerCount']"/></param>
    /// <param name="minDistance"><inheritdoc cref="AttachAudioPlayer(AudioPlayer, GameObject, float, int, float, float, bool, Assembly)" path="/param[@name='minDistance']"/></param>
    /// <param name="maxDistance"><inheritdoc cref="AttachAudioPlayer(AudioPlayer, GameObject, float, int, float, float, bool, Assembly)" path="/param[@name='maxDistance']"/></param>
    /// <param name="log"><inheritdoc cref="AttachAudioPlayer(AudioPlayer, GameObject, float, int, float, float, bool, Assembly)" path="/param[@name='log']"/></param>
    /// <returns>The AudioPlayer.</returns>
    public static AudioPlayer GetAudioPlayer(string audioPlayerName, GameObject parent = null, float speakerVolume = 1.0f, int speakerCount = 1, float minDistance = 5.0f, float maxDistance = 5.0f, bool log = false)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        return AudioPlayer.CreateOrGet(audioPlayerName, onIntialCreation: (audioPlayer) =>
        {
            if (parent is not null)
            {
                AttachAudioPlayer(audioPlayer, parent, speakerVolume, speakerCount, minDistance, maxDistance, log, callingAssembly);
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
    /// <param name="log">Whether debug messages should be logged. Defaults to false.</param>
    /// <param name="assembly">Only used internally for logging.
    ///     When provided, manually sets the calling assembly so that log messages will be named with the assembly that called the calling Core method.
    ///     When not provided, assumes that the calling assembly is the assembly that log messages will be named with.
    /// </param>
    /// <returns>The created Speaker object. If many speakers are created, the first is returned.</returns>
    public static Speaker AttachAudioPlayer(AudioPlayer audioPlayer, GameObject parent, float speakerVolume = 1.0f, int speakerCount = 1, float minDistance = 5.0f, float maxDistance = 5.0f, bool log = false, Assembly assembly = null)
    {
        Assembly callingAssembly = assembly ?? Assembly.GetCallingAssembly();
        try
        {
            // Attach created audio player to the gameObject's transform
            audioPlayer.transform.SetParent(parent.transform);

            Speaker outSpeaker = null;
            for (int i = 0; i < speakerCount; i++)
            {
                string speakerName = $"{audioPlayer.Name}-S{i + 1}";

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

            Log.Debug($"Setting {audioPlayer.Name} speaker to parent position: {audioPlayer.transform.parent.position}", print: log, assembly: callingAssembly);
            return outSpeaker;
        }
        catch (Exception ex)
        {
            Log.Error($"Exception during {nameof(AttachAudioPlayer)}: {ex.Message}", assembly: assembly);
            Log.Debug($"-- stacktrace: {ex.StackTrace}", print: log, assembly: assembly);
            return null;
        }
    }
}