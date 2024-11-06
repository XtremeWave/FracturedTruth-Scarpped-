using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static FracturedTruth.Helpers.PathHelper;
using static FracturedTruth.Translator;
using FracturedTruth.Helpers;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace FracturedTruth.Common.Panels.Audio;

public enum SupportMusics
{
    ModLoad__Fractured__Slok,
    TidalSurge__TidalSurge__Slok,
    GongXiFaCai__GongXiFaCai__Andy_Lau,
    NeverGonnaGiveYouUp__NeverGonnaGiveYouUp__Rick_Astley,
}
public enum XtremeSounds
{
    ModLoad
}
public enum AudiosStates
{
    Sound,
    NotExist,
    Exist,
    IsPlaying,
}
public enum AudioType
{
    Sound,
    Music
}
public enum PlayType
{
    System_Once,
    System_Loop,
    AmongUs_Once,
    AmongUs_Loop,
}

public class FracturedSounds
{
    public static List<FracturedSounds> fracturedSounds = new();

    public string Name = "";
    public string FileName = "";
    public string Author = "";
    public string Path = "";
    public string _NameInCode = "";

    public AudiosStates CurrectAudioStates;
    public AudiosStates LastAudioStates;
    public AudioType AudioType;

    public AudioClip Clip;


    public static void InitializeAll()
    {
        foreach (var file in EnumHelper.GetAllValues<SupportMusics>().ToList())
        {
            new FracturedSounds(file);
        }
        foreach (var file in EnumHelper.GetAllValues<XtremeSounds>().ToList())
        {
            new FracturedSounds(file);
        }
    }
    public static async Task<AudioClip> LoadAudioClipAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Logger.Error("File does not exist: " + filePath);
            return null;
        }

        byte[] audioData;

        try
        {
            audioData = await ReadAllBytesAsync(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to read file: " + filePath + "\n" + e.Message);
            return null;
        }

        if (audioData == null || audioData.Length < 2)
        {
            Debug.LogError("Audio data is empty or invalid.");
            return null;
        }

        int channels = 2; 
        int sampleCount = audioData.Length / (2 * channels); 

        AudioClip audioClip = AudioClip.Create("LoadedAudioClip", sampleCount, channels, 44100, false);
        float[] floatData = ConvertBytesToFloats(audioData);

        audioClip.SetData(floatData, 0);

        return audioClip;
    }

    private static async Task<byte[]> ReadAllBytesAsync(string filePath)
    {
        using (FileStream sourceStream = new(filePath,
            FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true))
        {
            MemoryStream memoryStream = new();
            await sourceStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }

    private static float[] ConvertBytesToFloats(byte[] audioBytes)
    {
        float[] floatData = new float[audioBytes.Length / 2];
        for (int i = 0; i < floatData.Length; i++)
        {
            floatData[i] = (float)BitConverter.ToInt16(audioBytes, i * 2) / 32768.0f;
        }
        return floatData;
    }

    public FracturedSounds(SupportMusics music)
    {
        _NameInCode = music.ToString();
        var Part = music.ToString().Split("__");
        FileName = Part[0];
        Name = GetString($"Mus.{Part[1]}");
        Author = Part[2].Replace("_", " ");
        Path = audioDownloader_savepath + FileName + ".wav";
        CurrectAudioStates = LastAudioStates = File.Exists(Path) ? AudiosStates.Exist : AudiosStates.NotExist;
        AudioType = AudioType.Music;
        Clip = LoadAudioClipAsync(Path).Result;
        lock (fracturedSoundsLock)
        {
            var file = fracturedSounds.Find(x => x._NameInCode == _NameInCode);
            if (file != null)
            {
                Logger.Info($"Replace {file.FileName}", "FracturedSounds");
                var index = fracturedSounds.IndexOf(file);
                if (file.CurrectAudioStates == AudiosStates.IsPlaying) CurrectAudioStates = AudiosStates.IsPlaying;
                fracturedSounds.RemoveAt(index);
                fracturedSounds.Insert(index, this);

            }
            else 
                fracturedSounds.Add(this);
        }
    }
    private static readonly object fracturedSoundsLock = new();
    public FracturedSounds(XtremeSounds sound)
    {
        _NameInCode = sound.ToString();
        FileName = sound.ToString();
        Path = audioDownloader_savepath + FileName + ".wav";
        CurrectAudioStates = LastAudioStates = AudiosStates.Sound;
        AudioType = AudioType.Sound;
        Clip = LoadAudioClipAsync(Path).Result;
        lock (fracturedSoundsLock)
        {
            var file = fracturedSounds.Find(x => x._NameInCode == _NameInCode);
            if (file != null)
            {
                Logger.Info($"Replace {file.FileName}", "FracturedSounds");
                var index = fracturedSounds.IndexOf(file);
                fracturedSounds.RemoveAt(index);
                fracturedSounds.Insert(index, this);
            }
            else
                fracturedSounds.Add(this);
        }
    }
}
