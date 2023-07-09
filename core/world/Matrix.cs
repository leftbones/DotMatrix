using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Matrix {
    public Engine Engine { get; private set; }                                  // Reference to the parent Engine instance
    public Theme Theme { get { return Engine.Theme; } }                         // Reference to the Theme class instance of the parent Engine
    public Pepper Pepper { get { return Engine.Pepper; } }                      // Reference to the Pepper class instanace of the parent Engine
    public int Scale { get; private set; }                                      // Scale of the Matrix texture (Matrix pixel to screen pixel)

    public int Seed { get; private set; }                                       // Seed used for all RNG instances
    public RNG RNG { get; private set; }                                        // RNG instance used for non-Chunk operations

    public Vector2i Size { get; private set; }                                  // Size of the Matrix (in Pixels)
    public Pixel[,] Pixels { get; private set; }                                // 2D array that stores the Pixels

    // Chunks
    public Chunk[,] Chunks { get; private set; }                                // 2D array that stores Chunks
    public Vector2i ChunkSize { get; private set; }                             // Size for each Chunk
    public int MaxChunksX { get; private set; }                                 // Number of Chunks in a row
    public int MaxChunksY { get; private set; }                                 // Number of Chunks in a column

    public bool RedrawAllChunks { get; set; } = true;                           // Draw all Chunks on the next Draw call, including sleeping ones

    private int ChunkWidth = 64;                                                // Width of each Chunk in Pixels
    private int ChunkHeight = 64;                                               // Height of each Chunk in Pixels

    // Statistics
    public int PixelsProcessed = 0;                                             // Total number of Pixel operations done
    public int PixelsMoved = 0;                                                 // Total number of Pixels moved

    public int TotalChunks = 0;                                                 // Total number of Chunks in the Matrix
    public int ActiveChunks = 0;                                                // Total number of Chunks currently awake

    public List<int> FPSList = new List<int>();
    public int FPSUpper = 0;
    public int FPSLower = 9999;
    public int FPSAverage = 0;

    // Textures + Shaders
    public Texture2D Texture { get; private set; }                              // Render texture that Pixels are drawn to
    private Image Buffer;                                                       // Buffer image used to create the render texture

    private Rectangle SourceRec;                                                // Actual size of the Matrix texture
    private Rectangle DestRec;                                                  // Scaled size of the Matrix texture

    private Shader BlurShader = LoadShader(null, "res/shaders/blur.fs");        // Blur shader (experimental)
    private Shader BloomShader = LoadShader(null, "res/shaders/bloom.fs");      // Bloom shader (experimental)

	// Tests
	private bool SpoutTestEnabled = false;										// Create Pixels in the center of the top of the Matrix
	private bool RainTestEnabled = false;										// Create Pixels across the top of the Matrix

	private bool PauseAfterTest = false;										// Pause the simulation when the test finishes
	private int TestLength = 1000;                                              // Length of the tests (in ticks)

    // Settings TODO: Move to dedicated Settings class
    public bool MultithreadingEnabled { get; set; } = true;


    public Matrix(Engine engine, int? seed=null) {
        Engine = engine;
        Scale = Engine.MatrixScale;

        Seed = seed ?? Environment.TickCount;
        RNG = new RNG(Seed);

		// Set the Matrix size, scaled
		Size = new Vector2i(2000 / Scale, 1000 / Scale);

        // Size the source and destination rectangles
        SourceRec = new Rectangle(0, 0, Size.X, Size.Y);
        DestRec = new Rectangle(0, 0, Engine.WindowSize.X, Engine.WindowSize.Y);

        // Create and populate the Pixel array
        Pixels = new Pixel[Size.X, Size.Y];
        for (int y = Size.Y - 1; y >= 0; y--) {
            for (int x = 0; x < Size.X; x++) {
                var P = new Pixel();
                P.Position = new Vector2i(x, y);
                Pixels[x, y] = P;
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

        // Finish
        Pepper.Log("Matrix initialized", LogType.MATRIX);
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

        if (P1.Weight > P2.Weight) {
            return true;
        } else if (P1 is Gas && P2 is not Solid) {
            if (P1.Weight < P2.Weight || (P1.ColorFade > P2.ColorFade && RNG.CoinFlip()))
                return true;
        }

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

    // Update each awake Chunk in the Matrix
    public void Update() {
        if (MultithreadingEnabled) {
            UpdateParallel();
        } else {
            bool IsEvenTick = Engine.Tick % 2 == 0;
            for (int y = MaxChunksY - 1; y >= 0; y--) {
                for (int x = IsEvenTick ? 0 : MaxChunksX - 1; IsEvenTick ? x <= MaxChunksX - 1 : x >= 0; x += IsEvenTick ? 1 : -1) {
                    var C = Chunks[x, y];
                    UpdateChunk(C);
                    C.Step();
                }
            }
        }
    }

    public void UpdateParallel() {
        var UpdateA = new List<Task>();
        var UpdateB = new List<Task>();
        var UpdateC = new List<Task>();
        var UpdateD = new List<Task>();

        bool IsEvenTick = Engine.Tick % 2 == 0;

        //for (int y = MaxChunksY - 1; y >= 0; y--) {
        //    for (int x = IsEvenTick ? 0 : MaxChunksX - 1; IsEvenTick ? x <= MaxChunksX - 1 : x >= 0; x += IsEvenTick ? 1 : -1) {
        //        var Chunk = Chunks[x, y];
			foreach (var Chunk in Chunks) { 
                switch (Chunk.ThreadOrder) {
                    case 1: UpdateA.Add(new Task(() => { UpdateChunk(Chunk); Chunk.Step(); })); break;
                    case 2: UpdateB.Add(new Task(() => { UpdateChunk(Chunk); Chunk.Step(); })); break;
                    case 3: UpdateC.Add(new Task(() => { UpdateChunk(Chunk); Chunk.Step(); })); break;
                    case 4: UpdateD.Add(new Task(() => { UpdateChunk(Chunk); Chunk.Step(); })); break;
                }
            }
        //}

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
        if (!C.Awake) return;

        bool IsEvenTick = Engine.Tick % 2 == 0;

        // Process pixels within dirty rect
        for (int y = C.Y2; y >= C.Y1; y--) {
            for (int x = IsEvenTick ? C.X1 : C.X2; IsEvenTick ? x <= C.X2 : x >= C.X1; x += IsEvenTick ? 1 : -1) {
                var P = Get(C.Position.X + x, C.Position.Y + y);

                if (P.ID > -1) {
                    // Skip already stepped Pixels
                    if (P.Stepped)
                        continue;

                    P.Step(this, C.RNG);
                    P.Stepped = true;

                    P.Tick(this);
                    P.Ticked = true;

                    if (!P.Settled) {
                        P.ActOnNeighbors(this, C.RNG);
                        if (P.Position == P.LastPosition)
                            P.Settled = true;
                    }

                    PixelsProcessed++;
                }
            }
        }
    }

    // Actions performed before the start of the normal Update
    public void UpdateStart() {
		// Spout Test
		if (SpoutTestEnabled) {
			if (Engine.Tick == TestLength) {
				if (PauseAfterTest)
					Engine.ToggleActive();
				SpoutTestEnabled = false;
			} else {
				int Width = 5;
				for (int i = 0; i < Width; i++) {
					if (RNG.CoinFlip()) {
						var Pos = new Vector2i((Size.X / 2) - Width + i, 0);
						if (IsEmpty(Pos))
							Set(Pos, new Powder(400, Pos));
					}
				}
			}
		}

		// Rain Test
		if (RainTestEnabled) {
			if (Engine.Tick == TestLength) {
				if (PauseAfterTest)
					Engine.ToggleActive();
				RainTestEnabled = false;
			} else {
				for (int i = 0; i < Size.X; i++) {
					if (RNG.Roll(25)) {
						var Pos = new Vector2i(i, 0);
						if (IsEmpty(Pos))
							Set(Pos, new Liquid(200, Pos));
					}
				}
			}
		}

		// FPS Statistics
		if (Engine.Tick > 150) {
			var FPS = GetFPS();
			if (FPS > FPSUpper) FPSUpper = FPS;
			if (FPS < FPSLower) FPSLower = FPS;
			FPSList.Add(FPS);
			FPSAverage = FPSList.Sum() / FPSList.Count;
		}

        // Reset Pixel Stepped and Ticked flags
        foreach (var P in Pixels) {
            P.Stepped = false;
            P.Ticked = false;
            P.Acted = false;

            P.LastPosition = P.Position;
        }
    }

    // Actions performed at the end of the normal Update
    public void UpdateEnd() {

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
        foreach (var C in Chunks) {
            if (C.Awake || C.ForceRedraw || RedrawAllChunks) {
                ImageClearBackground(ref C.Buffer, Color.BLACK);

                for (int y = ChunkSize.Y - 1; y >= 0; y--) {
                    for (int x = 0; x < ChunkSize.X; x++) {
                        var P = Get(C.Position.X + x, C.Position.Y + y);
                        if (P.ID == -1) {
                            ImageDrawPixel(ref C.Buffer, P.Position.X - C.Position.X, P.Position.Y - C.Position.Y, Theme.Transparent);
                            continue;
                        }

                        if (!P.ColorSet) {
                            int Offset = RNG.Range(-P.ColorOffset, P.ColorOffset);
                            P.Color = P.BaseColor;
                            P.ShiftColor(Offset);
                            P.ColorSet = true;
                        }

                        var Col = P.Color;

                        if (Engine.Canvas.DrawMovementOverlay)
                            Col = P.Position == P.LastPosition? Color.PURPLE: Color.YELLOW;
                        else if (Engine.Canvas.DrawSettledOverlay)
                            Col = P.Settled ? Color.RED : Color.BLUE;

                        ImageDrawPixel(ref C.Buffer, P.Position.X - C.Position.X, P.Position.Y - C.Position.Y, Col);
                    }
                }

                UpdateTexture(C.Texture, C.Buffer.data);

                if (C.ForceRedraw)
                    C.ForceRedraw = false;
            }
        }

        foreach (var C in Chunks) {
            DrawTexturePro(C.Texture, C.SourceRec, C.DestRec, Vector2.Zero, 0, Color.WHITE);
        }

        if (RedrawAllChunks)
            RedrawAllChunks = false;

        // Chunk Borders + Info
        if (Engine.Canvas.DrawChunks) {
            foreach (var C in Chunks) {
                var Col = C.Awake ? new Color(255, 255, 255, 150) : new Color(255, 255, 255, 50);
                var Str = $"{C.ThreadOrder} - {C.Position.X / ChunkSize.X}, {C.Position.Y / ChunkSize.Y} : {C.SleepTimer}";
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