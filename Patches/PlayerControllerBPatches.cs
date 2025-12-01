using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatches
    {
        public static Dictionary<GrabbableObject, PlayerControllerB> playerDroppedBy = new Dictionary<GrabbableObject, PlayerControllerB>();
        [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPrefix]
        private static void OnPlayerDeath(PlayerControllerB __instance, CauseOfDeath causeOfDeath = CauseOfDeath.Unknown)
        {

            var cause = causeOfDeath;
            Dictionary<string, string> callback = new Dictionary<string, string>();
            callback.Add("callback", "On Death");
            callback.Add("cause", cause.ToString());
            callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
            callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
            callback.Add("local", (__instance == StartOfRound.Instance.localPlayerController).ToString());
            NachoAchievements.CheckAchievements(callback);
            
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
                callback.Add("local", (__instance == StartOfRound.Instance.localPlayerController).ToString());
                NachoAchievements.CheckAchievements(callback);
            }
        }

        [HarmonyPatch(nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPrefix]
        private static void OnPlaceObject(PlayerControllerB __instance)
        {
            playerDroppedBy[__instance.currentlyHeldObjectServer] = __instance;
        }

        [HarmonyPatch(nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        [HarmonyPostfix]
        private static void OnClientConnect(PlayerControllerB __instance)
        {
            NachoAchievements.Instance.GetAchievementsOnServerRpc(__instance.playerSteamId);
            Dictionary<string, string> callback = new Dictionary<string, string>();
            callback.Add("callback", "On Player Join");
            callback.Add("players", __instance.playersManager.allPlayerScripts.Length.ToString());
            NachoAchievements.CheckAchievements(callback);
        }
    }
}
