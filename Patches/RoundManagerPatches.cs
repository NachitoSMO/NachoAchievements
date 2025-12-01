using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatches
    {
        [HarmonyPatch(nameof(RoundManager.CollectNewScrapForThisRound))]
        [HarmonyPrefix]
        private static void AddScrapAchievements(RoundManager __instance, GrabbableObject scrapObject)
        {
            if (!scrapObject.scrapPersistedThroughRounds && !__instance.scrapCollectedThisRound.Contains(scrapObject))
            {
                GrabbableObject[] grabbableObjects = Object.FindObjectsOfType<GrabbableObject>();
                int count = 0;

                foreach (var grabbableObject in grabbableObjects)
                {
                    if (grabbableObject.itemProperties.itemName == scrapObject.itemProperties.itemName && StartOfRound.Instance.shipBounds.bounds.Contains(grabbableObject.gameObject.transform.position))
                        count++;
                }

                if (scrapObject.itemProperties.isScrap)
                {
                    Dictionary<string, string> callback = new Dictionary<string, string>();
                    callback.Add("callback", "On Scrap Collected");
                    callback.Add("scrap", scrapObject.itemProperties.itemName);
                    callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
                    callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
                    callback.Add("amount in ship", count.ToString());
                    callback.Add("local", (scrapObject.playerHeldBy == StartOfRound.Instance.localPlayerController).ToString());
                    NachoAchievements.CheckAchievements(callback);
                }
                else
                {
                    Dictionary<string, string> callback = new Dictionary<string, string>();
                    callback.Add("callback", "On Item Collected");
                    callback.Add("scrap", scrapObject.itemProperties.itemName);
                    callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
                    callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
                    callback.Add("amount in ship", count.ToString());
                    callback.Add("local", (scrapObject.playerHeldBy == StartOfRound.Instance.localPlayerController).ToString());
                    NachoAchievements.CheckAchievements(callback);
                }
            }
        }

        [HarmonyPatch(nameof(RoundManager.FinishGeneratingLevel))]
        [HarmonyPostfix]
        private static void OnLevelFinishedLoading(RoundManager __instance)
        {
            Dictionary<string, string> callback = new Dictionary<string, string>();
            callback.Add("callback", "On Level Finished Loading");
            callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
            callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
            NachoAchievements.CheckAchievements(callback);
        }

        [HarmonyPatch(nameof(RoundManager.DestroyTreeAtPosition))]
        [HarmonyPrefix]
        private static void OnDestroyTree(RoundManager __instance, Vector3 pos, float range = 5f)
        {
            int num = Physics.OverlapSphereNonAlloc(pos, range, __instance.tempColliderResults, 33554432, QueryTriggerInteraction.Ignore);
            if (num != 0)
            {
                Dictionary<string, string> callback = new Dictionary<string, string>();
                callback.Add("callback", "On Tree Destroyed");
                callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
                callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
                callback.Add("local", (TerrainObstacleTriggerPatches.driverWhoCollidedIsSelf).ToString());
                NachoAchievements.CheckAchievements(callback);
                TerrainObstacleTriggerPatches.driverWhoCollidedIsSelf = false;
            }
        }
    }
}
