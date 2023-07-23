using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Engine {
    public Vector2i WindowSize { get; private set; }
    public int MatrixScale { get; private set; }

    public int Seed { get; private set; }
    public int Tick { get; private set; }
    public float Delta { get { return GetFrameTime(); } }

    // Core
    public Pepper Pepper { get; private set; }
    public Config Config { get; private set; }
    public Matrix Matrix { get; private set; }
    public Physics Physics { get; private set; }
    public Interface Interface { get; private set; }
    public Canvas Canvas { get; private set; }
    public Camera Camera { get; private set; }

    public Theme Theme { get { return Interface.Theme; } }

    public TestPlayer? Player { get; private set; }

    // ECS
    public List<Entity> Entities { get; private set; }

    // State
    public bool Active { get; private set; }        = true;     // Simulation (Matrix) pause state
    public bool StepOnce { get; private set; }      = false;    // (When paused) Reactivate Matrix, perform one step, pause again
    public bool FullStop { get; private set; }      = false;    // Stop everything except the bare minimum, set only by Pepper.Throw()
    public bool ShouldExit { get; private set; }    = false;    // Program will exit after the current update is completed

    // Extra
    private List<Timer> Timers = new List<Timer>();
    private List<KeyboardKey> HeldKeys = new List<KeyboardKey>();


    public Engine(Vector2i window_size, int matrix_scale) {
        WindowSize = window_size;
        MatrixScale = matrix_scale;

        // ECS
        Entities = new List<Entity>();

        // Core
        Pepper = new Pepper(this);
        Config = new Config(this);

        Pepper.Log("Engine initialized", LogType.ENGINE);

        Matrix = new Matrix(this);
        Physics = new Physics(this);
        Interface = new Interface(this);
        Canvas = new Canvas(this);
        Camera = new Camera(this);

        // Player = new Entity();
        // Camera.Target = Player;
    }

    public void HandleInput() {
        var Events = new List<Event>();

        // Key Press/Release
        var K = GetKeyPressed();
        while (K != 0) {
            var Key = (KeyboardKey)K;
            var E = new KeyPressEvent(Key);
            Events.Add(E);
            // Pepper.Log($"{E.Name}");

            if (!HeldKeys.Contains(Key))
                HeldKeys.Add(Key);

            K = GetKeyPressed();
        }

        // Key Down
        for (int i = HeldKeys.Count() - 1; i >= 0; i--) {
            var Key = HeldKeys[i];
            if (!IsKeyDown(Key)) {
                var E = new KeyReleaseEvent(Key);
                Events.Add(E);
                // Pepper.Log($"{E.Name}");
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

        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) Events.Add(new MousePressEvent(MouseButton.MOUSE_BUTTON_LEFT, MousePos));
        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT)) Events.Add(new MousePressEvent(MouseButton.MOUSE_BUTTON_RIGHT, MousePos));

        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) Events.Add(new MouseReleaseEvent(MouseButton.MOUSE_BUTTON_LEFT, MousePos));
        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT)) Events.Add(new MouseReleaseEvent(MouseButton.MOUSE_BUTTON_RIGHT, MousePos));

        // Handle Held Keys (Temporary)
        foreach (var Key in HeldKeys) {
            var E = new KeyDownEvent(Key);
            Events.Add(E);
        }

        // Handle Events (Temporary)
        foreach (var E in Events) {
            // Interface
            if (Interface.FireEvent(E))
                return;

            // Exit
            else if (E.Name == "KeyPress:KEY_ESCAPE")
                ShouldExit = true;

            // Pause
            else if (E.Name == "KeyPress:KEY_SPACE")
                Active = !Active;

            // Advance
            else if (!Active && E.Name == "KeyPress:KEY_T") {
                Active = true;
                StepOnce = true;
            }

            // Paint
            else if (!IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) && E.Name == "MousePress:MOUSE_BUTTON_LEFT") Canvas.Painting = true;
            else if (E.Name == "MouseRelease:MOUSE_BUTTON_LEFT") Canvas.Painting = false;

            // Erase
            else if (!IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) && E.Name == "MousePress:MOUSE_BUTTON_RIGHT") { Canvas.Painting = true; Canvas.Erasing = true; }
            else if (E.Name == "MouseRelease:MOUSE_BUTTON_RIGHT") { Canvas.Painting = false; Canvas.Erasing = false; }

            // Brush Size
            else if (E.Name.Contains("MouseWheel")) { Canvas.BrushSize = Math.Clamp(Canvas.BrushSize - ((MouseWheelEvent)E).Amount, 1, 100); }

            // Player Controls
            else if (Player is null && E.Name == "KeyDown:KEY_W") Camera.Pan(Direction.Up);
            else if (Player is null && E.Name == "KeyDown:KEY_S") Camera.Pan(Direction.Down);
            else if (Player is null && E.Name == "KeyDown:KEY_A") Camera.Pan(Direction.Left);
            else if (Player is null && E.Name == "KeyDown:KEY_D") Camera.Pan(Direction.Right);
            else if (Player is not null && E.Name == "KeyDown:KEY_W") Player.Jump();
            // else if (Player is not null && E.Name == "KeyDown:KEY_S") Player.Crouch();
            else if (Player is not null && E.Name == "KeyDown:KEY_A") Player.Move(Direction.Left);
            else if (Player is not null && E.Name == "KeyDown:KEY_D") Player.Move(Direction.Right);

            else if (Player is null && E.Name == "KeyPress:KEY_MENU") Camera.Reset();
            else if (Player is not null && E.Name == "KeyPress:KEY_MENU") Player.Respawn();
        }
    }

    public void Update() {
        if (FullStop) {
            Interface.Update();
            return;
        }

        if (!Active) {
            Canvas.Update();
            Interface.Update();
            Camera.Update();
            return;
        }

        // Timers
        for (int i = Timers.Count() - 1; i >= 0; i--) {
            var T = Timers[i];
            T.Tick();

            if (T.Done && !T.Repeat)
                Timers.Remove(T);
        }

        // ECS Updates
        PixelMapSystem.Update(Delta);
        Box2DSystem.Update(Delta);

        // Matrix + Canvas Updates
        Matrix.UpdateStart();
        Matrix.Update();
        Canvas.Update();
        Matrix.UpdateEnd();

        // Other Updates
        Physics.Update();
        Interface.Update();
        Camera.Update();

        // Advance Tick
        Tick++;

        if (StepOnce) {
            Active = false;
            StepOnce = false;
        }
    }

    public void Draw() {
        Camera.Draw();

        // Viewport
        BeginMode2D(Camera.Viewport);
        Matrix.Draw();

        RenderSystem.Update(Delta);
        Physics.Draw();

        EndMode2D();

        // Canvas + Interface
        Canvas.Draw();
        Interface.Draw();


        // Misc. HUD/Overlays
        if (!Active) {
            var PauseText = "[PAUSED]";
            var CenterPos = new Vector2i((WindowSize.X / 2) - (MeasureTextEx(Theme.Font, PauseText, Theme.FontSize, Theme.FontSpacing).X / 2), 5);

            DrawTextEx(Theme.Font, PauseText, CenterPos.ToVector2(), Theme.FontSize, Theme.FontSpacing, Theme.Foreground);
        }
    }

	// Toggle the Active state
	public void ToggleActive() {
		Active = !Active;
		var ActiveStr = Active ? "active" : "inactive";
		Pepper.Log($"Simulation is now {ActiveStr}", LogType.ENGINE);
	}

    // Completely stop all processing except for Interface (called only by Pepper.Throw)
    public void Halt() {
        FullStop = true;
    }
}