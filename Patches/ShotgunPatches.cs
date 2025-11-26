using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(ShotgunItem))]
    internal class ShotgunPatches
    {
        private static List<EnemyAI> enemiesThatWereAlive = new List<EnemyAI>();

        [HarmonyPatch(nameof(ShotgunItem.ShootGun))]
        [HarmonyPrefix]
        private static void OnGunShotPre(ShotgunItem __instance, Vector3 shotgunPosition, Vector3 shotgunForward)
        {
            enemiesThatWereAlive.Clear();
            var colliders = new RaycastHit[10];
            var ray = new Ray(shotgunPosition - shotgunForward * 10f, shotgunForward);
            int num3 = Physics.SphereCastNonAlloc(ray, 5f, colliders, 15f, 524288, QueryTriggerInteraction.Collide);
            List<EnemyAI> list = new List<EnemyAI>();
            for (int i = 0; i < num3; i++)
            {
                if (!colliders[i].transform.GetComponent<EnemyAICollisionDetect>())
                {
                    continue;
                }
                EnemyAI mainScript = colliders[i].transform.GetComponent<EnemyAICollisionDetect>().mainScript;
                if (__instance.heldByEnemy != null && __instance.heldByEnemy == mainScript)
                {
                    continue;
                }
                IHittable component;
                if (colliders[i].distance == 0f)
                {
                    continue;
                }
                else if (!Physics.Linecast(shotgunPosition, colliders[i].point, out var hitInfo, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) && colliders[i].transform.TryGetComponent<IHittable>(out component))
                {
                    float num4 = Vector3.Distance(shotgunPosition, colliders[i].point);
                    int force = ((num4 < 3.7f) ? 5 : ((!(num4 < 6f)) ? 2 : 3));
                    EnemyAICollisionDetect component2 = colliders[i].collider.GetComponent<EnemyAICollisionDetect>();
                    if ((!(component2 != null) || (!(component2.mainScript == null) && !list.Contains(component2.mainScript))) && component2 != null)
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
        [HarmonyPatch(nameof(ShotgunItem.ShootGun))]
        [HarmonyPostfix]
        private static void OnGunShot(ShotgunItem __instance, Vector3 shotgunPosition, Vector3 shotgunForward)
        {
            var colliders = new RaycastHit[10];
            var ray = new Ray(shotgunPosition - shotgunForward * 10f, shotgunForward);
            int num3 = Physics.SphereCastNonAlloc(ray, 5f, colliders, 15f, 524288, QueryTriggerInteraction.Collide);
            List<EnemyAI> list = new List<EnemyAI>();
            int pierceCount = 0;
            for (int i = 0; i < num3; i++)
            {
                if (!colliders[i].transform.GetComponent<EnemyAICollisionDetect>())
                {
                    continue;
                }
                EnemyAI mainScript = colliders[i].transform.GetComponent<EnemyAICollisionDetect>().mainScript;
                if (__instance.heldByEnemy != null && __instance.heldByEnemy == mainScript)
                {
                    continue;
                }
                IHittable component;
                if (colliders[i].distance == 0f)
                {
                    continue;
                }
                else if (!Physics.Linecast(shotgunPosition, colliders[i].point, out var hitInfo, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) && colliders[i].transform.TryGetComponent<IHittable>(out component))
                {
                    float num4 = Vector3.Distance(shotgunPosition, colliders[i].point);
                    int force = ((num4 < 3.7f) ? 5 : ((!(num4 < 6f)) ? 2 : 3));
                    EnemyAICollisionDetect component2 = colliders[i].collider.GetComponent<EnemyAICollisionDetect>();
                    if ((!(component2 != null) || (!(component2.mainScript == null) && !list.Contains(component2.mainScript))) && component2 != null)
                    {
                        if (component2.mainScript.isEnemyDead && enemiesThatWereAlive.Contains(component2.mainScript))
                        {
                            if (__instance.playerHeldBy == null && component2.mainScript.enemyType.enemyName == "Nutcracker")
                            {
                                NachoAchievements.AddAchievement("nutcrackerShootEnemy");
                            }
                            else if (__instance.playerHeldBy != null && __instance.playerHeldBy == StartOfRound.Instance.localPlayerController)
                            {
                                if (component2.mainScript.enemyType.enemyName == "Tulip Snake" && force == 5) NachoAchievements.AddAchievement("killTulipShotgun");

                                NachoAchievements.AddAchievement("killEnemiesShotgun");
                                pierceCount++;
                                if (pierceCount == 2)
                                {
                                    NachoAchievements.AddAchievement("shotgunPierceEnemy");
                                }
                            }
                        }
                        list.Add(component2.mainScript);
                    }
                }
            }
        }
    }
}
