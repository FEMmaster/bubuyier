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
        RoundRect(d, "#F7D6CF", "#8A6A64", 88, 182, 86, 41, 10, 1.8);
        double mx = 131 + Math.Clamp(MouseX, -1, 1) * 5.5;
        double my = 201 + Math.Clamp(MouseY, -1, 1) * 2.0;
        Ellipse(d, "#FFF9F4", "#866B64", mx, my, 23, 31, 2.0);
        Line(d, "#8A6F68", 1.4, mx, my - 23, mx, my + 1);
        if (MouseLeft) Ellipse(d, "#F1BBB2", null, mx - 8, my - 5, 8, 10);
        if (MouseRight) Ellipse(d, "#F1BBB2", null, mx + 8, my - 5, 8, 10);
        if (!MouseLeft && !MouseRight && MousePulse > 0.05)
            Ellipse(d, "#18D9A8A0", null, mx, my + 11, 14 + 5 * MousePulse, 6 + 2 * MousePulse);
    }
    private void DrawKeyboard(DrawingContext d)
    {
        // keyboard deck
        RoundRect(d, "#F8F1E7", "#8D8177", 226, 167, 228, 82, 15, 2.0);
        RoundRect(d, "#EFE5DA", null, 233, 172, 214, 69, 13, 0);
        foreach (var k in KeyboardLayout.Keys)
        {
            bool held = HeldKeys.Contains(k.Vk) || (k.Vk == 160 && HeldKeys.Contains(16)) || (k.Vk == 161 && HeldKeys.Contains(16));
            double y = k.Y + (held ? 1.8 : 0);
            if (!held)
                RoundRect(d, "#D0B79D", null, k.X, y + 1.7, k.W, k.H, 3.2, 0);
            RoundRect(d, held ? "#F0B89C" : "#FFF9EF", held ? "#B57258" : "#C6B4A5", k.X, y, k.W, k.H, 3.2, 0.9);
            var font = k.W > 28 ? 6.2 : 6.7;
            Label(d, k.Caption, k.CenterX, y + 1.2, font, held ? "#7D4C3A" : "#6F615A");
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
