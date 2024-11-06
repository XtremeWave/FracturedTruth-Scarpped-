using BepInEx.Unity.IL2CPP.Utils;
using static FracturedTruth.Common.ClientFeatures.ResourcesManager.ResourcesDownloader;
using static FracturedTruth.Translator;
using System;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using ListStr = System.Collections.Generic.List<string>;
using DictionaryStr = System.Collections.Generic.Dictionary<string, string>;
using System.IO;
using System.Collections.Generic;
using static FracturedTruth.Helpers.PathHelper;
using FracturedTruth.Helpers;
using FracturedTruth.Common.Panels.Audio;

namespace FracturedTruth.Common.Patches;


public class LoadPatch
{
    static Sprite Team_Logo = Utils.LoadSprite("XtremeWave-Logo.png", 120f);
    static Sprite BG = Utils.LoadSprite("FracturedTruth-Loading-BG.png", 100f);
    static Sprite Mod_Logo = Utils.LoadSprite("FracturedTruth-Logo.png", 125f);
    static Sprite Mod_Logo_Blurred = Utils.LoadSprite("FracturedTruth-Logo-Blurred.png", 125f);
    static TMPro.TextMeshPro loadText = null!;
    static TMPro.TextMeshPro loading = null!;

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
    class Start
    {
        static bool Prefix(SplashManager __instance)
        {
            ResolutionManager.SetResolution(1920, 1080, true);
            __instance.startTime = Time.time;
            __instance.StartCoroutine(InitializeRefdata(__instance));
            return false;
        }
        static IEnumerator InitializeRefdata(SplashManager __instance)
        {
            #region 加载所需资源下载
            var LogoAnimator = GameObject.Find("LogoAnimator");
            LogoAnimator.SetActive(false);

            string remoteResourcesUrl;
            ListStr remoteInitFileList = new()
            {
                "FracturedTruth-Loading-BG.png",
                "FracturedTruth-Logo.png",
                "FracturedTruth-Logo-Blurred.png",
                "XtremeWave-Logo.png",
                "ModLoad.wav",
            };
            remoteResourcesUrl = IsChineseLanguageUser ? initfileDownloader_objectstorage : initfileDownloader_savepath;
            foreach (var resource in remoteInitFileList)
            {
                var task = StartDownload(remoteResourcesUrl, "", resource);
                while (!task.IsCompleted)
                {
                    yield return null;
                }

                if (task.IsFaulted)
                {
                    Logger.Error($"Download of {remoteResourcesUrl + resource} failed: {task.Exception}", "Download Resource");
                }

            }
            LogoAnimator.SetActive(true);
            #endregion
            #region 动画初始化
            var teamlogo = ObjectHelper.CreateObject<SpriteRenderer>("Team_Logo", null, new Vector3(0, 0f, -5f));
            teamlogo.sprite = Team_Logo;
            float p;
            teamlogo.color = Color.white.AlphaMultiplied(0);
            yield return new WaitForSeconds(4f);

            p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                float alpha = 1 - p;
                teamlogo.color = Color.white.AlphaMultiplied(alpha);
                yield return null;
            }
            yield return new WaitForSeconds(1.5f);

            p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                teamlogo.color = Color.white.AlphaMultiplied(p);
                yield return null;
            }
            yield return new WaitForSeconds(2f);

            var bg = ObjectHelper.CreateObject<SpriteRenderer>("bg", null, new Vector3(0, 0f, -4f));
            bg.sprite = BG;

            var modlogo = ObjectHelper.CreateObject<SpriteRenderer>("Mod_Logo", null, new Vector3(0, 0.4f, -5f));
            modlogo.sprite = Mod_Logo;

            var modlogo_Blurred = ObjectHelper.CreateObject<SpriteRenderer>("Mod_Logo_Blurred", null, new Vector3(0, 0.4f, -5f));
            modlogo_Blurred.sprite = Mod_Logo_Blurred;

