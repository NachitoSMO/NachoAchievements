using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Dissonance;
using GameNetcodeStuff;
using HarmonyLib;
using NachoAchievements.Patches;
using Newtonsoft.Json;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NachoAchievements
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class NachoAchievements : BaseUnityPlugin
    {
        public static NachoAchievements Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        public static Dictionary<string, Dictionary<string, int>> Achievements = new Dictionary<string, Dictionary<string, int>>();

        public static Dictionary<string, string> AchievementDescriptions = new Dictionary<string, string>();

        public static AssetBundle NachoAssets;

        public static TMP_FontAsset AchievementsFont;

        public static List<GameObject> AchievementsText = new List<GameObject>();

        public static Canvas AchievementsCanvas;

        public static Dictionary<string, int> scrapFound = new Dictionary<string, int>();

        public static List<string> enemiesKilled = new List<string>();

        public static GameObject achievementGetText = null!;

        public static GameObject achievementGetTextSubtitle = null!;

        public static TextMeshProUGUI achievementEnterButton = null!;

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

            resetAchievements = Config.Bind<bool>("Debug", "Reset Achievements", false, new ConfigDescription("Wether to reset every achievement next time you boot up the game"));

            Achievements = GetAchievements();

            AchievementDescriptions = GetAchievementDescriptions();

            WriteAchievements();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private void Update()
        {
            if (achievementEnterButton != null)
            {
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
                    if (tmp != null && tmp.text == "> Achievements")
                    {
                        if (Mouse.current.leftButton.isPressed)
                        {
                            OnAchievementsClicked();
                        }
                    }
                }
            }

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
                        string input = tmp.text;
                        bool containsDigit = false;

                        for (int i = 0; i < input.Length; i++)
                        {
                            if (char.IsDigit(input[i]))
                            {
                                containsDigit = true;
                            }
                        }

                        if (containsDigit)
                        {
                            int index = input.IndexOf(input.First(char.IsDigit));
                            input = input.Substring(0, index).Trim();
                        }


                        if (NachoAchievements.AchievementDescriptions.ContainsKey(input))
                        {
                            string output = NachoAchievements.AchievementDescriptions[input];
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

        public static string LoadEmbeddedJson(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = fileName;

            using (Stream stream = assembly.GetManifestResourceStream("NachoAchievements." + resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }

        public static void AddAchievement(string achievement)
        {
            List<string> keys = [.. Achievements.Keys];

            foreach (string key in keys)
            {
                if (key == achievement)
                {
                    Achievements[key]["progress"] += 1;

                    List<string> keys2 = [.. Achievements[key].Keys];
                    int checkCompCount = 0;
                    foreach (string a in keys2)
                    {
                        if (a == "progress") continue;
                        if (a == "completed") continue;
                        checkCompCount++;
                        if (Achievements[key][a] <= Achievements[key]["progress"] && Achievements[key]["completed"] < checkCompCount)
                        {
                            CreateAchievementGetText(a);
                            Achievements[key]["completed"] = checkCompCount;
                            if (achievement != "getAll") AddAchievement("getAll");
                        }
                    }
                }
            }

            WriteAchievements();
        }

        public static Dictionary<string, Dictionary<string, int>> GetAchievements()
        {
            Util.Paths.CheckFolders();

            string path = Path.Combine(Util.Paths.DataFolder, "advancements.json");

            if (!File.Exists(path) || Instance.resetAchievements.Value)
            {
                File.WriteAllText(path, "");
            }

            string everyAchievement = LoadEmbeddedJson("EveryAchievement.json");
            Dictionary<string, Dictionary<string, int>>? internalJson = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(everyAchievement);

            string currentAchievements = File.ReadAllText(path);
            Dictionary<string, Dictionary<string, int>>? currentJson = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(currentAchievements);

            if (currentJson != null && internalJson != null)
            {
                foreach (string key in internalJson.Keys)
                {
                    if (currentJson.ContainsKey(key))
                    {
                        internalJson[key]["progress"] = currentJson[key]["progress"];
                        internalJson[key]["completed"] = currentJson[key]["completed"];
                    }
                }
            }

            return internalJson ?? new Dictionary<string, Dictionary<string, int>>();
            
        }

        public static Dictionary<string, string> GetAchievementDescriptions()
        {
            string everyDescription = LoadEmbeddedJson("EveryAchievementDescription.json");
            Dictionary<string, string>? internalJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(everyDescription);

            return internalJson ?? new Dictionary<string, string>();

        }

        public static void WriteAchievements()
        {
            Util.Paths.CheckFolders();

            string path = Path.Combine(Util.Paths.DataFolder, "advancements.json");

            File.WriteAllText(path, JsonConvert.SerializeObject(Achievements));
        }

        public static void OnAchievementsClicked()
        {
            var QMM = GameNetworkManager.Instance.localPlayerController.quickMenuManager;
            QMM.mainButtonsPanel.SetActive(false);
            QMM.playerListPanel.SetActive(false);
            Object.Destroy(achievementEnterButton.gameObject);

            List<string> keys = [.. Achievements.Keys];

            int count = 0;

            for (int i = 0; i < keys.Count; i++)
            {
                List<string> achievements = [.. Achievements[keys[i]].Keys];
                int checkCompCount = 0;
                for (int j = 0; j < achievements.Count; j++)
                {
                    if (achievements[j] == "progress") continue;
                    if (achievements[j] == "completed") continue;
                    var TMP = CreateAchievementsText(new Vector2(-150, 450 - count * 100), true);
                    checkCompCount++;
                    count++;
                    string text = achievements[j];
                    if (Achievements[keys[i]][achievements[j]] > 1) text += " " + (Achievements[keys[i]]["progress"] <= Achievements[keys[i]][achievements[j]] ? Achievements[keys[i]]["progress"] : Achievements[keys[i]][achievements[j]]) + " / " + (Achievements[keys[i]][achievements[j]] != 999 ? Achievements[keys[i]][achievements[j]] : "?");
                    TMP.text = text;
                    if (Achievements[keys[i]]["completed"] >= checkCompCount)
                    {
                        TMP.color = Color.yellow;
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

        public IEnumerator CheckSingleRunProgress()
        {
            yield return new WaitForSeconds(0.4f);
            Instance.ResetSingleRunProgress();

            Instance.AddTojetpackCollectsingleRunProgress();
            Instance.AddToWalkieCollectProgress();
            Instance.AddToMuseumProgress();
            Instance.AddToSigurdLoreProgress();
            Instance.AddToInteriorDecoratorProgress();
            Instance.AddToMoonsVisitedProgressServerRpc();

            List<string> keys1 = [.. scrapFound.Keys];

            foreach (string scrap in keys1)
            {
                scrapFound[scrap] = 0;
            }

            List<string> keys = [.. Achievements.Keys];

            foreach (string achievement in keys)
            {
                if (achievement == "singleRunEveryEnemyKilled")
                {
                    Instance.ReportEnemyKilled(SteamClient.SteamId, string.Empty);
                    Achievements[achievement]["progress"] = enemiesKilled.Count;
                }

                List<string> keys2 = [.. Achievements[achievement].Keys];
                int checkCompCount = 0;

                foreach (string a in keys2)
                {
                    if (a == "progress") continue;
                    if (a == "completed") continue;
                    checkCompCount++;
                    if (Achievements[achievement]["progress"] >= Achievements[achievement][a] && Achievements[achievement]["completed"] < checkCompCount)
                    {
                        CreateAchievementGetText(a);
                        Achievements[achievement]["completed"] = checkCompCount;
                        if (achievement != "getAll") AddAchievement("getAll");
                    }
                }
            }

            WriteAchievements();

        }

        public void ResetSingleRunProgress()
        {
            List<string> keys = [.. Achievements.Keys];

            foreach (string achievement in keys)
            {
                if (achievement.Contains("singleRun"))
                {
                    Achievements[achievement]["progress"] = 0;
                }
            }
        }

        public void AddToMuseumProgress()
        {
            int museumAchievement = 0;

            GrabbableObject[] array = Object.FindObjectsOfType<GrabbableObject>();
            foreach (var grabbable in array)
            {
                bool inShip = false;

                if (grabbable.itemProperties.isScrap && !grabbable.deactivated && !grabbable.itemUsedUp && StartOfRound.Instance.shipBounds.bounds.Contains(grabbable.gameObject.transform.position))
                {
                    inShip = true;
                }
                

                if (inShip)
                {
                    if (scrapFound.ContainsKey(grabbable.itemProperties.itemName))
                    {
                        if (scrapFound[grabbable.itemProperties.itemName] == 0)
                        {
                            museumAchievement++;
                            scrapFound[grabbable.itemProperties.itemName] = 1;
                        }
                    }
                }
            }

            Achievements["singleRunAllScrapTypes"]["progress"] = museumAchievement;
        }

        public void AddTojetpackCollectsingleRunProgress()
        {
            JetpackItem[] array = Object.FindObjectsOfType<JetpackItem>();

            foreach (var jet in array)
            {
                if (jet.itemProperties.itemName == "Jetpack" && !jet.deactivated && !jet.itemUsedUp && StartOfRound.Instance.shipBounds.bounds.Contains(jet.gameObject.transform.position))
                {
                    AddAchievement("jetpackCollectsingleRun");
                }
            }
        }

        public void AddToInteriorDecoratorProgress()
        {
            var unlockables = StartOfRound.Instance.unlockablesList.unlockables;

            foreach (var unlockable in unlockables)
            {
                if (unlockable.hasBeenUnlockedByPlayer && !unlockable.inStorage && !unlockable.alreadyUnlocked)
                {
                    AddAchievement("decoratorsingleRun");
                }
            }
        }

        public void AddToWalkieCollectProgress()
        {
            WalkieTalkie[] array2 = Object.FindObjectsOfType<WalkieTalkie>();

            foreach (var walkie in array2)
            {
                if (walkie.itemProperties.itemName == "Walkie-talkie" && !walkie.deactivated && !walkie.itemUsedUp && StartOfRound.Instance.shipBounds.bounds.Contains(walkie.gameObject.transform.position))
                {
                    AddAchievement("thirtyWalkiessingleRun");
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddToMoonsVisitedProgressServerRpc()
        {
            StartOfRoundPatches.moonsVisited = ES3.Load("NachoMoonsVisited", GameNetworkManager.Instance.currentSaveFileName, new List<int>());
            AddToMoonsVisitedProgressClientRpc(StartOfRoundPatches.moonsVisited.ToArray());
        }

        [ClientRpc]
        public void AddToMoonsVisitedProgressClientRpc(int[] moons)
        {
            StartOfRoundPatches.moonsVisited = moons.ToList();

            int current = Achievements["visitMoonssingleRun"]["progress"];

            for (int i = current; i < moons.Length; i++)
            {
                AddAchievement("visitMoonssingleRun");
            }
        }

        public void AddToSigurdLoreProgress()
        {
            Terminal terminal = Object.FindObjectOfType<Terminal>();
            int storyLogs = 0;
            int enemyLogs = 0;

            if (terminal != null)
            {
                for (int i = 0; i < terminal.logEntryFiles.Count; i++)
                {
                    if (terminal.logEntryFiles[i].storyLogFileID != -1)
                    {
                        storyLogs++;
                    }
                }

                for (int i = 0; i < terminal.enemyFiles.Count; i++)
                {
                    if (terminal.enemyFiles[i].creatureFileID != -1)
                    {
                        enemyLogs++;
                    }
                }

                Achievements["enemyEntryCollectsingleRun"]["Enemy Master"] = enemyLogs;
                Achievements["sigurdEntryCollectsingleRun"]["Lore Expert"] = storyLogs;

                foreach (var storyLog in terminal.unlockedStoryLogs)
                {
                    AddAchievement("sigurdEntryCollectsingleRun");
                }

                foreach (var enemyLog in terminal.scannedEnemyIDs)
                {
                    AddAchievement("enemyEntryCollectsingleRun");
                }
            }
        }

        public static void AddItems()
        {
            foreach (var item in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (item.isScrap) scrapFound[item.itemName] = 0;
            }

            Achievements["singleRunAllScrapTypes"]["Museum%"] = scrapFound.Count;
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddKillServerRpc(ulong id, string enemyName)
        {
            var list = ES3.Load("NachoEnemiesKilled" + id, GameNetworkManager.Instance.currentSaveFileName, new List<string>());

            if (enemyName != string.Empty && !list.Contains(enemyName))
            {
                list.Add(enemyName);
                ES3.Save("NachoEnemiesKilled" + id, list, GameNetworkManager.Instance.currentSaveFileName);
                AddKillAchievementClientRpc(id);
            }

            AddKillClientRpc(id, list.ToArray());
        }

        [ClientRpc]
        private void AddKillClientRpc(ulong id, string[] list)
        {
            if (id != SteamClient.SteamId)
                return;

            enemiesKilled = list.ToList();
        }

        [ClientRpc]
        public void AddKillAchievementClientRpc(ulong id)
        {
            if (id != SteamClient.SteamId)
                return;

            AddAchievement("singleRunEveryEnemyKilled");
        }
        public void ReportEnemyKilled(ulong killerId, string enemyName)
        {
            AddKillServerRpc(killerId, enemyName);
        }

        public void SetArtFullClearCount()
        {
            Achievements["artFullClear"]["MinMaxing"] = 0;
            GrabbableObject[] array = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
            for (int num6 = 0; num6 < array.Length; num6++)
            {
                if (array[num6].itemProperties.isScrap && !array[num6].isInShipRoom && !array[num6].isInElevator)
                {
                    Achievements["artFullClear"]["MinMaxing"]++;
                }
            }
            
        }

        public IEnumerator CheckKillEnemyAchievements(EnemyAI __instance, int force = 1, PlayerControllerB playerWhoHit = null)
        {
            yield return new WaitForFixedUpdate();
            if (EnemyAIPatches.enemyWasKilled.Contains(__instance) && EnemyAIPatches.enemyWasAlive.Contains(__instance))
            {
                if (playerWhoHit != null && StartOfRound.Instance.localPlayerController == playerWhoHit)
                {
                    if (__instance.enemyType.enemyName == "Nutcracker") NachoAchievements.AddAchievement("killNutcracker");
                    else if (__instance.enemyType.enemyName == "Maneater") NachoAchievements.AddAchievement("killManeater");
                    else if (__instance.enemyType.enemyName == "Flowerman" && force == 1) NachoAchievements.AddAchievement("killBrackenShovel");
                    else if (__instance.enemyType.enemyName == "Manticoil") NachoAchievements.AddAchievement("killManticoil");
                }

                if (playerWhoHit != null)
                    NachoAchievements.Instance.ReportEnemyKilled(playerWhoHit.playerSteamId, __instance.enemyType.enemyName);
            }
            yield return new WaitForFixedUpdate();
            EnemyAIPatches.enemyWasKilled.Clear();
            EnemyAIPatches.enemyWasAlive.Clear();
        }
    }
}
