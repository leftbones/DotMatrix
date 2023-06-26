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

    public int X1 { get; set; }                                                                                         // Top left X coordinate of the DirtyRect
    public int Y1 { get; set; }                                                                                         // Top left Y coordinate of the DirtyRect
    public int X2 { get; set; }                                                                                         // Bottom right X coordinate of the DirtyRect
    public int Y2 { get; set; }                                                                                         // Bottom right Y coordinate of the DirtyRect

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
        WakeNextStep = true;
        SleepTimer = WaitTime;

        if (!Awake || (X2 == 0 && Y2 == 0))
            CheckAll = true;
    }

    // Set the Chunk's Awake state, or tick down the SleepTimer
    public void Step() {
        if (Awake && !WakeNextStep) {
            SleepTimer--;
            if (SleepTimer == 0)
                Awake = false;
        } else {
            Awake = WakeNextStep;
        }

        WakeNextStep = false;
    }

    // Check if a position is within the bounds of the Chunk
    public bool InBounds(Vector2i pos) {
        return pos.X >= Position.X && pos.X < Position.X + Size.X && pos.Y >= Position.Y && pos.Y < Position.Y;
    }
}