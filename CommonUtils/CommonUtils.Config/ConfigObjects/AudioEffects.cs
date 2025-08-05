using Exiled.API.Features;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

namespace CommonUtils.Config.ConfigObjects;

public class AudioEffects
{
    public bool Enabled { get; set; } = false;

    // TODO: In the future if this grows into a full-scale AudioPlayer replacement, some of these settings may need to be unique for each list of audio clips

    [Description("Full path to the directory containing audio files. Defaults to <EXILED>/Audio/CommonUtils")]
    public string AudioPath { get; set; } = Path.Combine(Paths.Exiled, "Audio", "CommonUtils");

    [Description("Set the volume of each created speaker. Valid range is [0, 1]. Defaults to 1.")]
    public float SpeakerVolume { get; set; } = 1.0f;

    [Description("The audio will be at maximum volume for players within this range of the boombox.")]
    public float MinDistance { get; set; } = 3.0f;

    [Description("The audio will gradually fade in volume past the MinDistance until it completely disappears at this range.")]
    public float MaxDistance { get; set; } = 15.0f;

    [Description("List of audio files to randomly play when Plague Doctor (SCP-049) resurrects a player (Recall).")]
    public List<string> DoctorResurrectAudio { get; set; } = new();

    [Description("List of audio files to randomly play when Plague Doctor (SCP-049) uses the Call ability.")]
    public List<string> DoctorCallAudio { get; set; } = new();

    [Description("List of audio files to randomly play when Plague Doctor (SCP-049) kills a player.")]
    public List<string> DoctorKillAudio { get; set; } = new();

    [Description("List of audio files to randomly play when a Zombie (SCP-049-2) kills a player.")]
    public List<string> ZombieKillAudio { get; set; } = new();
}
