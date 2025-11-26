using HarmonyLib;
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
            if (!scrapObject.scrapPersistedThroughRounds && !__instance.scrapCollectedThisRound.Contains(scrapObject) && scrapObject.itemProperties.isScrap)
            {
                if (scrapObject.itemProperties.itemName == "Hive" && scrapObject.playerHeldBy == StartOfRound.Instance.localPlayerController)
                {
                    NachoAchievements.AddAchievement("beesCollect");
                }

                if (scrapObject.itemProperties.itemName == "Zed Dog" && scrapObject.playerHeldBy == StartOfRound.Instance.localPlayerController)
                {
                    NachoAchievements.AddAchievement("zedDogCollect");
                }

                if (scrapObject.playerHeldBy == StartOfRound.Instance.localPlayerController)
                    NachoAchievements.AddAchievement("scrapCollect");

                if (scrapObject.itemProperties.itemName == "Apparatus" && StartOfRound.Instance.currentLevelID == 12 && RoundManager.Instance.dungeonFinishedGeneratingForAllPlayers)
                {
                    NachoAchievements.AddAchievement("embrionApparatus");
                }

                if (StartOfRound.Instance.currentLevelID == 10 && NachoAchievements.Achievements["artFullClear"]["MinMaxing"] != 999)
                {
                    NachoAchievements.AddAchievement("artFullClear");
                }
            }

            NachoAchievements.Instance.StartCoroutine(NachoAchievements.Instance.CheckSingleRunProgress());
        }

        [HarmonyPatch(nameof(RoundManager.FinishGeneratingLevel))]
        [HarmonyPostfix]
        private static void AfterGenerationDone()
        {
            if (StartOfRound.Instance.currentLevelID == 10)
            {
                NachoAchievements.Instance.Invoke("SetartFullClearsingleRunCount", 2f);
            }
            else
            {
                NachoAchievements.Achievements["artFullClearsingleRun"]["MinMaxing"] = 999;
            }
        }

        [HarmonyPatch(nameof(RoundManager.DestroyTreeAtPosition))]
        [HarmonyPrefix]
        private static void OnDestroyTree(RoundManager __instance, Vector3 pos, float range = 5f)
        {
            if (TerrainObstacleTriggerPatches.driverWhoCollidedIsSelf)
            {
                int num = Physics.OverlapSphereNonAlloc(pos, range, __instance.tempColliderResults, 33554432, QueryTriggerInteraction.Ignore);
                if (num != 0)
                {
                    NachoAchievements.AddAchievement("destroyTrees");
                }

                TerrainObstacleTriggerPatches.driverWhoCollidedIsSelf = false;
            }
        }
    }
}
