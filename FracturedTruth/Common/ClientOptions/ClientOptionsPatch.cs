using HarmonyLib;
using UnityEngine;
using FracturedTruth.Modules.Managers;
using FracturedTruth.Helpers;
using FracturedTruth.Common.Modules;
using FracturedTruth.Common.Panels.Audio;
using FracturedTruth.Common.ClientFeatures.ResourcesManager;

namespace FracturedTruth.Common.ClientOptions;

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
public static class OptionsMenuBehaviourStartPatch
{
    private static ClientOptionItem_Boolean UnlockFPS;
    private static ClientOptionItem_String AprilFoolsMode;
    private static ClientOptionItem_Boolean KickPlayerFriendCodeNotExist;
    private static ClientOptionItem_Boolean ApplyDenyNameList;
    private static ClientOptionItem_Boolean ApplyBanList;
    private static ClientOptionItem_Boolean AutoStartGame;
    private static ClientOptionItem_Boolean AutoEndGame;
    private static ClientOptionItem_Boolean DisableVanillaSound;
    private static ClientOptionItem_Boolean AllowJoinVanilla;
    private static ClientActionItem UnloadMod;
    private static ClientActionItem DumpLog;
    private static ClientOptionItem_Boolean VersionCheat;
    private static ClientOptionItem_Boolean GodMode;

    private static ClientFeatureItem MyMusicBtn;
    private static ClientFeatureItem ResourceManagementBtn;

    private static bool reseted = false;
    public static void Postfix(OptionsMenuBehaviour __instance)
    {
        if (__instance.DisableMouseMovement == null) return;



        if (!reseted || !DebugModeManager.AmDebugger)
        {
            reseted = true;
            Main.VersionCheat.Value = false;
            Main.GodMode.Value = false;
        }



        if (UnlockFPS == null || UnlockFPS.ToggleButton == null)
        {
            UnlockFPS = ClientOptionItem_Boolean.Create("UnlockFPS", Main.UnlockFPS, __instance, UnlockFPSButtonToggle);
            static void UnlockFPSButtonToggle()
            {
                Application.targetFrameRate = Main.UnlockFPS.Value ? 165 : 60;
                Logger.SendInGame(string.Format(Translator.GetString("FPSSetTo"), Application.targetFrameRate));
            }
        }
        if (AprilFoolsMode == null || AprilFoolsMode.ToggleButton == null)
        {
            AprilFoolsMode = ClientOptionItem_String.Create(
                Main.AprilFoolsMode.Value ?? Main.allAprilFoolsModes[0]

                , Main.AprilFoolsMode, __instance, Main.allAprilFoolsModes, SwitchHorseMode);
            static void SwitchHorseMode()
            {
                AprilFoolsMode.UpdateToggle(Main.allAprilFoolsModes);
                if (Main.AprilFoolsMode.Value == Main.allAprilFoolsModes[1])
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        pc.MyPhysics.SetBodyType(pc.BodyType);
                        if (pc.BodyType == PlayerBodyTypes.Normal)
                        {
                            pc.cosmetics.currentBodySprite.BodySprite.transform.localScale = new(0.5f, 0.5f, 1f);
                        }
                    }

            }
            if (!GameStates.IsNotJoined)
            {
                AprilFoolsMode.ToggleButton.GetComponent<PassiveButton>().enabled = false;
                AprilFoolsMode.ToggleButton.Background.color = Palette.DisabledGrey;
                AprilFoolsMode.ToggleButton.Text.text = Translator.GetString("ChangeOutfit") + "|" + Translator.GetString("OnlyAvailableInMainMenu");
            }
            else
            {
                AprilFoolsMode.UpdateToggle(Main.allAprilFoolsModes);
                AprilFoolsMode.UpdateName(Main.AprilFoolsMode.Value);
                AprilFoolsMode.ToggleButton.GetComponent<PassiveButton>().enabled = true;
            }
        }

        if (KickPlayerFriendCodeNotExist == null || KickPlayerFriendCodeNotExist.ToggleButton == null)
        {
            KickPlayerFriendCodeNotExist = ClientOptionItem_Boolean.Create("KickPlayerFriendCodeNotExist", Main.KickPlayerFriendCodeNotExist, __instance);
        }
        if (ApplyBanList == null || ApplyBanList.ToggleButton == null)
        {
            ApplyBanList = ClientOptionItem_Boolean.Create("ApplyBanList", Main.ApplyBanList, __instance);
        }
        if (ApplyDenyNameList == null || ApplyDenyNameList.ToggleButton == null)
        {
            ApplyDenyNameList = ClientOptionItem_Boolean.Create("ApplyDenyNameList", Main.ApplyDenyNameList, __instance);
        }

