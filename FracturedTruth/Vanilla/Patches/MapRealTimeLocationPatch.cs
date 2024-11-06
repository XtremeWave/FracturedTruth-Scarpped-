using HarmonyLib;

namespace FracturedTruth.Vanilla.Patches;

[HarmonyPatch]
public class MapRealTimeLocationPatch
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap)), HarmonyPostfix]
    public static void ShowNormalMapAfter(MapBehaviour __instance)
    {
        var roleType = PlayerControl.LocalPlayer.Data.Role.Role;
        var color = Utils.GetRoleColor(roleType);
        __instance.ColorControl.SetColor(color);
    }
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap)), HarmonyPostfix]
    public static void ShowSabotageMapAfter(MapBehaviour __instance)
    {
        var color = Palette.DisabledGrey;
        __instance.ColorControl.SetColor(color);
    }
}