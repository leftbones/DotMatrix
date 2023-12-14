using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

/// <summary>
/// The main simulation for all Pixel instances, also handles rendering Chunk textures
/// `Matrix.UpdateStart()` and `Matrix.UpdateEnd()` are called before and after `Matrix.Update()` respectively
/// </summary>

class Matrix {
    public Engine Engine { get; private set; }                                  // Reference to the parent Engine instance
    public Theme Theme { get { return Engine.Theme; } }                         // Reference to the Theme class instance of the parent Engine
    public Pepper Pepper { get { return Engine.Pepper; } }                      // Reference to the Pepper class instanace of the parent Engine
    public int Scale { get; private set; }                                      // Scale of the Matrix texture (Matrix pixel to screen pixel)

    public int Seed { get; private set; }                                       // Seed used for all RNG instances
    public RNG RNG { get; private set; }                                        // RNG instance used for non-Chunk operations

    public Vector2i Size { get; private set; }                                  // Size of the Matrix (in Pixels)
    public Dictionary<(int, int), Pixel> Pixels { get;private set; }            // Dictionary that stores the Pixels

    // Chunks
    public Chunk[,] Chunks { get; private set; }                                // 2D array that stores Chunks
    public Vector2i ChunkSize { get; private set; }                             // Size for each Chunk
    public int MaxChunksX { get; private set; }                                 // Number of Chunks in a row
    public int MaxChunksY { get; private set; }                                 // Number of Chunks in a column
    public List<Chunk> ActiveChunks { get; private set; }                       // List of Chunks near the Camera
    public Vector2i ActiveArea { get; private set; }                            // Number of Chunks around the Camera in each direction to keep Active

    public bool RedrawAllChunks { get; set; } = true;                           // Draw all Chunks on the next Draw call, including sleeping ones

    private readonly int ChunkWidth = 100;                                      // Width of each Chunk in Pixels
    private readonly int ChunkHeight = 100;                                     // Height of each Chunk in Pixels

    // ECS
    public List<PixelMap> ActivePixelMaps { get; private set; }                 // List of all PixelMap Tokens added to the Matrix during UpdateStart


    // Statistics
    public int PixelsProcessed = 0;                                             // Total number of Pixel operations done
    public int PixelsMoved = 0;                                                 // Total number of Pixels moved

    public int TotalChunks = 0;                                                 // Total number of Chunks in the Matrix
    public int AwakeChunks = 0;                                                 // Total number of Chunks currently awake

    public List<int> FPSList = new List<int>();
    public int FPSUpper = 0;
    public int FPSLower = 9999;
    public int FPSAverage = 0;

    // Textures + Shaders
    public Texture2D Texture { get; private set; }                              // Render texture that Pixels are drawn to
    private Image Buffer;                                                       // Buffer image used to create the render texture

    private Rectangle SourceRec;                                                // Actual size of the Matrix texture
    private Rectangle DestRec;                                                  // Scaled size of the Matrix texture

    // Settings
    private bool UseMultithreading = false;

