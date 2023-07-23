using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Camera {
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Vector2 Position { get; private set; }
    public float PanSpeed { get; private set; }

    public Transform? Target { get; set; }
    public Vector2 TargetPos { get; set; }

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
        Viewport = new Camera2D();
        Viewport.target = Position;
        Viewport.offset = new Vector2i(Engine.WindowSize.X / 2, Engine.WindowSize.Y / 2).ToVector2();
        Viewport.rotation = 0.0f;
        Viewport.zoom = 1.0f;
    }

    public void Pan(Vector2i dir) {
        TargetPos = new Vector2(TargetPos.X + (dir.X * 4), TargetPos.Y + (dir.Y * 4));
    }

    public void Reset() {
        TargetPos = StartPos;
    }

    public void Update() {
        var Dest = Target is null ? TargetPos : Target.Position.ToVector2();
        if (Position != Dest) {
            Position = Vector2.Lerp(Position, Dest, PanSpeed);
        }

        Viewport.target = Position;
    }

    public void Draw() {
        // Skybox + Parallax
        if (DrawSkybox)
            DrawTexturePro(Skybox, new Rectangle(0, 0, Skybox.width, Skybox.height), new Rectangle(0, 0, Engine.WindowSize.X, Engine.WindowSize.Y), Vector2.Zero, 0.0f, Color.WHITE);
    }
}