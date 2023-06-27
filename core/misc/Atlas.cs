using Raylib_cs;
using static Raylib_cs.Raylib;
using Newtonsoft.Json;

namespace DotMatrix;

enum ElementType { Solid, Liquid, Gas, Powder };

static class Atlas {
    public static Dictionary<string, ElementData> Elements = JsonConvert.DeserializeObject<Dictionary<string, ElementData>>(File.ReadAllText("core/misc/Elements.json"))!;

    public Color HexToColor(string hex) {
        var C = new Color();
        return C;
    }
}

struct ElementData {
    public int ID;
    public string Name;
    public Color Color;
    public ElementType Type;

    public ElementData(int id, string name, Color color, ElementType type) {
        ID = id;
        Name = name;
        Color = color;
        Type = type;
    }
}