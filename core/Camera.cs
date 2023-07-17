using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Camera {
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Vector2i Position { get; private set; }
    public float PanSpeed { get; private set; }

    public Transform? Target { get; set; }
    public Vector2i TargetPos { get; set; }

    public Camera2D Viewport;                                                                           // Raylib Camera2D instance
    public Chunk Chunk { get { return Matrix.GetChunk(Position / Matrix.Scale); } }       // Matrix Chunk containing the Camera

    // Skybox (Experimental)
    public bool DrawSkybox { get; set; } = false;                                                       // Render the skybox in the background
    private Texture2D Skybox = LoadTexture("res/backgrounds/background.png");

    public Camera(Engine engine) {
        Engine = engine;
        Position = (Engine.Matrix.Size * Engine.MatrixScale) / 2;
        TargetPos = Position;
        PanSpeed = 0.033f;

        // Viewport
        Viewport = new Camera2D();
        Viewport.target = Position.ToVector2();
        Viewport.offset = new Vector2i(Engine.WindowSize.X / 2, Engine.WindowSize.Y / 2).ToVector2();
        Viewport.rotation = 0.0f;
        Viewport.zoom = 1.0f;
    }

    public void Pan(Vector2i dir) {
        TargetPos = new Vector2i(TargetPos.X + (dir.X * 4), TargetPos.Y + (dir.Y * 4));
    }

    public void Update() {
        var Dest = Target is null ? TargetPos : Target.Position;
        if (Position != Dest) {
            Position = Vector2i.Lerp(Position, Dest, PanSpeed);
        }

        Viewport.target = Position.ToVector2();
    }

    public void Draw() {
        // Skybox + Parallax
        if (DrawSkybox)
            DrawTexturePro(Skybox, new Rectangle(0, 0, Skybox.width, Skybox.height), new Rectangle(0, 0, Engine.WindowSize.X, Engine.WindowSize.Y), Vector2.Zero, 0.0f, Color.WHITE);
    }
}