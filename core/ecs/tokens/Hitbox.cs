using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

/// <summary>
/// Required for collision detection with any entities that do not have a Box2D token
/// </summary>

class Hitbox : Token {
    public int Width { get; private set; }
    public int Height { get; private set; }

    public Hitbox(int width, int height) {
        Width = width;
        Height = height;
    }
}