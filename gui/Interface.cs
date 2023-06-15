using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Interface {
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Vector2i WindowSize { get { return Engine.WindowSize; } }

    public List<Container> Containers { get; private set; } = new List<Container>();

    public Interface(Engine engine) {
        Engine = engine;
    }

    public void AddContainer(Container C) {
        if (Containers.Contains(C)) return;
        Containers.Add(C);
    }

    public bool FireEvent(Event E) {
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
    }
}