    public Matrix(Engine engine, int? seed=null) {
        Engine = engine;
        Scale = Engine.MatrixScale;

        Seed = seed ?? Environment.TickCount;
        RNG = new RNG(Seed);

		// Set the Matrix size, scaled
		// Size = new Vector2i(2048 / Scale, 1280 / Scale);
        Size = new Vector2i(ChunkWidth * 5, ChunkHeight * 5);

        // Size the source and destination rectangles
        SourceRec = new Rectangle(0, 0, Size.X, Size.Y);
        DestRec = new Rectangle(0, 0, Engine.WindowSize.X, Engine.WindowSize.Y);

        // Create and populate the Pixel array
        Pixels = new Dictionary<(int, int), Pixel>();
        for (int y = Size.Y - 1; y >= 0; y--) {
            for (int x = 0; x < Size.X; x++) {
                var P = new Pixel {
                    Position = new Vector2i(x, y)
                };
                Pixels.Add((x,y), P);
            }
        }

        // Generate the Buffer and Texture
        Buffer = GenImageColor(Size.X, Size.Y, Color.BLACK);
        Texture = LoadTextureFromImage(Buffer);

        // Setup Chunks
        ChunkSize = new Vector2i(ChunkWidth, ChunkHeight);
        MaxChunksX = Size.X / ChunkSize.X;
        MaxChunksY = Size.Y / ChunkSize.Y;
        Chunks = new Chunk[MaxChunksX, MaxChunksY];
        ActiveChunks = new List<Chunk>();
        ActiveArea = new Vector2i(2, 2);
		var ChunkSeed = RNG.Random.Next(int.MinValue, int.MaxValue);

        for (int x = 0; x < MaxChunksX; x++) {
            for (int y = 0; y < MaxChunksY; y++) {
                var TO = y % 2 == 0 ? x % 2 == 0 ? 1 : 2 : x % 2 == 0 ? 3 : 4;
                var Pos = new Vector2i(x * ChunkSize.X, y * ChunkSize.Y);
				var ChunkRNG = new RNG(ChunkSeed);
				ChunkSeed = ChunkRNG.Range(int.MinValue, int.MaxValue);
                Chunks[x, y] = new Chunk(this, ChunkRNG, Pos, ChunkSize, TO);
            }
        }

        TotalChunks = MaxChunksX * MaxChunksY;

        // ECS
        ActivePixelMaps = new List<PixelMap>();

        // Finish
        Pepper.Log("Matrix initialized", LogType.MATRIX);
    }

    // Apply changes to the Config
    public void ApplyConfig(Config C) {
        UseMultithreading = C.Items["UseMultithreading"];
        Pepper.Log("Matrix config applied", LogType.SYSTEM);

        if (UseMultithreading) {
            Pepper.Log("Experimental multithreading is enabled!", LogType.SYSTEM, LogLevel.WARNING);
        }
    }

    // Get a Pixel from the Matrix (Vector2i pos)
    public Pixel Get(Vector2i pos) {
        return Pixels[(pos.X, pos.Y)];
    }

    // Get a Pixel from the Matrix (int pos)
    public Pixel Get(int x, int y) {
        return Pixels[(x, y)];
    }

    // Place a Pixel in the Matrix and update it's position (Vector2i pos)
    public void Set(Vector2i pos, Pixel pixel, bool wake_chunk=true) {
        Pixels[(pos.X, pos.Y)] = pixel;
        pixel.Position = pos;

        if (wake_chunk) {
            WakeChunk(pos);
        }
    }

    // Place a Pixel in the Matrix and update it's position
    public void Set(int x, int y, Pixel pixel, bool wake_chunk=true) {
        Pixels[(x, y)] = pixel;
        pixel.Position = new Vector2i(x, y);

        if (wake_chunk) {
            WakeChunk(pixel.Position);
        }
    }

    // Swap two Pixels in the Matrix, checking if the destination is in bounds
    public bool Swap(Vector2i pos1, Vector2i pos2) {
        if (!InBounds(pos2)) {
            return false;
        }

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
        if (!InBounds(pos2)) {
            return false;
        }

        var P1 = Get(pos1);
        var P2 = Get(pos2);

        if (P1.Weight > P2.Weight) {
            return true;
        } else if (P1 is Gas && P2 is not Solid) {
            if (P1.Weight < P2.Weight || (P1.ColorFade > P2.ColorFade && RNG.CoinFlip())) {
                return true;
            }
        }

        return false;
    }

    // Check if a position is within the bounds of the Matrix
    public bool InBounds(Vector2i pos) {
        return pos.X >= 0 && pos.X < Size.X && pos.Y >= 0 && pos.Y < Size.Y;
    }

    public bool InBounds(int x, int y) {
        return x >= 0 && x < Size.X && y >= 0 && y < Size.Y;
    }

    // Check if a position in the Matrix is empty (ID -1)
    public bool IsEmpty(Vector2i pos) {
        return Get(pos).ID == -1;
    }

    public bool IsEmpty(int x, int y) {
        return Get(x, y).ID == -1;
    }

    // Check if a position is in bounds and empty
    public bool InBoundsAndEmpty(Vector2i pos) {
        if (InBounds(pos)) {
            return IsEmpty(pos);
        }
        return false;
    }

