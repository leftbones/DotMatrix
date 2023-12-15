using Raylib_cs;
using static Raylib_cs.Raylib;
using Newtonsoft.Json;

namespace DotMatrix;

/// <summary>
/// An index containing information about all of the Pixel element types and color data used by the Canvas as well as material maps (for elements with repeating material textures like stone)
/// </summary>

enum ElementType { Solid, Liquid, Gas, Powder };

static class Atlas {
    public static Dictionary<int, ElementData> Elements = new Dictionary<int, ElementData>();

    public static List<ElementData> Solid = new List<ElementData>();
    public static List<ElementData> Liquid = new List<ElementData>();
    public static List<ElementData> Gas = new List<ElementData>();
    public static List<ElementData> Powder = new List<ElementData>();

    public static Dictionary<int, string> Colors = new Dictionary<int, string>();
    public static Dictionary<int, MaterialMap> MaterialMaps = new Dictionary<int, MaterialMap>();

    public static void Initialize() {
        // Deserialize Elements
        int ID;

        ID = 1000;
        foreach (var Path in Directory.EnumerateFiles("core/world/elements/json/solid", "*.json")) {
            var JsonData = File.ReadAllText(Path);
            var ElementData = JsonConvert.DeserializeObject<ElementData>(JsonData);
            ElementData.ID = ID;
            Solid.Add(ElementData);
            ID++;
        }

        ID = 2000;
        foreach (var Path in Directory.EnumerateFiles("core/world/elements/json/liquid", "*.json")) {
            var JsonData = File.ReadAllText(Path);
            var ElementData = JsonConvert.DeserializeObject<ElementData>(JsonData);
            ElementData.ID = ID;
            Liquid.Add(ElementData);
            ID++;
        }

        ID = 3000;
        foreach (var Path in Directory.EnumerateFiles("core/world/elements/json/gas", "*.json")) {
            var JsonData = File.ReadAllText(Path);
            var ElementData = JsonConvert.DeserializeObject<ElementData>(JsonData);
            ElementData.ID = ID;
            Gas.Add(ElementData);
            ID++;
        }

        ID = 4000;
        foreach (var Path in Directory.EnumerateFiles("core/world/elements/json/powder", "*.json")) {
            var JsonData = File.ReadAllText(Path);
            var ElementData = JsonConvert.DeserializeObject<ElementData>(JsonData);
            ElementData.ID = ID;
            Powder.Add(ElementData);
            ID++;
        }

        // Store Elements
        foreach (var Data in Solid) Elements[Data.ID] = Data;
        foreach (var Data in Liquid) Elements[Data.ID] = Data;
        foreach (var Data in Gas) Elements[Data.ID] = Data;
        foreach (var Data in Powder) Elements[Data.ID] = Data;

        // Store Colors
        foreach (var Data in Solid) Colors[Data.ID] = Data.Color;
        foreach (var Data in Liquid) Colors[Data.ID] = Data.Color;
        foreach (var Data in Gas) Colors[Data.ID] = Data.Color;
        foreach (var Data in Powder) Colors[Data.ID] = Data.Color;

        // Store Textures
        foreach (var Data in Solid) {
            var TexturePath = Data.Texture != "" ? Data.Texture : "res/textures/missing.png";
            MaterialMaps[Data.ID] = new MaterialMap(LoadImage(TexturePath));
        }
    }

    // Returns an element ID based on the given color
    public static int GetIDFromColor(Color color) {
        if (color.a > 0) {
            string Hex = ColorToInt(color).ToString("X");
            if (Hex != "FF") {
                if (Hex.Length == 7) Hex = "0" + Hex;
                // if (Hex == "FF00FFFF") return -1;
                return Colors.Where(P => P.Value[0..^2] == Hex[0..^2]).Select(P => P.Key).FirstOrDefault();
            }
        }
        return -1;
    }

    // Returns an element ID based on the given string name
    public static int GetIDFromName(string name) {
        return (from Element in Elements where Element.Value.Name == name select Element.Value.ID).FirstOrDefault();
    }
}

// Stores information about elements read from the json files
struct ElementData {
    public int ID;
    public string Name;
    public string Color;
    public int ColorOffset;
    public string Texture;
    public int Lifetime;
    public int Friction;
    public int Fluidity;
    public int Diffusion;
    public int Drift;
    public ElementType Type;

    public ElementData(int id, string name, string color, int color_offset, string texture, int lifetime, int friction, int fluidity, int diffusion, int drift, ElementType type) {
        ID = id;
        Name = name;
        Color = color;
        ColorOffset = color_offset;
        Texture = texture;
        Lifetime = lifetime;
        Friction = friction;
        Fluidity = fluidity;
        Diffusion = diffusion;
        Drift = drift;
        Type = type;
    }
}

// Stores information about material maps for elements with repeating material textures
unsafe class MaterialMap {
    public Image Image { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    private Color* Colors;

    public unsafe MaterialMap(Image image) {
        Image = image;
        Width = image.width;
        Height = image.height;

        Colors = LoadImageColors(Image);
    }

    public Color GetColor(Vector2i pos) {
        int RX = pos.X == 0 ? 0 : Math.Abs(pos.X) % Width;
        int RY = pos.Y == 0 ? 0 : Math.Abs(pos.Y) % Height;

        int Index = (RY * Width) + RX;
        return Colors[Index];
    }
}