using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatches
    {
        private static Vector3 lastGroundedPos;
        [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPrefix]
        private static void OnPlayerHit(PlayerControllerB __instance, CauseOfDeath causeOfDeath = CauseOfDeath.Unknown)
        {
            if (__instance == StartOfRound.Instance.localPlayerController)
            {
                NachoAchievements.AddAchievement("deaths");
                if (causeOfDeath == CauseOfDeath.Fan) NachoAchievements.AddAchievement("killedByFan");
                if (StartOfRound.Instance.currentLevelID == 3) NachoAchievements.AddAchievement("dieCompany");
            }
        }

        [HarmonyPatch(nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void CheckFlying(PlayerControllerB __instance)
        {
            if (__instance == StartOfRound.Instance.localPlayerController)
            {
                if (__instance.IsPlayerNearGround() && !__instance.jetpackControls)
                {
                    lastGroundedPos = __instance.gameObject.transform.position;
                }

                if (__instance.jetpackControls && NachoAchievements.Achievements["flyingNoJetpack"]["completed"] < 1)
                {
                    if (__instance.ItemSlots != null)
                    {
                        for (int i = 0; i < __instance.ItemSlots.Length; i++)
                        {
                            if (__instance.ItemSlots[i] != null && __instance.ItemSlots[i].itemProperties.itemName == "Jetpack") return;
                        }
                    }

                    if (lastGroundedPos != null && Vector3.Distance(__instance.gameObject.transform.position, lastGroundedPos) >= 40)
                    {
                        NachoAchievements.AddAchievement("flyingNoJetpack");
                    }
                }
            }
        }

        [HarmonyPatch(nameof(PlayerControllerB.GrabObject))]
        [HarmonyPostfix]
        private static void CheckGoldBars(PlayerControllerB __instance)
        {
            if (__instance.ItemSlots != null)
            {
                if (NachoAchievements.Achievements["fillGoldBars"]["completed"] < 1)
                {
                    int goldBars = 0;
                    for (int i = 0; i < __instance.ItemSlots.Length; i++)
                    {
                        if (__instance.ItemSlots[i] != null && __instance.ItemSlots[i].itemProperties.itemName == "Gold bar") goldBars++;
                    }

                    if (goldBars == __instance.ItemSlots.Length)
                    {
                        NachoAchievements.AddAchievement("fillGoldBars");
                    }
                }

                if (NachoAchievements.Achievements["gainWeight"]["completed"] < 1)
                {
                    int weight = Mathf.RoundToInt(Mathf.Clamp(__instance.carryWeight - 1f, 0f, 100f) * 105f);
                    if (weight >= 250)
                    {
                        NachoAchievements.AddAchievement("gainWeight");
                    }
                }
            }
        }
    }
}
