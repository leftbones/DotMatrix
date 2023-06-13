using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Event {
    public string Name { get; private set; }

    public Event(string name) {
        Name = name;
    }

    public override string ToString() {
        return Name;
    }
}