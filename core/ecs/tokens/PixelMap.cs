using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

// Creates and stores a miniature matrix of Pixels taken from a material map image and pixel map image

// Material Map - An image that denotes which Pixel materials are placed where, like a silhouette of the Pixel Map
// Pixel Map - An image used to set the colors of the Pixels set by the Material Map image

class PixelMap : Token {
    public Pixel[,] Pixels { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public Texture2D Texture { get; private set; }
    private Image Buffer;

    private Color Transparent = new Color(0, 0, 0, 0);

    public unsafe PixelMap(string material_map, string pixel_map) {
        var MaterialImage = LoadImage(material_map);
        var MaterialColors = LoadImageColors(MaterialImage);

        var PixelImage = LoadImage(pixel_map);
        var PixelColors = LoadImageColors(PixelImage);

        Width = MaterialImage.width;
        Height = MaterialImage.height;

        Pixels = new Pixel[Width, Height];

        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                int Index = (y * Width) + x;
                var Color = PixelColors[Index];
                int ID = Atlas.GetIDFromColor(MaterialColors[Index]);

                var Pos = new Vector2i(x, y);
                var Pixel = new Pixel();
                if (ID > -1) {
                    var STRID = ID.ToString();
                    switch (STRID[0]) {
                        case '1': Pixel = new Solid(ID, Pos); break;
                        case '2': Pixel = new Liquid(ID, Pos); break;
                        case '3': Pixel = new Gas(ID, Pos); break;
                        case '4': Pixel = new Powder(ID, Pos); break;
                    }

                    Pixel.BaseColor = Color;
                    Pixel.Color = Color;
                    Pixel.ColorSet = true;
                    Pixels[x, y] = Pixel;
                }
            }
        }

        Buffer = GenImageColor(Width * 4, Height * 4, Transparent);
        Texture = LoadTextureFromImage(Buffer);

        PixelMapSystem.Register(this);
    }

    public override unsafe void Update(float delta) {
        ImageClearBackground(ref Buffer, Transparent);

        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                var P = Pixels[x, y];
                if (P is null || P.ID == -1) continue;

                ImageDrawPixel(ref Buffer, P.Position.X, P.Position.Y, P.Color);
            }
        }

        UpdateTexture(Texture, Buffer.data);
    }
}