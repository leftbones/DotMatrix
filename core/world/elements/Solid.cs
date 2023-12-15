using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Solid : Pixel {
    public Solid(int id, Vector2i position) : base(id, position){
        Weight = 9999;

        if (Atlas.Elements[ID].Texture == "") {
            BaseColor = GetColor(Convert.ToUInt32(Atlas.Colors[ID], 16));
        } else {
            BaseColor = Atlas.MaterialMaps[ID].GetColor(position);
        }

        ColorOffset = Atlas.Elements[ID].ColorOffset;
    }

    public override void Step(Matrix M, RNG RNG) {
        Settled = true;
    }
}