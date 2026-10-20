using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
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
    protected override void OnRender(DrawingContext d)
    {
        base.OnRender(d);
        d.PushTransform(new ScaleTransform(ActualWidth / BaseW, ActualHeight / BaseH));
        DrawDeskFrame(d);
        DrawShadows(d);
        DrawMousePadAndMouse(d);
        DrawKeyboard(d);
        DrawBubu(d);
        DrawYier(d);
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
        // 鼠标垫采用倾斜四边形
        DrawQuad(d, "#F7D6CF", "#8A6A64",
            new Point(101, 183),
            new Point(158, 194),
            new Point(178, 224),
            new Point(77, 211));
    
        double mx = 127 + Math.Clamp(MouseX, -1, 1) * 5;
        double my = 203 + Math.Clamp(MouseY, -1, 1) * 2;
    
        // 鼠标跟随桌面方向倾斜
        d.PushTransform(new RotateTransform(35, mx, my));
    
        Ellipse(d, "#FFF9F4", "#866B64",
            mx, my, 16, 25, 2);
    
        // 鼠标前端朝向布布
        Line(d, "#8A6F68", 1.4,
            mx, my + 19, mx, my - 1);
    
        if (MouseLeft)
            Ellipse(d, "#F1BBB2", null,
                mx - 6, my + 8, 6, 8);
    
        if (MouseRight)
            Ellipse(d, "#F1BBB2", null,
                mx + 6, my + 8, 6, 8);
    
        d.Pop();
    
        if (!MouseLeft && !MouseRight && MousePulse > 0.05)
            Ellipse(d, "#18D9A8A0", null,
                mx, my + 12, 12, 5);
    }

    private void DrawKeyboard(DrawingContext d)
    {
        // 斜视键盘：远端较短，近端较宽
        Point farLeft = new(260, 169);
        Point farRight = new(430, 185);
        Point nearRight = new(456, 239);
        Point nearLeft = new(228, 218);
    
        // 键盘侧面厚度
        DrawQuad(d, "#DCCABB", "#8D8177",
            nearLeft,
            nearRight,
            new Point(456, 247),
            new Point(228, 226));
    
        // 键盘顶面
        DrawQuad(d, "#F8F1E7", "#8D8177",
            farLeft, farRight, nearRight, nearLeft);
    
        foreach (var k in KeyboardLayout.Keys)
        {
            bool held =
                HeldKeys.Contains(k.Vk) ||
                (k.Vk == 160 && HeldKeys.Contains(16)) ||
                (k.Vk == 161 && HeldKeys.Contains(16));
    
            // 保留上次的180度方向调整
            double u0 = Math.Clamp(
                (445 - k.X - k.W) / 207.0, 0, 1);
            double u1 = Math.Clamp(
                (445 - k.X) / 207.0, 0, 1);
    
            double v0 = Math.Clamp(
                (247 - k.Y - k.H) / 77.0, 0, 1);
            double v1 = Math.Clamp(
                (247 - k.Y) / 77.0, 0, 1);
    
            Point a = ProjectKey(u0, v0,
                farLeft, farRight, nearRight, nearLeft);
            Point b = ProjectKey(u1, v0,
                farLeft, farRight, nearRight, nearLeft);
            Point c = ProjectKey(u1, v1,
                farLeft, farRight, nearRight, nearLeft);
            Point e = ProjectKey(u0, v1,
                farLeft, farRight, nearRight, nearLeft);
    
            // 阴影向屏幕下方投射，不随键盘反转
            if (!held)
            {
                DrawQuad(d, "#C7B09C", null,
                    new Point(a.X, a.Y + 2),
                    new Point(b.X, b.Y + 2),
                    new Point(c.X, c.Y + 2),
                    new Point(e.X, e.Y + 2));
            }
    
            DrawQuad(d,
                held ? "#F0B89C" : "#FFF9EF",
                held ? "#B57258" : "#C6B4A5",
                a, b, c, e);
    
            // 字母保持正向，不倒立
            Point center = ProjectKey(
                (u0 + u1) / 2,
                (v0 + v1) / 2,
                farLeft, farRight, nearRight, nearLeft);
    
            Label(d, k.Caption, center.X,
                center.Y - 4, 6.2,
                held ? "#7D4C3A" : "#6F615A");
        }
    }
    private void DrawBubu(DrawingContext d)
    {
        const string fur = "#D5A172";
        const string inner = "#5B4237";
        const string outline = "#3F322F";
        // Ears
        Ellipse(d, fur, outline, 132, 111, 18, 19, 2.4);
        Ellipse(d, inner, outline, 132, 111, 8, 9, 1.3);
        Ellipse(d, fur, outline, 184, 112, 18, 19, 2.4);
        Ellipse(d, inner, outline, 184, 112, 8, 9, 1.3);
        // Body then head
        Ellipse(d, fur, outline, 160, 196, 33, 25, 2.2);
        Ellipse(d, fur, outline, 158, 148, 46, 42, 2.6);
        Ellipse(d, "#EEDB74", null, 131, 159, 11, 12);
        Ellipse(d, "#EEDB74", null, 184, 160, 11, 12);
        Ellipse(d, outline, null, 145, 150, 4.3, 5.6);
        Ellipse(d, outline, null, 172, 150, 4.3, 5.6);
        Label(d, "ᴗ", 158.5, 152.2, 14.2, "#3F322F");
        // Mouse paw
        double pawX = 129 + Math.Clamp(MouseX, -1, 1) * 6;
        double pawY = MouseLeft || MouseRight ? 182.6 : 179.6 - MousePulse * 1.7;
        Ellipse(d, fur, outline, pawX, pawY, 14.6, 12.2, 2.0);
        if (MouseLeft || MouseRight)
            Ellipse(d, "#EDC1A3", null, pawX, pawY + 1.8, 5.5, 3.6);
        // Other paw near desk edge
        Ellipse(d, fur, outline, 173.5, 190, 12.8, 11.2, 1.8);
    }
    private void DrawYier(DrawingContext d)
    {
        const string fur = "#F9F8F3";
        const string ear = "#433738";
        const string outline = "#403535";
        double keyX = 340, keyY = 205;
        if (!TryGetPrimaryKey(out var focus))
            focus = new KeyShape(0, "", 338, 210, 0, 0);
        keyX = focus.CenterX;
        keyY = focus.CenterY;
        // Ears
        Ellipse(d, ear, outline, 314, 116, 17.5, 19, 2.1);
        Ellipse(d, ear, outline, 396, 116, 17.5, 19, 2.1);
        // Body & head
        Ellipse(d, fur, outline, 356, 198, 36, 28, 2.2);
        Ellipse(d, fur, outline, 355, 149, 51, 46, 2.6);
        Ellipse(d, "#F0CBC2", null, 323, 161, 10, 11.5);
        Ellipse(d, "#F0CBC2", null, 388, 161, 10, 11.5);
        Ellipse(d, outline, null, 338, 151, 4.3, 5.6);
        Ellipse(d, outline, null, 369, 151, 4.3, 5.6);
        Label(d, "ᴗ", 353.3, 153.7, 14.2, "#403535");
        // Keyboard paws aim toward active key cluster.
        double pulse = KeyboardPulse;
        double leftTargetX = Math.Clamp(keyX - 20, 305, 352);
        double rightTargetX = Math.Clamp(keyX + 18, 348, 398);
        double leftTargetY = Math.Clamp(keyY - 7 - pulse * 6.5, 178, 214);
        double rightTargetY = Math.Clamp(keyY - 4 - pulse * 3.5, 181, 216);
        Ellipse(d, fur, outline, leftTargetX, leftTargetY, 13.2, 11.4, 1.9);
        Ellipse(d, fur, outline, rightTargetX, rightTargetY, 13.2, 11.4, 1.9);
    }
    private bool TryGetPrimaryKey(out KeyShape key)
    {
        if (LastKey >= 0 && KeyboardLayout.TryGetKey(LastKey, out key)) return true;
        foreach (var vk in HeldKeys)
            if (KeyboardLayout.TryGetKey(vk, out key)) return true;
        key = default;
        return false;
    }
    private static Brush Ink(string hex) => (Brush)new BrushConverter().ConvertFromString(hex)!;
    private static Pen? Stroke(string? hex, double width) => hex is null ? null : new Pen(Ink(hex), width);
    private static void Ellipse(DrawingContext d, string fill, string? stroke, double x, double y, double rx, double ry, double width = 0) =>
        d.DrawEllipse(Ink(fill), Stroke(stroke, width), new Point(x, y), rx, ry);
    
    private static Point ProjectKey(
        double u, double v,
        Point a, Point b, Point c, Point e)
    {
        // 将平面坐标映射到倾斜四边形
        return new Point(
            (1-u)*(1-v)*a.X +
            u*(1-v)*b.X +
            u*v*c.X +
            (1-u)*v*e.X,
    
            (1-u)*(1-v)*a.Y +
            u*(1-v)*b.Y +
            u*v*c.Y +
            (1-u)*v*e.Y
        );
    }

    private static void DrawQuad(
        DrawingContext d,
        string fill,
        string? stroke,
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
    
        d.DrawGeometry(
            Ink(fill),
            stroke == null ? null : Stroke(stroke, 1.2),
            geometry
        );
    }
    
    private static void RoundRect(DrawingContext d, string fill, string? stroke, double x, double y, double w, double h, double r, double width = 0) =>
        d.DrawRoundedRectangle(Ink(fill), Stroke(stroke, width), new Rect(x, y, w, h), r, r);
    private static void Line(DrawingContext d, string color, double width, double x1, double y1, double x2, double y2) =>
        d.DrawLine(new Pen(Ink(color), width), new Point(x1, y1), new Point(x2, y2));
    private static void Label(DrawingContext d, string text, double x, double y, double size, string color)
    {
        var ft = new FormattedText(text, System.Globalization.CultureInfo.CurrentUICulture,
            global::System.Windows.FlowDirection.LeftToRight, UiFace, size, Ink(color), 1);
        d.DrawText(ft, new Point(x - ft.Width / 2, y));
    }
}
