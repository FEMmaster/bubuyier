using System;
using System.IO;
using System.Text.Json;

namespace YiErBuBu;

internal sealed class UserSettings
{
    public double Scale { get; set; } = 1;
    public double? Left { get; set; }
    public double? Top { get; set; }
    public bool Locked { get; set; }
    public bool Paused { get; set; }

    private static string PathName => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YiErBuBu", "settings.json");

    public static UserSettings Load()
    {
        try
        {
            if (File.Exists(PathName))
                return JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(PathName)) ?? new UserSettings();
        }
        catch (Exception ex) { Log.Write("读取设置失败：" + ex.Message); }
        return new UserSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathName)!);
            string temp = PathName + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temp, PathName, true);
        }
        catch (Exception ex) { Log.Write("保存设置失败：" + ex.Message); }
    }
}

internal static class Log
{
    internal static void Write(string message)
    {
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YiErBuBu");
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, "errors.log"), $"[{DateTime.Now:O}] {message}{Environment.NewLine}");
        }
        catch { }
    }
}
