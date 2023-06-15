using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

struct Quad {
    public int U;
    public int D;
    public int L;
    public int R;

    public Quad(int u, int d, int l, int r) {
        U = u;
        D = d;
        L = l;
        R = r;
    }
}

class Container {
    public Interface Parent { get; private set; }
    public Vector2i Position { get; private set; }
    public Vector2i Size { get; private set; }
    public Quad Margin { get; private set; }

    public bool Scroll { get; private set; } = false;
    public float ScrollOffset { get; private set; } = 0.0f;

    public Vector2i Origin { get { return new Vector2i(Position.X + Margin.L, Position.Y + Margin.U); } }
    public Rectangle Rect { get { return new Rectangle(Position.X, Position.Y, Size.X, Size.Y); } }
    public Rectangle Area { get { return new Rectangle(Position.X + Margin.L, Position.Y + Margin.U, Size.X - (Margin.L + Margin.R), Size.Y - (Margin.U + Margin.D)); } }

    public Color? Background { get; private set; }

    public List<Widget> Widgets { get; private set; } = new List<Widget>();

    public bool Active { get; private set; } = true;

    public Container(Interface parent, Vector2i position, Vector2i size, Quad? margin=null, Color? background=null) {
        Parent = parent;
        Position = position;
        Size = size;
        Margin = margin ?? new Quad(5, 5, 5, 5);
        Background = background;
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
        foreach (var W in Widgets)
            W.Update();
    }

    public virtual void Draw() {
        // Size
        int MinWidth = 0;
        int MinHeight = 0;

        foreach (var W in Widgets) {
            if (W.Size.X > MinWidth) MinWidth = W.Size.X;
            MinHeight += W.Size.Y;
        }

        Size = new Vector2i(Math.Max(Size.X, MinWidth), Math.Max(Size.Y, MinHeight));

        // Background
        if (Background is not null)
            DrawRectangleV(Position.ToVector2(), Size.ToVector2(), Background ?? Color.MAGENTA);

        // Widgets
        int Offset = 0;
        foreach (var W in Widgets) {
            var Pos = new Vector2i(Origin.X, Origin.Y + Offset - (ScrollOffset));
            W.Position = Pos;

            W.Draw();

            Offset += W.Size.Y;
        }

        // Scroll
        if (Offset > Size.Y) Scroll = true;
        else Scroll = false;
    }
}