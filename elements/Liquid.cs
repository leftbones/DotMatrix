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

            if (!Active)
                return;
        }

        if (RNG.Roll(5) && M.SwapIfValid(Position, Position + Direction.RandomHorizontal)) return;
        if (M.SwapIfValid(Position, Position + Direction.Down)) return;


        var HorizDir = M.Engine.Tick % 2 == 0 ? Direction.Left : Direction.Right;

        for (int i = 0; i < 10; i++) {
            if (!RNG.Roll(Fluidity) && !M.SwapIfValid(Position, Position + HorizDir)) return;
        }

        if (Position == LastPosition && !RNG.Roll(Fluidity))
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