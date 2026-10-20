using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YiErBuBu;
internal sealed class SceneView : FrameworkElement
{
    public HashSet<int> HeldKeys { get; } = new();
    public int LastKey { get; set; } = -1;
    public bool MouseLeft { get; set; }
    public bool MouseRight { get; set; }
    public double MouseX { get; set; }
    public double MouseY { get; set; }
    public double MousePulse { get; set; }
    public double KeyboardPulse { get; set; }

    private const double BaseW = 540;
    private const double BaseH = 275;
    private static readonly Typeface UiFace = new("Segoe UI");
    private static readonly BitmapImage BubuImage = Load("bubu.png");
    private static readonly BitmapImage YierImage = Load("yier.png");

    private static BitmapImage Load(string file)
    {
        var img = new BitmapImage();
        img.BeginInit();
        img.UriSource = new Uri($"pack://application:,,,/assets/characters/{file}");
        img.CacheOption = BitmapCacheOption.OnLoad;
        img.EndInit();
        img.Freeze();
        return img;
    }

    protected override void OnRender(DrawingContext d)
    {
        base.OnRender(d);
        if (ActualWidth <= 0 || ActualHeight <= 0) return;
        d.PushTransform(new ScaleTransform(ActualWidth / BaseW, ActualHeight / BaseH));
        DrawDeskFrame(d);
        DrawShadows(d);
        // Character PNGs are behind the foreground desk devices.
        // The original character paws are part of the image in this preview.
        d.DrawImage(BubuImage, new Rect(92, 66, 154, 158));
        d.DrawImage(YierImage, new Rect(284, 67, 155, 158));
        DrawMousePadAndMouse(d);
        DrawKeyboard(d);
        d.Pop();
    }

    private void DrawDeskFrame(DrawingContext d)
    {
        Line(d, "#4E403D", 2.4, 92, 198, 515, 198);
        Line(d, "#4E403D", 2.1, 92, 198, 55, 188);
        Line(d, "#4E403D", 2.1, 515, 198, 517, 70);
    }

    private void DrawShadows(DrawingContext d)
    {
        Ellipse(d, "#1A000000", null, 140, 216, 74, 14);
        Ellipse(d, "#16000000", null, 348, 231, 118, 14);
        Ellipse(d, "#12000000", null, 156, 214, 28, 8);
    }

    private void DrawMousePadAndMouse(DrawingContext d)
    {
        d.PushTransform(new TranslateTransform(40, -28));
        DrawQuad(d, "#F7D6CF", "#8A6A64",
            new Point(101, 183), new Point(158, 194),
            new Point(178, 224), new Point(77, 211));
        double mx = 127 + Math.Clamp(MouseX, -1, 1) * 5;
        double my = 203 + Math.Clamp(MouseY, -1, 1) * 2;
        d.PushTransform(new RotateTransform(35, mx, my));
        Ellipse(d, "#FFF9F4", "#866B64", mx, my, 16, 25, 2);
        Line(d, "#8A6F68", 1.4, mx, my + 19, mx, my - 1);
        if (MouseLeft) Ellipse(d, "#F1BBB2", null, mx - 6, my + 8, 6, 8);
        if (MouseRight) Ellipse(d, "#F1BBB2", null, mx + 6, my + 8, 6, 8);
        d.Pop();
        if (!MouseLeft && !MouseRight && MousePulse > 0.05)
            Ellipse(d, "#18D9A8A0", null, mx, my + 12, 12, 5);
        d.Pop();
    }

    private void DrawKeyboard(DrawingContext d)
    {
        Point farLeft = new(260, 169);
        Point farRight = new(430, 185);
        Point nearRight = new(456, 239);
        Point nearLeft = new(228, 218);
        DrawQuad(d, "#DCCABB", "#8D8177", nearLeft, nearRight,
            new Point(456, 247), new Point(228, 226));
        DrawQuad(d, "#F8F1E7", "#8D8177", farLeft, farRight, nearRight, nearLeft);
        foreach (var k in KeyboardLayout.Keys)
        {
            bool held = HeldKeys.Contains(k.Vk) ||
