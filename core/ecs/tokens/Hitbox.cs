using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

/// <summary>
/// Used for collision detection
/// Required Tokens: Transform
/// </summary>

class Hitbox : Token {
    public int Width { get; private set; }
    public int Height { get; private set; }
    public Vector2i Offset { get; private set; }

    public Rectangle Rect { get {
        var Transform = Entity!.GetToken<Transform>();
        return new Rectangle(
            Transform!.Position.X - (Width / 2 * Global.MatrixScale) + (Offset.X * Global.MatrixScale),
            Transform!.Position.Y - (Height / 2 * Global.MatrixScale) + (Offset.Y * Global.MatrixScale),
            Width * Global.MatrixScale,
            Height * Global.MatrixScale
        );
    }}

    public Hitbox(int width, int height, Vector2i? offset=null) {
        Width = width;
        Height = height;
        Offset = offset ?? Vector2i.Zero;
    }
}