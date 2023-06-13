using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Matrix {
    public Engine Engine { get; private set; }              // Reference to the parent Engine
    public int Scale { get; private set; }                  // Dots-to-pixel scale

    public Vector2i Size { get; private set; }              // Size of the Matrix (in Dots)
    public Dot[,] Dots { get; private set; }                // 2D array that stores the Dots

    public Texture2D Texture { get; private set; }          // Render texture that Dots are drawn to
    private Image Buffer;                                   // Buffer image used to create the render texture

    private Rectangle SourceRec;                            // Actual size of the Matrix texture (Pixel:Dot = 1:1)
    private Rectangle DestRec;                              // Scaled size of the Matrix texture (Pixel:Dot = 1:Scale)

    public Matrix(Engine engine, int scale) {
        Engine = engine;
        Scale = scale;

        // Calculate the Matrix size from the window size and scale
        Size = new Vector2i(Engine.WindowSize.X / Scale, Engine.WindowSize.Y / Scale);

        // Size the source and destination rectangles
        SourceRec = new Rectangle(0, 0, Size.X, Size.Y);
        DestRec = new Rectangle(0, 0, Engine.WindowSize.X, Engine.WindowSize.Y);

        // Create and populate the Dot array
        Dots = new Dot[Size.X, Size.Y];
        for (int y = Size.Y - 1; y >= 0; y--) {
            for (int x = 0; x < Size.X; x++) {
                Dots[x, y] = new Dot(-1, new Vector2i(x, y));
            }
        }

        // Generate the Buffer and Texture
        Buffer = GenImageColor(Size.X, Size.Y, Color.BLACK);
        Texture = LoadTextureFromImage(Buffer);
    }

    // Get a Dot from the Matrix (Vector2i pos)
    public Dot Get(Vector2i pos) {
        return Dots[pos.X, pos.Y];
    }

    // Get a Dot from the Matrix (int pos)
    public Dot Get(int x, int y) {
        return Dots[x, y];
    }

    // Set a Dot in the Matrix (Vector2i pos)
    public void Set(Vector2i pos, Dot pixel) {
        Dots[pos.X, pos.Y] = pixel;
        pixel.Position = pos;
    }

    // Place a Dot in the Matrix and update it's position
    public void Set(int x, int y, Dot pixel) {
        Dots[x, y] = pixel;
        pixel.LastPosition = pixel.Position;
        pixel.Position = new Vector2i(x, y);
    }

    // Swap two Dots in the Matrix (Vector2i pos)
    public void Swap(Vector2i pos1, Vector2i pos2) {
        var D1 = Get(pos1);
        var D2 = Get(pos2);
        Set(pos2, D1);
        Set(pos1, D2);
    }

    // Swap a Dot with another Dot if the destination is in bounds and empty
    public bool SwapIfValid(Vector2i pos1, Vector2i pos2) {
        if (IsValid(pos2)) {
            Swap(pos1, pos2);
            return true;
        }
        return false;
    }

    // Check if a position in the Matrix is in bounds and empty
    public bool IsValid(Vector2i pos) {
        if (InBounds(pos))
            return (IsEmpty(pos));
        return false;
    }

    // Check if a position is within the bounds of the Matrix
    public bool InBounds(Vector2i pos) {
        return pos.X >= 0 && pos.X < Size.X && pos.Y >= 0 && pos.Y < Size.Y;
    }

    // Check if a position in the Matrix is empty (ID -1)
    public bool IsEmpty(Vector2i pos) {
        return Get(pos).ID == -1;
    }

    // Update each Dot in the Matrix
    public void Update() {
        bool IsEvenTick = Engine.Tick % 2 == 0;

        for (int y = Size.Y - 1; y >= 0; y--) {
            for (int x = IsEvenTick ? 0 : Size.X - 1; IsEvenTick ? x < Size.X : x >= 0; x += IsEvenTick ? 1 : -1) {
                var D = Get(x, y);
                if (D.ID > -1)
                    D.Step(this);
            }
        }
    }

    // Draw each Dot in the Matrix to the render texture
    public unsafe void Draw() {
        // Update Texture
        ImageClearBackground(ref Buffer, Color.BLACK);

        foreach (var D in Dots)
            ImageDrawPixel(ref Buffer, D.Position.X, D.Position.Y, D.Color);

        UpdateTexture(Texture, Buffer.data);

        // Draw Texture
        DrawTexturePro(Texture, SourceRec, DestRec, Vector2.Zero, 0, Color.WHITE);

        // Matrix Lines (Debug)
        // for (int x = 0; x < Engine.WindowSize.X / Scale; x++)
        //     DrawLine(x * Scale, 0, x * Scale, Engine.WindowSize.Y, Color.DARKGRAY);
        // for (int y = 0; y < Engine.WindowSize.Y / Scale; y++)
        //     DrawLine(0, y * Scale, Engine.WindowSize.X, y * Scale, Color.DARKGRAY);
    }
}