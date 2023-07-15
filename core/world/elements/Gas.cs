using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Gas : Pixel {
    public Gas(int id, Vector2i position) : base(id, position){
        Lifetime = 750;

        Weight = -30;
        Diffusion = 25;

        BaseColor = BaseColor ?? GetColor(Convert.ToUInt32(Atlas.Colors[ID], 16));
        ColorOffset = 10;
    }

    public override void Step(Matrix M, RNG RNG) {
        // Color Shift (even when not Active)
        if (Lifetime > -1)
            FadeOpacity();

        if (Settled) {
            foreach (var Dir in Direction.Cardinal) {
                if (M.IsValid(Position, Position + Dir)) {
                    Settled = false;
                    break;
                }
            }

            if (Settled) return;
	    }

        M.WakeChunk(Position);

        // Weight + Diffusion
        var WeightDir = Direction.None;
        if (Weight != 0)
	        WeightDir = Weight > 0.0f ? Direction.Down : Direction.Up;

        // var MoveDir = new Vector2i(
        //     RNG.Roll(Diffusion) ? RNG.CoinFlip() ? -1 : 1 : 0,
        //     Weight != 0.0f ? RNG.Roll(Math.Abs(Weight)) ? WeightDir.Y : 0 : RNG.CoinFlip() ? Direction.Random(RNG, Direction.Vertical).Y : 0
        // );

        var DX = 0;
        var DY = 0;

        if (RNG.Roll(Diffusion))
            DX = RNG.CoinFlip() ? -1 : 1;

        if (DX == 0 && Weight != 0.0f && RNG.Roll(Math.Abs(Weight)))
            DY = Math.Sign(Weight);

        var MoveDir = new Vector2i(DX, DY);

        if (MoveDir == Direction.None) {
            Settled = true;
            return;
	    }

        if (M.SwapIfValid(Position, Position + MoveDir)) {
            return;
        } else if (MoveDir == Direction.Up || MoveDir == Direction.Down) {
            var HorizDir = Direction.Random(RNG, Direction.Horizontal);
            if (M.SwapIfValid(Position, Position + HorizDir)) return;
            else if (M.SwapIfValid(Position, Position + Direction.MirrorHorizontal(HorizDir))) return;
            else Settled = true;
        } else if (RNG.Roll(Diffusion)) {
            Settled = true;
        }
    }
}