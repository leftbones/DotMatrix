using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

// Abstract Mouse Event
abstract class MouseEvent : Event {
    public MouseButton? MouseButton;
    public MouseEvent(MouseButton? mouse_button, String name, EventType type, Action? action) : base(type, action) {
        MouseButton = mouse_button;
    }
}

// Mouse Press
class MousePressEvent : MouseEvent {
    public Vector2i Position;

    public MousePressEvent(MouseButton mouse_button, Vector2i position, EventType type, Action? action) : base(mouse_button, $"MousePress:{mouse_button}", type, action) {
        MouseButton = mouse_button;
        Position = position;
    }
}

// Mouse Release
class MouseReleaseEvent : MouseEvent {
    public Vector2i Position;

    public MouseReleaseEvent(MouseButton mouse_button, Vector2i position, EventType type, Action? action) : base(mouse_button, $"MouseRelease:{mouse_button}", type, action) {
        MouseButton = mouse_button;
        Position = position;
    }
}

// Mouse Down
class MouseDownEvent : MouseEvent {
    public Vector2i Position;

    public MouseDownEvent(MouseButton mouse_button, Vector2i position, EventType type, Action? action) : base(mouse_button, $"MouseDown:{mouse_button}", type, action) {
        MouseButton = mouse_button;
        Position = position;
    }
}

// Mouse Wheel
class MouseWheelEvent : MouseEvent {
    public int Amount { get; private set; }

    public MouseWheelEvent(int amount, EventType type, Action? action) : base(null, $"MouseWheel:{amount}", type, action) {
        Amount = amount;
    }
}