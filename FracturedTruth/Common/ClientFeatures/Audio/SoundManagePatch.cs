using AmongUs.HTTP;
using Hazel;
using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using System.Runtime.InteropServices;
using static FracturedTruth.Common.Panels.Audio.AudioManager;
using static FracturedTruth.Common.Panels.Audio.FracturedSounds;
using static FracturedTruth.Translator;
using System.Linq;
using UnityEngine;

namespace FracturedTruth.Common.Panels.Audio;

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySoundImmediate))]
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySound))]
public class PlaySoundPatch
{
    public static bool Prefix(SoundManager __instance, [HarmonyArgument(0)] AudioClip clip, [HarmonyArgument(1)] bool loop)
    {
        var isPlaying = fracturedSounds.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var modSound = fracturedSounds.Any(x => x.Clip == clip);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla) || modSound || !loop;
    }

}
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayDynamicSound))]
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayNamedSound))]
public class PlayDynamicandNamedSoundPatch
{
    public static bool Prefix([HarmonyArgument(1)] AudioClip clip, [HarmonyArgument(2)] bool loop)
    {
        var isPlaying = fracturedSounds.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var modSound = fracturedSounds.Any(x => x.Clip == clip);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla) || modSound || !loop;
    }
}

[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.CrossFadeSound))]
public class CrossFadeSoundPatch
{
    public static bool Prefix([HarmonyArgument(1)] AudioClip clip)
    {
        var isPlaying = fracturedSounds.Any(x => x.CurrectAudioStates == AudiosStates.IsPlaying);
        var modSound = fracturedSounds.Any(x => x.Clip == clip);
        var disableVanilla = Main.DisableVanillaSound.Value;
        return !(isPlaying || disableVanilla) || modSound;
    }
}
