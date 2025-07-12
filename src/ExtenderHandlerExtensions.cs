using Zorro.Settings;

public static class SettingsHandlerExtensions
{
    /// <summary>
    /// Creates <typeparamref name="T"/>, registers it, and returns it.
    /// </summary>
    public static T AddSetting<T>(this SettingsHandler handler)
        where T : Setting, new()
    {
        var setting = new T();
        handler.AddSetting(setting);
        return setting;
    }
}
