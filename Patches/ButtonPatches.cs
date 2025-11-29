using HarmonyLib;
using UnityEngine.UI;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(Button))]
    internal class ButtonPatches
    {
        [HarmonyPatch(nameof(Button.Press))]
        [HarmonyPrefix]
        private static bool OnPress(Button __instance)
        {
            if (NachoAchievements.achievementEnterButton != null && __instance == NachoAchievements.achievementEnterButton.GetComponentInChildren<Button>())
            {
                NachoAchievements.OnAchievementsClicked();
                return false;
            }
            return true;
        }
    }
}
