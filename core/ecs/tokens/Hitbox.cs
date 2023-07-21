using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Hitbox : Token {
    public int Width { get; private set; }
    public int Height { get; private set; }

    public Hitbox(int width, int height) {
        Width = width;
        Height = height;
    }
}