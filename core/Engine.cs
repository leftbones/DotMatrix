using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Engine {
    public Vector2i WindowSize { get; private set; }
    public int MatrixScale { get; private set; }

    public int Tick { get; private set; }
    public Matrix Matrix { get; private set; }
    public Interface Interface { get; private set; }
    public Canvas Canvas { get; private set; }

    public bool ShouldExit { get; private set; }

    public Theme Theme { get { return Interface.Theme; } }

    private List<Timer> Timers = new List<Timer>();
    private List<KeyboardKey> HeldKeys = new List<KeyboardKey>();

    public Engine(Vector2i window_size, int matrix_scale) {
        WindowSize = window_size;
        MatrixScale = matrix_scale;

        Matrix = new Matrix(this);
        Interface = new Interface(this);
        Canvas = new Canvas(this);
    }

    public void HandleInput() {
        var Events = new List<Event>();

        // Key Press/Release
        var K = GetKeyPressed();
        while (K != 0) {
            var Key = (KeyboardKey)K;
            Events.Add(new KeyPressEvent(Key));
            HeldKeys.Add(Key);

            K = GetKeyPressed();
        }

        for (int i = HeldKeys.Count() - 1; i >= 0; i--) {
            var Key = HeldKeys[i];
            if (!IsKeyDown(Key)) {
                Events.Add(new KeyReleaseEvent(Key));
                HeldKeys.Remove(Key);
            }
        }

        // Mouse Movement/Buttons/Wheel
        var MousePos = new Vector2i(
            (int)Math.Round((double)GetMouseX() / MatrixScale) * MatrixScale,
            (int)Math.Round((double)GetMouseY() / MatrixScale) * MatrixScale
        );

        Canvas.MousePrev = Canvas.MousePos;
        Canvas.MousePos = MousePos;

        int MouseWheelMove = (int)GetMouseWheelMove();
        if (MouseWheelMove != 0) Events.Add(new MouseWheelEvent(MouseWheelMove));

        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) Events.Add(new MouseDownEvent(MouseButton.MOUSE_BUTTON_LEFT, MousePos));
        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT)) Events.Add(new MouseDownEvent(MouseButton.MOUSE_BUTTON_RIGHT, MousePos));

        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) Events.Add(new MouseUpEvent(MouseButton.MOUSE_BUTTON_LEFT, MousePos));
        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT)) Events.Add(new MouseUpEvent(MouseButton.MOUSE_BUTTON_RIGHT, MousePos));

        // Handle Events (Temporary)
        foreach (var E in Events) {
            // Interface
            if (Interface.FireEvent(E))
                return;

            // Exit
            else if (E.Name == "KeyPress:KEY_ESCAPE")
                ShouldExit = true;

            // Paint
            else if (E.Name == "MouseDown:MOUSE_BUTTON_LEFT") Canvas.Painting = true;
            else if (E.Name == "MouseUp:MOUSE_BUTTON_LEFT") Canvas.Painting = false;

            // Erase
            else if (E.Name == "MouseDown:MOUSE_BUTTON_RIGHT") { Canvas.Painting = true; Canvas.Erasing = true; }
            else if (E.Name == "MouseUp:MOUSE_BUTTON_RIGHT") { Canvas.Painting = false; Canvas.Erasing = false; }

            // Canvas ID
            else if (E.Name == "KeyPress:KEY_ONE") Canvas.ID = 0;
            else if (E.Name == "KeyPress:KEY_TWO") Canvas.ID = 1;
            else if (E.Name == "KeyPress:KEY_THREE") Canvas.ID = 2;
            else if (E.Name == "KeyPress:KEY_FOUR") Canvas.ID = 3;
        }
    }

    public void Update() {
        // Start Update
        Matrix.UpdateStart();

        // Normal Update
        Canvas.Update();
        Matrix.Update();
        Interface.Update();

        // Timers
        for (int i = Timers.Count() - 1; i >= 0; i--) {
            var T = Timers[i];
            T.Tick();

            if (T.Done && !T.Repeat)
                Timers.Remove(T);
        }

        // End Update
        Matrix.UpdateEnd();

        // Advance Tick
        Tick++;
    }

    public void Draw() {
        Matrix.Draw();
        Interface.Draw();
    }
}