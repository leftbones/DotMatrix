using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Powder : Pixel {
    public Powder(Vector2i position) : base(position: position){
        ID = 3;

        Weight = 90;
        Friction = 60;

        BaseColor = new Color(230, 188, 92, 255);
        ColorOffset = 15;
    }

    public override void Step(Matrix M) {
        if (!Active) {
            if (!M.IsValid(Position, Position + Direction.Down)) return;
            else Active = true;
        }

        if (M.SwapIfValid(Position, Position + Direction.Down)) return;

        var DiagDir = Direction.RandomDiagonalDown;
        if (M.SwapIfValid(Position, Position + DiagDir)) return;
        else if (M.SwapIfValid(Position, Position + Direction.MirrorHorizontal(DiagDir))) return;

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

        if (RNG.Roll(Friction))
            Active = false;
    }

    public override bool ActOnOther(Matrix M, Pixel O) {
        if (O is Powder && !O.Active && !RNG.Roll(O.Friction)) {
            O.Active = true;
            return true;
        }

        return false;
    }
}