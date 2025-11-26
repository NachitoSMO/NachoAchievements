using HarmonyLib;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatches
    {
        [HarmonyPatch(nameof(HUDManager.AttemptScanNewCreature))]
        [HarmonyPostfix]
        private static void OnAttemptNewCreatureScan(HUDManager __instance)
        {
            NachoAchievements.Instance.StartCoroutine(NachoAchievements.Instance.CheckSingleRunProgress());
        }

        [HarmonyPatch(nameof(HUDManager.FillEndGameStats))]
        [HarmonyPostfix]
        private static void OnChallengeMoonResultsFilled(HUDManager __instance)
        {
            if (StartOfRound.Instance.isChallengeFile)
            {
                if (__instance.statsUIElements.gradeLetter.text == "A" || __instance.statsUIElements.gradeLetter.text == "S")
                {
                    NachoAchievements.AddAchievement("challengeMoonA");
                }
            }
        }
    }
}
