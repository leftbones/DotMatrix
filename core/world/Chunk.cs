using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Chunk {
    public Matrix Matrix { get; private set; }                                                                          // The parent Matrix this Chunk belongs to
    public Vector2i Position { get; private set; }                                                                      // Position of a Chunk within the parent Matrix
    public Vector2i Size { get; private set; }                                                                          // Size of a Chunk in Pixels

    public Texture2D Texture { get; set; }                                                                              // Render texture that Pixels are drawn to
    public Image Buffer;                                                                                                // Buffer image used to create the render texture

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

    private int BD = 1;                                                                                                 // Buffer Distance - how far to extend the dirty rect past the actual active pixels (1 seems fine, 2 if there are floating pixels left behind)

    public int SleepTimer { get; private set; }                                                                         // Timer that counts down before a Chunk sleeps
    private int WaitTime = 30;                                                                                          // Number of ticks used for the SleepTimer

    public Rectangle DirtyRect { get { return new Rectangle(Position.X + X1, Position.Y + Y1, X2 - X1, Y2 - Y1); } }    // Area within a Chunk containing active Pixels

    public bool Awake { get; private set; } = false;                                                                    // Chunk contains active Pixels
    public bool WakeNextStep { get; private set; } = false;                                                             // Chunk will become Awake during the next Matrix update

    public bool CheckAll { get; set; } = false;                                                                         // All Pixels in the Chunk should be checked rather than just those within the DirtyRect

    public Chunk(Matrix matrix, Vector2i position, Vector2i size) {
        Matrix = matrix;
        Position = position;
        Size = size;

        Buffer = GenImageColor(Size.X, Size.Y, Color.BLACK);
        Texture = LoadTextureFromImage(Buffer);

        SourceRec = new Rectangle(0, 0, Size.X, Size.Y);
        DestRec = new Rectangle(Position.X * Matrix.Scale, Position.Y * Matrix.Scale, Size.X * Matrix.Scale, Size.Y * Matrix.Scale);

        SleepTimer = WaitTime;
    }

    // Set the Chunk to be Awake for the next Matrix update
    public void Wake(Vector2i pos) {
        if (!Awake && !WakeNextStep)
            Matrix.ActiveChunks++;

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

            // if (X < X1w) X1w = X;
            // if (Y < Y1w) Y1w = Y;
            // if (X > X2w) X2w = X;
            // if (Y > Y2w) Y2w = Y;
        } else {
            X1w = 0;
            Y1w = 0;
            X2w = Size.X - 1;
            Y2w = Size.Y - 1;
        }

        // if (!Awake || (X2 == 0 && Y2 == 0))
        //     CheckAll = true;
    }

    // Update the dirty rect then update the awake state or tick down the SleepTimer
    public void Step() {
        UpdateRect();

        if (Awake && !WakeNextStep) {
            SleepTimer--;
            if (SleepTimer == 0) {
                Matrix.ActiveChunks--;
                Awake = false;
            }
        } else {
            Awake = WakeNextStep;
        }

        WakeNextStep = false;
    }

    // Update the current dirty rect, reset the working dirty rect
    public void UpdateRect() {
        X1 = X1w; X1w = Size.X;
        Y1 = Y1w; Y1w = Size.Y;
        X2 = X2w; X2w = 0;
        Y2 = Y2w; Y2w = 0;
    }

    // Check if a position is within the bounds of the Chunk
    public bool InBounds(Vector2i pos) {
        return pos.X >= Position.X && pos.X < Position.X + Size.X && pos.Y >= Position.Y && pos.Y < Position.Y;
    }
}