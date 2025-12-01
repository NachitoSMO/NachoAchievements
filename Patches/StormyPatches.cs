using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(StormyWeather))]
    internal class StormyPatches
    {
        [HarmonyPatch(nameof(StormyWeather.LightningStrike))]
        [HarmonyPrefix]
        private static void SpawnLightningPatch(StormyWeather __instance, Vector3 strikePosition, bool useTargetedObject)
        {
            Collider[] array = Physics.OverlapSphere(strikePosition + Vector3.up * 0.25f, 5f, 2621448, QueryTriggerInteraction.Collide);
            List<EnemyAI> list = new List<EnemyAI>();

            for (int i = 0; i < array.Length; i++)
            {
                float num2 = Vector3.Distance(strikePosition + Vector3.up * 0.25f, array[i].transform.position);
                if (array[i].gameObject.layer == 19)
                {
                    EnemyAICollisionDetect componentInChildren2 = array[i].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
                    if (componentInChildren2 != null && componentInChildren2.mainScript.IsOwner && num2 < 4.5f && componentInChildren2.mainScript.enemyHP > 0 && componentInChildren2.mainScript.enemyType.canDie && !list.Contains(componentInChildren2.mainScript))
                    {
                        Dictionary<string, string> callback = new Dictionary<string, string>();
                        callback.Add("callback", "On Kill Enemy");
                        callback.Add("enemy", componentInChildren2.mainScript.enemyType.enemyName);
                        callback.Add("weapon", "Lightning");
                        callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
                        callback.Add("local", (useTargetedObject && PlayerControllerBPatches.playerDroppedBy.ContainsKey(__instance.targetingMetalObject) && PlayerControllerBPatches.playerDroppedBy[__instance.targetingMetalObject] == StartOfRound.Instance.localPlayerController).ToString());
                        callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
                        NachoAchievements.CheckAchievements(callback);
                        list.Add(componentInChildren2.mainScript);
                    }
                }
            }
        }
    }
}
