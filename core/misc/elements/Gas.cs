using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Gas : Pixel {
    public Gas(int id, Vector2i position) : base(id, position){
        Lifetime = RNG.Range(1000, 1500);

        Weight = -20;
        Diffusion = 25;

        BaseColor = GetColor(Convert.ToUInt32(Atlas.Colors[ID], 16));
        ColorOffset = 10;
    }

    public override void Step(Matrix M) {
        // Color Shift (even when not Active)
        FadeOpacity();

        if (!Active) {
            foreach (var Dir in Direction.ShuffledCardinal) {
                if (M.IsValid(Position, Position + Dir)) {
                    Active = true;
                    break;
		        }
	        }
            if (!Active) return;
	    }

        // Weight + Diffusion
        var WeightDir = Direction.None;
        if (Weight != 0)
	        WeightDir = Weight > 0.0f ? Direction.Down : Direction.Up;

        var MoveDir = new Vector2i(
            RNG.Roll(Diffusion) ? RNG.CoinFlip() ? -1 : 1 : 0,
            Weight != 0.0f ? RNG.Roll(Math.Abs(Weight)) ? WeightDir.Y : 0 : RNG.CoinFlip() ? Direction.RandomVertical.Y : 0
        );

        if (MoveDir == Direction.None) {
            Active = false;
            return;
	    }

        if (M.SwapIfValid(Position, Position + MoveDir)) {
            return;
        } else if (MoveDir == Direction.Up || MoveDir == Direction.Down) {
            var HorizDir = Direction.RandomHorizontal;
            if (M.SwapIfValid(Position, Position + HorizDir)) return;
            else if (M.SwapIfValid(Position, Position + Direction.MirrorHorizontal(HorizDir))) return;
            else Active = false;
        } else {
            Active = false;
        }

        // if (!Active) {
        //     foreach (var Dir in Direction.CardinalUp) {
        //         if (M.IsValid(Position + Dir)) {
        //             Active = true;
        //             break;
        //         }
        //     }

        //     if (!Active)
        //         return;
        // }

        // if (M.SwapIfValid(Position, Position + Direction.Up)) return;

        // var HorizDir = Direction.GetMovementDirection(LastPosition, Position);
        // if (!Direction.Horizontal.Contains(HorizDir)) HorizDir = Direction.RandomHorizontal;
        // if (M.SwapIfValid(Position, Position + HorizDir)) return;
        // else if (M.SwapIfValid(Position, Position + Direction.MirrorHorizontal(HorizDir))) return;

        // Active = false;
    }
}