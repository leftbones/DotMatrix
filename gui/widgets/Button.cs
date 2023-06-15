using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Button : Widget {
    public string Text { get; set; }
    public Action Action { get; set; }
    public Quad Margin { get; set; }

    public Theme Theme { get { return Parent.Parent.Theme; } }

    public bool Hovered { get { return CheckCollisionPointRec(GetMousePosition(), ClickBox); } }
    public bool Clicked { get { return Hovered && IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT); } }

    public Vector2i TextSize { get { return new Vector2i(MeasureTextEx(Theme.Font, Text, Theme.FontSize, Theme.FontSpacing)); } }

    public Button(Container parent, string text, Action action, Vector2i size, Quad? margin=null) : base(parent) {
        Text = text;
        Action = action;
        Size = size;
        Margin = margin ?? new Quad(5, 5, 10, 10);
    }

    public override bool FireEvent(Event E) {
        if (E.Name == "MouseDown:MOUSE_BUTTON_LEFT" && Hovered) {
            Action.Invoke();
            return true;
        }

        return false;
    }

    public override void Draw() {
        // Background
        Color BG = Theme.ButtonBackground;
        if (Hovered) BG = Theme.ButtonHoverBackground;
        if (Clicked) BG = Theme.ButtonActiveBackground;

        DrawRectangleRec(ClickBox, BG);

        // Text
        DrawTextEx(Theme.Font, Text, new Vector2i(Position.X + (Size.X / 2) - (TextSize.X / 2), Position.Y + (Size.Y / 2) - (TextSize.Y / 2)).ToVector2(), Theme.FontSize, Theme.FontSpacing, Theme.ButtonForeground);
    }
}