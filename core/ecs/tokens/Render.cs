using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Render : Token {
    public Texture2D Texture { get; set; }
    public Vector2 Origin { get; set; }

    public int Width { get { return Texture.width; } }
    public int Height { get { return Texture.height; } }

    public Rectangle Rect { get { return new Rectangle(0, 0, Width, Height); } }

    public Render(string? sprite_path=null) {
        if (sprite_path is not null) {
            Texture = LoadTexture(sprite_path);
            Origin = new Vector2(Texture.width / 2, Texture.height / 2);
        }

        // Finish
        RenderSystem.Register(this);
    }

    public override void Update(float delta) {
        if (Entity is null) return;

        var Transform = Entity!.GetToken<Transform>();
        var PixelMap = Entity!.GetToken<PixelMap>();
        var Box2D = Entity!.GetToken<Box2D>();

        Rectangle DestRect;
        if (Box2D is null) DestRect = new Rectangle(Transform!.Position.X, Transform!.Position.Y, Width * Global.MatrixScale, Height * Global.MatrixScale);
        else DestRect = Box2D!.ScaledRect;

        Vector2 Orig;
        if (Box2D is null) Orig = Origin;
        else Orig = Box2D!.ScaledOrigin;

        float Rotation = Transform!.Rotation;

        // DrawTexturePro(Texture, Rect, DestRect, Orig, Rotation, Color.WHITE);
    }
}