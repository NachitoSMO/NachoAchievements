using HarmonyLib;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(StoryLog))]
    internal class StoryLogPatches
    {
        [HarmonyPatch(nameof(StoryLog.CollectLog))]
        [HarmonyPostfix]
        private static void CheckLogProgress(StoryLog __instance)
        {
            NachoAchievements.Instance.StartCoroutine(NachoAchievements.Instance.CheckSingleRunProgress());
        }
    }
}
