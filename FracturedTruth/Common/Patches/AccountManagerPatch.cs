﻿using HarmonyLib;
using UnityEngine;
using System.Collections;
using UnityEngine.Animations;
using BepInEx.Unity.IL2CPP.Utils;

namespace FracturedTruth.Common.Patches;

[HarmonyPatch(typeof(AccountTab), nameof(AccountTab.Awake))]
public static class AwakeFriendCodeUIPatch
{
    public static GameObject FriendsButton;
    public static void Prefix(AccountTab __instance)
    {
        var BarSprit = GameObject.Find("BarSprite");
        if (BarSprit)
        {
            GameObject CustomBarSprit = new();
            CustomBarSprit.transform.SetParent(BarSprit.transform.parent);
            CustomBarSprit.transform.localScale = BarSprit.transform.localScale;
            CustomBarSprit.transform.localPosition = BarSprit.transform.localPosition;

            void ResetParent(GameObject obj)
            {
                obj.transform.SetParent(CustomBarSprit.transform);
            }
            BarSprit.ForEachChild((Il2CppSystem.Action<GameObject>)ResetParent);
            BarSprit.SetActive(false);

        }
        void ApplyColor(GameObject obj)
        {
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr == null)
                obj.ForEachChild((Il2CppSystem.Action<GameObject>)ApplyColor);
            var color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f);
            sr.color = color;
        }
        var newRequest = GameObject.Find("NewRequest");
        if (newRequest != null)
        {
            newRequest.transform.localPosition -= new Vector3(0f, 0f, 10f);
            newRequest.transform.localScale = new Vector3(0.8f, 1f, 1f);
            newRequest.ForEachChild((Il2CppSystem.Action<GameObject>)ApplyColor);
        }

        FriendsButton = GameObject.Find("FriendsButton");

    }
}
[HarmonyPatch(typeof(AccountManager), nameof(AccountManager.Awake))]
public static class AwakeAccountManager
{
    public static Sprite[] AllRoleCharacterIllustration = {
        Utils.LoadSprite("CI_Crewmate.png", 450f),
        Utils.LoadSprite("CI_HnSEngineer.png", 450f),
        Utils.LoadSprite("CI_Engineer.png", 450f),
        Utils.LoadSprite("CI_GuardianAngel.png", 450f),
        Utils.LoadSprite("CI_Scientist.png", 450f),
        Utils.LoadSprite("CI_Tracker.png", 450f),
        Utils.LoadSprite("CI_Noisemaker.png", 450f),
        Utils.LoadSprite("CI_CrewmateGhost.png", 450f),
        Utils.LoadSprite("CI_Impostor.png", 450f),
        Utils.LoadSprite("CI_HnSImpostor.png", 450f),
        Utils.LoadSprite("CI_Shapeshifter.png", 450f),
        Utils.LoadSprite("CI_Phantom.png", 450f),
        Utils.LoadSprite("CI_ImpostorGhost.png", 450f),
    };
    private static int currentIndex = 0;

    static GameObject crewpet_walk0001;
    static GameObject ModLoading;
    public static void Prefix(AccountManager __instance)
    {
        var loading = GameObject.Find("Loading");
        loading.SetActive(false);

        var bgf = GameObject.Find("BackgroundFill");
        crewpet_walk0001 = bgf.transform.FindChild("crewpet_walk0001").gameObject;
        var r = crewpet_walk0001.GetComponent<WaitingRotate>();
        r.speed = 0f;
        ModLoading = new GameObject("ModLoading");
        ModLoading.transform.SetParent(crewpet_walk0001.transform.parent);
        ModLoading.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
        ModLoading.transform.localPosition = new Vector3(4.5f, -2.4f, -1f);
        var Sprite = ModLoading.AddComponent<SpriteRenderer>();
        Sprite.color = Color.white;
        Sprite.flipX = false;
        __instance.StartCoroutine(SwitchCharacterIllustration(Sprite));
        crewpet_walk0001.SetActive(false);



    }
    public static IEnumerator SwitchCharacterIllustration(SpriteRenderer spriter)
    {
        while (true)
        {
            if (AllRoleCharacterIllustration.Length == 0) yield break;

            spriter.sprite = AllRoleCharacterIllustration[currentIndex];
            var p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                float alpha = 1 - p;
                spriter.color = Color.white.AlphaMultiplied(alpha);
                yield return null;
            }
            currentIndex = (currentIndex + 1) % AllRoleCharacterIllustration.Length;


            yield return new WaitForSeconds(1f);
            p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                spriter.color = Color.white.AlphaMultiplied(p);
                yield return null;
            }
        }
    }
}
