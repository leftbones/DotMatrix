using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Chunk {
    public Vector2i Position { get; private set; }
    public Vector2i Size { get; private set; }

    public bool Awake { get; private set; } = true;
    public bool WakeNextStep { get; private set; } = false;

    public Chunk(Vector2i position, Vector2i size) {
        Position = position;
        Size = size;
    }

    public void Wake() {
        WakeNextStep = true;
    }

    public void Step() {
        Awake = WakeNextStep;
        WakeNextStep = false;
    }
}