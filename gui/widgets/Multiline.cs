using System.Text.RegularExpressions;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

// NOTE: Multiline works the same as Label, except it requires explicit definition of it's size, and doesn't support Anchors.

class Multiline : Widget {
    public string Text { get; set; }
    public int Width { get; set; }
    public Color Background { get; set; }
    public Action? UpdateAction { get; set; }

    public Theme Theme { get { return Parent.Parent.Theme; } }

    public Vector2i TextSize { get { return new Vector2i(MeasureTextEx(Theme.Font, Text, Theme.FontSize, Theme.FontSpacing)); } }

    public Multiline(Container parent, string text, int width, Quad? padding=null, Color? background=null, Action? update_action=null) : base(parent) {
        Text = text;
        Width = width;
        Padding = padding ?? Padding;
        Background = background ?? new Color(0, 0, 0, 0);
        UpdateAction = update_action;
    }

    public override void Update() {
        if (UpdateAction is not null)
            UpdateAction.Invoke();
    }

    public override void Draw() {
        // Background
        DrawRectangleRec(ClickBox, Background);

        // Text
        var StartPos = new Vector2i(Position.X + Padding.L, Position.Y + Padding.U + (TextSize.Y / 2));
        string[] Words = Regex.Split(Text, @"(?=(?<=[^\s])\s+)");
        float LineLen = 0.0f;
        int LineNum = 1;
        bool TrimNext = false;

        foreach (var Word in Words) {
            var WordStr = Word;

            if (TrimNext) {
                WordStr = Word.Trim();
                TrimNext = false;
            }

            var WordSize = MeasureTextEx(Theme.Font, WordStr, Theme.FontSize, Theme.FontSpacing);

            // Advance to the next line if the line length exceeds the max width or if %N is found
            if (Word.Trim() == "%N" || LineLen + WordSize.X > Width - Padding.X) {
                LineNum++;
                LineLen = 0.0f;

                if (Word.Trim() == "%N") {
                    TrimNext = true;
                    continue;
                }

                WordStr = Word.Trim();
                WordSize = MeasureTextEx(Theme.Font, WordStr, Theme.FontSize, Theme.FontSpacing);
            }
            
            var Pos = new Vector2i(StartPos.X + LineLen, StartPos.Y + ((LineNum - 1) * 20));

            DrawTextEx(Theme.Font, WordStr, Pos.ToVector2(), Theme.FontSize, Theme.FontSpacing, Theme.Foreground);
            LineLen += WordSize.X;
        }

        Size = new Vector2i(Width, LineNum * 20);
    }
}