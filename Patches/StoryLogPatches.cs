using HarmonyLib;
using System.Collections.Generic;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(StoryLog))]
    internal class StoryLogPatches
    {
        [HarmonyPatch(nameof(StoryLog.CollectLog))]
        [HarmonyPostfix]
        private static void OnStoryLogCollected(StoryLog __instance)
        {
            Dictionary<string, string> callback = new Dictionary<string, string>();
            callback.Add("callback", "On Log Collected");
            callback.Add("logID", __instance.storyLogID.ToString());
            callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
            callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
            NachoAchievements.CheckAchievements(callback);
        }
    }
}
