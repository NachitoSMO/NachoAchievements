using GameNetcodeStuff;
using HarmonyLib;
using Steamworks;
using System.Collections.Generic;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatches
    {
        public static List<int> moonsVisited = new List<int>();
        [HarmonyPatch(nameof(StartOfRound.ShipLeave))]
        [HarmonyPostfix]
        public static void OnShipLeave(StartOfRound __instance)
        {
            if (__instance.timeSinceRoundStarted < 60)
            {
                NachoAchievements.AddAchievement("chicken");
            }
        }

        [HarmonyPatch(nameof(StartOfRound.Start))]
        [HarmonyPostfix]
        private static void OnStart(StartOfRound __instance)
        {
            moonsVisited.Clear();
            moonsVisited = ES3.Load("NachoMoonsVisited", GameNetworkManager.Instance.currentSaveFileName, new List<int>());
        }

        [HarmonyPatch(nameof(StartOfRound.ShipHasLeft))]
        [HarmonyPostfix]
        private static void OnShipLeft(StartOfRound __instance)
        {
            NachoAchievements.Achievements["artFullClear"]["MinMaxing"] = 999;
            NachoAchievements.Achievements["artFullClear"]["progress"] = 0;
            NachoAchievements.Achievements["killEnemiesShotgun"]["progress"] = 0;
            if (RoundManagerPatches.sapsuckerEggsToday >= 1 && !__instance.allPlayersDead) NachoAchievements.AddAchievement("sapsuckerEggs");
            RoundManagerPatches.sapsuckerEggsToday = 0;

            int deadMates = 0;
            List<PlayerControllerB> alivePlayers = new List<PlayerControllerB>();
            foreach (var crewmate in __instance.allPlayerScripts)
            {
                if (crewmate.isPlayerDead) deadMates++;
                else alivePlayers.Add(crewmate);
            }

            if (deadMates >= 3 && alivePlayers.Contains(__instance.localPlayerController)) NachoAchievements.AddAchievement("clutch");

            NachoAchievements.Instance.StartCoroutine(NachoAchievements.Instance.CheckSingleRunProgress());
        }

        [HarmonyPatch(nameof(StartOfRound.SpawnUnlockable))]
        [HarmonyPostfix]
        private static void OnUnlockableSpawned(StartOfRound __instance)
        {
            NachoAchievements.Instance.StartCoroutine(NachoAchievements.Instance.CheckSingleRunProgress());
        }
    }

}