    public bool InBoundsAndEmpty(int x, int y) {
        if (InBounds(x, y)) {
            return IsEmpty(x, y);
        }
        return false;
    }

    // Update each awake Chunk in the Matrix
    public void Update() {
        if (UseMultithreading) {
            UpdateParallel();
        } else {
            foreach (var Chunk in ActiveChunks) {
                UpdateChunk(Chunk);
                Chunk.Step();
            }
        }
    }

    // Update each awake Chunk in the Matrix using multiple threads to improve simulation speed (experimental!)
    public void UpdateParallel() {
        var UpdateA = new List<Task>();
        var UpdateB = new List<Task>();
        var UpdateC = new List<Task>();
        var UpdateD = new List<Task>();

        // foreach (var Chunk in ActiveChunks.OrderBy(C => RNG.Random.Next()).ToList()) { 
        // foreach (var Chunk in ActiveChunks) {
        for (int i = ActiveChunks.Count - 1; i >= 0; i--) {
            var Chunk = ActiveChunks[i];
            switch (Chunk.ThreadOrder) {
                case 1: UpdateA.Add(new Task(() => { UpdateChunk(Chunk); Chunk.Step(); })); break;
                case 2: UpdateB.Add(new Task(() => { UpdateChunk(Chunk); Chunk.Step(); })); break;
                case 3: UpdateC.Add(new Task(() => { UpdateChunk(Chunk); Chunk.Step(); })); break;
                case 4: UpdateD.Add(new Task(() => { UpdateChunk(Chunk); Chunk.Step(); })); break;
            }
        }

        Parallel.ForEach(UpdateA, Task => Task.Start());
        while (!Task.WhenAll(UpdateA).IsCompletedSuccessfully) { }

        Parallel.ForEach(UpdateB, Task => Task.Start());
        while (!Task.WhenAll(UpdateB).IsCompletedSuccessfully) { }

        Parallel.ForEach(UpdateC, Task => Task.Start());
        while (!Task.WhenAll(UpdateC).IsCompletedSuccessfully) { }

        Parallel.ForEach(UpdateD, Task => Task.Start());
        while (!Task.WhenAll(UpdateD).IsCompletedSuccessfully) { }
    }

    // Update all of the Pixels within a Chunk's dirty rect
    public void UpdateChunk(Chunk C) {
        // Skip sleeping Chunks
        if (!C.Awake) {
            return;
        }

        bool IsEvenTick = Engine.Tick % 2 == 0;

        // Process pixels within dirty rect
        // for (int y = C.Y1; y < C.Y2; y++) {
        for (int y = C.Y2; y >= C.Y1; y--) {
            for (int x = IsEvenTick ? C.X1 : C.X2; IsEvenTick ? x <= C.X2 : x >= C.X1; x += IsEvenTick ? 1 : -1) {
                var P = Get(C.Position.X + x, C.Position.Y + y);

                if (P.ID > -1) {
                    // Skip already stepped Pixels
                    if (P.Stepped) {
                        continue;
                    }

                    P.Step(this, C.RNG);
                    P.Tick(this);

                    if (C.InBounds(P.Position)) {
                        P.Stepped = true;
                        P.Ticked = true;
                    }

                    if (!P.Settled) {
                        P.ActOnNeighbors(this, C.RNG);
                        if (P.Position == P.LastPosition) {
                            P.Settled = true;
                        }
                    }

                    PixelsProcessed++;
                }
            }
        }
    }

