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

        // if (RNG.Roll(Fluidity) && M.SwapIfValid(Position, Position + Direction.Down)) return;

        // var LR = LastDirection;
        // if (!Direction.Horizontal.Contains(LR)) LR = M.Engine.Tick % 2 == 0 ? Direction.Left : Direction.Right;
        // for (int i = 0; i < 32; i++) {
        //     if (!M.SwapIfValid(Position, Position + LR)) return;
        //     else if (M.InBoundsAndEmpty(Position + Direction.Down)) return;
        // }

        if (RNG.Roll(Fluidity) && M.SwapIfValid(Position, Position + Direction.Down)) return;

        var MoveDir = Direction.None;
        if (M.InBoundsAndEmpty(Position + Direction.Down)) {
            MoveDir = LastDirection;
            if (!Direction.Horizontal.Contains(MoveDir)) MoveDir = Direction.RandomHorizontal;

            if (M.SwapIfValid(Position, Position + MoveDir)) return;
            else if (M.SwapIfValid(Position, Position + Direction.MirrorHorizontal(MoveDir))) return;
        } else {
            MoveDir = LastDirection;
            if (!Direction.Horizontal.Contains(MoveDir)) MoveDir = Direction.RandomHorizontal;
            // if (!Direction.Horizontal.Contains(MoveDir)) MoveDir = M.Engine.Tick % 2 == 0 ? Direction.Right : Direction.Left;

            for (int i = 0; i < 32; i++) {
                if (!M.SwapIfValid(Position, Position + MoveDir)) return;
                else if (M.InBoundsAndEmpty(Position + Direction.Down)) return;
            }

            if (!RNG.Roll(Fluidity))
                Settled = true;
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