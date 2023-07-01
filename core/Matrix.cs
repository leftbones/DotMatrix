using System.Linq;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Matrix {
    public Engine Engine { get; private set; }              // Reference to the parent Engine instance
    public Theme Theme { get { return Engine.Theme; } }     // Reference to the Theme class instance of the parent Engine
    public Pepper Pepper { get { return Engine.Pepper; } }  // Reference to the Pepper class instanace of the parent Engine
    public int Scale { get; private set; }                  // Scale of the Matrix texture (Matrix pixel to screen pixel)

    public Vector2i Size { get; private set; }              // Size of the Matrix (in Pixels)
    public Pixel[,] Pixels { get; private set; }            // 2D array that stores the Pixels

    // Chunks
    public Chunk[,] Chunks { get; private set; }            // 2D array that stores Chunks
    public Vector2i ChunkSize { get; private set; }         // Size for each Chunk
    public int MaxChunksX { get; private set; }             // Number of Chunks in a row
    public int MaxChunksY { get; private set; }             // Number of Chunks in a column

    public bool RedrawAllChunks { get; set; }               // Draw all Chunks on the next Draw call, including sleeping ones

    private int ChunkWidth = 64;
    private int ChunkHeight = 64;

    // Statistics
    public int TotalPixels = 0;
    public int ActivePixels = 0;
    public int PixelsProcessed = 0;
    public int PixelsMoved = 0;

    public int TotalChunks = 0;
    public int ActiveChunks = 0;
    public int ChunksProcessed = 0;

    // Texture
    public Texture2D Texture { get; private set; }          // Render texture that Pixels are drawn to
    private Image Buffer;                                   // Buffer image used to create the render texture

    private Rectangle SourceRec;                            // Actual size of the Matrix texture
    private Rectangle DestRec;                              // Scaled size of the Matrix texture


    public Matrix(Engine engine) {
        Engine = engine;
        Scale = Engine.MatrixScale;

        // Set the Matrix size, scaled
        Size = new Vector2i(1024 / Scale, 768 / Scale);

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

        TotalPixels = 0;

        // Generate the Buffer and Texture
        Buffer = GenImageColor(Size.X, Size.Y, Color.BLACK);
        Texture = LoadTextureFromImage(Buffer);

        // Setup Chunks
        ChunkSize = new Vector2i(ChunkWidth, ChunkHeight);
        MaxChunksX = Size.X / ChunkSize.X;
        MaxChunksY = Size.Y / ChunkSize.Y;
        Chunks = new Chunk[MaxChunksX, MaxChunksY];
        for (int x = 0; x < MaxChunksX; x++) {
            for (int y = 0; y < MaxChunksY; y++) {
                var Pos = new Vector2i(x * ChunkSize.X, y * ChunkSize.Y);
                Chunks[x, y] = new Chunk(this, Pos, ChunkSize);
            }
        }

        TotalChunks = MaxChunksX * MaxChunksY;

        Pepper.Log(LogType.MATRIX, LogLevel.MESSAGE, "Matrix initialized.");
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

        if (pixel.ID > -1) TotalPixels++;
        else TotalPixels--;

        if (wake_chunk)
            WakeChunk(pos);
    }

    // Place a Pixel in the Matrix and update it's position
    public void Set(int x, int y, Pixel pixel, bool wake_chunk=true) {
        Pixels[x, y] = pixel;
        pixel.Position = new Vector2i(x, y);

        if (pixel.ID > -1) TotalPixels++;
        else TotalPixels--;

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

        PixelsMoved++;

        return true;
    }

    // Swap two Pixels in the Matrix without checking if the destination is in bounds, always returns true (or crashes if used wrong)
    public bool QuickSwap(Pixel p1, Pixel p2) {
        var Pos1 = p1.Position;
        var Pos2 = p2.Position;
        Set(Pos2, p1);
        Set(Pos1, p2);

        PixelsMoved++;

        return true;
    }

    // Swap a Pixel with another Pixel if the destination is in bounds and a valid move (based on weight/type)
    public bool SwapIfValid(Vector2i pos1, Vector2i pos2) {
        if (IsValid(pos1, pos2)) {
            var P1 = Get(pos1);
            var P2 = Get(pos2);
            return QuickSwap(P1, P2);
        }

        return false;
    }

    // Check if a movement is valid (based on weight/type)
    public bool IsValid(Vector2i pos1, Vector2i pos2) {
        // Destination is out of bounds
        if (!InBounds(pos2))
            return false;

        var P1 = Get(pos1);
        var P2 = Get(pos2);

        if (P1.Weight > P2.Weight)
            return true;

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

    // Check if a position is in bounds and empty
    public bool InBoundsAndEmpty(Vector2i pos) {
        if (InBounds(pos))
            return IsEmpty(pos);
        return false;
    }

    // Update each Pixel in the Matrix
    public void Update() {
        bool IsEvenTick = Engine.Tick % 2 == 0;

        for (int r = MaxChunksY - 1; r >= 0; r--) {
            for (int c = IsEvenTick ? 0 : MaxChunksX - 1; IsEvenTick ? c < MaxChunksX : c >= 0; c += IsEvenTick ? 1 : -1) {
                var C = Chunks[c, r];

                // Skip sleeping chunks
                if (!C.Awake) continue;

                // Process pixels within dirty rect
                // for (int y = C.Y1; y <= C.Y2; y++) { // Top to Bottom
                for (int y = C.Y2; y >= C.Y1; y--) { // Bottom to Top
                    for (int x = IsEvenTick ? C.X1 : C.X2; IsEvenTick ? x <= C.X2 : x >= C.X1; x += IsEvenTick ? 1 : -1) {
                        var P = Get(C.Position.X + x, C.Position.Y + y);

                        if (P.ID > -1) {
                            // Skip already stepped Pixels
                            if (P.Stepped)
                                continue;

                            P.Step(this);
                            P.Stepped = true;

                            P.Tick(this);
                            P.Ticked = true;

                            if (P.Active)
                                P.ActOnNeighbors(this);

                            PixelsProcessed++;
                        }
                    }
                }
            }
        }
    }

    // Actions performed before the start of the normal Update
    public void UpdateStart() {
        ActivePixels = 0;

        // Reset Pixel Stepped and Ticked flags
        foreach (var P in Pixels) {
            if (P.ID > -1 && P.Active) ActivePixels++;
            P.Stepped = false;
            P.Ticked = false;
            P.Acted = false;

            P.LastPosition = P.Position;
        }
    }

    // Actions performed at the end of the normal Update
    public void UpdateEnd() {
        // Update Chunks
        foreach (var C in Chunks) {
            // Calculate dirty rects of awake chunks
            // if (C.Awake) {
            //     if (C.CheckAll) {
            //         C.CheckAll = false;

            //         var XStart = false;
            //         var YStart = false;
            //         C.X2 = 0;
            //         C.Y2 = 0;

            //         for (int y = ChunkSize.Y - 1; y >= 0; y--) {
            //             for (int x = 0; x < ChunkSize.X; x++) {
            //                 var P = Get(C.Position.X + x, C.Position.Y + y);
            //                 if (P.ID > -1) {
            //                     if (!XStart || x < C.X1) { C.X1 = x; XStart = true; }
            //                     if (!YStart || y < C.Y1) { C.Y1 = y; YStart = true; }

            //                     if (x > C.X2) C.X2 = x;
            //                     if (y > C.Y2) C.Y2 = y;
            //                 }
            //             }
            //         }
            //     } else {
            //         var XStart = false;
            //         var YStart = false;
            //         C.X2 = 0;
            //         C.Y2 = 0;

            //         for (int y = ChunkSize.Y - 1; y >= 0; y--) {
            //             for (int x = 0; x < ChunkSize.X; x++) {
            //                 var P = Get(C.Position.X + x, C.Position.Y + y);
            //                 if (P.ID > -1) {// && (P.Active || P.Position != P.LastPosition)) {
            //                     if (!XStart || x < C.X1) { C.X1 = x; XStart = true; }
            //                     if (!YStart || y < C.Y1) { C.Y1 = y; YStart = true; }

            //                     if (x > C.X2) C.X2 = x;
            //                     if (y > C.Y2) C.Y2 = y;
            //                 }
            //             }
            //         }
            //     }
            // }

            // Step all chunks
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
    public void WakeChunk(Vector2i pos, bool check_all=false) {
        var Chunk = GetChunk(pos);
        Chunk.Wake(pos);

        if (check_all)
            Chunk.CheckAll = true;

        // Wake appropriate neighbor chunks if the position is on a border
        if (pos.X == Chunk.Position.X + Chunk.Size.X - 1 && InBounds(pos + Direction.Right)) GetChunk(pos + Direction.Right).Wake(pos + Direction.Right);
        if (pos.X == Chunk.Position.X && InBounds(pos + Direction.Left)) GetChunk(pos + Direction.Left).Wake(pos + Direction.Left);
        if (pos.Y == Chunk.Position.Y + Chunk.Size.Y - 1 && InBounds(pos + Direction.Down)) GetChunk(pos + Direction.Down).Wake(pos + Direction.Down);
        if (pos.Y == Chunk.Position.Y && InBounds(pos + Direction.Up)) GetChunk(pos + Direction.Up).Wake(pos + Direction.Up);
    }

    // Draw each Pixel in the Matrix to the render texture
    public unsafe void Draw() {
        // Update and Draw Chunk Textures (Per Chunk Textures)
        foreach (var C in Chunks) {
            if (C.Awake || RedrawAllChunks) {
                ImageClearBackground(ref C.Buffer, Color.BLACK);

                for (int y = ChunkSize.Y - 1; y >= 0; y--) {
                    for (int x = 0; x < ChunkSize.X; x++) {
                        var P = Get(C.Position.X + x, C.Position.Y + y);
                        if (P.ID == -1) continue;

                        if (!P.ColorSet) {
                            int Offset = RNG.Range(-P.ColorOffset, P.ColorOffset);
                            P.Color = P.BaseColor;
                            P.ShiftColor(Offset);
                            P.ColorSet = true;
                        }

                        var Col = P.Color;
                        if (Engine.Canvas.DrawActiveOverlay) {
                            if (!P.Active) Col = Color.RED;
                            else if (P.Settled) Col = Color.ORANGE;
                            else Col = Color.BLUE;
                        }

                        ImageDrawPixel(ref C.Buffer, P.Position.X - C.Position.X, P.Position.Y - C.Position.Y, Col);
                    }
                }

                UpdateTexture(C.Texture, C.Buffer.data);
            }

            DrawTexturePro(C.Texture, C.SourceRec, C.DestRec, Vector2.Zero, 0, Color.WHITE);
        }

        if (RedrawAllChunks)
            RedrawAllChunks = false;

        // Update  and Draw Texture (Entire Matrix)
        // ImageClearBackground(ref Buffer, Color.BLACK);

        // foreach (var P in Pixels) {
        //     if (P.ID == -1) continue;

        //     if (!P.ColorSet) {
        //         int Offset = RNG.Range(-P.ColorOffset, P.ColorOffset);
        //         P.Color = P.BaseColor;
        //         P.ShiftColor(Offset);
        //         P.ColorSet = true;
        //     }

        //     Color Col = P.Color;
        //     if (Engine.Canvas.DrawActiveOverlay && !P.Active) Col = Color.RED;
        //     ImageDrawPixel(ref Buffer, P.Position.X, P.Position.Y, Col);
        // }

        // UpdateTexture(Texture, Buffer.data);
        // DrawTexturePro(Texture, SourceRec, DestRec, Vector2.Zero, 0, Color.WHITE);

        // Chunk Borders
        if (Engine.Canvas.DrawChunks) {
            foreach (var C in Chunks) {
                var Col = C.Awake ? new Color(255, 255, 255, 150) : new Color(255, 255, 255, 50);
                var Str = $"{C.Position.X / ChunkSize.X}, {C.Position.Y / ChunkSize.Y} : {C.SleepTimer}";
                DrawRectangleLines(C.Position.X * Scale, C.Position.Y * Scale, C.Size.X * Scale, C.Size.Y * Scale, Col);
                DrawTextEx(Engine.Theme.Font, Str, new Vector2i((C.Position.X * Scale) + 5, (C.Position.Y * Scale) + 5).ToVector2(), Engine.Theme.FontSize, Engine.Theme.FontSpacing, Col);
            }
        }

        // Chunk DirtyRect
        if (Engine.Canvas.DrawDirtyRects) {
            foreach (var C in Chunks) {
                if (C.Awake) {
                    // Skip clean chunks
                    if (C.X2 == 0 && C.Y2 == 0) continue;

                    var R = C.DirtyRect;
                    var Rec = new Rectangle(R.x * Scale, R.y * Scale, (R.width + 1) * Scale, (R.height + 1) * Scale);
                    DrawRectangleLines((int)Rec.x, (int)Rec.y, (int)Rec.width, (int)Rec.height, Color.GREEN);
                }
            }
        }
    }
}