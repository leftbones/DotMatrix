using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

/// <summary>
/// The player's view into the world, it smoothly follows whatever Transform token is assigned to `Camera.Target`
/// Currently handles drawing the skybox (experimental testing only), this will likely be moved elsewhere later
/// </summary>

class Camera {
    public Engine Engine { get; private set; }
    public Pepper Pepper { get { return Engine.Pepper; } }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Vector2 Position { get; set; }
    public float PanSpeed { get; private set; }

    public Transform? Target { get; set; }
    public Vector2 TargetPos { get; set; }

    public bool Unlocked { get; private set; }

    public Camera2D Viewport;                                                                           // Raylib Camera2D instance
    public Chunk Chunk { get { return Matrix.GetChunk(new Vector2i(Position) / Matrix.Scale); } }                     // Matrix Chunk containing the Camera

    public Vector2 StartPos { get; private set; }

    // Skybox (Experimental)
    public bool DrawSkybox { get; set; } = false;                                                       // Render the skybox in the background
    private Texture2D Skybox = LoadTexture("res/backgrounds/background.png");

    public Camera(Engine engine) {
        Engine = engine;
        // Position = (Engine.Matrix.Size * Engine.MatrixScale) / 2;
        Position = new Vector2(500, 250);
        TargetPos = Position;
        PanSpeed = 0.033f;

        StartPos = Position;

        // Viewport
        Viewport = new Camera2D {
            target = Position,
            offset = new Vector2i(Engine.WindowSize.X / 2, Engine.WindowSize.Y / 2).ToVector2(),
            rotation = 0.0f,
            zoom = 1.0f
        };
    }

    public void ApplyConfig(Config C) {
        Pepper.Log("Camera config applied", LogType.SYSTEM);
    }

    public void Pan(Vector2i dir) {
        TargetPos = new Vector2(TargetPos.X + (dir.X * 4), TargetPos.Y + (dir.Y * 4));
        Unlocked = true;
    }

    public void Reset() {
        TargetPos = Target is null ? StartPos : Target.Position.ToVector2();
        Unlocked = false;
    }

    public void Update() {
        var Dest = Target is null || Unlocked ? TargetPos : Target.Position.ToVector2();
        if (Position != Dest) {
            Position = Vector2.Lerp(Position, Dest, PanSpeed);
            if (Vector2.Distance(Position, Dest) < 0.5f) {
                Position = Dest;
            }
        }

        Viewport.target = Position;
    }

    public void Draw() {
        // Skybox (Testing)
        if (DrawSkybox) {
            DrawTexturePro(Skybox, new Rectangle(0, 0, Skybox.width, Skybox.height), new Rectangle(0, 0, Engine.WindowSize.X, Engine.WindowSize.Y), Vector2.Zero, 0.0f, Color.WHITE);
        }
    }
}