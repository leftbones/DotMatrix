using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

// Mouse Press
class MousePressEvent : Event {
    public MouseButton MouseButton;
    public Vector2i Position;

    public MousePressEvent(MouseButton mouse_button, Vector2i position) : base($"MousePress:{mouse_button}") {
        MouseButton = mouse_button;
        Position = position;
    }
}

// Mouse Release
class MouseReleaseEvent : Event {
    public MouseButton MouseButton;
    public Vector2i Position;

    public MouseReleaseEvent(MouseButton mouse_button, Vector2i position) : base($"MouseRelease:{mouse_button}") {
        MouseButton = mouse_button;
        Position = position;
    }
}

// Mouse Down
class MouseDownEvent : Event {
    public MouseButton MouseButton;
    public Vector2i Position;

    public MouseDownEvent(MouseButton mouse_button, Vector2i position) : base($"MouseDown:{mouse_button}") {
        MouseButton = mouse_button;
        Position = position;
    }
}


// Wheel
class MouseWheelEvent : Event {
    public int Amount { get; private set; }

    public MouseWheelEvent(int amount) : base($"MouseWheel:{amount}") {
        Amount = amount;
    }
}