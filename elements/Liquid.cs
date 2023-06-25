using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Liquid : Pixel {
    public Liquid(Vector2i position) : base(position: position){
        ID = 1;

        Weight = 30;
        Fluidity = 75;

        BaseColor = new Color(1, 151, 244, 255);
    }

    public override void Step(Matrix M) {
        if (!Active) {
            foreach (var Dir in Direction.CardinalDown) {
                if (M.IsValid(Position, Position + Dir)) {
                    Active = true;
                    break;
                }
            }
            if (!Active) return;
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

            bool Moved = false;
            for (int i = 0; i < 5; i++) {
                if (!RNG.CoinFlip() && !M.SwapIfValid(Position, Position + HorizDir)) break;
                Moved = true;
            }

            if (!Moved && !RNG.Roll(Fluidity))
                Active = false;
        }
    }

    public override bool ActOnOther(Matrix M, Pixel O) {
        if (O is Powder && !O.Active && !RNG.Roll(O.Friction)) {
            O.Active = true;
            return true;
        }

        return false;
    }
}