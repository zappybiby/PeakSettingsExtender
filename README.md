# SettingsExtenderForked

This is a fork of the original [PeakSettingsExtender by jspapp](https://thunderstore.io/c/peak/p/JSPAPP/Settings_Extender/) with a few improvements to address the issues caused by Patch 1.7 as well as optional helpers that reduce boilerplate.

**Full Backward Compatibility:**  
Mods built using the original SettingsExtender should work with this forked version without any changes.

---

## Main Improvements

- **Patch 1.7 Compatible**  
  - Fixes the issue where your settings page tab would be renamed to “GENERAL.”  
  - Fixes the problem where every setting had “LOC:” in the name. 

- **Additional Helpers added to API**
  - See Github page for more info

## **Declarative Syntax with `[ExtenderSetting]` Attribute**  

  #### Original
  ```csharp
  // In your Plugin's Awake() method:
  SettingsRegistry.Register("My Mod Page");

  // In your setting's class file:
  internal class MyToggleSetting : BoolSetting, IExposedSetting
  {
      // You had to implement these methods yourself.
      public string GetDisplayName() => "Enable Awesome Feature";
      public string GetCategory()    => SettingsRegistry.GetPageId("My Mod Page");

      // ... other required methods
  }
  ```

  #### Forked
  ```csharp
  // No code needed in Awake()!

  // In your setting's class file:
  [ExtenderSetting(page: "My Mod Page", displayName: "Enable Awesome Feature")]
  internal class MyToggleSetting : ExtenderBoolSetting
  {
      // GetDisplayName() and GetCategory() are handled by the base class.
      // ... other logic
  }
  ```

## **Boilerplate-Reducing Base Classes**  

  #### Original
  ```csharp
  internal class MyToggleSetting : BoolSetting, IExposedSetting
  {
      // Required for UI
      public string GetDisplayName() => "My Toggle";
      public string GetCategory()    => "Some_Category_ID";

      // Required for functionality
      public override void ApplyValue()   { /* Your logic here */ }
      protected override bool GetDefaultValue() => true;

      // Required boilerplate for BoolSetting
      public override LocalizedString OnString  => null;
      public override LocalizedString OffString => null;
  }
  ```

  #### Forked
  ```csharp
  [ExtenderSetting("My Page", "My Toggle")]
  internal class MyToggleSetting : ExtenderBoolSetting
  {
      // ApplyValue is handled by the constructor callback (see next point).
      // OnString and OffString are handled by the base class.

      // You only need to provide what's unique to your setting!
      protected override bool GetDefaultValue() => true;
  }
  ```

## **Simplified OnChanged Callbacks**  

  #### Original
  ```csharp
  internal class MyVolumeSetting : FloatSetting, IExposedSetting
  {
      public override void ApplyValue()
      {
          // Call a method in your main plugin class
          MyPlugin.Instance.UpdateVolume(this.Value);
      }
      // ... other boilerplate
  }
  ```

  #### Forked
  ```csharp
  [ExtenderSetting("Audio", "Master Volume")]
  internal class MyVolumeSetting : ExtenderFloatSetting
  {
      public MyVolumeSetting() : base(newVolumeValue =>
      {
          // Your logic is now a clean one-liner.
          MyPlugin.Instance.UpdateVolume(newVolumeValue);
      })
      {
      }

      // ... other logic
  }
  ```

## **One-Liner Setting Registration**  

  #### Original
  ```csharp
  // In your Plugin's Start() method:
  var mySetting = new MyAwesomeSetting();
  SettingsHandler.Instance.AddSetting(mySetting);
  ```

  #### Forked
  ```csharp
  // In your Plugin's Start() method:
  var mySetting = SettingsHandler.Instance.Add<MyAwesomeSetting>();
  ```

## **Expanded Setting Types (e.g., Generic Enum)**  

  #### Original
  ```csharp
  public enum QualityLevel { Low, Medium, High }

  internal class QualitySetting : IntSetting, IEnumSetting, IExposedSetting
  {
      // A lot of manual implementation was needed.
      public List<string> GetUnlocalizedChoices() 
          => new List<string>(Enum.GetNames(typeof(QualityLevel)));

      public new GameObject GetSettingUICell() 
          => SingletonAsset<InputCellMapper>.Instance.EnumSettingCell;

      public int GetValue() => Value;
      public void SetValue(int v, ISettingHandler h, bool ui) 
          => base.SetValue(v, h);
      // ... plus all the IExposedSetting and other boilerplate.
  }
  ```

  #### Forked
  ```csharp
  public enum QualityLevel { Low, Medium, High }

  [ExtenderSetting("Graphics", "Texture Quality")]
  internal class QualitySetting : ExtenderEnumSetting<QualityLevel>
  {
      // That's it! The base class handles enum names
      // and all the IEnumSetting logic for you.
  }
  ```
