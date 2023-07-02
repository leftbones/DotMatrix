using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Solid : Pixel {
    public Solid(int id, Vector2i position) : base(id, position){
        Weight = 9999;

        BaseColor = GetColor(Convert.ToUInt32(Atlas.Colors[ID], 16));
        ColorOffset = 15;
    }

    public override void Step(Matrix M) {
        Settled = true;
    }
}