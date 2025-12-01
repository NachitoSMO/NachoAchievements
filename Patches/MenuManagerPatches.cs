using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NachoAchievements.Patches
{
    [HarmonyPatch(typeof(QuickMenuManager))]
    internal class MenuManagerPatches
    {

        [HarmonyPatch(nameof(QuickMenuManager.OpenQuickMenu))]
        [HarmonyPostfix]
        private static void AddMenuButton(QuickMenuManager __instance)
        {
            if (__instance.mainButtonsPanel.activeSelf)
            {
                foreach (var transform in __instance.mainButtonsPanel.GetComponentsInChildren<Transform>())
                {
                    var button = transform.gameObject;
                    if (button.name == "Resume" && NachoAchievements.achievementEnterButton == null)
                    {
                        button.GetComponentInChildren<RectTransform>().anchoredPosition += new Vector2(0, 90);
                        NachoAchievements.achievementEnterButton = Object.Instantiate(button, __instance.mainButtonsPanel.transform);
                        NachoAchievements.achievementEnterButton.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 90);
                        NachoAchievements.achievementEnterButton.GetComponentInChildren<TextMeshProUGUI>().text = "> Achievements";
                    }
                }
            }
        }
    }
}
