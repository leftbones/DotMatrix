using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Liquid : Pixel {
    public Liquid(int id, Vector2i position) : base(id, position){
        Weight = 30;
        Fluidity = 95;

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

        if (RNG.Roll(95) && M.SwapIfValid(Position, Position + Direction.Down)) return;

        if (M.InBoundsAndEmpty(Position + Direction.Down)) {
            var MoveDir = Direction.GetMovementDirection(Position, LastPosition);
            if (!Direction.Horizontal.Contains(MoveDir)) MoveDir = Direction.RandomHorizontal;

            if (M.SwapIfValid(Position, Position + MoveDir)) return;
            else if (M.SwapIfValid(Position, Position + Direction.MirrorHorizontal(MoveDir))) return;
        } else {
            var HorizDir = Direction.GetMovementDirection(LastPosition, Position);
            if (!Direction.Horizontal.Contains(HorizDir)) HorizDir = Direction.RandomHorizontal;

            for (int i = 0; i < 5; i++)
                if (!RNG.CoinFlip() && !M.SwapIfValid(Position, Position + HorizDir)) break;

            if (!RNG.Roll(Fluidity) && Position == LastPosition)
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