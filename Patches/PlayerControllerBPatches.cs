using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatches
    {
        private static Vector3 lastGroundedPos;
        [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPrefix]
        private static void OnPlayerDeath(PlayerControllerB __instance, CauseOfDeath causeOfDeath = CauseOfDeath.Unknown)
        {
            if (__instance == StartOfRound.Instance.localPlayerController)
            {
                var cause = causeOfDeath;
                Dictionary<string, string> callback = new Dictionary<string, string>();
                callback.Add("callback", "On Death");
                callback.Add("cause", cause.ToString());
                callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
                callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
                NachoAchievements.CheckAchievements(callback);
            }
        }

        [HarmonyPatch(nameof(PlayerControllerB.GrabObject))]
        [HarmonyPostfix]
        private static void OnGrabObject(PlayerControllerB __instance)
        {
            if (__instance.ItemSlots != null)
            {
                int weight = Mathf.RoundToInt(Mathf.Clamp(__instance.carryWeight - 1f, 0f, 100f) * 105f);
                Dictionary<string, string> callback = new Dictionary<string, string>();
                callback.Add("callback", "On Grab Object");
                callback.Add("weight", weight.ToString());
                callback.Add("scrap", __instance.currentlyGrabbingObject.itemProperties.itemName);
                callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
                callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
                NachoAchievements.CheckAchievements(callback);
            }
        }

        [HarmonyPatch(nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        [HarmonyPostfix]
        private static void OnClientConnect(PlayerControllerB __instance)
        {
            NachoAchievements.Instance.GetAchievementsOnServerRpc(__instance.playerSteamId);
        }
    }
}
