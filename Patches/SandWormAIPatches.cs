using HarmonyLib;
using System.Collections.Generic;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(SandWormAI))]
    internal class SandWormAIPatches
    {
        [HarmonyPatch(nameof(SandWormAI.OnCollideWithEnemy))]
        [HarmonyPostfix]
        private static void OnWormEatEnemy(SandWormAI __instance, EnemyAI enemyScript = null)
        {
            if (__instance.emerged && __instance.targetPlayer == StartOfRound.Instance.localPlayerController)
            {
                Dictionary<string, string> callback = new Dictionary<string, string>();
                callback.Add("callback", "On Kill Enemy");
                callback.Add("enemy", enemyScript.enemyType.enemyName);
                callback.Add("weapon", "Worm");
                callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
                callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
                NachoAchievements.CheckAchievements(callback);
            }
        }
    }
}
