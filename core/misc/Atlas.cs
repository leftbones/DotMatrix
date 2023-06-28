using Raylib_cs;
using static Raylib_cs.Raylib;
using Newtonsoft.Json;

namespace DotMatrix;

enum ElementType { Solid, Liquid, Gas, Powder };

static class Atlas {
    public static Dictionary<string, ElementData> Solids = JsonConvert.DeserializeObject<Dictionary<string, ElementData>>(File.ReadAllText("core/misc/elements/Solids.json"))!;
    public static Dictionary<string, ElementData> Liquids = JsonConvert.DeserializeObject<Dictionary<string, ElementData>>(File.ReadAllText("core/misc/elements/Liquids.json"))!;
    public static Dictionary<string, ElementData> Gases = JsonConvert.DeserializeObject<Dictionary<string, ElementData>>(File.ReadAllText("core/misc/elements/Gases.json"))!;
    public static Dictionary<string, ElementData> Powders = JsonConvert.DeserializeObject<Dictionary<string, ElementData>>(File.ReadAllText("core/misc/elements/Powders.json"))!;
}

struct ElementData {
    public int ID;
    public string Name;
    public string Color;
    public ElementType Type;

    public ElementData(int id, string name, string color, ElementType type) {
        ID = id;
        Name = name;
        Color = color;
        Type = type;
    }
}