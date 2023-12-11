using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

/// <summary>
/// An area within the Matrix that tracks if it has Pixels within it's bounds that need to be updated, also has it's own texture that is updated when necessary and drawn by the Matrix
/// Chunks that do not have any changes after 30 steps will go to sleep and won't be processed again by the Matrix until woken up
/// </summary>

class Chunk {
    public Matrix Matrix { get; private set; }                                                                          // The parent Matrix this Chunk belongs to
    public RNG RNG { get; private set; }                                                                                // Thread safe RNG instance local to this Chunk
    public Vector2i Position { get; private set; }                                                                      // The position of a Chunk within the parent Matrix
    public Vector2i Size { get; private set; }                                                                          // The size of a Chunk, in Pixels
    public int ThreadOrder { get; private set; }                                                                        // The "substep" in which a Chunk is processed when multithreading is enabled (1-4)

    public List<List<Vector2>> Bounds { get; private set; }                                                             // List of boundaries (outlines) generated using marching squares based on the contents of the chunk
    public List<Entity> Bodies { get; set; }                                                                             // The Box2D tokens generated for this Chunk based on its contents
    public bool RecalculateBounds { get; set; }                                                                         // When true, recalculate the boundaries contained in the chunk

    public Texture2D Texture { get; set; }                                                                              // Texture that Pixels are drawn to
    public Image Buffer;                                                                                                // Buffer image used to create the texture

    public Rectangle SourceRec { get; private set; }                                                                    // Actual size of the Chunk Texture
    public Rectangle DestRec { get; private set; }                                                                      // Scaled size of the Chunk Texture

    public int X1 { get; set; }                                                                                         // Top left X coord for the current dirty rectangle
    public int Y1 { get; set; }                                                                                         // Top left Y coord for the current dirty rectangle
    public int X2 { get; set; }                                                                                         // Bottom right X coord for the current dirty rectangle
    public int Y2 { get; set; }                                                                                         // Bottom right Y coord for the current dirty rectangle

    private int X1w = 0;                                                                                                // Top left X coord for the working dirty rectangle
    private int Y1w = 0;                                                                                                // Top left Y coord for the working dirty rectangle
    private int X2w = 0;                                                                                                // Bottom right X coord for the working dirty rectangle
    private int Y2w = 0;                                                                                                // Bottom right Y coord for the working dirty rectangle

    private readonly int BD = 1;                                                                                        // "Buffer Distance", how far to extend the dirty rect past the actual active pixels (1 seems fine, 2 if there are floating pixels left behind)

    public int SleepTimer { get; private set; }                                                                         // Timer that counts down before a Chunk sleeps
    private readonly int WaitTime = 30;                                                                                 // Number of ticks used for the SleepTimer

    public int BoundsTimer { get; private set; }
    private readonly int RecalcTime = 10;

    public Rectangle DirtyRect { get { return new Rectangle(Position.X + X1, Position.Y + Y1, X2 - X1, Y2 - Y1); } }    // Area within a Chunk containing active Pixels

    public bool Awake { get; private set; } = false;                                                                    // Chunk contains active Pixels
    public bool WakeNextStep { get; private set; } = false;                                                             // Chunk will become Awake during the next Matrix update

    public bool ForceRedraw { get; set; } = false;                                                                      // Force this Chunk to be redrawn during the next Matrix draw

    public Chunk(Matrix matrix, RNG rng, Vector2i position, Vector2i size, int thread_order) {
        Matrix = matrix;
        RNG = rng;
        Position = position;
        Size = size;
        ThreadOrder = thread_order;

        Buffer = GenImageColor(Size.X, Size.Y, Color.BLACK);
        Texture = LoadTextureFromImage(Buffer);

        SourceRec = new Rectangle(0, 0, Size.X, Size.Y);
        DestRec = new Rectangle(Position.X * Matrix.Scale, Position.Y * Matrix.Scale, Size.X * Matrix.Scale, Size.Y * Matrix.Scale);

        SleepTimer = WaitTime;

        Bounds = new List<List<Vector2>>();
        Bodies = new List<Entity>();
    }

    // Set the Chunk to be Awake for the next Matrix update
    public void Wake(Vector2i pos) {
        if (!Awake && !Matrix.Engine.Active) {
            ForceRedraw = true;
        }

        if (!Awake && !WakeNextStep) {
            Matrix.AwakeChunks++;
        }

        WakeNextStep = true;
        SleepTimer = WaitTime;

        if (Awake) {
            // Update working dirty rect
            var X = pos.X - Position.X;
            var Y = pos.Y - Position.Y;

            X1w = Math.Clamp(Math.Min(X - BD, X1w), 0, Size.X);
            Y1w = Math.Clamp(Math.Min(Y - BD, Y1w), 0, Size.Y);
            X2w = Math.Clamp(Math.Max(X + BD, X2w), 0, Size.X - 1);
            Y2w = Math.Clamp(Math.Max(Y + BD, Y2w), 0, Size.Y - 1);
        } else {
            X1w = 0;
            Y1w = 0;
            X2w = Size.X - 1;
            Y2w = Size.Y - 1;
        }
    }

    // Update the dirty rect then update the awake state or tick down the SleepTimer
    public void Step() {
        if (BoundsTimer == 0) {
            RecalculateBounds = true;
            Bounds.Clear();
            BoundsTimer = RecalcTime;

            foreach (var E in Bodies) {
                E.Destroy();
            }
        } else {
            RecalculateBounds = false;
            BoundsTimer--;
        }

        if (RecalculateBounds) {
            var Shapes = Boundaries.Calculate(Matrix, Position.X, Position.Y, Position.X + Size.X, Position.Y + Size.Y);
            if (Shapes.Count > 0) {
                foreach (var Shape in Shapes) {
                    var MaxPoints = Math.Max(Shape.Count / 2, 10);
                    var Simplified = Boundaries.Simplify(Shape, MaxPoints, 1.0f).Distinct().ToList();

                    if (Simplified.Count > 0) {
                        for (int i = 0; i < Simplified.Count; i++) {
                            Simplified[i] = (Simplified[i] - Position.ToVector2()) / Global.MatrixScale;
                        }

                        Bounds.Add(Simplified);
                    }
                }
            }
        }

        UpdateRect();

        if (Awake && !WakeNextStep) {
            SleepTimer--;
            if (SleepTimer == 0) {
                Matrix.AwakeChunks--;
                Awake = false;
            }
        } else {
            Awake = WakeNextStep;
        }

        WakeNextStep = false;
    }

    // Update the current dirty rect, reset the working dirty rect
    public void UpdateRect() {
        X1 = X1w; X1w = Size.X - 1;
        Y1 = Y1w; Y1w = Size.Y - 1;
        X2 = X2w; X2w = 0;
        Y2 = Y2w; Y2w = 0;
    }

    // Check if a position is within the bounds of the Chunk
    public bool InBounds(Vector2i pos) {
        return pos.X >= Position.X && pos.X < Position.X + Size.X && pos.Y >= Position.Y && pos.Y < Position.Y;
    }
}