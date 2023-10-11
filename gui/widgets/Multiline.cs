using System.Text.RegularExpressions;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

// NOTE: Multiline works the same as Label, except it requires explicit definition of it's size, and doesn't support Anchors.

class Multiline : Widget {
    public string Text { get; set; }
    public int Width { get; set; }
    public Color Background { get; set; }
    public bool FitWidth { get; set; }
    public bool FitHeight { get; set; }
    public Action? UpdateAction { get; set; }

    public Theme Theme { get { return Parent.Parent.Theme; } }

    public Vector2i TextSize { get { return new Vector2i(MeasureTextEx(Theme.Font, Text, Theme.FontSize, Theme.FontSpacing)); } }

    public Multiline(Container parent, string text, int? width=0, Quad? margin=null, Color? background=null, bool fit_width=false, bool fit_height=false, Action? update_action=null) : base(parent) {
        Text = text;
        Width = width ?? 0;
        Margin = margin ?? Quad.Zero;
        Background = background ?? new Color(0, 0, 0, 0);
        UpdateAction = update_action;

        Padding = Quad.Zero;
    }

    public override void Update() {
        UpdateAction?.Invoke();
    }

    public override void Draw() {
        // Background
        var Rec = ClickBox;
        Rec = new Rectangle(
            ClickBox.x, ClickBox.y,
            FitWidth ? Parent.Size.X + Margin.X : ClickBox.width,
            FitHeight ? Parent.Size.Y + Margin.Y : ClickBox.height
        );

        DrawRectangleRec(Rec, Background);

        // Text
        var StartPos = new Vector2i(Position.X, Position.Y + (TextSize.Y / 2));
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
            if (Word.Trim() == "%N" || LineLen + WordSize.X > Width) {
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

        Size = new Vector2i(Width + Margin.X + Padding.X, LineNum * 20 + Margin.Y + Padding.Y);
    }
}