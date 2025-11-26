using HarmonyLib;
using UnityEngine;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(QuickMenuManager))]
    internal class MenuManagerPatches
    {
        [HarmonyPatch(nameof(QuickMenuManager.Update))]
        [HarmonyPostfix]
        private static void AddMenuButtonUpdate(QuickMenuManager __instance)
        {
            if ((!__instance.mainButtonsPanel.activeSelf || !__instance.isMenuOpen) && NachoAchievements.achievementEnterButton != null)
                Object.Destroy(NachoAchievements.achievementEnterButton.gameObject);
            else
            {
                if (NachoAchievements.achievementEnterButton == null && __instance.isMenuOpen && __instance.mainButtonsPanel.activeSelf)
                {
                    NachoAchievements.achievementEnterButton = NachoAchievements.CreateAchievementsText(new Vector2(-205, -450), false, 42);
                    NachoAchievements.achievementEnterButton.text = "> Achievements";
                }
            }
        }
    }
}
