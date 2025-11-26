using HarmonyLib;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(BlobAI))]
    internal class BlobAIPatches
    {
        [HarmonyPatch(nameof(BlobAI.DetectNoise))]
        [HarmonyPostfix]
        private static void BlobDetectNoise(BlobAI __instance, Vector3 noisePosition, float noiseLoudness, int timesPlayedInOneSpot = 0, int noiseID = 0)
        {
            if (NachoAchievements.Achievements["boomboxParty"]["completed"] >= 1) return;
            if (noiseID == 5 && !Physics.Linecast(__instance.transform.position, noisePosition, StartOfRound.Instance.collidersAndRoomMask) && Vector3.Distance(__instance.transform.position, noisePosition) < 12f)
            {
                NachoAchievements.AddAchievement("boomboxParty");
            }
        }
    }
}
