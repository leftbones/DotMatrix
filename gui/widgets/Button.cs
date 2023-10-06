using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

// TODO: Make Size an optional parameter, calculate size based on contents (for Label and Button, that would be text width)

class Button : Widget {
    public string Text { get; set; }
    public Action Action { get; set; }
    public Anchor TextAnchor { get; set; }
    public Quad Margin { get; set; }
    public bool Background { get; set; }

    public Theme Theme { get { return Parent.Parent.Theme; } }

    public bool Hovered { get { return CheckCollisionPointRec(GetMousePosition(), ClickBox); } }
    public bool Clicked { get { return Hovered && IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT); } }

    public Vector2i TextSize { get { return new Vector2i(MeasureTextEx(Theme.Font, Text, Theme.FontSize, Theme.FontSpacing)); } }

    public Button(Container parent, string text, Action action, Vector2i? size=null, Quad? margin=null, Quad? padding=null, Anchor? anchor=null, Anchor? text_anchor=null, bool background=true) : base(parent) {
        Text = text;
        Action = action;
        Size = size ?? Vector2i.Zero;
        Margin = margin ?? Quad.Zero;
        Padding = padding ?? new Quad(0, 0, 5, 5);
        Anchor = anchor ?? Anchor;
        TextAnchor = text_anchor ?? Anchor.Center;
        Background = background;
    }

    public override bool FireEvent(Key K) {
        if (K.Type == EventType.Press && K.Code == (int)MouseButton.MOUSE_BUTTON_LEFT && Hovered) {
            Action.Invoke();
            return true;
        }

        return false;
    }

    public override void Draw() {
        // Size Calculation
        var TextSize = MeasureTextEx(Theme.Font, Text, Theme.FontSize, Theme.FontSpacing);

        int MinWidth = (int)TextSize.X;
        int MinHeight = (int)TextSize.Y;

        Size = new Vector2i(Math.Max(Size.X, MinWidth), Math.Max(Size.Y, MinHeight));

        // Background
        Color BG = Background ? Theme.ButtonBackground : new Color(0, 0, 0, 0);
        if (Hovered) BG = Theme.ButtonHoverBackground;
        if (Clicked) BG = Theme.ButtonActiveBackground;

        DrawRectangleRec(ClickBox, BG);

        var Pos = Vector2i.Zero;
        switch (TextAnchor) {
            case Anchor.Left:
                Pos = new Vector2i(Position.X + Padding.L + Margin.L, Position.Y + ((Size.Y + Padding.Y) / 2) - (TextSize.Y / 2));
                break;
            case Anchor.Center:
                Pos = new Vector2i(Position.X + (ClickBox.width / 2) - (TextSize.X / 2) + (Padding.L - Padding.R), Position.Y + (ClickBox.height / 2) - (TextSize.Y / 2) + (Padding.U - Padding.D));
                break;

            case Anchor.Right:
                Pos = new Vector2i(Position.X + (Size.X - Padding.R - Margin.R) - TextSize.X, Position.Y + ((Size.Y + Padding.Y) / 2) - (TextSize.Y / 2));
                break;
        }

        Text = $"{ClickBox.width}";
        DrawTextEx(Theme.Font, Text, Pos.ToVector2(), Theme.FontSize, Theme.FontSpacing, Theme.ButtonForeground);
    }
}