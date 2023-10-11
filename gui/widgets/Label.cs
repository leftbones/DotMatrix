using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Label : Widget {
    public string Text { get; set; }
    public Anchor TextAnchor { get; set; }
    public Color Background { get; set; }
    public bool FitWidth { get; set; }
    public bool FitHeight { get; set; }
    public Action? UpdateAction { get; set; }

    public Theme Theme { get { return Parent.Parent.Theme; } }

    public Vector2i TextSize { get { return new Vector2i(MeasureTextEx(Theme.Font, Text, Theme.FontSize, Theme.FontSpacing)); } }

    public Label(Container parent, string text, Vector2i? size=null, Quad? margin=null, Anchor? anchor=null, Anchor? text_anchor=null, Color? background=null, bool fit_width=false, bool fit_height=false, Action? update_action=null) : base(parent) {
        Text = text;
        Size = size ?? new Vector2i(MeasureTextEx(Theme.Font, Text, Theme.FontSize, Theme.FontSpacing));
        Margin = margin ?? Quad.Zero;
        Anchor = anchor ?? Anchor;
        TextAnchor = text_anchor ?? Anchor.Center;
        Background = background ?? new Color(0, 0, 0, 0);
        FitWidth = fit_width;
        FitHeight = fit_height;
        UpdateAction = update_action;

        // Margin = new Quad(2, 2, 2, 2);
        Margin = Quad.Zero;
        Padding = new Quad(5, 5, 5, 5);
    }

    public override void Update() {
        UpdateAction?.Invoke();
    }

    public override void Draw() {
        // Size Calculation
        var TextSize = MeasureTextEx(Theme.Font, Text, Theme.FontSize, Theme.FontSpacing);

        int MinWidth = (int)TextSize.X + Margin.X;
        int MinHeight = (int)TextSize.Y + Margin.Y;

        Size = new Vector2i(Math.Max(Size.X, MinWidth), Math.Max(Size.Y, MinHeight));

        // Background
        var Rec = new Rectangle(
            ClickBox.x, ClickBox.y,
            FitWidth ? Parent.Size.X - Parent.Margin.X : ClickBox.width,
            FitHeight ? Parent.Size.Y - Parent.Margin.Y : ClickBox.height
        );

        DrawRectangleRec(Rec, Background);

        // Text
        var Pos = Vector2i.Zero;
        switch (TextAnchor) {
            case Anchor.Left:
                Pos = new Vector2i(
                    Position.X + Padding.L + Margin.L,
                    Position.Y + ((Size.Y + Padding.Y) / 2) - (TextSize.Y / 2)
                );
                break;

            case Anchor.Center:
                Pos = new Vector2i(
                    Position.X + Size.X / 2 - TextSize.X / 2 + Padding.L,
                    Position.Y + Size.Y / 2 - TextSize.Y / 2 + Padding.U
                );
                break;

            case Anchor.Right:
                Pos = new Vector2i(
                    Position.X + (Size.X - Margin.R) - TextSize.X,
                    Position.Y + (Size.Y / 2) - (TextSize.Y / 2)
                );
                break;
        }

        DrawTextEx(Theme.Font, Text, Pos.ToVector2(), Theme.FontSize, Theme.FontSpacing, Theme.ButtonForeground);

        // Debug
        // var Center = new Vector2i(
        //     Position.X + Size.X / 2 + Padding.X,
        //     Position.Y + Size.Y / 2 + Padding.Y
        // );

        // DrawRectangleLines(Position.X, Position.Y, Size.X + Padding.X, Size.Y + Padding.Y, Color.WHITE);
        // DrawCircleV(Center.ToVector2(), 4.0f, Color.GREEN);
        // DrawCircleV(Pos.ToVector2(), 4.0f, Color.RED);
    }
}