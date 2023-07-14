using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Solid : Pixel {
    public Solid(int id, Vector2i position) : base(id, position){
        Weight = 9999;

        BaseColor = Atlas.MaterialMaps[ID].GetColor(position);
        ColorOffset = 0;
    }

    public override void Step(Matrix M, RNG RNG) {
        Settled = true;
    }
}