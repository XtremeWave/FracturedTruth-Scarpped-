using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using FracturedTruth.Common.Modules;
using HarmonyLib;
using Il2CppSystem.Runtime.Remoting.Messaging;
using Innersloth.DebugTool;
using TMPro;
using UnityEngine;

namespace FracturedTruth.Common.Patches;

[HarmonyPatch(typeof(LobbyInfoPane), nameof(LobbyInfoPane.Update))]
class LobbyInfoPanePatch
{

    static void Postfix()
    {
        var AspectSize = GameObject.Find("AspectSize");
        AspectSize.transform.FindChild("Background").gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
        if (GameStates.MapIsActive(MapNames.Dleks))
            AspectSize.transform.FindChild("MapImage").gameObject.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite($"DleksBanner-Wordart.png", 160f);
    }

}
[HarmonyPatch]
class LobbyViewSettingsPanePatch
{
    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Awake)), HarmonyPostfix]
    static void Awake()
    {
        GameObject.Find("RulesPopOutWindow").transform.localPosition += Vector3.left * 0.4f;
    }
}