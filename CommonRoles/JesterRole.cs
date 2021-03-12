using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.IL2CPP;
using CrowdedRoles.Attributes;
using CrowdedRoles.Extensions;
using CrowdedRoles.GameOverReasons;
using CrowdedRoles.Roles;
using HarmonyLib;
using InnerNet;
using Reactor.Extensions;
using UnityEngine;

namespace CommonRoles
{
    [RegisterCustomRole]
    public class JesterRole : BaseRole
    {
        public JesterRole(BasePlugin plugin) : base(plugin)
        {
        }

        public override string Name { get; } = "Jester";
        public override Color Color { get; } = Color.magenta;
        public override Team Team { get; } = Team.Alone;

        public override string Description { get; } = $"Trick crewmates to [{Color.magenta.ToHtmlStringRGBA()}]get voted out";
        public override RevealRole RevealExiledRole { get; } = RevealRole.Always;

        public override void AssignTasks(PlayerTaskList taskList, List<NormalPlayerTask> defaultTasks)
        {
            taskList.AddStringTask($"[{Color.magenta.ToHtmlStringRGBA()}]You're the Jester! Get voted out to win\n");
            taskList.AddStringTask("Fake tasks:");
            taskList.TaskCompletion = TaskCompletion.Fake;
            base.AssignTasks(taskList, defaultTasks);
        }
    }

    [RegisterCustomGameOverReason]
    public class JesterWon : CustomGameOverReason
    {
        public JesterWon(BasePlugin plugin) : base(plugin)
        {
        }
        
        public static GameData.PlayerInfo? Winner { get; internal set; }

        public override Color GetWinTextColor(bool _) => Color.magenta;

        public override Color GetBackgroundColor(bool _) => Color.magenta;

        public override string Name { get; } = "Jester";
        public override string WinText { get; } = "Tricked";

        public override IEnumerable<GameData.PlayerInfo> Winners =>
            Winner != null
                ? new[] { Winner }
                // if for some reason Winner is still not set, display all Jesters
                : GameData.Instance.AllPlayers.ToArray().Where(p => p.Is<JesterRole>());
    }

    internal static class MeetingHud_VotingComplete
    {
        private static void Postfix(MeetingHud __instance)
        {
            if (__instance.exiledPlayer?.Is<JesterRole>() ?? false)
            {
                JesterWon.Winner = __instance.exiledPlayer;
            }
            // separated to 2 patches because some players don't set Winner at the moment where
            // they get EndGame rpc
        }
    }
    
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Method_24))]
    internal static class ExileController_WrapUp
    {
        private static bool Prefix(ExileController __instance)
        {
            bool isJester = __instance.exiled?.Is<JesterRole>() ?? false;
            if (isJester && AmongUsClient.Instance.AmHost)
            {
                PlayerControl.LocalPlayer.RpcCustomEndGame<JesterWon>();
            }

            return !isJester; // don't make jester dead (why not)
        }
    }

    [HarmonyPatch]
    internal static class ResetWinnerPatches
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(EndGameManager).GetMethod("BPHOLDGAGHI")!; // SetEverythingUp
            yield return typeof(InnerNetClient).GetMethod("FOOLFJHCEOE")!; // OnDisconnected
        }
        private static void Postfix()
        {
            JesterWon.Winner = null;
        }
    }
}