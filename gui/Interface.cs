using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

enum Anchor { Left, Center, Right, Top, Bottom }; // TODO: Implement TopLeft, BottomLeft, TopCenter, BottomCenter, TopRight, BottomRight

class Interface {
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Pepper Pepper { get { return Engine.Pepper; } }
    public Vector2i WindowSize { get { return Engine.WindowSize; } }

    public List<Container> Containers { get; private set; } = new List<Container>();

    public Theme Theme { get; private set; } = new Theme("res/pixantiqua.png");

    public Interface(Engine engine) {
        Engine = engine;

        Pepper.Log(LogType.INTERFACE, LogLevel.MESSAGE, "Interface initialized.");
    }

    public void AddContainer(Container C) {
        if (Containers.Contains(C)) return;
        Containers.Add(C);
    }

    public bool FireEvent(Event E) {
        // Close any open containers
        if (E.Name == "KeyPress:KEY_ESCAPE") {
            bool Fired = false;
            foreach (Container C in Containers) {
                if (C.Active && C != Engine.Canvas.Toolbar) {
                    Fired = true;
                    C.Toggle();
                }
            }
            if (Fired) return true;
        }

        // Pass event to all containers
        foreach (var C in Containers) {
            if (C.FireEvent(E))
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

        // Tick
        var Tick = $"T: {Engine.Tick}";
        Pos = new Vector2i(WindowSize.X - ((int)MeasureTextEx(Theme.Font, Tick, Theme.FontSize, Theme.FontSpacing).X + 5), 20);
        DrawTextEx(Theme.Font, Tick, Pos.ToVector2(), Theme.FontSize, Theme.FontSpacing, Theme.Foreground);
    }
}