using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Event {
    public string Name { get; private set; }

    public Event(string name) { // TODO: Change string name to use the keycode identifier instead, not as nice for debugging purposes but faster to check than comparing strings
        Name = name;
    }

    public override string ToString() {
        return Name;
    }
}