using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

enum Anchor { Left, Center, Right }; // TODO: Implement TopLeft, BottomLeft, TopCenter, BottomCenter, TopRight, BottomRight

class Widget {
    public Container Parent { get; set; }
    public Vector2i Position { get; set; } = Vector2i.Zero;
    public virtual Vector2i Size { get; set; } = Vector2i.Zero;
    public virtual Quad Padding { get; set; } = Quad.Zero;
    public bool Active { get; set; } = true;

    public virtual Rectangle ClickBox { get { return new Rectangle(Position.X, Position.Y, Size.X + Padding.X, Size.Y + Padding.Y); } }

    public Widget(Container parent) {
        Parent = parent;
    }

    public virtual bool FireEvent(Event E) {
        return false;
    }

    public virtual void Update() { }

    public virtual void Draw() { }
}