    // Actions performed before the start of the normal Update
    public void UpdateStart() {
        // Get Active Chunks
        var PrevActive = new List<Chunk>();
        foreach (var Chunk in ActiveChunks) {
            PrevActive.Add(Chunk);
        }

        ActiveChunks.Clear();
        var CenterPos = new Vector2i(Engine.Camera.Position) / Scale / ChunkSize;
        var SX = Math.Max(0, CenterPos.X - ActiveArea.X);
        var SY = Math.Max(0, CenterPos.Y - ActiveArea.Y);
        var EX = Math.Min(CenterPos.X + ActiveArea.X + 1, MaxChunksX);
        var EY = Math.Min(CenterPos.Y + ActiveArea.Y + 1, MaxChunksY);
        for (int x = SX; x < EX; x++) {
            for (int y = SY; y < EY; y++) {
                var Chunk = Chunks[x, y];
                ActiveChunks.Add(Chunk);

                if (!PrevActive.Contains(Chunk)) {
                    Chunk.ForceRedraw = true;
                }
            }
        }

        // Add PixelMap Pixels into the Matrix
        ActivePixelMaps.Clear();
        foreach (var PixelMap in PixelMapSystem.Tokens) {
            var Entity = PixelMap!.Entity!;
            var Transform = Entity.GetToken<Transform>()!;

            ActivePixelMaps.Add(PixelMap);

            var Start = PixelMap.Position - PixelMap.Origin;
            var End = Start + new Vector2i(PixelMap.Width, PixelMap.Height);

            for (int x = Start.X; x < End.X; x++) {
                for (int y = Start.Y; y < End.Y; y++) {
                    var MPos = Vector2i.Rotate(new Vector2i(x, y), PixelMap.Position, Transform.Rotation * DEG2RAD);

                    if (!InBounds(MPos)) continue;

                    var MPixel = Get(MPos);
                    if (MPixel.ID > -1) continue;

                    var PX = x - Start.X;
                    var PY = y - Start.Y;

                    var PMPixel = PixelMap.Pixels[PX, PY];
                    if (PMPixel is null) continue;

                    Set(MPos, PMPixel, wake_chunk: true);
                }
            }
        }

        // Reset Pixel Stepped and Ticked flags
        foreach (var P in Pixels.Values) {
            P.Stepped = false;
            P.Ticked = false;
            P.Acted = false;

            P.LastPosition = P.Position;
        }

		// FPS Statistics
		if (Engine.Tick > 150) {
			var FPS = GetFPS();
			if (FPS > FPSUpper) FPSUpper = FPS;
			if (FPS < FPSLower) FPSLower = FPS;
			FPSList.Add(FPS);
			FPSAverage = FPSList.Sum() / FPSList.Count;
		}
    }