        if (DisableVanillaSound == null || DisableVanillaSound.ToggleButton == null)
        {
            DisableVanillaSound = ClientOptionItem_Boolean.Create("DisableVanillaSound", Main.DisableVanillaSound, __instance, () =>
            {
                if (Main.DisableVanillaSound.Value)
                    AudioManager.StopPlay();
            });
        }

        if (AutoStartGame == null || AutoStartGame.ToggleButton == null)
        {
            AutoStartGame = ClientOptionItem_Boolean.Create("AutoStartGame", Main.AutoStartGame, __instance, AutoStartButtonToggle);
            static void AutoStartButtonToggle()
            {
                if (Main.AutoStartGame.Value == false && GameStates.IsCountDown)
                {
                    GameStartManager.Instance.ResetStartState();
                }
            }
        }
        if (AutoEndGame == null || AutoEndGame.ToggleButton == null)
        {
            AutoEndGame = ClientOptionItem_Boolean.Create("AutoEndGame", Main.AutoEndGame, __instance);
        }
        

        if (UnloadMod == null || UnloadMod.ToggleButton == null)
        {
            UnloadMod = ClientActionItem.Create("UnloadMod", ModUnloaderScreen.Show, __instance);
        }
        if (DumpLog == null || DumpLog.ToggleButton == null)
        {
            DumpLog = ClientActionItem.Create("DumpLog", () => Utils.DumpLog(), __instance);
        }
        if (AllowJoinVanilla == null || AllowJoinVanilla.ToggleButton == null)
        {
            AllowJoinVanilla = ClientOptionItem_Boolean.Create("AllowJoinVanilla", Main.AllowJoinVanilla, __instance);
        }
        if ((VersionCheat == null || VersionCheat.ToggleButton == null) && DebugModeManager.AmDebugger)
        {
            VersionCheat = ClientOptionItem_Boolean.Create("VersionCheat", Main.VersionCheat, __instance);
        }

        if ((GodMode == null || GodMode.ToggleButton == null) && DebugModeManager.AmDebugger)
        {
            GodMode = ClientOptionItem_Boolean.Create("GodMode", Main.GodMode, __instance);
        }

        if (MyMusicBtn == null || MyMusicBtn.ToggleButton == null)
        {
            MyMusicBtn = ClientFeatureItem.Create("SoundOption", () =>
            {
                MyMusic.CustomBackground?.gameObject?.SetActive(true);
            }, __instance);
        }

        if (ResourceManagementBtn == null || ResourceManagementBtn.ToggleButton == null)
        {
            ResourceManagementBtn = ClientFeatureItem.Create("SoundManager", () =>
            {
                ResourceManagement.CustomBackground?.gameObject?.SetActive(true);
            }, __instance);
        }

        MyMusicBtn.ToggleButton.Text.text = Translator.GetString("SoundOptions");
        MyMusicBtn.ToggleButton.GetComponent<PassiveButton>().enabled = true;
        MyMusicBtn.ToggleButton.Background.color = ColorHelper.ClientFeaColor;
        ResourceManagementBtn.ToggleButton.Text.text = Translator.GetString("AudioManagementOptions");
        ResourceManagementBtn.ToggleButton.GetComponent<PassiveButton>().enabled = true;
        ResourceManagementBtn.ToggleButton.Background.color = ColorHelper.ClientFeaColor;
        if (!GameStates.IsNotJoined)
        {
            ResourceManagementBtn.ToggleButton.Text.text = Translator.GetString("AudioManagementOptions") + "|" + Translator.GetString("OnlyAvailableInMainMenu");
            ResourceManagementBtn.ToggleButton.GetComponent<PassiveButton>().enabled = false;
            ResourceManagementBtn.ToggleButton.Background.color = Palette.DisabledGrey;
        }


        MyMusic.Init(__instance);
        ResourceManagement.Init(__instance);

        if (ModUnloaderScreen.Popup == null)
            ModUnloaderScreen.Init(__instance);

    }
}

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Close))]
public static class OptionsMenuBehaviourClosePatch
{
    public static void Postfix()
    {
        ClientActionItem.CustomBackground?.gameObject?.SetActive(false);
        ClientFeatureItem.CustomBackground?.gameObject?.SetActive(false);
        ModUnloaderScreen.Hide();
        MyMusic.Hide();
        ResourceManagement.Hide();
    }
}
