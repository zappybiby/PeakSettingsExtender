using System;

namespace SettingsExtender;

/// <summary>
/// Marks a Setting class with its tab name and the label that should be shown in-game.
/// Use as [ExtenderSetting("My Page Name", "My Setting Display Name")].
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ExtenderSettingAttribute : Attribute
{
    public readonly string Page;
    public readonly string? DisplayName;


    // Constructor renamed to match the class name
    public ExtenderSettingAttribute(string page, string? displayName = null) 
    {
        Page = page;
        DisplayName = displayName;
    }
}