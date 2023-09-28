using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

/// <summary>
/// Handles the creation, updating, event firing, and drawing of all interface elements.
/// All input is sent here first, and if no children respond, the input is processed further in the Input class. See `Input` for more details.
/// Inputs are sent to each child Container, and each Container sends it to each child Widget. If any child anywhere in the chain returns true,
/// that means the input was handled and does not need to be processed further.
/// </summary>

enum Anchor { Left, Center, Right, Top, Bottom }; // TODO: Implement TopLeft, BottomLeft, TopCenter, BottomCenter, TopRight, BottomRight

class Interface {
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Pepper Pepper { get { return Engine.Pepper; } }
    public Vector2i WindowSize { get { return Engine.WindowSize; } }

    public List<Container> Containers { get; private set; } = new List<Container>();

    public Theme Theme { get; private set; } = new Theme("res/fonts/pixantiqua.png");

    public Interface(Engine engine) {
        Engine = engine;

        Pepper.Log("Interface initialized", LogType.INTERFACE);
    }

    public void ApplyConfig(Config C) {
        Pepper.Log("Interface config applied", LogType.SYSTEM);
    }

    public void AddContainer(Container C) {
        if (Containers.Contains(C)) return;
        Containers.Add(C);
    }

    public bool FireEvent(Key K) {
        // Pass event to all containers
        foreach (var C in Containers) {
            if (C.FireEvent(K))
                return true;
        }

        return false;
    }

    public void Update() {
        foreach (var C in Containers)
            C.Update();
    }

    public void Draw() {
        foreach (var C in Containers)
            C.Draw();

        // FPS
        var FPS = $"{GetFPS()} FPS";
        var Pos = new Vector2i(WindowSize.X - ((int)MeasureTextEx(Theme.Font, FPS, Theme.FontSize, Theme.FontSpacing).X + 5), 5);
        DrawTextEx(Theme.Font, FPS, Pos.ToVector2(), Theme.FontSize, Theme.FontSpacing, Theme.Foreground);
    }
}