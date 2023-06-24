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
    private int WaitTime = 10;

    public Rectangle DirtyRect { get { return new Rectangle(Position.X + X1, Position.Y + Y1, X2 - X1, Y2 - Y1); } }

    public bool Awake { get; private set; } = true;
    public bool WakeNextStep { get; private set; } = false;

    public bool CheckAll { get; set; }

    public Chunk(Vector2i position, Vector2i size) {
        Position = position;
        Size = size;

        SleepTimer = WaitTime;
    }

    public void Wake(Vector2i pos, bool check_all=false) {
        WakeNextStep = true;
        SleepTimer = WaitTime;
        CheckAll = check_all;
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
}