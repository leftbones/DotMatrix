using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

/// <summary>
/// The main "handler" class for basically every other system, calls update methods for subsystems like Matrix, Canvas, handles Timers, and tracks ECS entities
/// Currently handles and dispatches user input, but that will be moved to a subsystem eventually
/// Also keeps track of the Tick count, Delta time, and the Active state of the Matrix
/// </summary>

class Engine {
    public Vector2i WindowSize { get; private set; }
    public int MatrixScale { get; private set; }

    public int Seed { get; private set; }
    public int Tick { get; private set; }
    public static float Delta { get { return GetFrameTime(); } }

    // Core
    public Pepper Pepper { get; private set; }
    public Config Config { get; private set; }
    public Matrix Matrix { get; private set; }
    public Interface Interface { get; private set; }
    public Canvas Canvas { get; private set; }
    public Camera Camera { get; private set; }
    public Input Input { get; private set; }

    public Theme Theme { get { return Interface.Theme; } }

    // ECS
    public List<Entity> Entities { get; private set; }

    // State
    public bool Active { get; private set; }        = true;     // Simulation (Matrix) pause state
    public bool StepOnce { get; private set; }      = false;    // (When paused) Reactivate Matrix, perform one step, pause again
    public bool FullStop { get; private set; }      = false;    // Stop everything except the bare minimum, set only by Pepper.Throw()
    public bool ShouldExit { get; private set; }    = false;    // Program will exit after the current update is completed

    // Config
    public bool PauseOnStart { get; private set; } = false;     // If the simulation should be paused before tick 0 is started

    // Extra
    private List<Timer> Timers = new List<Timer>();
    private Test? Test;


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
        Interface = new Interface(this);
        Canvas = new Canvas(this);
        Camera = new Camera(this);
        Input = new Input(this);

        // Apply Config
        Config.ApplyChanges();

        if (PauseOnStart) {
            ToggleActive();
        }


        //
        // TESTING

        var Guy = new Entity();

        Guy.AddToken(new Render("res/objects/guy.png"));
        Guy.AddToken(new Transform(new Vector2i(Matrix.Size.X / 2, 425) * MatrixScale));
        Guy.AddToken(new Hitbox(11, 28, new Vector2i(-1, 0)));
        Guy.AddToken(new Control(new Dictionary<int, Event>() {
            /* Move Left    */ { (int)KeyboardKey.KEY_A, new Event(EventType.Hold, new Action(() => { Guy.GetToken<Transform>()!.Position = Guy.GetToken<Transform>()!.Position + new Vector2i(-4, 0); }))},
            /* Move Right   */ { (int)KeyboardKey.KEY_D, new Event(EventType.Hold, new Action(() => { Guy.GetToken<Transform>()!.Position = Guy.GetToken<Transform>()!.Position + new Vector2i(4, 0); }))},
            /* Move Up      */ { (int)KeyboardKey.KEY_W, new Event(EventType.Hold, new Action(() => { Guy.GetToken<Transform>()!.Position = Guy.GetToken<Transform>()!.Position + new Vector2i(0, -4); }))},
            /* Move Down    */ { (int)KeyboardKey.KEY_S, new Event(EventType.Hold, new Action(() => { Guy.GetToken<Transform>()!.Position = Guy.GetToken<Transform>()!.Position + new Vector2i(0, 4); }))},
            /* Respawn      */ { (int)KeyboardKey.KEY_R, new Event(EventType.Press, new Action(() => { Guy.GetToken<Transform>()!.Position = Matrix.Size * MatrixScale / 2; }))},
        }));

        Entities.Add(Guy);

        Camera.Target = Guy.GetToken<Transform>();
        Camera.Position = Guy.GetToken<Transform>()!.Position.ToVector2();

        Test = new SnowTest(this, 1000);
    }

    // Apply changes to the Config
    public void ApplyConfig(Config C) {
        Pepper.Log("Canvas config applied", LogType.SYSTEM);
    }

    // Main engine update loop
    public void Update() {
        // Handle input before anything else
        Input.Update();

        // If engine is hard-stopped, only update the interface
        if (FullStop) {
            Interface.Update();
            return;
        }

        // If simulation is paused, only update the canvas, interface, and camera
        if (!Active) {
            Canvas.Update();
            Interface.Update();
            Camera.Update();
            return;
        }

        // Run Tests (if active)
        if (Test != null) {
            Test.Tick(this);
            if (!Test.Active) {
                Test = null;
            }
        }

        // Handle timers
        for (int i = Timers.Count() - 1; i >= 0; i--) {
            var T = Timers[i];
            T.Tick();

            if (T.Done && !T.Repeat)
                Timers.Remove(T);
        }

        // ECS Updates
        PixelMapSystem.Update(Delta);

        // Matrix + Canvas Updates
        Matrix.UpdateStart();
        Matrix.Update();
        Canvas.Update();
        Matrix.UpdateEnd();

        // Other Updates
        Interface.Update();
        Camera.Update();

        // Advance Tick
        Tick++;

        // If stepping once (while paused) pause again before ending the update
        if (StepOnce) {
            Active = false;
            StepOnce = false;
        }
    }

    // Main engine draw loop
    public void Draw() {
        Camera.Draw();

        // Viewport
        BeginMode2D(Camera.Viewport);
        Matrix.Draw();

        RenderSystem.Update(Delta);

        //
        // Debug Drawing

        // Entity Hitboxes
        if (Canvas.DrawEntityHitboxes) {
            foreach (var E in Entities) {
                var Hitbox = E.GetToken<Hitbox>();
                if (Hitbox != null) {
                    // DrawRectangleRec(Hitbox.Rect, new Color(255, 0, 0, 50));
                    DrawRectangleLinesEx(Hitbox.Rect, 1.0f, Color.RED);
                }
            }
        }

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

    // Tell the engine to advance one tick and then pause the simulation
    public void TickOnce() {
        if (!Active) {
            Active = true;
            StepOnce = true;
        }
    }

    // Completely stop all processing except for Interface (called only by Pepper.Throw)
    public void Halt() {
        FullStop = true;
        Pepper.Log("Exception thrown.", LogType.ENGINE);
    }

    // Set the "ShouldExit" flag to true
    public void Exit() {
        ShouldExit = true;
        Pepper.Log("Engine will exit at the end of the current tick", LogType.ENGINE);
    }
}