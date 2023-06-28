using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Container {
    public Interface Parent { get; private set; }
    public Vector2i Position { get; private set; }
    public Vector2i Size { get; private set; }
    public Quad Margin { get; private set; }
    public Anchor DrawAnchor { get; private set; } // NOTE: Container only supports Top, Bottom, and Center Anchors
    public bool Background { get; private set; }
    public bool Horizontal { get; private set; }

    public bool Scroll { get; private set; } = false;
    public float ScrollOffset { get; private set; } = 0.0f;

    public Theme Theme { get { return Parent.Theme; } }

    public Vector2i Origin { get { return new Vector2i(Position.X + Margin.L, Position.Y + Margin.U); } }
    public Rectangle Rect { get { return new Rectangle(Position.X, Position.Y, Size.X, Size.Y); } }
    public Rectangle Area { get { return new Rectangle(Position.X + Margin.L, Position.Y + Margin.U, Size.X - (Margin.L + Margin.R), Size.Y - (Margin.U + Margin.D)); } }

    public List<Widget> Widgets { get; private set; } = new List<Widget>();

    public bool Active { get; private set; } = true;

    public Container(Interface parent, Vector2i position, Vector2i? size=null, Quad? margin=null, Anchor? draw_anchor=null, bool background=true, bool horizontal=false, bool activated=true) {
        Parent = parent;
        Position = position;
        Size = size ?? Vector2i.Zero;
        Margin = margin ?? new Quad(5, 5, 5, 5);
        DrawAnchor = draw_anchor ?? Anchor.Top;
        Background = background;
        Horizontal = horizontal;
        Active = activated;

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

    public virtual bool FireEvent(Event E) {
        if (!Active) return false;

        // Scrolling
        if (Scroll && E.Name.Contains("MouseWheel")) {
            ScrollOffset -= ((MouseWheelEvent)E).Amount;
            return true;
        }

        // Widgets
        foreach (var W in Widgets) {
            if (W.FireEvent(E))
                return true;
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
            DrawOffset = new Vector2i(0, Widgets.Sum(W => W.Size.Y + W.Padding.Y + 5) + Margin.U);
            DrawPos = new Vector2i(Position.X - DrawOffset.X, Position.Y - DrawOffset.Y);
        }

        // Size
        int MinWidth = 0;
        int MinHeight = 0;

        ////
        // Left to Right
        if (Horizontal) {
            // Calculate size -- TODO: Change to use Widgets.MaxBy(W => W.Size.X + W.Padding.X + 5)
            foreach (var W in Widgets) {
                MinWidth += W.Size.X + W.Padding.X + 5;
                if (W.Size.Y > MinHeight) MinHeight = W.Size.Y + W.Padding.Y + 5;
            }

            Size = new Vector2i(Math.Max(Size.X, MinWidth + 5), Math.Max(Size.Y, MinHeight + 5));

            // Background
            if (Background)
                DrawRectangleV(DrawPos.ToVector2(), Size.ToVector2(), Theme.Background);

            // Widgets
            int Offset = 0;
            foreach (var W in Widgets) {
                var Pos = new Vector2i(Origin.X + Offset, Origin.Y);
                W.Position = Pos;

                W.Draw();

                Offset += W.Size.X + W.Padding.X + 5;
            }
        }

        ////
        // Top to Bottom (Default)
        else {
            // Calculate size
            foreach (var W in Widgets) {
                if (W.Size.X > MinWidth) MinWidth = W.Size.X + W.Padding.X + 5;
                MinHeight += W.Size.Y + W.Padding.Y + 5;
            }

            if (DrawAnchor == Anchor.Center) {
                DrawOffset = new Vector2i(MinWidth / 2, (Widgets.Sum(W => W.Size.Y + W.Padding.Y + 5) + Margin.U) / 2);
                DrawPos = new Vector2i(Position.X - DrawOffset.X, Position.Y - DrawOffset.Y);
            }

            Size = new Vector2i(Math.Max(Size.X, MinWidth + 5), Math.Max(Size.Y, MinHeight + 5));

            // Background
            if (Background)
                DrawRectangleV(DrawPos.ToVector2(), Size.ToVector2(), Theme.Background);

            // Widgets
            int Offset = 0;
            Vector2i Pos;
            foreach (var W in Widgets) {
                switch (W.Anchor) {
                    case Anchor.Left:
                        Pos = new Vector2i(Origin.X, Origin.Y + Offset - ScrollOffset) - DrawOffset;
                        W.Position = Pos;
                        break;
                    case Anchor.Right:
                        Pos = new Vector2i(DrawPos.X + Size.X - W.Size.X - W.Padding.R, Origin.Y + Offset - ScrollOffset) - DrawOffset;
                        W.Position = Pos;
                        break;
                    case Anchor.Center:
                        break;
                }

                W.Draw();

                Offset += W.Size.Y + W.Padding.Y + 5;
            }

            // Scroll
            if (Offset > Size.Y) Scroll = true;
            else Scroll = false;
        }
    }
}