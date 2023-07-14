using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Liquid : Pixel {
    public Liquid(int id, Vector2i position) : base(id, position){
        Weight = 30;
        Fluidity = 75;

        BaseColor = GetColor(Convert.ToUInt32(Atlas.Colors[ID], 16));
    }

    public override void Step(Matrix M, RNG RNG) {
        if (Settled) {
            var HorizDir = Direction.Random(RNG, Direction.Horizontal);
            if (M.IsValid(Position, Position + Direction.Down) ||
                M.IsValid(Position, Position + HorizDir) ||
                M.IsValid(Position, Position + Direction.MirrorHorizontal(HorizDir))) Settled = false;
            else return;
        }

        var LR = LastDirection;
        if (!Direction.Horizontal.Contains(LR))
            LR = M.Engine.Tick % 2 == 0 ? Direction.Left : Direction.Right;

        if (RNG.Roll(5) && M.SwapIfValid(Position, Position + LR)) return;
        if (M.SwapIfValid(Position, Position + Direction.Down)) return;

        for (int i = 0; i < 8; i++) {
            if (!M.SwapIfValid(Position, Position + LR)) {
                if (RNG.Roll(Fluidity)) LR = Direction.MirrorHorizontal(LR);
                else if (M.IsValid(Position, Position + Direction.Down)) return;
            }

            if (RNG.CoinFlip() && M.InBoundsAndEmpty(Position + Direction.Down)) return;
        }
    }

    public override bool ActOnOther(Matrix M, RNG RNG, Pixel O) {
        if (!Settled && O is Powder && !RNG.Roll(O.Friction)) {
            O.Settled = false;
            return true;
        }

        return false;
    }
}