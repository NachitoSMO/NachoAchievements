using GameNetcodeStuff;
using HarmonyLib;
using Steamworks;
using System.Collections.Generic;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatches
    {
        [HarmonyPatch(nameof(StartOfRound.OnLocalDisconnect))]
        [HarmonyPostfix]
        private static void OnDisconnect(StartOfRound __instance)
        {
            NachoAchievements.ResetSingleDayAchievements();
        }

        [HarmonyPatch(nameof(StartOfRound.ShipHasLeft))]
        [HarmonyPostfix]
        private static void OnShipLeft(StartOfRound __instance)
        {
            NachoAchievements.ResetSingleDayAchievements();
        }
    }

}
