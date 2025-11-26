using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(EnemyAI))]
    internal class EnemyAIPatches
    {
        public static List<EnemyAI> enemyWasKilled = new List<EnemyAI>();
        public static List<EnemyAI> enemyWasAlive = new List<EnemyAI>();

        [HarmonyPatch(nameof(EnemyAI.HitEnemy))]
        [HarmonyPrefix]
        private static void OnHitEnemyPrefix(EnemyAI __instance, int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
        {
            if (!__instance.isEnemyDead)
                enemyWasAlive.Add(__instance);
        }

        [HarmonyPatch(nameof(EnemyAI.HitEnemy))]
        [HarmonyPostfix]
        private static void OnHitEnemy(EnemyAI __instance, int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
        {
            NachoAchievements.Instance.StartCoroutine(NachoAchievements.Instance.CheckKillEnemyAchievements(__instance, force, playerWhoHit));
        }

        [HarmonyPatch(nameof(EnemyAI.KillEnemy))]
        [HarmonyPrefix]
        private static void OnKillEnemyPrefix(EnemyAI __instance, bool destroy)
        {
            if (__instance.enemyType.canDie)
            {
                enemyWasKilled.Add(__instance);
            }
        }
    }
}
