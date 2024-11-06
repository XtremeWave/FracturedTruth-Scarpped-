using FracturedTruth.Helpers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FracturedTruth.Common.Panels.Audio.AudioManager;
using static FracturedTruth.Common.Panels.Audio.FracturedSounds;
using static FracturedTruth.Translator;
using Object = UnityEngine.Object;

namespace FracturedTruth.Common.Panels.Audio;

public static class MyMusic
{
    public static SpriteRenderer CustomBackground { get; private set; }
    public static List<GameObject> Items { get; private set; }
    public static OptionsMenuBehaviour OptionsMenuBehaviourNow { get; private set; }
    static List<Action> UpdateButton = new();
    static bool OnlyExist = false;
    static int currentPage = 1;
    static int numItems = 0;
    static int ItemsPerPage => 7;
    static int TotalPageCount => (fracturedSounds.Count + ItemsPerPage - 1) / ItemsPerPage;
    static int CurrectPageCount => OnlyExist? (fracturedSounds.Count(x => x.LastAudioStates == AudiosStates.Exist) + ItemsPerPage - 1) / ItemsPerPage : TotalPageCount;
    public static void Hide()
    {
        if (CustomBackground != null)
            CustomBackground?.gameObject?.SetActive(false);
    }
    public static void Init(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        UpdateButton = new();
        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
        OptionsMenuBehaviourNow = optionsMenuBehaviour;
        if (CustomBackground == null)
        {
            currentPage = 1;
            numItems = 0;

            CustomBackground = Object.Instantiate(optionsMenuBehaviour.Background, optionsMenuBehaviour.transform);
            CustomBackground.name = "My Music Panel";
            CustomBackground.transform.localScale = new(0.9f, 0.9f, 1f);
            CustomBackground.transform.localPosition += Vector3.back * 18;
            CustomBackground.gameObject.SetActive(false);

            CreateButton(
                "Close Button",
                Color.red,
                GetString("Close"),
                new Vector3(1.3f, -2.43f, -16f),
                new Action(() =>
                {
                    CustomBackground.gameObject.SetActive(false);
                }));

            CreateButton(
                "Stop Button",
                Color.white,
                GetString("Stop"),
                new Vector3(1.3f, -1.88f, -16f),
                new Action(StopPlay));

            CreateButton(
                "Next Page Button",
                Color.white,
                GetString("NextPage"),
                new Vector3(-1.3f, -1.33f, -16f),
                new Action(() =>
                {
                    currentPage++;
                    if (currentPage > CurrectPageCount)
                    {
                        currentPage = 1;
                    }
                    RefreshTagList();
                }),
                CurrectPageCount > 1);

            CreateButton(
                "Switch Only Exist Button",
                Color.white,
                GetString("SwitchOnlyExist"),
                new Vector3(1.3f, -1.33f, -16f),
                new Action(() =>
                {
                    OnlyExist = !OnlyExist;
                    RefreshTagList();
                }));
            var helpText = Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement.Text, CustomBackground.transform);
            helpText.name = "Help Text";
            helpText.transform.localPosition = new(-1.25f, -2.15f, -15f);
            helpText.transform.localScale = new(1f, 1f, 1f);
            var helpTextTMP = helpText.GetComponent<TextMeshPro>();
            helpTextTMP.text = GetString("CustomSoundHelp");
            helpText.gameObject.GetComponent<RectTransform>().sizeDelta = new(2.45f, 1f);
        }

        RefreshTagList();

        void CreateButton( string name, Color color, string text, Vector3 position, Action action, bool active = true, Action onupdate = null)
        {
            var btn = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            btn.transform.localPosition = position;
            btn.name = name;
            btn.Text.text = text;
            btn.Background.color = color;
            var btnPassiveButton = btn.GetComponent<PassiveButton>();
            btnPassiveButton.OnClick = new();
            btnPassiveButton.OnClick.AddListener(action);
            btn.gameObject.SetActive(active);
            if (btn.name == "Switch Only Exist Button")
            onupdate = new Action(() =>
            {
                if (OnlyExist)
                    btn.Background.color = Color.cyan;
                else
                    btn.Background.color = Color.green;
            });
            UpdateButton.Add(onupdate);
        }
    }

    public static void RefreshTagList()
    {
        try
        {

            UpdateButton.Where(x => x != null).Do(x => x());
        }
        catch { }
        Items?.Do(Object.Destroy);
        Items = new();
        numItems = 0;
        var optionsMenuBehaviour = OptionsMenuBehaviourNow;

        int startIndex = (currentPage - 1) * ItemsPerPage;

        int count = 0;
        if (OnlyExist)
        foreach (var audio in fracturedSounds.Where(x => x.AudioType == AudioType.Music && x.LastAudioStates == AudiosStates.Exist).Skip(startIndex))
        {
            if (count >= ItemsPerPage)
            {
                break;
            }

            RefreshTags(optionsMenuBehaviour, audio);
            count++;
        }
        else
        foreach (var audio in fracturedSounds.Where(x => x.AudioType == AudioType.Music).Skip(startIndex))
        {
            if (count >= ItemsPerPage)
            {
                break;
            }

            RefreshTags(optionsMenuBehaviour, audio);
            count++;
        }
    }
    static float OffsetX => numItems % 2 == 0 ? -1.3f : 1.3f;
    static float OffsetY => 2.2f - 0.5f * (numItems / 2);
    static float OffsetZ => -6f;
    static void RefreshTags(OptionsMenuBehaviour optionsMenuBehaviour, FracturedSounds audio)
    {
        try
        {
            var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
            var name = audio.Name;
            var path = audio.Path;
            var filename = audio.FileName;
            var author = audio.Author;
            var audioExist = audio.CurrectAudioStates is not AudiosStates.NotExist;


            var ToggleButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            ToggleButton.transform.localPosition = new Vector3(OffsetX, OffsetY, OffsetZ);
            ToggleButton.name = "Btn-" + filename;
            ToggleButton.Text.text = $"{name}{(author != string.Empty ? $" -{author}" : "")}";
            ToggleButton.Background.color = Color.white;
            numItems++;

            var previewText = Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement.Text, CustomBackground.transform);
            previewText.transform.localPosition = new Vector3(OffsetX, OffsetY, OffsetZ);
            previewText.fontSize = ToggleButton.Text.fontSize;
            previewText.name = "PreText-" + filename;

            Color color;
            string preview;

            if (audio.CurrectAudioStates is AudiosStates.IsPlaying)
            {
                preview = GetString("Playing");
                color = ColorHelper.OutColor;
            }
            else if (audioExist)
            {
                color = Color.cyan;
                preview = GetString("CanPlay");
            }
            else
            {
                color = Palette.DisabledGrey;
                preview = GetString("NoFound");
            }


            previewText.text = preview;
            ToggleButton.Background.color = color;
            var passiveButton = ToggleButton.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener(new Action(OnClick));
            void OnClick()
            {
                Logger.Info($"Try To Play {filename}:{path}", "MyMusic");
                Play(audio, PlayType.System_Loop);
            }
            numItems++;
            Logger.Test($"{numItems}");
            Items.Add(ToggleButton.gameObject);
            Items.Add(previewText.gameObject);
        }
        catch { }

    }

}
