using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using System.Collections.Generic;
using static FracturedTruth.Translator;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using FracturedTruth.Attributes;
using static FracturedTruth.Helpers.PathHelper;
using Il2CppSystem.Linq;

namespace FracturedTruth.Common.ClientFeatures.ResourcesManager;

public static class VersionChecker
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPriority(Priority.LowerThanNormal)]
    public class Start
    {
        public static void Postfix()
        {
            CustomPopup.Init();
            if (!isChecked && firstStart) CheckForUpdate();
            ModUpdater.SetUpdateButtonStatus();
            firstStart = false;
        }
    }


    private static IReadOnlyList<string> GetInfoFileUrlList()
    {
        var list = URLs_info.ToList();
        if (IsChineseUser) list.Reverse();
        return list;
    }
    private static IReadOnlyList<string> GetResourcesFileUrlList()
    {
        var list = URLs_resources.ToList();
        if (IsChineseUser) list.Reverse();
        return list;
    }
    public static bool firstStart = true;

    public static bool hasUpdate = false;
    public static bool forceUpdate = false;
    public static bool isBroken = false;
    public static bool isChecked = false;
    public static bool DebugUnused = false;
    public static string versionInfoRaw = "";

    public static Version latestVersion = null;
    public static string showVer = "";
    public static Version DebugVer = null;
    public static bool CanUpdate = false;
    public static string verHead = "";
    public static string verDate = "";
    public static Version minimumVersion = null;
    public static int creation = 0;
    public static string md5 = "";
    public static bool IsSupported { get; private set; } = true;
    public static int visit => isChecked ? 216822 : 0;

    private static int retried = 0;
    private static bool firstLaunch = true;

    public static void Check()
    {
        var amongUsVersion = Version.Parse(Application.version);
        var lowestSupportedVersion = Version.Parse(Main.LowestSupportedVersion);
        IsSupported = amongUsVersion >= lowestSupportedVersion;
        if (!IsSupported)
        {
            ErrorText.Instance.AddError(ErrorCode.UnsupportedVersion);
        }
    }
    public static void Retry()
    {
        retried++;
        CustomPopup.Show(GetString("updateCheckPopupTitle"), GetString("PleaseWait"), null);
        _ = new LateTask(CheckForUpdate, 0.3f, "Retry Check Update");
    }
    [PluginModuleInitializer]
    public static void CheckForInit()
    {
        foreach (var url in GetInfoFileUrlList())
        {
            if (GetVersionInfo(url).GetAwaiter().GetResult())
            {
                break;
            }
        }
    }
    public static void CheckForUpdate()
    {
        isChecked = false;
        ModUpdater.DeleteOldFiles();

        foreach (var url in GetInfoFileUrlList())
        {
            if (GetVersionInfo(url).GetAwaiter().GetResult())
            {
                isChecked = true;
                break;
            }
        }
        foreach (var url in GetResourcesFileUrlList())
        {
            Logger.Test("try to");

            if (GetAllResources(url).GetAwaiter().GetResult())
            {
                Logger.Test("Find Resources");
                break;
            }
        }


        Logger.Msg("Check For Update: " + isChecked, "CheckRelease");
        isBroken = !isChecked;
        if (isChecked)
        {
            Logger.Info("Has Update: " + hasUpdate, "CheckRelease");
            Logger.Info("Latest Version: " + latestVersion.ToString(), "CheckRelease");
            Logger.Info("Minimum Version: " + minimumVersion.ToString(), "CheckRelease");
            Logger.Info("Creation: " + creation.ToString(), "CheckRelease");
            Logger.Info("Force Update: " + forceUpdate, "CheckRelease");
            Logger.Info("File MD5: " + md5, "CheckRelease");
            Logger.Info("Announcement (English): " + ModUpdater.announcement_en, "CheckRelease");
            Logger.Info("Announcement (SChinese): " + ModUpdater.announcement_zh, "CheckRelease");

            if (firstLaunch || isBroken)
            {
                firstLaunch = false;
                var annos = IsChineseUser ? ModUpdater.announcement_zh : ModUpdater.announcement_en;
                if (isBroken) CustomPopup.Show(GetString(StringNames.AnnouncementLabel), annos, new() { (GetString(StringNames.ExitGame), Application.Quit) });
                else CustomPopup.Show(GetString(StringNames.AnnouncementLabel), annos, new() { (GetString(StringNames.Okay), null) });
            }
        }
        else
        {
            if (retried >= 2) CustomPopup.Show(GetString("updateCheckPopupTitle"), GetString("updateCheckFailedExit"), new() { (GetString(StringNames.Okay), null) });
            else CustomPopup.Show(GetString("updateCheckPopupTitle"), GetString("updateCheckFailedRetry"), new() { (GetString("Retry"), Retry) });
        }

        ModUpdater.SetUpdateButtonStatus();
    }
    public static async Task<bool> GetVersionInfo(string url)
    {
        Logger.Msg(url, "CheckRelease");
        try
        {
            string result;
            if (url.StartsWith("file:///"))
            {
                result = File.ReadAllText(url[8..]);
            }
            else
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "FracturedTruth Updater");
                client.DefaultRequestHeaders.Add("Referer", "www.xtreme.net.cn");
                using var response = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseContentRead);
                if (!response.IsSuccessStatusCode || response.Content == null)
                {
                    Logger.Error($"Failed: {response.StatusCode}", "CheckRelease");
                    return false;
                }
                result = await response.Content.ReadAsStringAsync();
                result = result.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
            }

            JObject data = JObject.Parse(result);

            verHead = new(data["verHead"]?.ToString());

            DebugVer = new(data["DebugVer"]?.ToString());


            CanUpdate = bool.Parse(new(data["CanUpdate"]?.ToString()));



            verDate = new(data["verDate"]?.ToString());
            md5 = data["md5"]?.ToString();
            latestVersion = new(data["version"]?.ToString());

            showVer = $"{verHead}_{verDate}";

            var minVer = data["minVer"]?.ToString();
            minimumVersion = minVer.ToLower() == "latest" ? latestVersion : new(minVer);
            creation = int.Parse(data["creation"]?.ToString());
            isBroken = data["allowStart"]?.ToString().ToLower() != "true";

            JObject announcement = data["announcement"].Cast<JObject>();
            ModUpdater.announcement_en = announcement["English"]?.ToString();
            ModUpdater.announcement_zh = announcement["SChinese"]?.ToString();

            JObject downloadUrl = data["url"].Cast<JObject>();

            hasUpdate = Main.version < latestVersion;
            forceUpdate = Main.version < minimumVersion || creation > Main.PluginCreation;
