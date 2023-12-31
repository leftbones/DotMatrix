using System.Numerics;

namespace DotMatrix;

/// <summary>
/// Tracks where the entity is in the world, required for any entity that physically exists in the world
/// </summary>

class Transform : Token {
    public Vector2i Position { get; set; }
    public float Rotation { get; set; }

    public Transform(Vector2i? position=null, float? rotation=null) {
        Position = position ?? Vector2i.Zero;
        Rotation = rotation ?? 0.0f;
    }
}