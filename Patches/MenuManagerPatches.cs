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
        [HarmonyPatch(nameof(QuickMenuManager.Update))]
        [HarmonyPostfix]
        private static void AddMenuButtonUpdate(QuickMenuManager __instance)
        {
            if (NachoAchievements.achievementEnterButton == null && __instance.isMenuOpen && __instance.mainButtonsPanel.activeSelf)
            {
                foreach (var transform in __instance.mainButtonsPanel.GetComponentsInChildren<Transform>())
                {
                    var button = transform.gameObject;
                    if (button.name == "Resume")
                    {
                        button.GetComponentInChildren<RectTransform>().anchoredPosition += new Vector2(0, 90);
                        if (NachoAchievements.achievementEnterButton == null)
                        {
                            NachoAchievements.achievementEnterButton = Object.Instantiate(button, __instance.mainButtonsPanel.transform);
                            NachoAchievements.achievementEnterButton.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, 90);
                            NachoAchievements.achievementEnterButton.GetComponentInChildren<TextMeshProUGUI>().text = "> Achievements";
                        }
                        break;
                    }
                }

            }
            
        }
    }
}
