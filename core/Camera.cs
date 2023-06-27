using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Camera {
    public Engine Engine { get; private set; }
    public Vector2i Position { get; private set; }
    public int PanSpeed { get; private set; }

    public Camera2D Viewport;

    public Camera(Engine engine) {
        Engine = engine;
        Position = (Engine.Matrix.Size * Engine.MatrixScale) / 2;
        PanSpeed = 4;

        Viewport = new Camera2D();
        Viewport.target = Position.ToVector2();
        Viewport.offset = new Vector2i(Engine.WindowSize.X / 2, Engine.WindowSize.Y / 2).ToVector2();
        Viewport.rotation = 0.0f;
        Viewport.zoom = 1.0f;
    }

    public void Pan(Vector2i dir) {
        Position = new Vector2i(Position.X + (dir.X * PanSpeed), Position.Y + (dir.Y * PanSpeed));
    }

    public void Update() {
        Viewport.target = Position.ToVector2();
    }
}