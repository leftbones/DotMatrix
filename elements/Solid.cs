using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Solid : Pixel {
    public Solid(Vector2i position) : base(position: position){
        ID = 0;

        Active = false;

        Weight = 9999;

        BaseColor = new Color(127, 128, 118, 255);
        ColorOffset = 15;
    }

    public override void Step(Matrix M) {

    }
}