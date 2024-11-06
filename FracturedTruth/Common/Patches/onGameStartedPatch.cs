using AmongUs.GameOptions;
using FracturedTruth.Attributes;
using FracturedTruth.Vanilla.Patches;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace FracturedTruth.Common.Patches;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
internal class CoStartGamePatch
{
    public static void Postfix()
    {
        IntroCutsceneOnDestroyPatch.introDestroyed = false;
        GameModuleInitializerAttribute.InitializeAll();
    }

}