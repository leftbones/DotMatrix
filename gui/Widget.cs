using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Widget {
    public Container? Parent { get; set; }
    public Vector2i Position { get; set; } = Vector2i.Zero;
    public Vector2i Size { get; set; } = Vector2i.Zero;
    public bool Active { get; set; } = true;

    public Rectangle ClickBox { get { return new Rectangle(Position.X, Position.Y, Size.X, Size.Y); } }

    public Widget() { }

    public virtual bool FireEvent(Event E) {
        return false;
    }

    public virtual void Update() { }

    public virtual void Draw() { }
}