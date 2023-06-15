using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Matrix {
    public Engine Engine { get; private set; }              // Reference to the parent Engine instance
    public int Scale { get; private set; }                  // Scale of the Matrix texture (Matrix pixel to screen pixel)

    public Vector2i Size { get; private set; }              // Size of the Matrix (in Pixels)
    public Pixel[,] Pixels { get; private set; }            // 2D array that stores the Pixels

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
                // Pixels[x, y] = new Pixel(-1, new Vector2i(x, y));
                Set(new Vector2i(x, y), new Pixel());
            }
        }

        // Generate the Buffer and Texture
        Buffer = GenImageColor(Size.X, Size.Y, Color.BLACK);
        Texture = LoadTextureFromImage(Buffer);
    }

    // Get a Pixel from the Matrix (Vector2i pos)
    public Pixel Get(Vector2i pos) {
        return Pixels[pos.X, pos.Y];
    }

    // Get a Pixel from the Matrix (int pos)
    public Pixel Get(int x, int y) {
        return Pixels[x, y];
    }

    // Set a Pixel in the Matrix (Vector2i pos)
    public void Set(Vector2i pos, Pixel pixel) {
        Pixels[pos.X, pos.Y] = pixel;
        pixel.Position = pos;
    }

    // Place a Pixel in the Matrix and update it's position
    public void Set(int x, int y, Pixel pixel) {
        Pixels[x, y] = pixel;
        pixel.LastPosition = pixel.Position;
        pixel.Position = new Vector2i(x, y);
    }

<<<<<<< Updated upstream
    // Swap two Pixels in the Matrix (Vector2i pos)
    public void Swap(Vector2i pos1, Vector2i pos2) {
        var P1 = Get(pos1);
        var P2 = Get(pos2);
        Set(pos2, P1);
        Set(pos1, P2);
    }

    // Swap two Pixels in the Matrix (p1, p2)
    public void Swap(Pixel p1, Pixel p2) {
        var Pos1 = p1.Position;
        var Pos2 = p2.Position;
        Set(Pos2, p1);
        Set(Pos1, p2);
=======
    // Swap two Pixels in the Matrix, checking if the destination is in bounds
    public bool Swap(Vector2i pos1, Vector2i pos2) {
        if (!InBounds(pos2))
            return false;

        var D1 = Get(pos1);
        var D2 = Get(pos2);
        Set(pos2, D1);
        Set(pos1, D2);

        return true;
>>>>>>> Stashed changes
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
        if (RNG.Roll(75) && P1 is Gas) {
            if (P2 is Gas && P1.ColorFade > P2.ColorFade)
                return QuickSwap(P1, P2);
        }

        var MoveDir = Direction.GetMovementDirection(P1.Position, P2.Position);

        switch (MoveDir.Y) {
            case 0: // Horizontal only movement
                return QuickSwap(P1, P2);
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

        var MoveDir = Direction.GetMovementDirection(P1.Position, P2.Position);

        switch (MoveDir.Y) {
            case 0: // Horizontal only movement
                return QuickSwap(P1, P2);
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
                var P = Get(x, y);
                if (P.ID > -1) {
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

    // Final actions performed at the end of the Update
    public void UpdateEnd() {
        foreach (var P in Pixels) {
            P.Stepped = false;
            P.Ticked = false;
        }
    }

    // Draw each Pixel in the Matrix to the render texture
    public unsafe void Draw() {
        // Update Texture
        ImageClearBackground(ref Buffer, Color.BLACK);

        foreach (var P in Pixels) {
            Color C = P.Active ? P.Color : Color.RED;
            // Color C = P.Color;
            ImageDrawPixel(ref Buffer, P.Position.X, P.Position.Y, C);
        }

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