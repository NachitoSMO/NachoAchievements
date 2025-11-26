using GameNetcodeStuff;
using HarmonyLib;
using Steamworks;
using System.Collections.Generic;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatches
    {
        [HarmonyPatch(nameof(StartOfRound.ShipLeave))]
        [HarmonyPostfix]
        public static void OnShipLeave(StartOfRound __instance)
        {
            if (__instance.timeSinceRoundStarted < 60)
            {
                NachoAchievements.AddAchievement("chicken");
            }
        }

        [HarmonyPatch(nameof(StartOfRound.ShipHasLeft))]
        [HarmonyPostfix]
        private static void WhenShipReady(StartOfRound __instance)
        {
            NachoAchievements.Achievements["artFullClear"]["MinMaxing"] = 999;
            NachoAchievements.Achievements["artFullClear"]["progress"] = 0;
            NachoAchievements.Achievements["killEnemiesShotgun"]["progress"] = 0;

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

        [HarmonyPatch(nameof(StartOfRound.Start))]
        [HarmonyPostfix]
        private static void OnGameStart()
        {
            NachoAchievements.Achievements["artFullClear"]["MinMaxing"] = 999;
            NachoAchievements.Achievements["artFullClear"]["progress"] = 0;
            NachoAchievements.Achievements["killEnemiesShotgun"]["progress"] = 0;

            List<string> keys = [.. NachoAchievements.Achievements.Keys];
            int totalAchievements = 0;

            foreach (string achievement in keys)
            {
                if (achievement == "getAll") continue;
                List<string> keys2 = [.. NachoAchievements.Achievements[achievement].Keys];

                foreach (string a in keys2)
                {
                    if (a == "progress") continue;
                    if (a == "completed") continue;
                    totalAchievements++;
                }
            }

            NachoAchievements.Achievements["getAll"]["Completionist"] = totalAchievements;

            NachoAchievements.AddItems();
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
