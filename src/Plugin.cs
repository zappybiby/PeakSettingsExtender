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

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private GameObject buttonSrc;
    private Coroutine updateTabsRoutine;
        
    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void Start() { }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (updateTabsRoutine != null)
        {
            StopCoroutine(updateTabsRoutine);
            updateTabsRoutine = null;
        }
    }

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

        while (buttonSrc == null)
        {
            List<SettingsTABSButton> buttons = FindAllInScene(scene);
            if (buttons.Count != 0)
                buttonSrc = buttons[0].gameObject;
            yield return new WaitForSeconds(.050f);
        }

        foreach (Transform child in buttonSrc.transform.parent)
            if (child != buttonSrc.transform)
                Destroy(child.gameObject);

        Logger.LogInfo("Found TABS/General");

        foreach (var (name, category) in SettingsRegistry.GetPages())
        {
            Logger.LogInfo($"Creating {name} with category {category}");
            GameObject newButton = Instantiate(buttonSrc, buttonSrc.transform.parent);
            var loc = newButton.GetComponentInChildren<LocalizedText>();
            // Fix General tab issue: remove LocalizedText so our custom tab name isn't overwritten
            if (loc != null) Destroy(loc);
            SettingsTABSButton tabsButton = newButton.GetComponent<SettingsTABSButton>();
            tabsButton.category = category;
            tabsButton.text.text = name;
        }
    }

    public static List<SettingsTABSButton> FindAllInScene(Scene scene)
    {
        var result = new List<SettingsTABSButton>();
        foreach (var root in scene.GetRootGameObjects())
            result.AddRange(root.GetComponentsInChildren<SettingsTABSButton>(true));
        return result;
    }
}

public class SettingsRegistry
{
    internal static Dictionary<string, SettingsCategory> nameToCategoryId = new();

    public static void Register(string name)
    {
        if (nameToCategoryId.ContainsKey(name)) return;

        int builtInMax = Enum.GetValues(typeof(SettingsCategory)).Cast<int>().Max();
        int customMax  = nameToCategoryId.Count == 0 ? 0 : nameToCategoryId.Values.Max(v => (int)v);
        nameToCategoryId[name] = (SettingsCategory)(Math.Max(builtInMax, customMax) + 1);
    }

    public static string GetPageId(string name) => nameToCategoryId[name].ToString();

    public static Dictionary<string, SettingsCategory> GetPages() => nameToCategoryId;
}

/* Strip “LOC: ” prefix when localisation key is missing */
[HarmonyPatch(typeof(LocalizedText), "GetText", new Type[] { typeof(string), typeof(bool) })]
static class StripLocPrefixPatch
{
    static void Postfix(ref string __result)
    {
        const string prefix = "LOC: ";
        if (__result.StartsWith(prefix))
            __result = __result.Substring(prefix.Length);
    }
}
