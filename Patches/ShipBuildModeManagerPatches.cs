using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(ShipBuildModeManager))]
    internal class ShipBuildModeManagerPatches
    {
        [HarmonyPatch(nameof(ShipBuildModeManager.PlaceShipObject))]
        [HarmonyPostfix]
        private static void OnPlaceShipObject(ShipBuildModeManager __instance)
        {
            NachoAchievements.Instance.StartCoroutine(NachoAchievements.Instance.CheckSingleRunProgress());
        }

        [HarmonyPatch(nameof(ShipBuildModeManager.StoreObject_performed))]
        [HarmonyPostfix]
        private static void OnStoreShipObject(ShipBuildModeManager __instance)
        {
            NachoAchievements.Instance.StartCoroutine(NachoAchievements.Instance.CheckSingleRunProgress());
        }

    }
}
