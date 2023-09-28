using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class NoInputEvent : Event {
    public NoInputEvent() : base(EventType.Any, null) {

    }
}