    // Actions performed at the end of the normal Update
    public void UpdateEnd() {
        // Remove PixelMap Pixels from the Matrix
        foreach (var PixelMap in ActivePixelMaps) {
            var Entity = PixelMap.Entity!;
            var Transform = Entity.GetToken<Transform>()!;

            var Start = PixelMap.Position - PixelMap.Origin;
            var End = Start + new Vector2i(PixelMap.Width, PixelMap.Height);

            for (int x = Start.X; x < End.X; x++) {
                for (int y = Start.Y; y < End.Y; y++) {
                    var MPos = Vector2i.Rotate(new Vector2i(x, y), PixelMap.Position, Transform.Rotation * DEG2RAD);

                    if (!InBounds(MPos)) continue;

                    var PX = x - Start.X;
                    var PY = y - Start.Y;

                    if (PixelMap.Pixels[PX, PY] is null)
                        continue;

                    // FIXME: This block allows other Pixels to interact with the PixelMap Pixels, but rotation of the PixelMap causes issues
                    // var MPixel = Get(MPos);
                    // PixelMap.Pixels[PX, PY] = MPixel;

                    Set(MPos, new Pixel(-1, MPos), wake_chunk: true);
                }
            }
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

        // Wake appropriate neighbor chunks if the position is on a border
        if (pos.X == Chunk.Position.X + Chunk.Size.X - 1 && InBounds(pos + Direction.Right)) GetChunk(pos + Direction.Right).Wake(pos + Direction.Right);
        if (pos.X == Chunk.Position.X && InBounds(pos + Direction.Left)) GetChunk(pos + Direction.Left).Wake(pos + Direction.Left);
        if (pos.Y == Chunk.Position.Y + Chunk.Size.Y - 1 && InBounds(pos + Direction.Down)) GetChunk(pos + Direction.Down).Wake(pos + Direction.Down);
        if (pos.Y == Chunk.Position.Y && InBounds(pos + Direction.Up)) GetChunk(pos + Direction.Up).Wake(pos + Direction.Up);
    }

    // Draw each Pixel in the Matrix to the render texture
    public unsafe void Draw() {
        // Update and Draw Chunk Textures (Per Chunk Textures)
        foreach (var C in ActiveChunks) {
            if (C.Awake || C.ForceRedraw || RedrawAllChunks) {
                ImageClearBackground(ref C.Buffer, Theme.Transparent);

                for (int y = ChunkSize.Y - 1; y >= 0; y--) {
                    for (int x = 0; x < ChunkSize.X; x++) {
                        var P = Get(C.Position.X + x, C.Position.Y + y);

                        if (P.ID == -1 && !P.ColorSet) continue;

                        if (!P.ColorSet) {
                            int Offset = RNG.Range(-P.ColorOffset, P.ColorOffset);
                            P.Color = P.BaseColor;
                            P.ShiftColor(Offset);
                            P.ColorSet = true;
                        }

                        var Col = P.Color;

                        if (Engine.Config.DebugEnabled) {
                            if (Engine.Canvas.DrawMovementOverlay)
                                Col = P.Position == P.LastPosition? Color.PURPLE: Color.YELLOW;
                            else if (Engine.Canvas.DrawSettledOverlay)
                                Col = P.Settled ? Color.RED : Color.BLUE;
                        }

                        ImageDrawPixel(ref C.Buffer, P.Position.X - C.Position.X, P.Position.Y - C.Position.Y, Col);
                    }
                }

                UpdateTexture(C.Texture, C.Buffer.data);

                if (C.ForceRedraw)
                    C.ForceRedraw = false;
            }
        }

        foreach (var C in ActiveChunks) {
            DrawTexturePro(C.Texture, C.SourceRec, C.DestRec, Vector2.Zero, 0, Color.WHITE);
        }

        if (RedrawAllChunks)
            RedrawAllChunks = false;

        // Chunk Borders + Info
        if (Engine.Config.DebugEnabled) {
            if (Engine.Canvas.DrawChunkBorders) {
                foreach (var C in ActiveChunks) {
                    var Col = C.Awake ? new Color(255, 255, 255, 150) : new Color(255, 255, 255, 25);
                    var Str = $"{C.Position.X / ChunkSize.X}, {C.Position.Y / ChunkSize.Y} ({C.Position.X}, {C.Position.Y})";
                    DrawRectangleLines(C.Position.X * Scale, C.Position.Y * Scale, C.Size.X * Scale, C.Size.Y * Scale, Col);
                    DrawTextEx(Engine.Theme.Font, Str, new Vector2i((C.Position.X * Scale) + 5, (C.Position.Y * Scale) + 5).ToVector2(), Engine.Theme.FontSize, Engine.Theme.FontSpacing, Col);
                }
            }

            // World Border
            if (Engine.Canvas.DrawWorldBorder) {
                DrawRectangleLines(0, 0, Size.X * Scale, Size.Y * Scale, new Color(255, 255, 255, 150));
            }

            // Chunk DirtyRect
            if (Engine.Canvas.DrawDirtyRects) {
                foreach (var C in ActiveChunks) {
                    if (C.Awake) {
                        // Skip clean chunks
                        if (C.X2 == 0 && C.Y2 == 0) continue;

                        var R = C.DirtyRect;
                        var Rec = new Rectangle(R.x * Scale, R.y * Scale, (R.width + 1) * Scale, (R.height + 1) * Scale);
                        DrawRectangleLines((int)Rec.x, (int)Rec.y, (int)Rec.width, (int)Rec.height, Color.GREEN);
                    }
                }
            }

            // Chunk Collision Borders
            if (Engine.Canvas.DrawChunkCollision) {
                foreach (var C in ActiveChunks) {
                    foreach (var S in C.Bounds) {
                        var LP = (S[0] * Scale) + C.Position.ToVector2();
                        for (int i = 1; i < S.Count; i++) {
                            var P = (S[i] * Scale) + C.Position.ToVector2();
                            DrawLineEx(LP * Scale, P * Scale, 2.0f, Color.BLUE);
                            LP = P;
                        }
                    }
                }
            }
        }
    }
}