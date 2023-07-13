using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Entity {
    public int ID { get; private set; }
    public Vector2i Position { get; private set; }
    public Vector2i Velocity { get; private set; }
    public Rectangle Hitbox { get; private set; }

    public Vector2i Size { get { return new Vector2i(Hitbox.width, Hitbox.height); } }

    private int MoveSpeed = 4;
    private int JumpSpeed = 12;

    private Vector2i Gravity = new Vector2i(0, 1);
    private Vector2i MaxVelocity = new Vector2i(10, 10);

    private Vector2i SpawnPosition;

    public Entity() {
        ID = 0;
        Position = new Vector2i(1000, 500);
        Velocity = Vector2i.Zero;
        Hitbox = new Rectangle(0, 0, 50, 100);

        SpawnPosition = Position;
    }

    public void Move(Vector2i dir) {
        Position = new Vector2i(Position.X + (dir.X * MoveSpeed), Position.Y + (dir.Y * MoveSpeed));
    }

    public void Jump() {
        Velocity = new Vector2i(Velocity.X, Velocity.Y - JumpSpeed);
    }

    public void Respawn() {
        Position = SpawnPosition;
    }

    public void Update() {
        Velocity = new Vector2i(
            Math.Clamp(Velocity.X + Gravity.X, -MaxVelocity.X, MaxVelocity.X),
            Math.Clamp(Velocity.Y + Gravity.Y, -MaxVelocity.Y, MaxVelocity.Y)
        );

        Position = Position + Velocity;
    }

    public void Draw() {
        DrawRectangleV(Position.ToVector2() - (Size.ToVector2() / 2), Size.ToVector2(), new Color(0, 255, 255, 100));
        DrawRectangleLines(Position.X - (Size.X / 2), Position.Y - (Size.Y / 2), Size.X, Size.Y, new Color(0, 255, 255, 255));
    }
}