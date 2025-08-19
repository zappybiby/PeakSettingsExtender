using BepInEx;
using UnityEngine;
using Zorro.Settings;
using SettingsExtender;
using Unity.Mathematics;

namespace HelloWorldExample;

// Define plugin metadata and declare a dependency on the forked SettingsExtender
[BepInPlugin("com.myname.helloworldexample", "HelloWorldExample", "1.0.0")]
// [BepInDependency(SettingsExtender.PluginInfo.PLUGIN_GUID)]
public class Plugin : BaseUnityPlugin
{
    // A singleton instance for easy access from other classes (like our setting class)
    public static Plugin Instance { get; private set; }

    // A static field to hold our setting instance so we can read its value
    internal static FontSizeSetting MyFontSizeSetting;
    
    // A style object to control how our text is drawn
    private GUIStyle _textStyle;

    private void Awake()
    {
        Instance = this;
        Logger.LogInfo("Hello World Plugin is loaded!");
    }

    private void Start()
    {
        // 1. REGISTER THE SETTING
        // Use the new Add<T>() method. It creates the setting, registers it,
        // and returns the instance all in one clean line.
        MyFontSizeSetting = SettingsHandler.Instance.AddSetting<FontSizeSetting>();
        
        // 2. INITIALIZE THE GUI STYLE
        // Create a style for our OnGUI text. We do this once in Start() for performance.
        _textStyle = new GUIStyle();
        _textStyle.alignment = TextAnchor.UpperRight; // Align text to the top-right
        _textStyle.normal.textColor = Color.white;
        
        // 3. APPLY THE INITIAL SETTING VALUE
        // Call our update method once at the start to set the initial font size.
        UpdateFontSize();
    }

    /// <summary>
    /// This is the callback method that our setting will trigger when its value changes.
    /// It updates the font size of our GUIStyle.
    /// </summary>
    public void UpdateFontSize()
    {
        // Guard against this being called before everything is ready
        if (_textStyle == null || MyFontSizeSetting == null) return;

        // Read the .Value from our setting and apply it
        _textStyle.fontSize = (int)MyFontSizeSetting.Value;
        
        Logger.LogInfo($"Hello World font size updated to: {_textStyle.fontSize}px");
    }

    private void OnGUI()
    {
        // Guard clause to prevent drawing before the setting is initialized
        if (MyFontSizeSetting == null) return;
        
        // Define the rectangle for our text. 
        // We position it 10 pixels from the right edge and 10 pixels from the top.
        var drawRect = new Rect(0, 10, Screen.width - 10, 100);

        // Draw the label using our custom style
        GUI.Label(drawRect, "Hello World", _textStyle);
    }
}

// ═════════════════════════════════════════════════════════════════════════
//  SETTING CLASS DEFINITION
// ═════════════════════════════════════════════════════════════════════════

/// <summary>
/// The [ExtenderSetting] attribute defines the setting's page and display name.
/// The plugin automatically creates the "Hello World" page for us.
/// </summary>
[ExtenderSetting(page: "Hello World", displayName: "Font Size")]
internal class FontSizeSetting : ExtenderFloatSetting
{
    /// <summary>
    /// The constructor passes a callback to the base class.
    /// This lambda function will be executed whenever the setting's value is applied.
    /// </summary>
    public FontSizeSetting() : base(_ => Plugin.Instance?.UpdateFontSize())
    {
    }

    // Set the default value the slider will have on first launch.
    protected override float GetDefaultValue() => 30f;

    // Set the slider's minimum and maximum values.
    protected override float2 GetMinMaxValue() => new float2(12f, 70f);

    /// <summary>
    /// Override Clamp() to control the slider's behavior.
    /// Here, we round the value to the nearest multiple of 2 to create steps.
    /// </summary>
    public override float Clamp(float value)
    {
        // The formula for stepping is: round(value / step) * step
        float steppedValue = Mathf.Round(value / 2f) * 2f;
        
        // Use the base class MinValue and MaxValue properties for clamping
        return Mathf.Clamp(steppedValue, MinValue, MaxValue);
    }
    
    /// <summary>
    /// Override Expose() to customize the text displayed next to the slider.
    /// We format it as a whole number with "px" at the end.
    /// </summary>
    public override string Expose(float value)
    {
        // We apply the same stepping logic here to ensure the display is consistent
        float steppedValue = Mathf.Round(value / 2f) * 2f;
        return steppedValue.ToString("F0") + " px";
    }
}
