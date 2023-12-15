using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Powder : Pixel {
    public Powder(int id, Vector2i position) : base(id, position){
        Weight = 90;
        Friction = Atlas.Elements[ID].Friction;
        Drift = Atlas.Elements[ID].Drift;

        BaseColor = GetColor(Convert.ToUInt32(Atlas.Colors[ID], 16));
        ColorOffset = Atlas.Elements[ID].ColorOffset;
    }

    public override void Step(Matrix M, RNG RNG) {
        if (Settled) {
            if (!M.IsValid(Position, Position + Direction.Down)) return;
            else Settled = false;
        }

        // Drift
        if (RNG.Roll(Drift) && M.IsValid(Position, Position + Direction.Down)) {
            var MoveDir = Direction.GetMovementDirection(Position, LastPosition);
            if (!Direction.Horizontal.Contains(MoveDir)) MoveDir = Direction.Random(RNG, Direction.Horizontal);
            
            if (M.SwapIfValid(Position, Position + MoveDir)) return;
            else if (M.SwapIfValid(Position, Position + Direction.MirrorHorizontal(MoveDir))) return;
        }

        // Move Down
        if (RNG.Roll(85) && M.SwapIfValid(Position, Position + Direction.Down)) return;

        // Move Horizontal
        if (RNG.Roll(50)) {
            var MoveDir = Direction.GetMovementDirection(Position, LastPosition);
            if (!Direction.Horizontal.Contains(MoveDir)) MoveDir = Direction.Random(RNG, Direction.Horizontal);
            
            if (M.SwapIfValid(Position, Position + MoveDir)) return;
            else if (M.SwapIfValid(Position, Position + Direction.MirrorHorizontal(MoveDir))) return;
        }

        // Move Diagonal Down 
        else {
            var MoveDir = Direction.GetMovementDirection(Position, LastPosition);
            if (!Direction.DiagonalDown.Contains(MoveDir)) MoveDir = Direction.Random(RNG, Direction.DiagonalDown);

            if (M.SwapIfValid(Position, Position + MoveDir)) return;
            else if (M.SwapIfValid(Position, Position + Direction.MirrorHorizontal(MoveDir))) return;
        }
    }

    public override bool ActOnOther(Matrix M, RNG RNG, Pixel O) {
        if (!Settled && !RNG.Roll(O.Friction)) {
            O.Settled = false;
            return true;
        }

        return false;
    }
}