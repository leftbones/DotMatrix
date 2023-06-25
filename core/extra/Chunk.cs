using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Chunk {
    public Vector2i Position { get; private set; }
    public Vector2i Size { get; private set; }

    public int X1 { get; set; }
    public int Y1 { get; set; }
    public int X2 { get; set; }
    public int Y2 { get; set; }

    public int SleepTimer { get; private set; }
    private int WaitTime = 30;

    public Rectangle DirtyRect { get { return new Rectangle(Position.X + X1, Position.Y + Y1, X2 - X1, Y2 - Y1); } }

    public bool Awake { get; private set; } = false;
    public bool WakeNextStep { get; private set; } = false;

    public bool CheckAll { get; set; } = false;

    public Chunk(Vector2i position, Vector2i size) {
        Position = position;
        Size = size;

        SleepTimer = WaitTime;
    }

    public void Wake(Vector2i pos) {
        WakeNextStep = true;
        SleepTimer = WaitTime;

        if (!Awake || (X2 == 0 && Y2 == 0))
            CheckAll = true;
    }

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

    public bool InBounds(Vector2i pos) {
        return pos.X >= Position.X && pos.X < Position.X + Size.X && pos.Y >= Position.Y && pos.Y < Position.Y;
    }
}