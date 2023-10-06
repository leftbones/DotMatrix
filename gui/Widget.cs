using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Widget {
    public Container Parent { get; set; }
    public Vector2i Position { get; set; } = Vector2i.Zero;
    public virtual Vector2i Size { get; set; } = Vector2i.Zero;
    public Anchor Anchor { get; set; } = Anchor.Left;
    public bool Active { get; set; } = true;

    public virtual Rectangle ClickBox { get { return new Rectangle(Position.X, Position.Y, Size.X + Padding.X, Size.Y + Padding.Y); } }
    public virtual Quad Padding { get; set; } = Quad.Zero;

    public Widget(Container parent) {
        Parent = parent;
    }

    public virtual bool FireEvent(Key K) {
        return false;
    }

    public virtual void Update() { }

    public virtual void Draw() { }
}