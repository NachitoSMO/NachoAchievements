using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(Landmine))]
    internal class LandminePatches
    {
        private static bool playerSetOffMine = false;
        private static List<EnemyAI> enemiesThatWereAlive = new List<EnemyAI>();
        [HarmonyPatch(nameof(Landmine.OnTriggerExit))]
        [HarmonyPrefix]
        private static void OnMineSetOff(Landmine __instance, Collider other)
        {
            if (!__instance.hasExploded && __instance.mineActivated)
            {
                PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                if (component != null && component == StartOfRound.Instance.localPlayerController)
                {
                    playerSetOffMine = true;
                }
            }
        }

        [HarmonyPatch(nameof(Landmine.SpawnExplosion))]
        [HarmonyPrefix]
        private static void OnMineExploded(Landmine __instance, Vector3 explosionPosition, bool spawnExplosionEffect = false, float killRange = 1f, float damageRange = 1f, int nonLethalDamage = 50, float physicsForce = 0f, GameObject overridePrefab = null, bool goThroughCar = false)
        {
            if (playerSetOffMine)
            {
                Collider[] array = Physics.OverlapSphere(explosionPosition, damageRange, 2621448, QueryTriggerInteraction.Collide);
                RaycastHit hitInfo;
                for (int i = 0; i < array.Length; i++)
                {
                    float num2 = Vector3.Distance(explosionPosition, array[i].transform.position);
                    if (Physics.Linecast(explosionPosition, array[i].transform.position + Vector3.up * 0.3f, out hitInfo, 1073742080, QueryTriggerInteraction.Ignore) && ((!goThroughCar && hitInfo.collider.gameObject.layer == 30) || num2 > 4f))
                    {
                        continue;
                    }

                    if (array[i].gameObject.layer == 19)
                    {
                        EnemyAICollisionDetect componentInChildren2 = array[i].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
                        if (componentInChildren2 != null && componentInChildren2.mainScript.IsOwner && num2 < 4.5f)
                        {
                            if (!componentInChildren2.mainScript.isEnemyDead)
                            {
                                enemiesThatWereAlive.Add(componentInChildren2.mainScript);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(nameof(Landmine.SpawnExplosion))]
        [HarmonyPostfix]
        private static void PostMineExploded(Landmine __instance, Vector3 explosionPosition, bool spawnExplosionEffect = false, float killRange = 1f, float damageRange = 1f, int nonLethalDamage = 50, float physicsForce = 0f, GameObject overridePrefab = null, bool goThroughCar = false)
        {
            if (playerSetOffMine)
            {
                Collider[] array = Physics.OverlapSphere(explosionPosition, damageRange, 2621448, QueryTriggerInteraction.Collide);
                RaycastHit hitInfo;
                for (int i = 0; i < array.Length; i++)
                {
                    float num2 = Vector3.Distance(explosionPosition, array[i].transform.position);
                    if (Physics.Linecast(explosionPosition, array[i].transform.position + Vector3.up * 0.3f, out hitInfo, 1073742080, QueryTriggerInteraction.Ignore) && ((!goThroughCar && hitInfo.collider.gameObject.layer == 30) || num2 > 4f))
                    {
                        continue;
                    }

                    if (array[i].gameObject.layer == 19)
                    {
                        EnemyAICollisionDetect componentInChildren2 = array[i].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
                        if (componentInChildren2 != null && componentInChildren2.mainScript.IsOwner && num2 < 4.5f)
                        {
                            if (componentInChildren2.mainScript.isEnemyDead && enemiesThatWereAlive.Contains(componentInChildren2.mainScript))
                            {
                                Dictionary<string, string> callback = new Dictionary<string, string>();
                                callback.Add("callback", "On Kill Enemy");
                                callback.Add("enemy", componentInChildren2.mainScript.enemyType.enemyName);
                                callback.Add("weapon", "Landmine");
                                callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
                                callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
                                NachoAchievements.CheckAchievements(callback);
                            }
                        }
                    }
                }
            }
        }
    }
}
