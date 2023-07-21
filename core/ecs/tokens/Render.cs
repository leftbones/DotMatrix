using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Render : Token {
    public Render() {
        RenderSystem.Register(this);
    }

    public override void Update(float delta) {
        if (Entity is null) return;

        var Transform = Entity!.GetToken<Transform>();
        var PixelMap = Entity!.GetToken<PixelMap>();
        var Box2D = Entity!.GetToken<Box2D>();

        var Rotation = Box2D is null ? 0.0f : Box2D!.Body.GetAngle() * RAD2DEG;
        var Origin = new Vector2((PixelMap!.Width / 2) * Global.MatrixScale, (PixelMap!.Height / 2) * Global.MatrixScale);
        

        var Rect = new Rectangle(0, 0, PixelMap!.Width, PixelMap!.Height);
        var Dest = new Rectangle(Transform!.Position.X * Global.MatrixScale, Transform!.Position.Y * Global.MatrixScale, PixelMap!.Width * Global.MatrixScale, PixelMap!.Height * Global.MatrixScale);
        DrawTexturePro(PixelMap!.Texture, Rect, Dest, Origin, Rotation, Color.WHITE);
    }
}