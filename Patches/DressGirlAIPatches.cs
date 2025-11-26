using HarmonyLib;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(DressGirlAI))]
    internal class DressGirlAIPatches
    {
        private static float stareTime = 0;
        private static float timeToStareAchievement = 6f;
        [HarmonyPatch(nameof(DressGirlAI.Update))]
        [HarmonyPostfix]
        private static void CheckDressGirlStaring(DressGirlAI __instance)
        {
            if (__instance.hauntingLocalPlayer && __instance.enemyMeshEnabled)
            {
                if (__instance.hauntingPlayer.HasLineOfSightToPosition(__instance.transform.position + Vector3.up * 0.4f, 60f, 100, 5f))
                {
                    stareTime += Time.deltaTime;
                    if (stareTime >= timeToStareAchievement)
                    {
                        NachoAchievements.AddAchievement("ghostGirlStare");
                    }
                }
                else
                {
                    stareTime = 0f;
                }
            }
            else
            {
                stareTime = 0f;
            }
        }
    }
}
