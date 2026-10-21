using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace YiErBuBu;
internal sealed class PetWindow : Window
{
    private const string AutoStartName = "YiErBuBu";
    private const string AutoStartRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private readonly SceneView _scene = new();
    private readonly DispatcherTimer _timer;
    private readonly UserSettings _settings = UserSettings.Load();
    private InputListener? _listener;
    private DateTime _lastKeyboard = DateTime.MinValue;
    private DateTime _lastMouse = DateTime.MinValue;
    private bool _locked, _paused;

    public PetWindow()
    {
        Title = "一二和布布 · 优化版";
        Width = 540; Height = 275;
        Left = SystemParameters.WorkArea.Right - 585;
        Top = SystemParameters.WorkArea.Bottom - 320;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Topmost = true;
        Content = _scene;

        _locked = _settings.Locked;
        _paused = _settings.Paused;
        SetScale(_settings.Scale, save: false);
        if (_settings.Left is double x && double.IsFinite(x)) Left = x;
        if (_settings.Top is double y && double.IsFinite(y)) Top = y;
        KeepOnScreen();

        MouseLeftButtonDown += (_, e) =>
        {
            if (!_locked && e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
                SaveSettings();
            }
        };
        ContextMenu = CreateMenu();
        _scene.ContextMenu = ContextMenu;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += (_, _) => Tick();
        Loaded += (_, _) => { StartListener(); _timer.Start(); };
        Closed += (_, _) =>
        {
            _timer.Stop();
            _listener?.Dispose();
            SaveSettings();
        };
    }

    private void KeepOnScreen()
    {
        var w = SystemParameters.VirtualScreenWidth;
        var h = SystemParameters.VirtualScreenHeight;
        var left = SystemParameters.VirtualScreenLeft;
        var top = SystemParameters.VirtualScreenTop;
        Left = Math.Clamp(Left, left - Width + 64, left + w - 64);
        Top = Math.Clamp(Top, top - Height + 64, top + h - 64);
    }

    private ContextMenu CreateMenu()
    {
        var menu = new ContextMenu();
        foreach (var size in new[] { .75, 1.0, 1.25, 1.5 })
        {
            var value = size;
            Add(menu, $"大小 {(int)(size * 100)}%", () => SetScale(value));
        }
        menu.Items.Add(new Separator());
        Add(menu, "锁定 / 解锁位置", () => { _locked = !_locked; SaveSettings(); });
        Add(menu, "暂停 / 恢复输入动画", TogglePause);
        Add(menu, "重新启动键鼠监听", StartListener);
        menu.Items.Add(new Separator());
        var auto = new MenuItem { Header = "开机自启", IsCheckable = true };
        menu.Opened += (_, _) => auto.IsChecked = IsAutoStartEnabled();
        auto.Click += (_, _) =>
        {
            try { SetAutoStart(auto.IsChecked); }
            catch (Exception ex)
            {
                Log.Write("开机自启修改失败：" + ex);
                auto.IsChecked = IsAutoStartEnabled();
                MessageBox.Show("设置开机自启失败：" + ex.Message, "一二和布布");
            }
        };
        menu.Items.Add(auto);
        menu.Items.Add(new Separator());
        Add(menu, "退出", Close);
        return menu;
    }

    private static void Add(ContextMenu menu, string header, Action action)
    {
        var item = new MenuItem { Header = header };
        item.Click += (_, _) => action();
        menu.Items.Add(item);
    }

    private static bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(AutoStartRegistryPath);
            string? value = key?.GetValue(AutoStartName) as string;
            if (value is null) return false;
            string current = Environment.ProcessPath ?? string.Empty;
            return string.Equals(value.Trim().Trim('"'), current, StringComparison.OrdinalIgnoreCase)
                && File.Exists(current);
        }
        catch (Exception ex) { Log.Write("检查开机自启失败：" + ex); return false; }
    }

    private static void SetAutoStart(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(AutoStartRegistryPath, writable: true)
            ?? throw new InvalidOperationException("无法访问当前用户启动项");
        if (!enabled) { key.DeleteValue(AutoStartName, false); return; }
        string exe = Environment.ProcessPath ?? throw new InvalidOperationException("找不到程序路径");
        if (!string.Equals(Path.GetFileName(exe), "YiErBuBu.exe", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("请运行发布版 YiErBuBu.exe 再启用开机自启");
        key.SetValue(AutoStartName, $"\"{exe}\"", RegistryValueKind.String);
    }

    private void SetScale(double scale, bool save = true)
    {
        scale = Math.Clamp(scale, .5, 2);
        Width = 540 * scale;
        Height = 275 * scale;
        if (save) { KeepOnScreen(); SaveSettings(); }
    }
    private void SaveSettings()
    {
        _settings.Scale = Width / 540;
        _settings.Left = Left;
        _settings.Top = Top;
        _settings.Locked = _locked;
        _settings.Paused = _paused;
        _settings.Save();
    }
    private void TogglePause()
    {
        _paused = !_paused;
        if (_paused) _scene.HeldKeys.Clear();
        SaveSettings();
    }
    private void StartListener()
    {
        InputListener? newer = null;
        try
        {
            newer = new InputListener();
            newer.KeyboardChanged += key =>
            {
                if (_paused) return;
                // The low-level hooks are installed by the UI thread. Never block this callback.
                if (key.Down)
                {
                    _scene.HeldKeys.Add(key.VkCode);
                    _scene.LastKey = key.VkCode;
                    _lastKeyboard = DateTime.UtcNow;
                }
                else _scene.HeldKeys.Remove(key.VkCode);
            };
            newer.MouseChanged += action =>
            {
                if (_paused) return;
                if (action == MouseAction.LeftDown) _scene.MouseLeft = true;
                if (action == MouseAction.LeftUp) _scene.MouseLeft = false;
                if (action == MouseAction.RightDown) _scene.MouseRight = true;
                if (action == MouseAction.RightUp) _scene.MouseRight = false;
                if (action is MouseAction.Wheel or MouseAction.Move or MouseAction.LeftDown or MouseAction.RightDown)
                    _lastMouse = DateTime.UtcNow;
            };
            newer.CallbackError += ex => Log.Write("键鼠回调失败：" + ex);
            var old = _listener;
            _listener = newer;
            old?.Dispose();
            _scene.HeldKeys.Clear();
            _scene.MouseLeft = _scene.MouseRight = false;
        }
        catch (Exception ex)
        {
            newer?.Dispose();
            Log.Write("键鼠监听启动失败：" + ex);
            MessageBox.Show("键鼠监听无法启动：" + ex.Message, "一二和布布");
        }
    }

    private void Tick()
    {
        var now = DateTime.UtcNow;
        double elapsedKey = (now - _lastKeyboard).TotalMilliseconds;
        _scene.KeyboardPulse = elapsedKey < 95 ? 1 : elapsedKey < 220 ? 1 - (elapsedKey - 95) / 125 : 0;
        _scene.MousePulse = Math.Clamp(1 - (now - _lastMouse).TotalMilliseconds / 170, 0, 1);
        var p = Forms.Cursor.Position;
        double centerX = Left + Width * .25;
        double centerY = Top + Height * .68;
        _scene.MouseX = Math.Clamp((p.X - centerX) / 280, -1, 1);
        _scene.MouseY = Math.Clamp((p.Y - centerY) / 240, -1, 1);
        _scene.Step(now);
        _scene.InvalidateVisual();
    }
}
