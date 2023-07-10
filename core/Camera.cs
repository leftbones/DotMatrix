using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Camera {
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Vector2i Position { get; private set; }
    public int PanSpeed { get; private set; }

    public Camera2D Viewport;                                                       // Raylib Camera2D instance
    public Chunk Chunk { get { return Matrix.GetChunk(Position); } }                // Matrix Chunk containing the Camera

    // Skybox (Experimental)
    public bool DrawSkybox { get; set; } = false;                                   // Render the skybox in the background
    private Texture2D Skybox = LoadTexture("res/backgrounds/background.png");

    // Minimap (Experimental)
    private Vector2i MinimapPos;                                                    // Position of the minimap in the viewport
    private Vector2i MinimapSize;                                                   // Size of the minimap (in pixels)
    private float MinimapFrameSize;                                                 // Thickness of the minimap outline
    private Rectangle MinimapRec;                                                   // Rectangle of the minimap outline


    public Camera(Engine engine) {
        Engine = engine;
        Position = (Engine.Matrix.Size * Engine.MatrixScale) / 2;
        PanSpeed = 4;

        // Viewport
        Viewport = new Camera2D();
        Viewport.target = Position.ToVector2();
        Viewport.offset = new Vector2i(Engine.WindowSize.X / 2, Engine.WindowSize.Y / 2).ToVector2();
        Viewport.rotation = 0.0f;
        Viewport.zoom = 1.0f;

        // Minimap
        MinimapSize = new Vector2i(4 * Matrix.ChunkSize.X, 3 * Matrix.ChunkSize.Y);
        MinimapPos = new Vector2i(Engine.WindowSize.X - 10, Engine.WindowSize.Y - 10);
        MinimapFrameSize = 1.0f;
        MinimapRec = new Rectangle(MinimapPos.X - MinimapSize.X - MinimapFrameSize, MinimapPos.Y - MinimapSize.Y - MinimapFrameSize, MinimapSize.X + (MinimapFrameSize * 2), MinimapSize.Y + (MinimapFrameSize * 2));
    }

    public void Pan(Vector2i dir) {
        Position = new Vector2i(Position.X + (dir.X * PanSpeed), Position.Y + (dir.Y * PanSpeed));
    }

    public void Update() {
        Viewport.target = Position.ToVector2();
    }

    public void Draw() {
        // Skybox + Parallax
        if (DrawSkybox)
            DrawTexturePro(Skybox, new Rectangle(0, 0, Skybox.width, Skybox.height), new Rectangle(0, 0, Engine.WindowSize.X, Engine.WindowSize.Y), Vector2.Zero, 0.0f, Color.WHITE);

        // Minimap
        DrawRectangleRec(MinimapRec, new Color(255, 255, 255, 50));
        DrawRectangleLinesEx(MinimapRec, MinimapFrameSize, Color.WHITE);
        foreach (var Chunk in Matrix.Chunks) {
            var Pos = new Vector2i(MinimapPos.X - MinimapSize.X + Chunk.Position.X, MinimapPos.Y - MinimapSize.Y + Chunk.Position.Y);
            DrawTexture(Chunk.Texture, Pos.X, Pos.Y, Color.WHITE);
        }
    }
}