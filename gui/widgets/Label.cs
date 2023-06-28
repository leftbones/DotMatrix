using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Label : Widget {
    public string Text { get; set; }
    public Anchor TextAnchor { get; set; }
    public Color Background { get; set; }
    public Action? UpdateAction { get; set; }

    public Theme Theme { get { return Parent.Parent.Theme; } }

    public Vector2i TextSize { get { return new Vector2i(MeasureTextEx(Theme.Font, Text, Theme.FontSize, Theme.FontSpacing)); } }

    public Label(Container parent, string text, Vector2i? size=null, Quad? padding=null, Anchor? anchor=null, Anchor? text_anchor=null, Color? background=null, Action? update_action=null) : base(parent) {
        Text = text;
        Size = size ?? new Vector2i(MeasureTextEx(Theme.Font, Text, Theme.FontSize, Theme.FontSpacing));
        Padding = padding ?? Padding;
        Anchor = anchor ?? Anchor;
        TextAnchor = text_anchor ?? Anchor.Center;
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
        var Pos = Vector2i.Zero;
        switch (TextAnchor) {
            case Anchor.Left:
                Pos = new Vector2i(Position.X + Padding.L, Position.Y + ((Size.Y + Padding.Y) / 2) - (TextSize.Y / 2));
                break;
            case Anchor.Center:
                Pos = new Vector2i(Position.X + (ClickBox.width / 2) - (TextSize.X / 2) + (Padding.L - Padding.R), Position.Y + (ClickBox.height / 2) - (TextSize.Y / 2) + (Padding.U - Padding.D));
                break;

            case Anchor.Right:
                Pos = new Vector2i(Position.X + (Size.X - Padding.R) - TextSize.X, Position.Y + ((Size.Y + Padding.Y) / 2) - (TextSize.Y / 2));
                break;
        }

        DrawTextEx(Theme.Font, Text, Pos.ToVector2(), Theme.FontSize, Theme.FontSpacing, Theme.Foreground);
    }
}