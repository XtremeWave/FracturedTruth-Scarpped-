using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

[HarmonyPatch(typeof(StarGen))]
public class StarGenPatch
{

    [HarmonyPatch(nameof(StarGen.Update))]
    [HarmonyPatch(nameof(StarGen.Start))]
    [HarmonyPostfix]
    public static void Update(StarGen __instance)
    {
        if (__instance.mesh == null || __instance.stars == null) return;

        __instance.mesh.RecalculateBounds();

    }
}
