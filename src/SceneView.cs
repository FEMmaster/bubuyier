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
        
        DrawShadows(d);
        
        // 中景：鼠标和键盘整体绕画面中心顺时针旋转30°
        d.PushTransform(new RotateTransform(10, BaseW / 2, BaseH / 2));
        DrawMousePadAndMouse(d);
        DrawKeyboard(d);
        d.Pop();
      
        // 后景：人物
        d.DrawImage(BubuImage, new Rect(140, 48, 122, 125));
        d.DrawImage(YierImage, new Rect(250, 40, 148, 151));
        
        // 前景：桌面边缘线
        // DrawDeskFrame(d);
        
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
        var tg = new TransformGroup();
        tg.Children.Add(new ScaleTransform(0.88, 0.88, 127, 203));
        tg.Children.Add(new TranslateTransform(44, -18));
        d.PushTransform(tg);
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
                (k.Vk == 160 && HeldKeys.Contains(16)) ||
                (k.Vk == 161 && HeldKeys.Contains(16));
            double u0 = Math.Clamp((445 - k.X - k.W) / 207.0, 0, 1);
            double u1 = Math.Clamp((445 - k.X) / 207.0, 0, 1);
            double v0 = Math.Clamp((247 - k.Y - k.H) / 77.0, 0, 1);
            double v1 = Math.Clamp((247 - k.Y) / 77.0, 0, 1);
            Point a = ProjectKey(u0, v0, farLeft, farRight, nearRight, nearLeft);
            Point b = ProjectKey(u1, v0, farLeft, farRight, nearRight, nearLeft);
            Point c = ProjectKey(u1, v1, farLeft, farRight, nearRight, nearLeft);
            Point e = ProjectKey(u0, v1, farLeft, farRight, nearRight, nearLeft);
            if (!held)
                DrawQuad(d, "#C7B09C", null,
                    new Point(a.X, a.Y + 2), new Point(b.X, b.Y + 2),
                    new Point(c.X, c.Y + 2), new Point(e.X, e.Y + 2));
            DrawQuad(d, held ? "#F0B89C" : "#FFF9EF",
                held ? "#B57258" : "#C6B4A5", a, b, c, e);
            Point center = ProjectKey((u0 + u1) / 2, (v0 + v1) / 2,
                farLeft, farRight, nearRight, nearLeft);
            Label(d, k.Caption, center.X, center.Y - 4, 6.2,
                held ? "#7D4C3A" : "#6F615A");
        }
    }

    private static Point ProjectKey(double u, double v, Point a, Point b, Point c, Point e) =>
        new((1-u)*(1-v)*a.X + u*(1-v)*b.X + u*v*c.X + (1-u)*v*e.X,
            (1-u)*(1-v)*a.Y + u*(1-v)*b.Y + u*v*c.Y + (1-u)*v*e.Y);

    private static void DrawQuad(DrawingContext d, string fill, string? stroke,
        Point a, Point b, Point c, Point e)
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(a, true, true);
            ctx.LineTo(b, true, false);
            ctx.LineTo(c, true, false);
            ctx.LineTo(e, true, false);
        }
        geometry.Freeze();
        d.DrawGeometry(Ink(fill), stroke == null ? null : Stroke(stroke, 1.2), geometry);
    }
    private static Brush Ink(string hex) => (Brush)new BrushConverter().ConvertFromString(hex)!;
    private static Pen? Stroke(string? hex, double width) => hex is null ? null : new Pen(Ink(hex), width);
    private static void Ellipse(DrawingContext d, string fill, string? stroke,
        double x, double y, double rx, double ry, double width = 0) =>
        d.DrawEllipse(Ink(fill), Stroke(stroke, width), new Point(x, y), rx, ry);
    private static void Line(DrawingContext d, string color, double width,
        double x1, double y1, double x2, double y2) =>
        d.DrawLine(new Pen(Ink(color), width), new Point(x1, y1), new Point(x2, y2));
    private static void Label(DrawingContext d, string text, double x, double y,
        double size, string color)
    {
        var ft = new FormattedText(text, System.Globalization.CultureInfo.CurrentUICulture,
            global::System.Windows.FlowDirection.LeftToRight, UiFace, size, Ink(color), 1);
        d.DrawText(ft, new Point(x - ft.Width / 2, y));
    }
}
