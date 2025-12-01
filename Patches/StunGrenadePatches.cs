using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(StunGrenadeItem))]
    internal class StunGrenadePatches
    {
        [HarmonyPatch(nameof(StunGrenadeItem.ExplodeStunGrenade))]
        [HarmonyPrefix]
        private static void ExplodeGrenadePatch(StunGrenadeItem __instance)
        {
            if (__instance.itemProperties.itemName == "Easter egg" && __instance.explodeOnThrow)
            {
                Collider[] array = Physics.OverlapSphere(__instance.transform.position + Vector3.up * 0.2f, 3f, 2621448, QueryTriggerInteraction.Collide);
                List<EnemyAI> list = new List<EnemyAI>();

                for (int i = 0; i < array.Length; i++)
                {
                    float num2 = Vector3.Distance(__instance.transform.position + Vector3.up * 0.2f, array[i].transform.position);
                    if (array[i].gameObject.layer == 19)
                    {
                        EnemyAICollisionDetect componentInChildren2 = array[i].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
                        if (componentInChildren2 != null && componentInChildren2.mainScript.IsOwner && num2 < 4.5f && !list.Contains(componentInChildren2.mainScript) && componentInChildren2.mainScript.enemyType.canDie)
                        {
                            if (!componentInChildren2.mainScript.isEnemyDead)
                            {
                                Dictionary<string, string> callback = new Dictionary<string, string>();
                                callback.Add("callback", "On Kill Enemy");
                                callback.Add("enemy", componentInChildren2.mainScript.enemyType.enemyName);
                                callback.Add("weapon", "Easter egg");
                                callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
                                callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
                                callback.Add("local", (__instance.playerThrownBy == StartOfRound.Instance.localPlayerController).ToString());
                                NachoAchievements.CheckAchievements(callback);
                            }
                            list.Add(componentInChildren2.mainScript);
                        }
                    }
                }
            }
        }
    }
}
