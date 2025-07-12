using System;
using System.Reflection;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Localization;
using Zorro.Settings;
using Zorro.Core;

namespace SettingsExtender;

/// <summary>Shared helper – pulls [ExtenderMeta] and auto-registers the tab.</summary>
internal static class ExtenderHelper
{
    internal static (string pageId, string display) GetMeta(System.Type t)
    {
        var meta = t.GetCustomAttribute<ExtenderSettingAttribute>();
        string page = meta?.Page ?? "General";
        string display = meta?.DisplayName ?? t.Name;
        // If another mod already registered the same tab this is a no-op
        SettingsRegistry.Register(page);
        return (SettingsRegistry.GetPageId(page), display);
    }
}

/* ==== one liner abstractions ==== */

public abstract class ExtenderBoolSetting : BoolSetting, IExposedSetting
{
    readonly string _pageId;
    readonly string _display;
    protected ExtenderBoolSetting()
    {
        (_pageId, _display) = ExtenderHelper.GetMeta(GetType());
    }

    // implement IExposedSetting
    public string GetCategory()    => _pageId;
    public string GetDisplayName() => _display;

    // optional sugar-hook a callback
     protected readonly System.Action<bool>? _onChanged;
    protected ExtenderBoolSetting(System.Action<bool> cb) : this() => _onChanged = cb;
    protected override bool GetDefaultValue() => false;
    public override void ApplyValue() => _onChanged?.Invoke(Value);
    public override LocalizedString OnString  => null!;
    public override LocalizedString OffString => null!;
}

public abstract class ExtenderFloatSetting : FloatSetting, IExposedSetting
{
    readonly string _pageId;
    readonly string _display;
    protected ExtenderFloatSetting()
    {
        (_pageId, _display) = ExtenderHelper.GetMeta(GetType());
    }
    public string GetCategory()    => _pageId;
    public string GetDisplayName() => _display;

    protected readonly System.Action<float>? _onChanged;

    protected ExtenderFloatSetting(System.Action<float> cb) : this() => _onChanged = cb;

    public override void ApplyValue() => _onChanged?.Invoke(Value);
}

public abstract class ExtenderIntEnumSetting : IntSetting, IEnumSetting, IExposedSetting
{
    readonly string _pageId;
    readonly string _display;
    protected ExtenderIntEnumSetting()
    {
        (_pageId, _display) = ExtenderHelper.GetMeta(GetType());
    }
    public string GetCategory()    => _pageId;
    public string GetDisplayName() => _display;

    protected readonly System.Action<int>? _onChanged;

    protected ExtenderIntEnumSetting(System.Action<int> cb) : this() => _onChanged = cb;

    // —— IEnumSetting pass-through ——
    public virtual List<LocalizedString>? GetLocalizedChoices()  => null;

    public abstract List<string>         GetUnlocalizedChoices();
    public new virtual GameObject GetSettingUICell() => 
        SingletonAsset<InputCellMapper>.Instance.EnumSettingCell;
    public int  GetValue()                        => Value;
    public void SetValue(int v,ISettingHandler h,bool ui) => base.SetValue(v,h);
    public override void ApplyValue()             => _onChanged?.Invoke(Value);
}

public abstract class ExtenderButtonSetting : ButtonSetting, IExposedSetting
{
    readonly string _pageId;
    readonly string _display;

    protected ExtenderButtonSetting()
    {
        (_pageId, _display) = ExtenderHelper.GetMeta(GetType());
    }

    /* IExposedSetting */
    public string GetCategory()    => _pageId;
    public string GetDisplayName() => _display;

    /* mandatory abstract members from ButtonSetting still need to be
       implemented by the concrete setting class:
         - string GetButtonText()
         - void   OnClicked(ISettingHandler) */
}

/* ════════════════════════════════════════════════════════════════════════ */
/*  2. STRING                                                               */
public abstract class ExtenderStringSetting : StringSetting, IExposedSetting
{
    readonly string _pageId;
    readonly string _display;
    protected readonly Action<string>? _onChanged;


    protected ExtenderStringSetting(Action<string>? cb = null)
    {
        (_pageId, _display) = ExtenderHelper.GetMeta(GetType());
        _onChanged = cb;
    }

    public string GetCategory()    => _pageId;
    public string GetDisplayName() => _display;

    public override void ApplyValue() => _onChanged?.Invoke(Value);
}

/* ════════════════════════════════════════════════════════════════════════ */
/*  3. INT                                                                  */
public abstract class ExtenderIntSetting : IntSetting, IExposedSetting
{
    readonly string _pageId;
    readonly string _display;
    protected readonly Action<int>? _onChanged;


    protected ExtenderIntSetting(Action<int>? cb = null)
    {
        (_pageId, _display) = ExtenderHelper.GetMeta(GetType());
        _onChanged = cb;
    }

    public string GetCategory()    => _pageId;
    public string GetDisplayName() => _display;

    public override void ApplyValue() => _onChanged?.Invoke(Value);
}

/* ════════════════════════════════════════════════════════════════════════ */
/*  4. KEYCODE                                                              */
public abstract class ExtenderKeyCodeSetting : KeyCodeSetting, IExposedSetting
{
    readonly string _pageId;
    readonly string _display;
    protected readonly Action<KeyCode>? _onChanged;


    protected ExtenderKeyCodeSetting(Action<KeyCode>? cb = null)
    {
        (_pageId, _display) = ExtenderHelper.GetMeta(GetType());
        _onChanged = cb;
    }

    public string GetCategory()    => _pageId;
    public string GetDisplayName() => _display;

    public override void ApplyValue() => _onChanged?.Invoke(Keycode());
}

/* ════════════════════════════════════════════════════════════════════════ */
/*  5. ENUM (generic)                                                       */
public abstract class ExtenderEnumSetting<T> : EnumSetting<T>, IExposedSetting
    where T : unmanaged, System.Enum
{
    readonly string _pageId;
    readonly string _display;
    protected readonly Action<T>? _onChanged;


    protected ExtenderEnumSetting(Action<T>? cb = null)
    {
        (_pageId, _display) = ExtenderHelper.GetMeta(GetType());
        _onChanged = cb;
    }

    public string GetCategory()    => _pageId;
    public string GetDisplayName() => _display;

    /* optional override to expose nicer labels */
    public override List<string> GetUnlocalizedChoices()
        => new List<string>(Enum.GetNames(typeof(T)));

    public override void ApplyValue() => _onChanged?.Invoke(Value);
}