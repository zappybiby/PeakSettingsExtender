## How to Use

This guide will walk you through creating your first custom settings page from scratch. The process is designed to be simple and requires minimal code.

Creating settings for your mod involves two main steps:

1. **Define a Class for Each Setting:** Each toggle, slider, or dropdown in your menu will be its own C# class.  
2. **Register the Setting:** In your main plugin file, you'll tell the game to create and display the setting you defined.

Let's build a settings page for a hypothetical mod called "CoolMod".

### Step 1: Defining Your Setting Class

For each setting you want to add, you create a new class. This fork provides several `Extender...Setting` base classes to make this incredibly easy.

#### A. Choose a Base Class

First, decide what kind of setting you need and pick the corresponding base class:

- `ExtenderBoolSetting`: For a simple on/off toggle.  
- `ExtenderFloatSetting`: For a slider that controls a decimal value.  
- `ExtenderIntSetting`: For a slider that controls a whole number.  
- `ExtenderEnumSetting<T>`: For a dropdown menu based on a C# `enum`.  
- `ExtenderStringSetting`: For a text input field.  
- `ExtenderKeyCodeSetting`: For a keybind input.  
- `ExtenderButtonSetting`: For a clickable button.  

#### B. Create the Class

Let's create a simple toggle that enables a feature in "CoolMod".

1. **Inherit from `ExtenderBoolSetting`.**  
2. Add the `[ExtenderSetting]` attribute above the class.  
   - `page`: This is the name of the tab your setting will appear on. All settings with the same page name will be grouped together.  
   - `displayName`: This is the label that appears next to your setting in the menu.  
3. Define the setting's default value by overriding `GetDefaultValue()`.  
4. (Optional) Add a constructor that passes a callback to `base()` to react when the setting is changed.

Here is the complete class for our toggle:

```csharp
// CoolFeatureToggle.cs

using SettingsExtender;
using Zorro.Settings;

// The attribute defines all the UI text and location for this setting.
[ExtenderSetting(page: "CoolMod", displayName: "Enable Cool Feature")]
internal class CoolFeatureToggle : ExtenderBoolSetting
{
    // This constructor is where you define what happens when the value changes.
    // The `_` means we don't need the new boolean value, we just need to know it changed.
    public CoolFeatureToggle() : base(_ => CoolModPlugin.Instance.ApplySettings())
    {
    }
    
    // This is the value the setting will have by default.
    protected override bool GetDefaultValue() => true;
}
```

### Step 2: Registering Your Setting

Now that you've defined your setting, you need to tell the game to load it. This is done in your main BepInEx plugin file.

1. In your plugin's `Start()` method, get the game's `SettingsHandler`.  
2. Use the `Add<T>()` method provided by this fork to instantiate and register your setting in one line.  
3. Store the returned instance in a static field so you can access its value from anywhere in your mod.

Here's what your main plugin file would look like:

```csharp
// CoolModPlugin.cs

using BepInEx;
using Zorro.Settings;
using SettingsExtender; // Don't forget this!

[BepInPlugin("com.myname.coolmod", "CoolMod", "1.0.0")]
public class CoolModPlugin : BaseUnityPlugin
{
    public static CoolModPlugin Instance { get; private set; }
    
    // A static field to hold our setting instance.
    internal static CoolFeatureToggle coolFeatureToggle;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Add<T> creates the setting, registers it, and returns the instance.
        coolFeatureToggle = SettingsHandler.Instance.Add<CoolFeatureToggle>();
        
        Logger.LogInfo("CoolMod settings registered!");
    }

    // You can now read the setting's value anywhere in your code!
    private void Update()
    {
        if (coolFeatureToggle.Value)
        {
            // Do the cool feature...
        }
    }
    
    // This is the method our setting's callback will execute.
    public void ApplySettings()
    {
        Logger.LogInfo("Settings have been applied! The new value for Cool Feature is: " + coolFeatureToggle.Value);
    }
}
```

---

### Exploring Other Setting Types

The process is similar for other types. Here are a couple more examples.

#### Example: A Float Setting (Slider)

Let's add a "Feature Intensity" slider.

```csharp
using Unity.Mathematics; // Required for float2
using SettingsExtender;
using Zorro.Settings;

[ExtenderSetting(page: "CoolMod", displayName: "Feature Intensity")]
internal class IntensitySlider : ExtenderFloatSetting
{
    public IntensitySlider() : base(_ => CoolModPlugin.Instance.ApplySettings()) { }
    
    // Set the default value.
    protected override float GetDefaultValue() => 0.5f;

    // Set the slider's minimum and maximum values.
    protected override float2 GetMinMaxValue() => new float2(0f, 1f);

    // (Optional) Customize how the value is displayed in the menu.
    // Here, we show it as a percentage.
    public override string Expose(float value) => (value * 100f).ToString("F0") + "%";
}
```

To register it, just add another line in your `Start()` method:

```csharp
// In CoolModPlugin.Start()
intensitySlider = SettingsHandler.Instance.Add<IntensitySlider>();
```

#### Example: An Enum Setting (Dropdown)

This is perfect for quality levels or modes. This fork makes it incredibly simple.

First, define your `enum`:

```csharp
public enum EffectQuality
{
    Low,
    Medium,
    High,
    Ultra
}
```

Now, create the setting class. `ExtenderEnumSetting<T>` handles everything for you!

```csharp
using SettingsExtender;
using Zorro.Settings;

[ExtenderSetting(page: "CoolMod", displayName: "Effect Quality")]
internal class QualityDropdown : ExtenderEnumSetting<EffectQuality>
{
    // The base class automatically uses the enum names for the choices.
    // You only need to define the default value.
    protected override EffectQuality GetDefaultValue() => EffectQuality.Medium;
}
```

Register it just like the others:

```csharp
// In CoolModPlugin.Start()
qualityDropdown = SettingsHandler.Instance.Add<QualityDropdown>();
```

You can then access the selected value with `qualityDropdown.Value`, which will be of type `EffectQuality`.
