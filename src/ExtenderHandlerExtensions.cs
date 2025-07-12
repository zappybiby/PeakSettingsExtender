using System;
using System.Reflection;    // for fallback reflection
using Zorro.Settings;       // ISettingHandler, Setting

// no namespace on SettingsHandler in the decompile, so use the global
// qualifier when we need to reference it ⬇
public static class ExtenderHandlerExtensions
{
    /// <summary>
    /// Creates a new Setting of type <typeparamref name="T"/>,
    /// registers it with whatever concrete handler we have,
    /// then returns the instance.
    /// </summary>
    public static T Add<T>(this ISettingHandler handler)
        where T : Setting, new()
    {
        var setting = new T();

        // Fast path: if the handler actually *is* the in-game SettingsHandler,
        // call its public AddSetting(Setting) directly.
        if (handler is global::SettingsHandler concrete)
        {
            concrete.AddSetting(setting);
            return setting;
        }

        // Fallback: look for any AddSetting(Setting) method via reflection
        var mi = handler.GetType()
                        .GetMethod("AddSetting",
                                   BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                   null,
                                   new[] { typeof(Setting) },
                                   null);

        if (mi != null)
        {
            mi.Invoke(handler, new object[] { setting });
            return setting;
        }

        // Last-ditch: no way to add, so throw
        throw new MissingMethodException(
            $"ISettingHandler implementation {handler.GetType().Name} has no AddSetting(Setting) method.");
    }
}
