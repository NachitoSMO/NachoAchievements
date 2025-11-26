using HarmonyLib;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(SandWormAI))]
    internal class SandWormAIPatches
    {
        [HarmonyPatch(nameof(SandWormAI.OnCollideWithEnemy))]
        [HarmonyPostfix]
        private static void OnWormEatEnemy(SandWormAI __instance)
        {
            if (__instance.emerged && __instance.targetPlayer == StartOfRound.Instance.localPlayerController)
            {
                NachoAchievements.AddAchievement("killEnemyWorm");
            }
        }
    }
}
