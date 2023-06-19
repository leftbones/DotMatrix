using System.Linq;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Matrix {
    public Engine Engine { get; private set; }              // Reference to the parent Engine instance
    public int Scale { get; private set; }                  // Scale of the Matrix texture (Matrix pixel to screen pixel)

    public Vector2i Size { get; private set; }              // Size of the Matrix (in Pixels)
    public Pixel[,] Pixels { get; private set; }            // 2D array that stores the Pixels

    public Chunk[,] Chunks { get; private set; }            // 2D array that stores Chunks
    public Vector2i ChunkSize { get; private set; }         // Size for each Chunk
    public int MaxChunksX { get; private set; }             // Number of Chunks in a row
    public int MaxChunksY { get; private set; }             // Number of Chunks in a column

    public Texture2D Texture { get; private set; }          // Render texture that Pixels are drawn to
    private Image Buffer;                                   // Buffer image used to create the render texture

    private Rectangle SourceRec;                            // Actual size of the Matrix texture
    private Rectangle DestRec;                              // Scaled size of the Matrix texture

    public Matrix(Engine engine) {
        Engine = engine;
        Scale = Engine.MatrixScale;

        // Calculate the Matrix size from the window size and scale
        Size = new Vector2i(Engine.WindowSize.X / Scale, Engine.WindowSize.Y / Scale);

        // Size the source and destination rectangles
        SourceRec = new Rectangle(0, 0, Size.X, Size.Y);
        DestRec = new Rectangle(0, 0, Engine.WindowSize.X, Engine.WindowSize.Y);

        // Create and populate the Pixel array
        Pixels = new Pixel[Size.X, Size.Y];
        for (int y = Size.Y - 1; y >= 0; y--) {
            for (int x = 0; x < Size.X; x++) {
                Set(new Vector2i(x, y), new Pixel(), wake_chunk: false);
            }
        }

        // Generate the Buffer and Texture
        Buffer = GenImageColor(Size.X, Size.Y, Color.BLACK);
        Texture = LoadTextureFromImage(Buffer);

        // Setup Chunks
        ChunkSize = new Vector2i(20, 20);
        MaxChunksX = Size.X / ChunkSize.X;
        MaxChunksY = Size.Y / ChunkSize.Y;
        Chunks = new Chunk[MaxChunksX, MaxChunksY];
        for (int x = 0; x < MaxChunksX; x++) {
            for (int y = 0; y < MaxChunksY; y++) {
                var Pos = new Vector2i(x * ChunkSize.X, y * ChunkSize.Y);
                Chunks[x, y] = new Chunk(Pos, ChunkSize);
            }
        }
    }

    // Get a Pixel from the Matrix (Vector2i pos)
    public Pixel Get(Vector2i pos) {
        return Pixels[pos.X, pos.Y];
    }

    // Get a Pixel from the Matrix (int pos)
    public Pixel Get(int x, int y) {
        return Pixels[x, y];
    }

    // Place a Pixel in the Matrix and update it's position (Vector2i pos)
    public void Set(Vector2i pos, Pixel pixel, bool wake_chunk=true) {
        Pixels[pos.X, pos.Y] = pixel;
        pixel.Position = pos;

        if (wake_chunk)
            WakeChunk(pos);
    }

    // Place a Pixel in the Matrix and update it's position
    public void Set(int x, int y, Pixel pixel, bool wake_chunk=true) {
        Pixels[x, y] = pixel;
        pixel.LastPosition = pixel.Position;
        pixel.Position = new Vector2i(x, y);

        if (wake_chunk)
            WakeChunk(pixel.Position);
    }

    // Swap two Pixels in the Matrix, checking if the destination is in bounds
    public bool Swap(Vector2i pos1, Vector2i pos2) {
        if (!InBounds(pos2))
            return false;

        var P1 = Get(pos1);
        var P2 = Get(pos2);
        Set(pos2, P1);
        Set(pos1, P2);

        return true;
    }

    // Swap two Pixels in the Matrix without checking if the destination is in bounds, always returns true (or crashes if used wrong)
    public bool QuickSwap(Pixel p1, Pixel p2) {
        var Pos1 = p1.Position;
        var Pos2 = p2.Position;
        Set(Pos2, p1);
        Set(Pos1, p2);
        return true;
    }

    // Swap a Pixel with another Pixel if the destination is in bounds and a valid move (based on weight/type)
    public bool SwapIfValid(Vector2i pos1, Vector2i pos2) {
        // Destination is out of bounds
        if (!InBounds(pos2))
            return false;

        var P1 = Get(pos1);
        var P2 = Get(pos2);

        // Destination is empty
        if (P2.ID == -1)
            return QuickSwap(P1, P2);

        // Pixel 2 is Solid 
        if (P2 is Solid)
            return false;

        // Both Pixels are Gas and P1 is less faded than P2
        if (P1 is Gas && P2 is Gas) {
            if (RNG.Roll(75) && P1.ColorFade > P2.ColorFade)
                return QuickSwap(P1, P2);
        }

        var MoveDir = Direction.GetMovementDirection(P1.Position, P2.Position);

        switch (MoveDir.Y) {
            case 0: // Horizontal only movement
                if (P1.GetType() != P2.GetType() && P1.Weight > P2.Weight)
                    return QuickSwap(P1, P2);
                return false;
            case 1: // Downward Y movement
                if (P1.Weight > P2.Weight)
                    return QuickSwap(P1, P2);
                return false;
            case -1: // Upward Y movement
                if (P1.Weight < P2.Weight)
                    return QuickSwap(P1, P2);
                return false;
            default:
                return false;
        }
    }

    // Check if a movement is valid (based on weight/type)
    public bool IsValid(Vector2i pos1, Vector2i pos2) {
        // Destination is out of bounds
        if (!InBounds(pos2))
            return false;

        var P1 = Get(pos1);
        var P2 = Get(pos2);

        // Destination is empty
        if (P2.ID == -1)
            return true;

        // Pixel 2 is Solid 
        if (P2 is Solid)
            return false;

        // Both Pixels are Gas and P1 is less faded than P2
        if (P1 is Gas && P2 is Gas) {
            if (RNG.Roll(75) && P1.ColorFade > P2.ColorFade)
                return QuickSwap(P1, P2);
        }

        var MoveDir = Direction.GetMovementDirection(P1.Position, P2.Position);

        switch (MoveDir.Y) {
            case 0: // Horizontal only movement
                if (P1.GetType() != P2.GetType() && P1.Weight > P2.Weight)
                    return true;
                return false;
            case 1: // Downward Y movement
                if (P1.Weight > P2.Weight)
                    return true;
                return false;
            case -1: // Upward Y movement
                if (P1.Weight < P2.Weight)
                    return true;
                return false;
            default:
                return false;
        }
    }

    // Check if a position is within the bounds of the Matrix
    public bool InBounds(Vector2i pos) {
        return pos.X >= 0 && pos.X < Size.X && pos.Y >= 0 && pos.Y < Size.Y;
    }

    // Check if a position in the Matrix is empty (ID -1)
    public bool IsEmpty(Vector2i pos) {
        return Get(pos).ID == -1;
    }

    // Check if a position is in bounds and empty
    public bool InBoundsAndEmpty(Vector2i pos) {
        if (InBounds(pos))
            return IsEmpty(pos);
        return false;
    }

    // Update each Pixel in the Matrix
    public void Update() {
        bool IsEvenTick = Engine.Tick % 2 == 0;

        for (int y = Size.Y - 1; y >= 0; y--) {
            for (int x = IsEvenTick ? 0 : Size.X - 1; IsEvenTick ? x < Size.X : x >= 0; x += IsEvenTick ? 1 : -1) {
                // Skip sleeping chunks
                var C = GetChunk(x, y);
                if (!C.Awake) continue;

                // Step all non-empty Pixels
                var P = Get(x, y);
                if (P.ID > -1) {
                    // Skip Pixels that have already stepped
                    if (P.Stepped)
                        continue;

                    P.Step(this);
                    P.Stepped = true;

                    P.Tick(this);
                    P.Ticked = true;

                    if (P.Active && P.Position != P.LastPosition)
                        P.ActOnNeighbors(this);
                }
            }
        }
    }

    // Actions performed before the start of the normal Update
    public void UpdateStart() {

    }

    // Actions performed at the end of the normal Update
    public void UpdateEnd() {
        // Reset Pixel Stepped and Ticked flags
        foreach (var P in Pixels) {
            P.Stepped = false;
            P.Ticked = false;
            P.Acted = false;
        }

        // Step Chunks
        foreach (var C in Chunks) {
            C.Step();
        }
    }

    // Return the chunk containing the given position (Vector2i pos)
    public Chunk GetChunk(Vector2i pos) {
        return Chunks[pos.X / ChunkSize.X, pos.Y / ChunkSize.Y];
    }

    // Return the chunk containing the given position (int x, int y)
    public Chunk GetChunk(int x, int y) {
        return Chunks[x / ChunkSize.X, y / ChunkSize.Y];
    }

    // Wake the chunk at the given position
    public void WakeChunk(Vector2i pos) {
        var Chunk = GetChunk(pos);
        Chunk.Wake();

        // Wake appropriate neighbor chunks if the position is on a border
        if (pos.X == Chunk.Position.X + Chunk.Size.X - 1 && InBounds(pos + Direction.Right)) GetChunk(pos + Direction.Right).Wake();
        if (pos.X == Chunk.Position.X && InBounds(pos + Direction.Left)) GetChunk(pos + Direction.Left).Wake();
        if (pos.Y == Chunk.Position.Y + Chunk.Size.Y - 1 && InBounds(pos + Direction.Down)) GetChunk(pos + Direction.Down).Wake();
        if (pos.Y == Chunk.Position.Y && InBounds(pos + Direction.Up)) GetChunk(pos + Direction.Up).Wake();
    }

    // Draw each Pixel in the Matrix to the render texture
    public unsafe void Draw() {
        // Update Texture
        ImageClearBackground(ref Buffer, Color.BLACK);

        foreach (var P in Pixels) {
            if (P.ID == -1) continue;
            Color C = P.Color;
            // Color C = P.Active ? P.Color : Color.RED;
            ImageDrawPixel(ref Buffer, P.Position.X, P.Position.Y, C);
        }

        UpdateTexture(Texture, Buffer.data);

        // Draw Texture
        DrawTexturePro(Texture, SourceRec, DestRec, Vector2.Zero, 0, Color.WHITE);

        // Pixel Grid
        // for (int y = 0; y < Engine.WindowSize.Y / Scale; y++) {
        //     DrawLine(x * Scale, 0, x * Scale, Engine.WindowSize.Y, Color.DARKGRAY);
        //     DrawLine(0, y * Scale, Engine.WindowSize.X, y * Scale, Color.DARKGRAY);
        // }

        // Chunk Borders
        if (Engine.Canvas.DrawChunks) {
            foreach (var C in Chunks) {
                var Col = C.Awake ? Color.WHITE : Color.DARKGRAY;
                var Str = $"{C.Position.X / ChunkSize.X}, {C.Position.Y / ChunkSize.Y}";
                DrawRectangleLines(C.Position.X * Scale, C.Position.Y * Scale, C.Size.X * Scale, C.Size.Y * Scale, Col);
                DrawTextEx(Engine.Theme.Font, Str, new Vector2i((C.Position.X * Scale) + 5, (C.Position.Y * Scale) + 5).ToVector2(), Engine.Theme.FontSize, Engine.Theme.FontSpacing, Col);
            }
        }
    }
}