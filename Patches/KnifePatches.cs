using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(KnifeItem))]
    internal class KnifePatches
    {
        [HarmonyPatch(nameof(KnifeItem.HitKnife))]
        [HarmonyPrefix]
        
        private static void CheckBackstab(KnifeItem __instance, bool cancel = false)
        {
            if (__instance.playerHeldBy == StartOfRound.Instance.localPlayerController)
            {
                if (__instance.previousPlayerHeldBy == null)
                {
                    return;
                }
                if (!cancel && Time.realtimeSinceStartup - __instance.timeAtLastDamageDealt > 0.43f)
                {
                    var objectsHitByKnife = Physics.SphereCastAll(__instance.previousPlayerHeldBy.gameplayCamera.transform.position + __instance.previousPlayerHeldBy.gameplayCamera.transform.right * 0.1f, 0.3f, __instance.previousPlayerHeldBy.gameplayCamera.transform.forward, 0.75f, __instance.knifeMask, QueryTriggerInteraction.Collide);
                    var objectsHitByKnifeList = objectsHitByKnife.OrderBy((RaycastHit x) => x.distance).ToList();
                    List<EnemyAI> list = new List<EnemyAI>();
                    for (int i = 0; i < objectsHitByKnifeList.Count; i++)
                    {
                        if (objectsHitByKnifeList[i].transform.gameObject.layer == 8 || objectsHitByKnifeList[i].transform.gameObject.layer == 11)
                        {
                            continue;
                        }
                        else
                        {
                            if (!objectsHitByKnifeList[i].transform.TryGetComponent<IHittable>(out var component) || objectsHitByKnifeList[i].transform == __instance.previousPlayerHeldBy.transform || (!(objectsHitByKnifeList[i].point == Vector3.zero) && Physics.Linecast(__instance.previousPlayerHeldBy.gameplayCamera.transform.position, objectsHitByKnifeList[i].point, out var _, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore)))
                            {
                                continue;
                            }
                            Vector3 forward = __instance.previousPlayerHeldBy.gameplayCamera.transform.forward;

                            if (objectsHitByKnifeList[i].transform.GetComponent<PlayerControllerB>() != null)
                            {
                                var playerHit = objectsHitByKnifeList[i].transform.GetComponent<PlayerControllerB>();
                                if (!playerHit.isPlayerDead && playerHit.AllowPlayerDeath())
                                {
                                    if (playerHit.health - 20 <= 0 && playerHit.criticallyInjured)
                                    {
                                        NachoAchievements.AddAchievement("killCrewmate");
                                    }
                                }
                            }
                            EnemyAICollisionDetect component2 = objectsHitByKnifeList[i].transform.GetComponent<EnemyAICollisionDetect>();
                            if (component2 != null)
                            {
                                if (!(component2.mainScript == null) && !list.Contains(component2.mainScript))
                                {
                                    if (component2.mainScript.enemyType.enemyName == "Butler" && !component2.mainScript.isEnemyDead) NachoAchievements.AddAchievement("killButlerStab");
                                    list.Add(component2.mainScript);
                                }
                            }

                        }
                    }
                }
            }
        }
    }
}
