using System.IO;
using static FracturedTruth.Translator;
using HarmonyLib;
using System.Threading.Tasks;
using static FracturedTruth.Helpers.PathHelper;
using static FracturedTruth.Common.Panels.Audio.FracturedSounds;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using FracturedTruth.Attributes;
using System.Linq;

namespace FracturedTruth.Common.Panels.Audio;

#nullable enable
public static class AudioManager
{
    [PluginModuleInitializer]
    public static void AudioManagerInit()
    {
        if (!Directory.Exists(audioDownloader_savepath)) Directory.CreateDirectory(audioDownloader_savepath);
        InitializeAll();
    }
    public static void Play(SupportMusics sound, PlayType play = PlayType.System_Once)
    {
        var audio = fracturedSounds.Find(x => x._NameInCode == sound.ToString());
        if (audio == null) return;
        Play(audio, play);
    }
    public static void Play(XtremeSounds sound, PlayType play = PlayType.System_Once)
    {
        var audio = fracturedSounds.Find(x => x._NameInCode == sound.ToString());
        if (audio == null) return;
        Play(audio, play);
    }
    public static void Play(FracturedSounds audio, PlayType play)
    {
#nullable disable
        if (audio.CurrectAudioStates is AudiosStates.NotExist or AudiosStates.IsPlaying) return;
#nullable enable
        if (!Constants.ShouldPlaySfx()) return;
        if (!Directory.Exists(audioDownloader_savepath))
        {
            Directory.CreateDirectory(audioDownloader_savepath);
            return;
        }

        StopPlay();
        fracturedSounds.Where(x => x != audio).Do(x => x.CurrectAudioStates = x.LastAudioStates);
        fracturedSounds.Where(x => x == audio && x.AudioType == AudioType.Music).Do(x => x.CurrectAudioStates = AudiosStates.IsPlaying);
        switch (play)
        {
            case PlayType.System_Once:
                PlaySound(@$"{audio.Path}", 0, 1);
                break;
            case PlayType.System_Loop:
                PlaySound(@$"{audio.Path}", 0, 9);
                break;
            case PlayType.AmongUs_Once:
                StartPlayInAmongUs(audio, false);
                break;
            case PlayType.AmongUs_Loop:
                StartPlayInAmongUs(audio, true);
                break;
        }
        Logger.Msg($"播放声音：{audio.Name}", "CustomSounds");
        MyMusic.RefreshTagList();
    }
    [DllImport("winmm.dll")]
    public static extern bool PlaySound(string? Filename, int Mod, int Flags);
    public static void StopPlay()
    {
        new Action(() =>
        {
            PlaySound(null, 0, 0);
            fracturedSounds.Do(x => x.CurrectAudioStates = x.LastAudioStates);
            SoundManager.Instance.StopAllSound();
            MyMusic.RefreshTagList();
        })();
    }
    public static void StartPlayInAmongUs(FracturedSounds audio, bool loop)
    {
        if (loop)
            SoundManager.Instance.CrossFadeSound(audio.Name, audio.Clip, 0.5f);
        else
            SoundManager.Instance.PlayNamedSound(audio.Name, audio.Clip, loop, null);
    }

}
