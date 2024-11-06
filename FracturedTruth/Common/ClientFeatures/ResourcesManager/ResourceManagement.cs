using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FracturedTruth.Helpers.PathHelper;
using static FracturedTruth.Translator;
using Object = UnityEngine.Object;
using AmongUs.HTTP;
using System.IO;
using static FracturedTruth.Common.ClientFeatures.ResourcesManager.VersionChecker;
using FracturedTruth.Common.Modules;
using FracturedTruth.Common.Panels.Audio;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace FracturedTruth.Common.ClientFeatures.ResourcesManager;

public static class ResourceManagement
{
    public static SpriteRenderer CustomBackground { get; private set; }
    public static GameObject Slider { get; private set; }
    public static Dictionary<string, GameObject> Items { get; private set; }
    public static bool FirstStart = true;
    static int numItems = 0;
    public static void Hide()
    {
        if (CustomBackground != null)
            CustomBackground?.gameObject?.SetActive(false);
    }
    public static void Init(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
        if (!GameStates.IsNotJoined) return;

        if (CustomBackground == null)
        {
            numItems = 0;
            CustomBackground = Object.Instantiate(optionsMenuBehaviour.Background, optionsMenuBehaviour.transform);
            CustomBackground.name = "Resource Management";
            CustomBackground.transform.localScale = new(0.9f, 0.9f, 1f);
            CustomBackground.transform.localPosition += Vector3.back * 18;
            CustomBackground.gameObject.SetActive(false);

            var closeButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            closeButton.transform.localPosition = new(1.3f, -2.43f, -16f);
            closeButton.name = "Close";
            closeButton.Text.text = GetString("Close");
            closeButton.Background.color = Palette.DisabledGrey;
            var closePassiveButton = closeButton.GetComponent<PassiveButton>();
            closePassiveButton.OnClick = new();
            closePassiveButton.OnClick.AddListener(new Action(() =>
            {
                CustomBackground.gameObject.SetActive(false);
            }));

            var helpText = Object.Instantiate(CustomPopup.InfoTMP.gameObject, CustomBackground.transform);
            helpText.name = "Help Text";
            helpText.transform.localPosition = new(-1.25f, -2.15f, -15f);
            helpText.transform.localScale = new(1f, 1f, 1f);
            var helpTextTMP = helpText.GetComponent<TextMeshPro>();
            helpTextTMP.text = GetString("CustomResourceManagementHelp");
            helpText.gameObject.GetComponent<RectTransform>().sizeDelta = new(2.45f, 1f);

            var sliderTemplate = AccountManager.Instance.transform.FindChild("MainSignInWindow/SignIn/AccountsMenu/Accounts/Slider").gameObject;
            if (sliderTemplate != null && Slider == null)
            {
                Slider = Object.Instantiate(sliderTemplate, CustomBackground.transform);
                Slider.name = "Slider";
                Slider.transform.localPosition = new Vector3(0f, 0.5f, -11f);
                Slider.transform.localScale = new Vector3(1f, 1f, 1f);
                Slider.GetComponent<SpriteRenderer>().size = new(5f, 4f);
                var scroller = Slider.GetComponent<Scroller>();
                scroller.ScrollWheelSpeed = 0.3f;
                var mask = Slider.transform.FindChild("Mask");
                mask.transform.localScale = new Vector3(4.9f, 3.92f, 1f);
            }
            if (FirstStart)
                foreach (var resource in AllResources)
                {
                    var filepath = filestatePath + resource.Key;

                    File.Delete(filepath + "_downloading");
                }
            FirstStart = false;

        }

        RefreshTagList();
    }
    public static void RefreshTagList()
    {
        if (!GameStates.IsNotJoined) return;
        numItems = 0;
        var scroller = Slider.GetComponent<Scroller>();
        scroller.Inner.gameObject.ForEachChild((Action<GameObject>)DestroyObj);
        static void DestroyObj(GameObject obj)
        {
            if (obj.name.StartsWith("AccountButton")) Object.Destroy(obj);
        }

        var numberSetter = AccountManager.Instance.transform.FindChild("DOBEnterScreen/EnterAgePage/MonthMenu/Months").GetComponent<NumberSetter>();
        var buttonPrefab = numberSetter.ButtonPrefab.gameObject;

        Items?.Values?.Do(Object.Destroy);
        Items = new();
        foreach (var resource in AllResources)
        {
            numItems++;
            var resourcepackagename = resource.Key;

            var button = Object.Instantiate(buttonPrefab, scroller.Inner);
            button.transform.localPosition = new(-1f, 1.6f - 0.6f * numItems, -10.5f);
            button.transform.localScale = new(1.2f, 1.2f, 1.2f);
            button.name = "Btn-" + resourcepackagename;

            var renderer = button.GetComponent<SpriteRenderer>();
            var rollover = button.GetComponent<ButtonRolloverHandler>();

            var previewText = Object.Instantiate(button.transform.GetChild(0).GetComponent<TextMeshPro>(), button.transform);
            previewText.transform.SetLocalX(1.9f);
            previewText.fontSize = 1f;
            previewText.name = "PreText-" + resourcepackagename;


            Object.Destroy(button.GetComponent<UIScrollbarHelper>());
            Object.Destroy(button.GetComponent<NumberButton>());

            string buttontext;
            Color buttonColor;
            bool enable = true;
            string preview = "???";
            var remoteResourcesUrl = IsChineseLanguageUser ? packageDownloader_objectstorage : packageDownloader_github;
            remoteResourcesUrl = remoteResourcesUrl.Replace("{{packagename}}", resourcepackagename);
            var filepath = filestatePath + resourcepackagename;


            List<string> stringItems = new();
            var jArray = resource.Value;
            for (int i = 0; i < jArray.Count; i++)
            {
                stringItems.Add(jArray[i].ToString());
            }

            var exist = File.Exists(filepath + "_exist");
            var downloading = File.Exists(filepath + "_downloading");
            if (downloading)
            {
                buttontext = GetString("downloadInProgress");
                buttonColor = Color.yellow;
                enable = false;
            }
            else if (exist)
            {
                buttontext = GetString("PackageExists");
                buttonColor = Color.cyan;
                enable = false;
            }
            else
            {
                buttontext = GetString("download");
                buttonColor = Color.green;
            }
            preview = resourcepackagename;


            var passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener(new Action(() =>
            {
                File.Create(filepath + "_downloading").Close();
                RefreshTagList();

                List<Task<bool>> tasks = new();
                foreach (var item in stringItems)
                {
                    var task = ResourcesDownloader.StartDownload(remoteResourcesUrl, "", item);
                    tasks.Add(task);
                }
                var t = new Task(async () =>
                {
                    await Task.WhenAll(tasks);

                    File.Delete(filepath + "_downloading");
                    if (tasks.All(x => x.IsCompletedSuccessfully && x.Result))
                    {
                        File.Create(filepath + "_exist").Close();
                    }

                    new LateTask(() =>
                    {
                        RefreshTagList();
                        MyMusic.RefreshTagList();
                    }, 0.01f);
                });

                t.Start();

            }));

            button.transform.GetChild(0).GetComponent<TextMeshPro>().text = buttontext;
            rollover.OutColor = renderer.color = buttonColor;
            button.GetComponent<PassiveButton>().enabled = enable;
            previewText.text = preview;
            Items.Add(resourcepackagename, button);
        }

        scroller.SetYBoundsMin(0f);
        scroller.SetYBoundsMax(0.6f * numItems);
    }
}
