using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

// TODO
// - Finish documenting properties
// - Document methods

class Container {
    public Interface Parent { get; private set; }                       // Reference to the parent Interface class
    public Vector2i Position { get; private set; }                      // Position within the interface area
    public Vector2i Size { get; private set; }                          // Size of the container, either defined or determined by child Widgets
    public Quad Margin { get; private set; }                            // Gap between the edges of the container and child Widgets
    public Quad Spacing { get; private set; }                           // Gap between child Widgets
    public Anchor DrawAnchor { get; private set; }                      // Anchor from which to draw the container
    public bool Background { get; private set; }                        // If the container should have a background
    public bool Horizontal { get; private set; }                        // If the child Widgets should be laid out horizontally (false is vertical)

    public bool Scroll { get; private set; } = false;                   // If scrolling is enabled (auto set to true when the combined height of all child Widgets exceeds the height of the container)
    public float ScrollOffset { get; private set; } = 0.0f;             // Current scroll offset, used to determine drawing position of child Widgets

    public Theme Theme { get { return Parent.Theme; } }                 // Theme of the parent Interface class

    public Vector2i Origin { get { return new Vector2i(Position.X + Margin.L, Position.Y); } }
    public Rectangle Rect { get { return new Rectangle(Position.X, Position.Y, Size.X, Size.Y); } }
    public Rectangle Area { get { return new Rectangle(Position.X + Margin.L, Position.Y + Margin.U, Size.X - (Margin.L + Margin.R), Size.Y - (Margin.U + Margin.D)); } }

    public List<Widget> Widgets { get; private set; } = new List<Widget>(); // TODO: Change to allow Containers as children (the Toolbar is a good example of why this is necessary) then make position an optional parameter

    public bool Active { get; private set; } = true;

    public Container(Interface parent, Vector2i position, Vector2i? size=null, Quad? margin=null, Quad? spacing=null, Anchor? draw_anchor=null, bool background=true, bool horizontal=false, bool activated=true) {
        Parent = parent;
        Position = position;
        Size = size ?? Vector2i.Zero;
        Margin = margin ?? new Quad(5, 5, 5, 5);
        Spacing = spacing ?? Quad.Zero;
        DrawAnchor = draw_anchor ?? Anchor.Top;
        Background = background;
        Horizontal = horizontal;
        Active = activated;

        // NOTE: Container only supports Top, Bottom, and Center Anchors
        if (DrawAnchor != Anchor.Top && DrawAnchor != Anchor.Bottom && DrawAnchor != Anchor.Center)
            DrawAnchor = Anchor.Top;
    }

    public void Toggle() {
        Active = !Active;
    }

    public void AddWidget(Widget W) {
        if (Widgets.Contains(W)) return;

        W.Parent = this;
        Widgets.Add(W);
    }

    public virtual bool FireEvent(Key K) {
        if (!Active) return false;

        if (K.Type == EventType.Press) {
            if (Scroll) {
                if (Global.IsKey("mouse_wheel_up", K.Code)) {
                    ScrollOffset -= 1;
                    return true;
                }

                if (Global.IsKey("mouse_wheel_down", K.Code)) {
                    ScrollOffset += 1;
                    return true;
                }
            }
        }

        // Widgets
        foreach (var W in Widgets) {
            if (W.FireEvent(K)) {
                return true;
            }
        }

        return false;
    }

    public virtual void Update() {
        if (!Active) return;

        foreach (var W in Widgets)
            W.Update();
    }

    public virtual void Draw() {
        if (!Active) return;

        // Draw Anchor
        Vector2i DrawPos = Position;
        Vector2i DrawOffset = Vector2i.Zero;

        if (DrawAnchor == Anchor.Bottom) {
            DrawOffset = new Vector2i(0, Widgets.Sum(W => W.Size.Y + W.Padding.Y + Spacing.Y) + Margin.U);
            DrawPos = new Vector2i(Position.X - DrawOffset.X, Position.Y - DrawOffset.Y - Margin.U);
        }

        // Size
        int MinWidth = 0;
        int MinHeight = 0;

        ////
        // Left to Right
        if (Horizontal) {
            // Calculate size -- TODO: Change to use Widgets.MaxBy(W => W.Size.X + W.Padding.X + Spacing.X)
            foreach (var W in Widgets) {
                MinWidth += W.Size.X + W.Margin.X + W.Padding.X + Spacing.X;
                if (W.Size.Y > MinHeight) MinHeight = W.Size.Y + W.Margin.Y + W.Padding.Y + Spacing.Y;
            }

            Size = new Vector2i(Math.Max(Size.X, MinWidth + Margin.X + Spacing.X), Math.Max(Size.Y, MinHeight + Margin.Y + Spacing.Y));

            // Background
            if (Background)
                DrawRectangleV(DrawPos.ToVector2(), Size.ToVector2(), Theme.Background);

            // Widgets
            int Offset = 0;
            foreach (var W in Widgets) {
                var Pos = new Vector2i(Origin.X + Offset, Origin.Y);
                W.Position = Pos;

                W.Draw();

                Offset += W.Size.X + W.Padding.X + Spacing.X;
            }
        }

        ////
        // Top to Bottom (Default)
        else {
            // Calculate size
            foreach (var W in Widgets) {
                if (W.Size.X > MinWidth) MinWidth = W.Size.X + W.Margin.X + W.Padding.X + Spacing.X;
                MinHeight += W.Size.Y + W.Margin.Y + W.Padding.Y + Spacing.Y;
            }

            if (DrawAnchor == Anchor.Center) {
                DrawOffset = new Vector2i(MinWidth / 2, (Widgets.Sum(W => W.Size.Y + W.Padding.Y + Spacing.Y) + Margin.U) / 2);
                DrawPos = new Vector2i(Position.X - DrawOffset.X, Position.Y - DrawOffset.Y - Margin.Y);
            }

            Size = new Vector2i(Math.Max(Size.X, MinWidth + Margin.X + Spacing.X), Math.Max(Size.Y, MinHeight + Margin.Y + Spacing.Y));

            // Background
            if (Background)
                DrawRectangleV(DrawPos.ToVector2(), Size.ToVector2(), Theme.Background);

            // Widgets
            int Offset = 0;
            Vector2i Pos;
            foreach (var W in Widgets) {
                switch (W.Anchor) {
                    case Anchor.Left:
                        Pos = new Vector2i(
                            Origin.X,
                            Origin.Y + Offset - ScrollOffset
                        ) - DrawOffset;

                        W.Position = Pos;
                        break;
                    case Anchor.Right:
                        Pos = new Vector2i(
                            Origin.X + Size.X - W.Size.X - W.Padding.X - W.Margin.X - Margin.X,
                            Origin.Y + Offset - ScrollOffset
                        ) - DrawOffset;

                        W.Position = Pos;
                        break;
                    case Anchor.Center:
                        Pos = new Vector2i(
                            Origin.X + (Size.X / 2) - (W.Size.X / 2) - Margin.X,
                            Origin.Y + Offset - ScrollOffset
                        ) - DrawOffset;

                        W.Position = Pos;
                        break;
                }

                W.Draw();

                Offset += W.Size.Y + W.Padding.Y + W.Margin.Y + Spacing.Y;
            }

            // Scroll
            if (Offset > Size.Y) Scroll = true;
            else Scroll = false;
        }
    }
}