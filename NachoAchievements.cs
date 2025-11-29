using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
using UnityEngine.UI;

namespace NachoAchievements
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class NachoAchievements : BaseUnityPlugin
    {
        public static NachoAchievements Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        public static Dictionary<string, Dictionary<string, string>> Achievements = new Dictionary<string, Dictionary<string, string>>();

        public static AssetBundle NachoAssets;

        public static TMP_FontAsset AchievementsFont;

        public static List<GameObject> AchievementsText = new List<GameObject>();

        public static Canvas AchievementsCanvas;

        public static Dictionary<string, int> scrapFound = new Dictionary<string, int>();

        public static List<string> enemiesKilled = new List<string>();

        public static GameObject achievementGetText = null!;

        public static GameObject achievementGetTextSubtitle = null!;

        public static GameObject achievementEnterButton = null!;

        private static EventSystem events = EventSystem.current;
        private static PointerEventData pointerData;
        public static TextMeshProUGUI TMP;
        public static string prevText;
        public static float destroyTimer;

        private ConfigEntry<bool> resetAchievements;

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            gameObject.hideFlags = HideFlags.DontSave;

            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            NachoAssets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "nachoachievements"));
            if (NachoAssets == null)
            {
                Logger.LogError("Failed to load custom assets.");
                return;
            }

            AchievementsFont = NachoAssets.LoadAsset<TMP_FontAsset>("assets/3270-regular sdf.asset");

            Patch();

            resetAchievements = Config.Bind<bool>("Debug", "Reset Achievements", false, new ConfigDescription("Wether to reset every achievement next time you boot up the game (does not include save file-dependent achievements)"));

            CreateAchievements();

            WriteAchievements();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private void Update()
        {
            if (NachoAchievements.AchievementsText != null && NachoAchievements.AchievementsText.Count > 0)
            {
                float amount = 200;

                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    for (int i = 0; i < NachoAchievements.AchievementsText.Count; i++)
                    {
                        Object.Destroy(NachoAchievements.AchievementsText[i]);
                    }
                    var QMM = GameNetworkManager.Instance.localPlayerController.quickMenuManager;
                    QMM.CloseQuickMenu();
                    Object.Destroy(TMP);
                    prevText = string.Empty;
                    NachoAchievements.AchievementsText.Clear();
                }

                if (Mouse.current.scroll.y.ReadValue() > 0f)
                {
                    for (int i = 0; i < NachoAchievements.AchievementsText.Count; i++)
                    {
                        RectTransform rt = NachoAchievements.AchievementsText[i].GetComponent<RectTransform>();
                        if (i == 0 && rt.anchoredPosition.y <= 450) return;
                        rt.anchoredPosition = rt.anchoredPosition - new Vector2(0, amount);
                    }

                    if (TMP != null)
                    {
                        RectTransform r = TMP.GetComponent<RectTransform>();
                        if (r != null) r.anchoredPosition = r.anchoredPosition - new Vector2(0, amount);
                    }

                }

                if (Mouse.current.scroll.y.ReadValue() < 0f)
                {
                    for (int i = 0; i < NachoAchievements.AchievementsText.Count; i++)
                    {
                        RectTransform rt = NachoAchievements.AchievementsText[i].GetComponent<RectTransform>();
                        if (i == 0 && rt.anchoredPosition.y >= -450 + NachoAchievements.AchievementsText.Count * 100) return;
                        rt.anchoredPosition = rt.anchoredPosition + new Vector2(0, amount);
                    }

                    if (TMP != null)
                    {
                        RectTransform r = TMP.GetComponent<RectTransform>();
                        if (r != null) r.anchoredPosition = r.anchoredPosition + new Vector2(0, amount);
                    }
                }

                pointerData = new PointerEventData(events)
                {
                    position = Mouse.current.position.ReadValue()
                };

                List<RaycastResult> results = new List<RaycastResult>();
                Canvas canvas = Object.FindObjectOfType<Canvas>();
                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                raycaster.Raycast(pointerData, results);

                foreach (var result in results)
                {
                    TextMeshProUGUI tmp = result.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null && (TMP == null || tmp.text != prevText))
                    {
                        string[] achievements = [.. Achievements.Keys];
                        
                        int index = AchievementsText.IndexOf(result.gameObject);
                        if (index != -1)
                        {
                            if (!Achievements.ContainsKey(achievements[index])) continue;
                            if (!Achievements[achievements[index]].ContainsKey("description")) continue;
                            string output = Achievements[achievements[index]]["description"];
                            NachoAchievements.CreateAchievementDescription(tmp, output);
                        }
                        
                    }
                }

                if (results.Count <= 0)
                {
                    if (TMP != null) Object.Destroy(TMP.gameObject);
                    prevText = string.Empty;
                }
            }

            if (NachoAchievements.achievementGetText != null)
            {
                destroyTimer += Time.deltaTime;
                if (destroyTimer >= 5f)
                {
                    Object.Destroy(NachoAchievements.achievementGetText);
                    Object.Destroy(NachoAchievements.achievementGetTextSubtitle);
                }
            }
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll();

            Logger.LogDebug("Finished patching!");
        }

        public static void CheckAchievements(Dictionary<string, string> callback)
        {
            List<string> keys = [.. Achievements.Keys];

            if (!callback.ContainsKey("callback"))
            {
                Logger.LogError("Error! Callback does not exist.");
                return;
            }


            foreach (string key in keys)
            {
                if (!Achievements[key].ContainsKey("callback"))
                {
                    Logger.LogError("Error! Achievement " + key + " does not contain a callback. Skipping...");
                    continue;
                }

                if (Achievements[key]["callback"] != callback["callback"]) continue;

                List<string> variables = new List<string>();

                List<string> callbackKeys = [.. callback.Keys];

                foreach (string var in callbackKeys)
                {
                    if (var == "callback") continue;
                    variables.Add(var);
                }

                bool shouldAddAchievement = true;

                foreach (string var in variables)
                {
                    string callbackVar = callback[var];
                    string callbackVarSpecified = string.Empty;

                    if (!Achievements[key].ContainsKey(var))
                    {
                        callbackVarSpecified = "Any";
                    }
                    else
                    {
                        callbackVarSpecified = Achievements[key][var];
                    }

                    if (var == "rank")
                    {
                        if (int.TryParse(callbackVarSpecified, out int result))
                        {
                            if (int.Parse(callbackVar) >= result) continue;
                        }
                    }
                    else if (var == "soundMinDistance")
                    {
                        if (float.TryParse(callbackVarSpecified, out float result))
                        {
                            if (float.Parse(callbackVar) <= result) continue;
                        }
                    }
                    else if (callbackVarSpecified == "Unique")
                    {
                        var unique = ES3.Load(key, GameNetworkManager.generalSaveDataName, new List<string>());

                        if ((CheckIfSingleRun(key) && Instance.GetUniqueOnServerRpc(key, StartOfRound.Instance.localPlayerController.playerSteamId).Contains(callbackVar)) || !CheckIfSingleRun(key) && unique.Contains(callbackVar))
                        {
                            shouldAddAchievement = false;
                        }
                        else
                        {
                            Instance.SaveUniqueOnServerRpc(key, StartOfRound.Instance.localPlayerController.playerSteamId, callbackVar);
                            unique.Add(callbackVar);
                            ES3.Save(key, unique, GameNetworkManager.generalSaveDataName);
                        }
                    }

                    if (callbackVar != callbackVarSpecified && callbackVarSpecified != "Any" && callbackVarSpecified != "Unique") shouldAddAchievement = false;
                }

                if (shouldAddAchievement)
                {
                    Achievements[key]["progress"] = (int.Parse(Achievements[key]["progress"]) + 1).ToString();
                    if (CheckIfSingleRun(key))
                        Instance.SaveAchievementOnServerRpc(key, int.Parse(Achievements[key]["progress"]), StartOfRound.Instance.localPlayerController.playerSteamId);
                }
                
            }

            CheckAchievementCount();

            foreach (string a in keys)
            {
                if (int.TryParse(Achievements[a]["count"], out int result))
                {
                    if (int.Parse(Achievements[a]["progress"]) >= result && Achievements[a]["completed"] == "false")
                    {
                        CreateAchievementGetText(a);
                        Achievements[a]["completed"] = "true";
                    }
                }
            }

            WriteAchievements();
        }

        public static bool CheckIfSingleRun(string key)
        {
            return (Achievements[key].ContainsKey("single run") && Achievements[key]["single run"] == "true");
        }

        [ServerRpc(RequireOwnership = false)]
        public void SaveAchievementOnServerRpc(string key, int progress, ulong playerSteamID)
        {
            ES3.Save("SingleRun" + key + playerSteamID.ToString(), progress, GameNetworkManager.Instance.currentSaveFileName);
        }

        [ServerRpc(RequireOwnership = false)]
        public string[] GetUniqueOnServerRpc(string key, ulong playerSteamID)
        {
            return ES3.Load(key + playerSteamID.ToString(), GameNetworkManager.Instance.currentSaveFileName, new List<string>()).ToArray();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SaveUniqueOnServerRpc(string key, ulong playerSteamID, string enemy)
        {
            List<string> enemiesKilled = ES3.Load(key + playerSteamID, GameNetworkManager.Instance.currentSaveFileName, new List<string>());

            if (!enemiesKilled.Contains(enemy))
            {
                enemiesKilled.Add(enemy);
                ES3.Save(key + playerSteamID, enemiesKilled, GameNetworkManager.Instance.currentSaveFileName);
            }
            
        }

        [ServerRpc(RequireOwnership = false)]
        public void GetAchievementsOnServerRpc(ulong playerSteamID)
        {
            List<string> keys = [.. Achievements.Keys];

            foreach (string key in keys)
            {
                if (CheckIfSingleRun(key))
                {
                    int progress = ES3.Load("SingleRun" + key + playerSteamID.ToString(), GameNetworkManager.Instance.currentSaveFileName, 0);
                    SaveAchievementOnClientRpc(key, progress, playerSteamID);
                }
            }
        }

        [ClientRpc]
        public void SaveAchievementOnClientRpc(string key, int progress, ulong playerSteamID)
        {
            if (playerSteamID != GameNetworkManager.Instance.localPlayerController.playerSteamId) return;
            Achievements[key]["progress"] = progress.ToString();
        }

        public static void ResetSingleDayAchievements()
        {
            List<string> keys = [.. Achievements.Keys];
            foreach (string key in keys)
            {
                if (Achievements[key].ContainsKey("single day") && Achievements[key]["single day"] == "true")
                {
                    Achievements[key]["progress"] = "0";
                }
            }
        }

        public static void ResetSingleRunAchievements()
        {
            List<string> keys = [.. Achievements.Keys];
            foreach (string key in keys)
            {
                if (CheckIfSingleRun(key))
                {
                    Achievements[key]["progress"] = "0";
                    Instance.SaveAchievementOnServerRpc(key, 0, GameNetworkManager.Instance.localPlayerController.playerSteamId);
                }
            }
        }

        public static void CreateAchievements()
        {
            Util.Paths.CheckFolders();

            string path = Path.Combine(Util.Paths.DataFolder, "achievements.json");

            if (!File.Exists(path) || Instance.resetAchievements.Value)
            {
                File.WriteAllText(path, "");
            }

            string pluginsPath = Paths.PluginPath;
            string[] plugins = Directory.GetDirectories(pluginsPath);

            foreach (string modFolder in plugins)
            {
                if (!Directory.Exists(modFolder)) continue;
                string everyAchievementPath = Path.Combine(modFolder, "achievements.json");
                if (!File.Exists(everyAchievementPath)) continue;
                string everyAchievement = File.ReadAllText(everyAchievementPath);
                Dictionary<string, Dictionary<string, string>>? internalJson = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(everyAchievement);

                string currentAchievements = File.ReadAllText(path);
                Dictionary<string, Dictionary<string, string>>? currentJson = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(currentAchievements);

                if (internalJson != null)
                {
                    foreach (var dict in internalJson)
                    {
                        if (currentJson != null)
                        {
                            if (currentJson.ContainsKey(dict.Key))
                            {
                                internalJson[dict.Key]["progress"] = currentJson[dict.Key]["progress"];
                                internalJson[dict.Key]["completed"] = currentJson[dict.Key]["completed"];
                            }
                        }

                        Achievements.Add(dict.Key, dict.Value);
                        Logger.LogInfo("Added Achievement " + dict.Key);
                    }
                }
            }

            WriteAchievements();

        }

        public static void WriteAchievements()
        {
            Util.Paths.CheckFolders();

            string path = Path.Combine(Util.Paths.DataFolder, "achievements.json");

            File.WriteAllText(path, JsonConvert.SerializeObject(Achievements));
        }

        public static void OnAchievementsClicked()
        {
            var QMM = GameNetworkManager.Instance.localPlayerController.quickMenuManager;
            QMM.mainButtonsPanel.SetActive(false);
            QMM.playerListPanel.SetActive(false);

            List<string> keys = [.. Achievements.Keys];

            CheckAchievementCount();

            int count = 0;

            for (int i = 0; i < keys.Count; i++)
            {
                if (Achievements[keys[i]].ContainsValue("Unique") && !CheckIfSingleRun(keys[i]))
                {
                    Achievements[keys[i]]["progress"] = ES3.Load(keys[i], GameNetworkManager.generalSaveDataName, new List<string>()).Count.ToString();
                }


                var TMP = CreateAchievementsText(new Vector2(-150, 450 - count * 100), true);
                count++;
                string text = keys[i];
                if (int.TryParse(Achievements[keys[i]]["count"], out int result))
                {
                    if (result > 1)
                        text += " " + (int.Parse(Achievements[keys[i]]["progress"]) <= result ? Achievements[keys[i]]["progress"] : result) + " / " + result;
                }
                TMP.text = text;
                if (Achievements[keys[i]]["completed"] == "true")
                {
                    TMP.color = Color.yellow;
                }
            }
        }

        public static void CheckAchievementCount()
        {
            List<string> keys = [.. Achievements.Keys];

            foreach (string key in keys)
            {
                if (Achievements[key].ContainsKey("count callback"))
                {
                    if (Achievements[key]["count callback"] == "Unique Scrap Total")
                    {
                        int scrapItems = 0;

                        foreach (var scrap in StartOfRound.Instance.allItemsList.itemsList)
                        {
                            if (scrap.isScrap) scrapItems++;
                        }

                        Achievements[key]["count"] = scrapItems.ToString();
                    }
                    else if (Achievements[key]["count callback"] == "All Scrap Today")
                    {
                        if (RoundManager.Instance.dungeonFinishedGeneratingForAllPlayers)
                        {
                            int outsideShip = 0;
                            int insideShip = 0;
                            foreach (GrabbableObject grabbable in Object.FindObjectsOfType<GrabbableObject>())
                            {
                                if (grabbable.itemProperties && grabbable.itemProperties.isScrap)
                                {
                                    if (!grabbable.isInShipRoom)
                                    {
                                        outsideShip++;
                                    }
                                    else if (RoundManager.Instance.scrapCollectedThisRound.Contains(grabbable))
                                    {
                                        insideShip++;
                                    }
                                }
                            }

                            Achievements[key]["count"] = (outsideShip + insideShip).ToString();
                        }
                    }
                }
            }
        }

        public static TextMeshProUGUI CreateAchievementsText(Vector2 pos, bool add, int size = 36)
        {
            if (AchievementsCanvas == null)
                CreateAchievementsCanvas();

            var obj = new GameObject("AchievementsText" + pos.y);
            obj.transform.SetParent(AchievementsCanvas.transform, false);

            obj.AddComponent<CanvasRenderer>();
            TextMeshProUGUI TMP = obj.AddComponent<TextMeshProUGUI>();

            TMP.font = AchievementsFont;
            TMP.fontSize = size;
            TMP.characterSpacing = 8f;

            RectTransform rt = TMP.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(900, 60);
            rt.anchoredPosition = pos;

            if (add) AchievementsText.Add(obj);

            return TMP;
        }

        public static TextMeshProUGUI CreateAchievementGetText(string text)
        {
            if (AchievementsCanvas == null)
                CreateAchievementsCanvas();

            if (achievementGetText != null)
                Object.Destroy(achievementGetText);

            if (achievementGetTextSubtitle != null)
                Object.Destroy(achievementGetTextSubtitle);

            achievementGetText = new GameObject("AchievementGetText");
            achievementGetText.transform.SetParent(AchievementsCanvas.transform, false);

            achievementGetText.AddComponent<CanvasRenderer>();
            TextMeshProUGUI TMP = achievementGetText.AddComponent<TextMeshProUGUI>();

            TMP.font = AchievementsFont;
            TMP.fontSize = 128;
            TMP.text = "ACHIEVEMENT GET!";
            TMP.m_textAlignment = TextAlignmentOptions.Center;
            TMP.color = Color.yellow;
            TMP.SetOutlineThickness(0.05f);
            TMP.SetOutlineColor(Color.black);
            TMP.characterSpacing = 8f;

            RectTransform rt = TMP.GetComponent<RectTransform>();
            rt.anchoredPosition = rt.anchoredPosition + new Vector2(300, 500);
            rt.sizeDelta = rt.sizeDelta + new Vector2(1500, 0);

            achievementGetTextSubtitle = new GameObject("AchievementGetText");
            achievementGetTextSubtitle.transform.SetParent(AchievementsCanvas.transform, false);
            achievementGetTextSubtitle.AddComponent<CanvasRenderer>();

            TextMeshProUGUI TMP2 = achievementGetTextSubtitle.AddComponent<TextMeshProUGUI>();

            TMP2.font = AchievementsFont;
            TMP2.fontSize = 64;
            TMP2.text = text;
            TMP2.m_textAlignment = TextAlignmentOptions.Center;
            TMP2.color = Color.yellow;
            TMP2.SetOutlineThickness(0.05f);
            TMP2.SetOutlineColor(Color.black);
            TMP2.characterSpacing = 8f;

            RectTransform rt2 = TMP2.GetComponent<RectTransform>();
            rt2.sizeDelta = rt2.sizeDelta + new Vector2(1500, 0);
            rt2.anchoredPosition = rt.anchoredPosition - new Vector2(0, 100);

            destroyTimer = 0;
            HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.levelIncreaseSFX);

            return TMP;
        }

        public static void CreateAchievementsCanvas()
        {
            GameObject canvasObj = new GameObject("AchievementsCanvas");
            AchievementsCanvas = canvasObj.AddComponent<Canvas>();
            AchievementsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        public static void CreateAchievementDescription(TextMeshProUGUI tmp, string text)
        {
            if (text == prevText) return;
            if (TMP != null) UnityEngine.Object.Destroy(TMP.gameObject);
            TMP = NachoAchievements.CreateAchievementsText(tmp.GetComponent<RectTransform>().anchoredPosition + new Vector2(600, 0), false);
            TMP.text = text;
            prevText = text;
        }
    }
}
