using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Liquid : Pixel {
    public Liquid(int id, Vector2i position) : base(id, position){
        Weight = 30;
        Fluidity = 75;

        BaseColor = GetColor(Convert.ToUInt32(Atlas.Colors[ID], 16));
    }

    public override void Step(Matrix M) {
        if (Settled) {
            var HorizDir = Direction.RandomHorizontal;
            if (M.IsValid(Position, Position + Direction.Down) ||
                M.IsValid(Position, Position + HorizDir) ||
                M.IsValid(Position, Position + Direction.MirrorHorizontal(HorizDir))) Settled = false;
            else return;
        }

        if (RNG.Roll(Fluidity)) {
            if (M.SwapIfValid(Position, Position + Direction.Down)) return;
        } else {
            var LR = LastDirection;
            if (!Direction.Horizontal.Contains(LR)) LR = Direction.RandomHorizontal;
            if (M.SwapIfValid(Position, Position + LR)) return;
        }
    }

    public override bool ActOnOther(Matrix M, Pixel O) {
        if (!Settled && !RNG.Roll(O.Friction)) {
            O.Settled = false;
            return true;
        }

        return false;
    }
}