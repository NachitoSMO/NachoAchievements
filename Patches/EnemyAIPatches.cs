using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(EnemyAI))]
    internal class EnemyAIPatches
    {
        private static Dictionary<EnemyAI, float> enemiesStareTime = new Dictionary<EnemyAI, float>();
        [HarmonyPatch(nameof(EnemyAI.DetectNoise))]
        [HarmonyPostfix]
        private static void OnDetectNoise(EnemyAI __instance, Vector3 noisePosition, float noiseLoudness, int timesPlayedInOneSpot = 0, int noiseID = 0)
        {
            if (!Physics.Linecast(__instance.transform.position, noisePosition, StartOfRound.Instance.collidersAndRoomMask))
            {
                int id = noiseID;
                Dictionary<string, string> callback = new Dictionary<string, string>();
                callback.Add("callback", "On Enemy Sound Heard");
                callback.Add("enemy", __instance.enemyType.enemyName);
                callback.Add("sound", id.ToString());
                callback.Add("soundMinDistance", Vector3.Distance(__instance.transform.position, noisePosition).ToString());
                callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
                callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
                NachoAchievements.CheckAchievements(callback);
            }
        }

        [HarmonyPatch(nameof(EnemyAI.Update))]
        [HarmonyPostfix]
        private static void CheckStaring(EnemyAI __instance)
        {
            if (!enemiesStareTime.ContainsKey(__instance))
            {
                enemiesStareTime.Add(__instance, 0);
            }

            if (StartOfRound.Instance.localPlayerController.HasLineOfSightToPosition(__instance.transform.position + Vector3.up * 0.4f, 60f, 100, 5f))
            {
                enemiesStareTime[__instance] += Time.deltaTime;
                if (enemiesStareTime[__instance] >= 5f)
                {
                    Dictionary<string, string> callback = new Dictionary<string, string>();
                    callback.Add("callback", "Enemy Stared At");
                    callback.Add("enemy", __instance.enemyType.enemyName);
                    callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
                    callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
                    NachoAchievements.CheckAchievements(callback);
                    enemiesStareTime[__instance] = 0;
                }

            }
            else
            {
                enemiesStareTime[__instance] = 0;
            }

        }

        [HarmonyPatch(nameof(EnemyAI.OnDestroy))]
        [HarmonyPostfix]
        private static void OnDestroyed(EnemyAI __instance)
        {
            enemiesStareTime.Clear();
        }
    }
}