#if DEBUG
            DebugUnused = Main.version < DebugVer;
            hasUpdate = forceUpdate = DebugUnused;
#endif

            return true;
        }
        catch
        {
            return false;
        }
    }
    public static Dictionary<string, JArray> AllResources = new();
    public static async Task<bool> GetAllResources(string url)
    {
        try
        {
            Logger.Test("0");
            string result;
            if (url.StartsWith("file:///"))
            {
                result = File.ReadAllText(url[8..]);
            }
            else
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "FracturedTruth Updater");
                client.DefaultRequestHeaders.Add("Referer", "www.xtreme.net.cn");
                using var response = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseContentRead);
                if (!response.IsSuccessStatusCode || response.Content == null)
                {
                    Logger.Error($"Failed: {response.StatusCode}", "CheckRelease");
                    return false;
                }
                result = await response.Content.ReadAsStringAsync();
                result = result.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
            }
            Logger.Test(result);
            Logger.Test("1");
            JObject data = JObject.Parse(result);
            Logger.Test("2");
            foreach (var property in data.Properties().ToList())
            {
                Logger.Test("3");
                if (property.Value.Type == JTokenType.Array)
                {
                    JArray arrayValue = new JArray(property.Value.Children());
                    AllResources.Add(property.Name, arrayValue);
                }
            }
            Logger.Test("4");


            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Exception: {ex.Message}", "CheckRelease");
            return false; 
        }
    }


    }