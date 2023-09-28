using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Button : Widget {
    public string Text { get; set; }
    public Action Action { get; set; }
    public Anchor TextAnchor { get; set; }
    public bool Background { get; set; }

    public Theme Theme { get { return Parent.Parent.Theme; } }

    public bool Hovered { get { return CheckCollisionPointRec(GetMousePosition(), ClickBox); } }
    public bool Clicked { get { return Hovered && IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT); } }

    public Vector2i TextSize { get { return new Vector2i(MeasureTextEx(Theme.Font, Text, Theme.FontSize, Theme.FontSpacing)); } }

    public Button(Container parent, string text, Action action, Vector2i size, Quad? padding=null, Anchor? anchor=null, Anchor? text_anchor=null, bool background=true) : base(parent) {
        Text = text;
        Action = action;
        Size = size;
        Padding = padding ?? Padding;
        Anchor = anchor ?? Anchor;
        TextAnchor = text_anchor ?? Anchor.Center;
        Background = background;
    }

    public override bool FireEvent(Event E) {
        // if (E.Name == "MousePress:MOUSE_BUTTON_LEFT" && Hovered) {
        //     Action.Invoke();
        //     return true;
        // }

        return false;
    }

    public override void Draw() {
        // Background
        Color BG = Background ? Theme.ButtonBackground : new Color(0, 0, 0, 0);
        if (Hovered) BG = Theme.ButtonHoverBackground;
        if (Clicked) BG = Theme.ButtonActiveBackground;

        DrawRectangleRec(ClickBox, BG);

        // Text
        var Pos = new Vector2i(
            x: Position.X + (Size.X / 2) - (TextSize.X / 2) + Padding.X,
            y: Position.Y + (Size.Y / 2) - (TextSize.Y / 2) + Padding.Y
        ).ToVector2();

        DrawTextEx(Theme.Font, Text, Pos, Theme.FontSize, Theme.FontSpacing, Theme.ButtonForeground);
    }
}