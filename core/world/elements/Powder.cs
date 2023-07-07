using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Powder : Pixel {
    public Powder(int id, Vector2i position) : base(id, position){
        Weight = 90;
        Friction = 25;

        BaseColor = GetColor(Convert.ToUInt32(Atlas.Colors[ID], 16));
        ColorOffset = 15;
    }

    public override void Step(Matrix M) {
        if (Settled) {
            if (!M.IsValid(Position, Position + Direction.Down)) return;
            else Settled = false;
        }

        if (RNG.Roll(85) && M.SwapIfValid(Position, Position + Direction.Down)) return;

        if (RNG.Roll(50)) {
            var MoveDir = Direction.GetMovementDirection(Position, LastPosition);
            if (!Direction.Horizontal.Contains(MoveDir)) MoveDir = Direction.RandomHorizontal;

            if (M.SwapIfValid(Position, Position + MoveDir)) return;
            else if (M.SwapIfValid(Position, Position + Direction.MirrorHorizontal(MoveDir))) return;
        } else {
            var MoveDir = Direction.GetMovementDirection(Position, LastPosition);
            if (!Direction.DiagonalDown.Contains(MoveDir)) MoveDir = Direction.RandomDiagonalDown;

            if (M.SwapIfValid(Position, Position + MoveDir)) return;
            else if (M.SwapIfValid(Position, Position + Direction.MirrorHorizontal(MoveDir))) return;
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