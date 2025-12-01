using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(Shovel))]
    internal class ShovelPatches
    {
        private static List<EnemyAI> enemiesThatWereAlive = new List<EnemyAI>();
        [HarmonyPatch(nameof(Shovel.HitShovel))]
        [HarmonyPrefix]

        private static void PreHitShovel(Shovel __instance, bool cancel = false)
        {
            enemiesThatWereAlive.Clear();
            if (!cancel)
            {
                __instance.previousPlayerHeldBy.twoHanded = false;
                __instance.objectsHitByShovel = Physics.SphereCastAll(__instance.previousPlayerHeldBy.gameplayCamera.transform.position + __instance.previousPlayerHeldBy.gameplayCamera.transform.right * -0.35f, 0.8f, __instance.previousPlayerHeldBy.gameplayCamera.transform.forward, 1.5f, __instance.shovelMask, QueryTriggerInteraction.Collide);
                __instance.objectsHitByShovelList = __instance.objectsHitByShovel.OrderBy((RaycastHit x) => x.distance).ToList();
                List<EnemyAI> list = new List<EnemyAI>();
                for (int i = 0; i < __instance.objectsHitByShovelList.Count; i++)
                {
                    if (!__instance.objectsHitByShovelList[i].transform.TryGetComponent<IHittable>(out var component) || __instance.objectsHitByShovelList[i].transform == __instance.previousPlayerHeldBy.transform || (!(__instance.objectsHitByShovelList[i].point == Vector3.zero) && Physics.Linecast(__instance.previousPlayerHeldBy.gameplayCamera.transform.position, __instance.objectsHitByShovelList[i].point, out var _, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore)))
                    {
                        continue;
                    }
                    Vector3 forward = __instance.previousPlayerHeldBy.gameplayCamera.transform.forward;

                    EnemyAICollisionDetect component2 = __instance.objectsHitByShovelList[i].transform.GetComponent<EnemyAICollisionDetect>();
                    if (component2 != null)
                    {
                        if (!(component2.mainScript == null) && !list.Contains(component2.mainScript) && (!StartOfRound.Instance.hangarDoorsClosed || component2.mainScript.isInsidePlayerShip == __instance.previousPlayerHeldBy.isInHangarShipRoom))
                        {
                            if (!component2.mainScript.isEnemyDead)
                            {
                                enemiesThatWereAlive.Add(component2.mainScript);
                            }
                            list.Add(component2.mainScript);
                        }
                    }

                }
            }
        }

        [HarmonyPatch(nameof(Shovel.HitShovel))]
        [HarmonyPostfix]

        private static void PostHitShovel(Shovel __instance, bool cancel = false)
        {
            if (!cancel)
            {
                __instance.previousPlayerHeldBy.twoHanded = false;
                __instance.objectsHitByShovel = Physics.SphereCastAll(__instance.previousPlayerHeldBy.gameplayCamera.transform.position + __instance.previousPlayerHeldBy.gameplayCamera.transform.right * -0.35f, 0.8f, __instance.previousPlayerHeldBy.gameplayCamera.transform.forward, 1.5f, __instance.shovelMask, QueryTriggerInteraction.Collide);
                __instance.objectsHitByShovelList = __instance.objectsHitByShovel.OrderBy((RaycastHit x) => x.distance).ToList();
                List<EnemyAI> list = new List<EnemyAI>();
                for (int i = 0; i < __instance.objectsHitByShovelList.Count; i++)
                {
                    if (!__instance.objectsHitByShovelList[i].transform.TryGetComponent<IHittable>(out var component) || __instance.objectsHitByShovelList[i].transform == __instance.previousPlayerHeldBy.transform || (!(__instance.objectsHitByShovelList[i].point == Vector3.zero) && Physics.Linecast(__instance.previousPlayerHeldBy.gameplayCamera.transform.position, __instance.objectsHitByShovelList[i].point, out var _, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore)))
                    {
                        continue;
                    }
                    Vector3 forward = __instance.previousPlayerHeldBy.gameplayCamera.transform.forward;

                    EnemyAICollisionDetect component2 = __instance.objectsHitByShovelList[i].transform.GetComponent<EnemyAICollisionDetect>();
                    if (component2 != null)
                    {
                        if (!(component2.mainScript == null) && !list.Contains(component2.mainScript) && (!StartOfRound.Instance.hangarDoorsClosed || component2.mainScript.isInsidePlayerShip == __instance.previousPlayerHeldBy.isInHangarShipRoom))
                        {
                            if (component2.mainScript.isEnemyDead && enemiesThatWereAlive.Contains(component2.mainScript))
                            {
                                Dictionary<string, string> callback = new Dictionary<string, string>();
                                callback.Add("callback", "On Kill Enemy");
                                callback.Add("enemy", component2.mainScript.enemyType.enemyName);
                                callback.Add("weapon", "Shovel");
                                callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
                                callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
                                callback.Add("local", (__instance.playerHeldBy == StartOfRound.Instance.localPlayerController).ToString());
                                NachoAchievements.CheckAchievements(callback);
                            }
                            list.Add(component2.mainScript);
                        }
                    }

                }
            }
        }

    }
}
