using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using TMPro;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatches
    {
        [HarmonyPatch(nameof(HUDManager.AttemptScanNewCreature))]
        [HarmonyPostfix]
        private static void OnAttemptNewCreatureScan(HUDManager __instance)
        {
            
        }

        [HarmonyPatch(nameof(HUDManager.FillEndGameStats))]
        [HarmonyPostfix]
        private static void OnResultsFilled(HUDManager __instance)
        {
            int num = 0;
            int num2 = 0;

            for (int i = 0; i < __instance.playersManager.allPlayerScripts.Length; i++)
            {
                PlayerControllerB playerControllerB = __instance.playersManager.allPlayerScripts[i];
                if (playerControllerB.disconnectedMidGame || playerControllerB.isPlayerDead || playerControllerB.isPlayerControlled)
                {
                    if (playerControllerB.isPlayerDead)
                    {
                        num++;
                    }
                    else if (playerControllerB.isPlayerControlled)
                    {
                        num2++;
                    }
                }
                int num3 = 0;
                float num4 = (float)RoundManager.Instance.scrapCollectedInLevel / RoundManager.Instance.totalScrapValueInLevel;
                if (num2 == StartOfRound.Instance.connectedPlayersAmount + 1)
                {
                    num3++;
                }
                else if (num > 1)
                {
                    num3--;
                }
                if (num4 >= 0.99f)
                {
                    num3 += 2;
                }
                else if (num4 >= 0.6f)
                {
                    num3++;
                }
                else if (num4 <= 0.25f)
                {
                    num3--;
                }

                Dictionary<string, string> callback = new Dictionary<string, string>();
                callback.Add("callback", "On Rank Or Above Gotten");
                callback.Add("rank", num3.ToString());
                callback.Add("weapon", "Landmine");
                callback.Add("moon", StartOfRound.Instance.currentLevelID.ToString());
                callback.Add("challenge", StartOfRound.Instance.isChallengeFile.ToString());
                NachoAchievements.CheckAchievements(callback);
            }
        }
    }
}
