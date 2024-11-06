using AmongUs.GameOptions;
using FracturedTruth.Attributes;
using FracturedTruth.Common.Data;
using FracturedTruth.Common.Modules;
using FracturedTruth.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracturedTruth.Vanilla.Data;

public class VanillaPlayerData : IDisposable
{
    ///////////////PLAYER_INFO\\\\\\\\\\\\\\\
    public static Dictionary<byte, VanillaPlayerData> AllPlayerData;
    public PlayerControl Player { get; private set; }

    public string PlayerName { get; private set; }
    public int PlayerColor { get; private set; }

    public bool IsImpostor { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsDisconnected => RealDeathReason == VanillaDeathReason.Disconnect;


    public RoleTypes? RoleWhenAlive { get; private set; }
    public RoleTypes? RoleAfterDeath { get; private set; }
    public bool RoleAssgined { get; private set; }

    public VanillaDeathReason RealDeathReason { get; private set; }
    public VanillaPlayerData RealKiller { get; private set; }

    public int TotalTaskCount { get; private set; }
    public int CompleteTaskCount { get; private set; }
    public bool TaskCompleted => TotalTaskCount == CompleteTaskCount;
    public int KillCount { get; private set; }

    ///////////////\\\\\\\\\\\\\\\

    public VanillaPlayerData(
    PlayerControl player,
    string playername,
    int colorId)
    {
        Player = player;
        PlayerName = playername;
        PlayerColor = colorId;
        IsImpostor = IsDead = RoleAssgined = false;
        CompleteTaskCount = KillCount = TotalTaskCount = 0;
        RealDeathReason = VanillaDeathReason.None;
        RealKiller = null;
    }


    ///////////////FUNCTIONS\\\\\\\\\\\\\\\
    public static VanillaPlayerData GetPlayerDataById(byte id) => AllPlayerData[id] ?? null;
    public static PlayerControl GetPlayerById(byte id) => GetPlayerDataById(id).Player ?? Utils.GetPlayerById(id);
    public static string GetPlayerNameById(byte id) => GetPlayerDataById(id).PlayerName;

    public static RoleTypes GetRoleById(byte id) =>
        GetPlayerDataById(id).IsDead == true ?
        GetPlayerDataById(id).RoleAfterDeath ?? GetPlayerById(id).Data.Role.Role :
        GetPlayerDataById(id).RoleWhenAlive ?? GetPlayerById(id).Data.Role.Role;
    public static int GetLongestNameByteCount() => AllPlayerData.Values.Select(data => data.PlayerName.GetByteCount()).OrderByDescending(byteCount => byteCount).FirstOrDefault();


    public void SetDead() => IsDead = true;
    public void SetDisconnected()
    {
        SetDead();
        SetDeathReason(VanillaDeathReason.Disconnect);
    }
    public void SetIsImp(bool isimp) => IsImpostor = isimp;
    public void SetRole(RoleTypes role)
    {
        if (!RoleAssgined)
            RoleWhenAlive = role;
        else
            RoleAfterDeath = role;
        RoleAssgined = !GameStates.IsFreePlay;
    }
    public void SetDeathReason(VanillaDeathReason deathReason, bool focus = false)
    {
        if (IsDead && RealDeathReason == VanillaDeathReason.None || focus)
            RealDeathReason = deathReason;
    }
    public void SetRealKiller(VanillaPlayerData killer)
    {
        SetDead();
        SetDeathReason(VanillaDeathReason.Kill);
        killer.KillCount++;
        RealKiller = killer;
    }
    public void SetTaskTotalCount(int TaskTotalCount) => TotalTaskCount = TaskTotalCount;
    public void CompleteTask() => CompleteTaskCount++;



    [GameModuleInitializer]
    public static void InitializeAll()
    {
        AllPlayerData = new();
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            var colorId = pc.Data.DefaultOutfit.ColorId;
            var id = pc.PlayerId;
            AllPlayerData[id] = new VanillaPlayerData(pc, pc.GetRealName(), colorId);
        }
    }
#pragma warning disable CA1816
    public void Dispose()
    {
        if (GameStates.IsLobby) return;
        AllPlayerData.Remove(Player.PlayerId);
        Player = null;
        PlayerName = null;
        PlayerColor = -1;
        IsImpostor = IsDead = RoleAssgined = false;
        CompleteTaskCount = KillCount = TotalTaskCount = 0;
        RealDeathReason = VanillaDeathReason.None;
        RealKiller = null;
    }
#pragma warning restore CA1816
}
public enum VanillaDeathReason
{
    None,
    Exile,
    Kill,
    Disconnect
}