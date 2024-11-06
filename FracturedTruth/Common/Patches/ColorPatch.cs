﻿using FracturedTruth.Helpers;
using HarmonyLib;
using UnityEngine;

namespace FracturedTruth.Common.Patches;

[HarmonyPatch(typeof(ButtonRolloverHandler))]
class ButtonRolloverHandlerPatch
{
    [HarmonyPatch(nameof(ButtonRolloverHandler.DoMouseOver)), HarmonyPrefix]
    public static void DoMouseOver_Prefix(ButtonRolloverHandler __instance)
    {
        if (__instance.OverColor == new Color(0, 1, 0, 1) || __instance.OverColor == Palette.AcceptedGreen)
            __instance.OverColor = ColorHelper.OutColor2;
    }
    [HarmonyPatch(nameof(ButtonRolloverHandler.ChangeOutColor)), HarmonyPrefix]
    public static void ChangeOutColor_Prefix(ButtonRolloverHandler __instance, ref Color color)
    {
        if (color.r == 0 && color.g == 1 && color.b is > 0.163f and < 0.165f && color.a == 1)
            color = ColorHelper.OutColor;
    }
}
[HarmonyPatch(typeof(Palette))]
class PalettePath
{
    [HarmonyPatch(nameof(Palette.AcceptedGreen), MethodType.Getter), HarmonyPrefix]
    public static bool Get_AcceptedGreen_Prefix(ref Color __result)
    {
        __result = new Color32(156, 153, 194, (byte)(__result.a * 255));
        return false;
    }
}