using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Render : Token {
    public Texture2D Sprite;

    public Render(string texture_path) {
        Sprite = LoadTexture(texture_path);

        RenderSystem.Register(this);
    }

    public override void Update(float delta) {
        var Transform = Entity!.GetToken<Transform>();
    }
}