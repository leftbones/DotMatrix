using System.Numerics;

namespace DotMatrix;

class Transform : Token {
    public Vector2 Position { get; set; }
    public Vector2 Scale { get; set; }
    public float Rotation { get; set; }

    public Transform() : this(Vector2.Zero, Vector2.One, 0.0f) { }
    public Transform(Vector2 position, Vector2 scale, float rotation) {
        Position = position;
        Scale = scale;
        Rotation = rotation;
    }
}