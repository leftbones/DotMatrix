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

        DrawTextureEx(PixelMap!.Texture, new Vector2i(Transform!.Position.X * 4, Transform.Position.Y * 4).ToVector2(), 0.0f, 4.0f, Color.WHITE);
    }
}