            p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                float alpha = 1 - p;
                modlogo.color = Color.white.AlphaMultiplied(alpha);
                bg.color = Color.white.AlphaMultiplied(alpha);
                modlogo_Blurred.color = Color.white.AlphaMultiplied(Mathf.Min(1f, alpha * (p * 2)));
                modlogo.transform.localScale = Vector3.one * (p * p * 0.012f + 1f);
                modlogo_Blurred.transform.localScale = Vector3.one * (p * p * 0.012f + 1f);
                yield return null;
            }
            modlogo.color = Color.white;
            modlogo_Blurred.gameObject.SetActive(false);
            modlogo.transform.localScale = Vector3.one;
            loadText = UnityEngine.Object.Instantiate(__instance.errorPopup.InfoText, null);
            loadText.transform.localPosition = new(0f, -1.08f, -10f);
            loadText.fontStyle = TMPro.FontStyles.Bold;
            loadText.text = string.Empty;
            Action action = new (() => { AudioManager.Play(XtremeSounds.ModLoad, PlayType.System_Loop); });
            action();

            #endregion
            #region AmongUs自身的加载
            yield return DestroyableSingleton<ReferenceDataManager>.Instance.Initialize();
            try
            {
                DestroyableSingleton<TranslationController>.Instance.Initialize();
            }
            catch { }
            #endregion
            #region 校验依赖项
            ListStr remoteDependList = new()
            {
                "YamlDotNet.dll",
                "YamlDotNet.xml",
            };

            remoteResourcesUrl = IsChineseLanguageUser ? dependDownloader_objectstorage : dependDownloader_github;

            foreach (var resource in remoteDependList)
            {
                string localFilePath = dependDownloader_savepath;
                if (File.Exists(localFilePath + resource)) continue;

                var task = StartDownload(remoteResourcesUrl, localFilePath, resource);
                while (!task.IsCompleted)
                {
                    yield return null;
                }

                if (task.IsFaulted)
                {
                    Logger.Error($"Download of {remoteResourcesUrl} failed: {task.Exception}", "Download Resource");
                }

            }
            loading = UnityEngine.Object.Instantiate(__instance.errorPopup.InfoText, null);
            loading.transform.localPosition = new(0f, -0.68f, -10f);
            loading.fontStyle = TMPro.FontStyles.Bold;
            loading.text = GetString("Loading");
            loading.color = Color.white.AlphaMultiplied(0.6f);
            loadText.color = Color.white.AlphaMultiplied(0.6f);
            #endregion
            #region 资源校验
            loadText.text = GetString("CheckingForFiles");

            ListStr remoteImageList = new()
            {
                "CI_Crewmate.png",
                "CI_CrewmateGhost.png",
                "CI_Engineer.png",
                "CI_GuardianAngel.png",
                "CI_HnSEngineer.png",
                "CI_HnSImpostor.png",
                "CI_Impostor.png",
                "CI_ImpostorGhost.png",
                "CI_Noisemaker.png",
                "CI_Phantom.png",
                "CI_Scientist.png",
                "CI_Shapeshifter.png",
                "CI_Tracker.png",
                "DleksBanner.png",
                "DleksBanner-Wordart.png",
                "DleksButton.png",
                "FracturedTruth-BG.jpg",
                "RightPanelCloseButton.png",
            };
            remoteResourcesUrl = IsChineseLanguageUser ? imageDownloader_objectstorage : imageDownloader_github;

            ListStr needDownloadsPath = new();
            foreach (var resource in remoteImageList)
            {
                string localFilePath = imageDownloader_savepath;
                if (File.Exists(localFilePath + resource)) continue;

                needDownloadsPath.Add(resource);
                Logger.Warn($"File do not exists: {localFilePath}", "Check");

            }
            #endregion
            #region 缺失资源下载
            yield return new WaitForSeconds(0.5f);
            FileAttributes attributes = File.GetAttributes(imageDownloader_savepath);
            File.SetAttributes(imageDownloader_savepath, attributes | FileAttributes.Hidden);

            foreach (var resource in needDownloadsPath)
            {
                Color yellow = new Color32(252, 255, 152, 255);
                loadText.color = yellow.AlphaMultiplied(0.6f);
                loadText.text = GetString("DownloadingResources");
                var task = StartDownload(remoteResourcesUrl, imageDownloader_savepath, resource);
                while (!task.IsCompleted)
                {
                    yield return null;
                }

                if (task.IsFaulted)
                {
                    Logger.Error($"Download of {resource} failed: {task.Exception}", "Download Resource");
                }
            }
            yield return new WaitForSeconds(0.5f);

            #endregion
            #region 加载完成
            yield return new WaitForSeconds(1f);
            UnityEngine.Object.Destroy(loadText.gameObject);

            loading.color = Color.white.AlphaMultiplied(0.6f);
            loading.text = GetString("LoadingComplete");
            for (int i = 0; i < 3; i++)
            {
                loading.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.03f);
                loading.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.03f);
            }
            yield return new WaitForSeconds(0.5f);
            action = new Action(() => { AudioManager.StopPlay(); });
            action();
            UnityEngine.Object.Destroy(loading.gameObject);

            p = 1f;
            while (p > 0f)
            {
                bg.color = Color.white.AlphaMultiplied(p);
                p -= Time.deltaTime * 1.2f;
                modlogo.color = Color.white.AlphaMultiplied(p);
                yield return null;
            }
            modlogo.color = Color.clear;
            #endregion
            #region AmongUs自身的加载结束所需内容
            __instance.loadingObject.SetActive(false);
            __instance.sceneChanger.BeginLoadingScene();
            __instance.doneLoadingRefdata = true;
            #endregion
            yield break;
        }

    }
    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    class Update
    {
        static bool Prefix(SplashManager __instance)
        {
            if (__instance.doneLoadingRefdata && !__instance.startedSceneLoad && Time.time - __instance.startTime > __instance.minimumSecondsBeforeSceneChange)
            {
                __instance.sceneChanger.AllowFinishLoadingScene();
                __instance.startedSceneLoad = true;
            }
            return false;
        }
    }



}