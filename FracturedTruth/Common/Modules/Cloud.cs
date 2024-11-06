﻿using AmongUs.Data;
using HarmonyLib;
using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using FracturedTruth.Attributes;
using FracturedTruth.Common.Modules;

namespace FracturedTruth;

internal class Cloud
{
    private static string IP;
    private static int LOBBY_PORT = 0;
    private static int EAC_PORT = 0;
    private static Socket ClientSocket;
    private static Socket EacClientSocket;
    private static long LastRepotTimeStamp = 0;

    [PluginModuleInitializer]
    public static void Init()
    {
        try
        {
            var content = GetResourcesTxt("FracturedTruth.Resources.Configs.Port.txt");
            string[] ar = content.Split('|');
            IP = ar[0];
            LOBBY_PORT = int.Parse(ar[1]);
            EAC_PORT = int.Parse(ar[2]);
        }
        catch (Exception e)
        {
            Logger.Exception(e, "Cloud Init");
        }
    }
    private static string GetResourcesTxt(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    public static string ServerName ="";
    public static bool ShareLobby(bool command = false)
    {
        try
        {
            if (!Main.NewLobby || !GameStates.IsLobby) return false;
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance || AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return false;

            if (IP == null || LOBBY_PORT == 0) throw new("Has no IP or port");
            
            string msg = $"{GameStartManager.Instance.GameRoomNameCode.text}|{Main.DisplayedVersion_Head}|{GameData.Instance.PlayerCount}|{TranslationController.Instance.currentLanguage.languageID}|{ServerName}|{DataManager.player.customization.name}";
            if (msg.Length <= 60)
            {
                byte[] buffer = Encoding.Default.GetBytes(msg);
                ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ClientSocket.Connect(IP, LOBBY_PORT);
                ClientSocket.Send(buffer);
                ClientSocket.Close();
            }
            Main.NewLobby = false; 

        }
        catch (Exception e)
        {
            Logger.Exception(e, "SentLobbyToQQ");
            throw;
        }
        return true;
    }

    private static bool connecting = false;
    public static void StartConnect()
    {
        if (connecting || EacClientSocket != null && EacClientSocket.Connected) return;
        connecting = true;
        _ = new LateTask(() =>
        {
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance || AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame)
            {
                connecting = false;
                return;
            }
            try
            {
                if (IP == null || EAC_PORT == 0) throw new("Has no IP or port");
                LastRepotTimeStamp = Utils.GetTimeStamp();
                EacClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                EacClientSocket.Connect(IP, EAC_PORT);
                Logger.Warn("已连接至FracturedTruth服务器", "EAC Cloud");
            }
            catch (Exception ex)
            {
                connecting = false;
                Logger.Error($"Connect To EAC Failed:\n{ex.Message}", "EAC Cloud", false);
            }
            connecting = false;
        }, 3.5f, "EAC Cloud Connect");
    }
    public static void StopConnect()
    {
        if (EacClientSocket != null && EacClientSocket.Connected)
            EacClientSocket.Close();
    }
    public static void SendData(string msg)
    {
        StartConnect();
        if (EacClientSocket == null || !EacClientSocket.Connected)
        {
            Logger.Warn("未连接至FracturedTruth服务器，报告被取消", "EAC Cloud");
            return;
        }
        EacClientSocket.Send(Encoding.Default.GetBytes(msg));
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class EACConnectTimeOut
    {
        public static void Postfix()
        {
            if (LastRepotTimeStamp != 0 && LastRepotTimeStamp + 8 < Utils.GetTimeStamp())
            {
                LastRepotTimeStamp = 0;
                StopConnect();
                Logger.Warn("超时自动断开与FracturedTruth服务器的连接", "EAC Cloud");
            }
        }
    }
}