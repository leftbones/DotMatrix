namespace DotMatrix;

class Transform : Token {
    public Vector2i Position { get; set; }
    public float Rotation { get; set; }

    public Transform(Vector2i? position=null, float? rotation=null) {
        Position = position ?? Vector2i.Zero;
        Rotation = rotation ?? 1.0f;
    }
}