// -----------------------------------------------------------------------------
//  SettingsExtenderForked – v0.1.0
//  Based on PeakSettingsExtender v0.1.0
//    by jspapp (https://github.com/jspapp/PeakSettingsExtender)
// -----------------------------------------------------------------------------

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SettingsExtender;
public static class PluginInfo
{
	public const string PLUGIN_GUID    = "com.pharmacomaniac.settingsextenderforked";
	public const string PLUGIN_NAME    = "Settings Extender Forked";
	public const string PLUGIN_VERSION = "0.1.0";
}
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger = null!;
    private GameObject? buttonSrc;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void Start()
    {
        //
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (updateTabsRoutine != null)
        {
            StopCoroutine(updateTabsRoutine);
            updateTabsRoutine = null;
        }
    }

    private Coroutine? updateTabsRoutine;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        buttonSrc = null;

        if (updateTabsRoutine != null)
        {
            StopCoroutine(updateTabsRoutine);
            updateTabsRoutine = null;
        }

        updateTabsRoutine = StartCoroutine(UpdateTabs(scene));
    }

    private IEnumerator UpdateTabs(Scene scene)
    {
        Logger.LogInfo("Scene loadeeeed: " + scene.name);

        while (buttonSrc == null) {
            List<SettingsTABSButton> buttons = FindAllInScene(scene);
            if (buttons.Count != 0) {
                buttonSrc = buttons[0].gameObject;
            }
            yield return new WaitForSeconds(.050f);
        }

        if (buttonSrc != null)
        {
            Logger.LogInfo("Found TABS/General");

            foreach (var (name, category) in SettingsRegistry.GetPages())
            {
                Logger.LogInfo($"Creating {name} with category {category}");
                GameObject newButton = Instantiate(buttonSrc, buttonSrc.transform.parent);
                // Fix General tab issue: prevent LocalizedText overwrite
                var loc = newButton.GetComponentInChildren<LocalizedText>();
                if (loc != null)
                    Destroy(loc);
                SettingsTABSButton tabsButton = newButton.GetComponent<SettingsTABSButton>();
                tabsButton.category = category;
                tabsButton.text.text = name;
            }
        }
    }

    public static List<SettingsTABSButton> FindAllInScene(Scene scene)
    {
        var result = new List<SettingsTABSButton>();
        foreach (var root in scene.GetRootGameObjects())
        {
            var found = root.GetComponentsInChildren<SettingsTABSButton>();
            result.AddRange(found);
        }
        return result;
    }
}

public class SettingsRegistry
{
    internal static Dictionary<string, SettingsCategory> nameToCategoryId = new();

    public static void Register(string name)
    {
        if (!nameToCategoryId.ContainsKey(name))
        {
            SettingsCategory highestId = Enum.GetValues(typeof(SettingsCategory)).Cast<SettingsCategory>().Max();
            if (nameToCategoryId.Count != 0)
            {
                highestId = nameToCategoryId.Values.Max();
            }

            nameToCategoryId[name] = highestId + 1;
        }
    }

    public static string GetPageId(string name)
    {
        return nameToCategoryId[name].ToString();
    }

    public static Dictionary<string, SettingsCategory> GetPages() {
        return nameToCategoryId;
    }
}

// Prefix patch to skip LOC fallback
[HarmonyPatch(typeof(LocalizedText), "GetText", new Type[] { typeof(string), typeof(bool) })]
static class GetTextPrefixPatch
{
    static bool Prefix(string id, bool printDebug, ref string __result)
    {
        var upper = id.ToUpperInvariant();
        if (LocalizedText.mainTable.TryGetValue(upper, out var row))
            return true;
        __result = upper;
        return false;
    }
}
