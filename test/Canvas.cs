using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

// TODO: Add "freeze" mode to brush painting (painted Pixels are static until mouse button is released)

class Canvas {
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Camera Camera { get { return Engine.Camera; } }
    public Pepper Pepper { get { return Engine.Pepper; } }
    public Theme Theme { get { return Engine.Theme; } }

    public RNG RNG { get { return Matrix.RNG; } }

    public Vector2i WindowSize { get { return Engine.WindowSize; } }
    public Vector2i MatrixSize { get { return Engine.Matrix.Size; } }

    public Vector2i MousePos { get; set; }
    public Vector2i MousePrev { get; set; }

    // Brush
    public bool Painting { get; set; } = false;
    public bool Erasing { get; set; } = false;

    public int ID { get; set; }             = 400;
    public int BrushSize { get; set; }      = 10;

    // Menus + Windows
    public Container Toolbar { get; private set; }
    public List<Container> Menus { get; private set; }

    public Container SceneMenu { get; private set; }
    public Container BrushMenu { get; private set; }
    public Container ViewMenu { get; private set; }
    public Container CheatsMenu { get; private set; }
    public Container DebugMenu { get; private set; }

    public List<Container> Windows { get; private set; }
    public Container ExceptionWindow { get; private set; }
    public Container StatsWindow { get; private set; }

    public Multiline StatsContent { get; private set; }

    // Debug
    public bool DrawChunks          = true;         // Draw lines on the border of each Chunk
    public bool DrawDirtyRects      = false;         // Draw each Chunk's dirty rectangle
    public bool DrawMovementOverlay = false;         // Draw Pixels in purple when they have not moved since the last tick and yellow when they have
    public bool DrawSettledOverlay  = false;        // Draw Pixels in red when settled and blue when not settled

    public Canvas(Engine engine) {
        Engine = engine;

        ////
        // Windows
        ExceptionWindow = new Container(
            parent: Engine.Interface,
            position: new Vector2i(Engine.WindowSize.X / 2, Engine.WindowSize.Y / 2),
            draw_anchor: Anchor.Center,
            background: true,
            activated: false
        );

        ExceptionWindow.AddWidget(new Label(ExceptionWindow, "Exception", new Vector2i(750, 20), background: Theme.HeaderBackground));

        StatsWindow = new Container(
            parent: Engine.Interface,
            position: new Vector2i(10, Engine.WindowSize.Y - 10),
            draw_anchor: Anchor.Bottom,
            background: true,
            activated: true
        );

        StatsContent = new Multiline(StatsWindow, "", 300, update_action: UpdateStats);

        StatsWindow.AddWidget(new Label(StatsWindow, "Statistics", new Vector2i(300, 20), background: Theme.HeaderBackground));
        StatsWindow.AddWidget(StatsContent);

        Windows = new List<Container>();
        Windows.Add(ExceptionWindow);
        Windows.Add(StatsWindow);


        ////
        // Containers
        Toolbar = new Container(
            parent: Engine.Interface,
            position: Vector2i.Zero,
            background: true,
            horizontal: true
        );

        SceneMenu = new Container(
            parent: Engine.Interface,
            position: new Vector2i(0, 25),
            activated: false
        );

        BrushMenu = new Container(
            parent: Engine.Interface,
            position: new Vector2i(105, 25),
            activated: false
        );

        ViewMenu = new Container(
            parent: Engine.Interface,
            position: new Vector2i(210, 25),
            activated: false
        );

        CheatsMenu = new Container(
            parent: Engine.Interface,
            position: new Vector2i(315, 25),
            activated: false
        );

        DebugMenu = new Container(
            parent: Engine.Interface,
            position: new Vector2i(420, 25),
            activated: false
        );

        Menus = new List<Container>();
        Menus.Add(SceneMenu);
        Menus.Add(BrushMenu);
        Menus.Add(ViewMenu);
        Menus.Add(CheatsMenu);
        Menus.Add(DebugMenu);

        // Toolbar
        Toolbar.AddWidget(new Button(Toolbar, "Scene", () => { ChangeMenu(SceneMenu); }, new Vector2i(100, 20)));
        Toolbar.AddWidget(new Button(Toolbar, "Brush", () => { ChangeMenu(BrushMenu); }, new Vector2i(100, 20)));
        Toolbar.AddWidget(new Button(Toolbar, "View", () => { ChangeMenu(ViewMenu); }, new Vector2i(100, 20)));
        Toolbar.AddWidget(new Button(Toolbar, "Cheats", () => { ChangeMenu(CheatsMenu); }, new Vector2i(100, 20)));
        Toolbar.AddWidget(new Button(Toolbar, "Debug", () => { ChangeMenu(DebugMenu); }, new Vector2i(100, 20)));

        // Scene Menu
        SceneMenu.AddWidget(new Button(SceneMenu, "Save", () => { SaveScene(); ChangeMenu(); }, new Vector2i(100, 20), background: false));
        SceneMenu.AddWidget(new Button(SceneMenu, "Load", () => { LoadScene(); ChangeMenu(); }, new Vector2i(100, 20), background: false));

        // Brush Menu
        BrushMenu.AddWidget(new Button(BrushMenu, "Stone", () => { ID = 100; ChangeMenu(); }, new Vector2i(100, 20), background: false));
        BrushMenu.AddWidget(new Button(BrushMenu, "Water", () => { ID = 200; ChangeMenu(); }, new Vector2i(100, 20), background: false));
        BrushMenu.AddWidget(new Button(BrushMenu, "Smoke", () => { ID = 300; ChangeMenu(); }, new Vector2i(100, 20), background: false));
        BrushMenu.AddWidget(new Button(BrushMenu, "Sand", () => { ID = 400; ChangeMenu(); }, new Vector2i(100, 20), background: false));

        // Window Menu
        ViewMenu.AddWidget(new Button(ViewMenu, "Statistics", () => { StatsWindow.Toggle(); ChangeMenu(); }, new Vector2i(150, 20), background: false));
        ViewMenu.AddWidget(new Button(ViewMenu, "Skybox", () => { Camera.DrawSkybox = !Camera.DrawSkybox; }, new Vector2i(150, 20), background: false));

        // Cheats Menu
        CheatsMenu.AddWidget(new Label(CheatsMenu, "(dust)", new Vector2i(100, 20)));

        // Debug Menu
        DebugMenu.AddWidget(new Button(DebugMenu, "Movement Overlay", () => { DrawMovementOverlay = !DrawMovementOverlay; if (DrawSettledOverlay) { DrawSettledOverlay = false; } ChangeMenu(); Matrix.RedrawAllChunks = true; }, new Vector2i(150, 20), background: false));
        DebugMenu.AddWidget(new Button(DebugMenu, "Settled Overlay", () => { DrawSettledOverlay = !DrawSettledOverlay; if (DrawMovementOverlay) { DrawMovementOverlay = false; } ChangeMenu(); Matrix.RedrawAllChunks = true; }, new Vector2i(150, 20), background: false));
        DebugMenu.AddWidget(new Button(DebugMenu, "Chunk Borders", () => { DrawChunks = !DrawChunks; ChangeMenu(); }, new Vector2i(150, 20), background: false));
        DebugMenu.AddWidget(new Button(DebugMenu, "Dirty Rects", () => { DrawDirtyRects = !DrawDirtyRects; ChangeMenu(); }, new Vector2i(150, 20), background: false));


        ////
        // Finish
        Engine.Interface.AddContainer(Toolbar);

        foreach (var W in Windows) Engine.Interface.AddContainer(W);
        foreach (var M in Menus) Engine.Interface.AddContainer(M);

        Pepper.Log("Canvas initialized");
    }

    // Close all active menus, if `menu` is given and not active, open that menu
    public void ChangeMenu(Container? menu=null) {
        foreach (var M in Menus) {
            if (M == menu) M.Toggle();
            else if (M.Active) M.Toggle();
        }
    }

    // Update the Statistics menu Multiline widget
    public void UpdateStats() {
        StatsContent.Text = $"Mouse Pos (Screen): {MousePos / Engine.MatrixScale} %N " +
                            $"Mouse Pos (Matrix): {((Engine.Camera.Position - (Engine.WindowSize / 2)) / Engine.MatrixScale) + (MousePos / Engine.MatrixScale)} %N " + 
                            $"Camera Pos: {Engine.Camera.Position} %N %N " + 
                            $"Chunks: {Matrix.TotalChunks:n0} ({Matrix.ActiveChunks:n0} awake) %N %N " +
                            $"Pixel Ops (Total): {Matrix.PixelsProcessed:n0} %N " + 
                            $"Pixels Moved: {Matrix.PixelsMoved:n0} %N %N " +
                            $"Tick: {Engine.Tick:n0} %N " +
                            $"Avg. FPS: {Matrix.FPSAverage} (H: {Matrix.FPSUpper}, L: {Matrix.FPSLower})";
    }

    // Save a Pixel Scene to a PNG image of a specified size from the origin position given
    public void SaveScene(Vector2i? origin=null, int? width=null, int? height=null) {
        var Origin = origin ?? Vector2i.Zero;
        var Width = width ?? Matrix.Size.X;
        var Height = height ?? Matrix.Size.Y;
    }

    // Load a Pixel Scene from a PNG image to the origin position given
    public unsafe void LoadScene(Vector2i? origin=null) {
        var Origin = origin ?? Vector2i.Zero;

        var SceneImage = LoadImage("res/scenes/ocean_tower.png");
        var SceneColors = LoadImageColors(SceneImage);

        for (int x = 0; x < SceneImage.width; x++) {
            for (int y = 0; y < SceneImage.height; y++) {
                var Pos = new Vector2i(Origin.X + x, Origin.Y + y);
                int Index = (y * SceneImage.width) + x;
                int ID = Atlas.GetIDFromColor(SceneColors[Index]);
                var Pixel = new Pixel();

                if (ID > -1) {
                    var STRID = ID.ToString();
                    switch (STRID[0]) {
                        case '1': Pixel = new Solid(ID, Pos); break;
                        case '2': Pixel = new Liquid(ID, Pos); break;
                        case '3': Pixel = new Gas(ID, Pos); break;
                        case '4': Pixel = new Powder(ID, Pos); break;
                    }
                }

                Matrix.Set(Pos, Pixel);
            }
        }

        Pepper.Log("Loaded scene", LogType.MATRIX);
    }

    // Paint Pixels to the Matrix using a brush
    public void Paint() {
        if (Menus.Any(M => M.Active)) ChangeMenu();

        var LinePoints = GetLinePoints(MousePrev, MousePos, BrushSize);
        var PointCache = new List<Vector2i>();

        foreach (var Point in LinePoints) {
            if (!Erasing && ID >= 200 && RNG.Roll(0.95)) continue;

            var P = ((Engine.Camera.Position - (Engine.WindowSize / 2)) / Engine.MatrixScale) + Point;

            if (PointCache.Contains(P)) continue;
            PointCache.Add(P);

            if (Matrix.InBounds(P)) {
                // Painting and point is not empty
                if (!Erasing && !Matrix.IsEmpty(P))
                    continue;

                // Erasing and point is already empty
                if (Erasing && Matrix.IsEmpty(P))
                    continue;

                var Pixel = new Pixel();
                if (!Erasing) {
                    var STRID = ID.ToString();
                    switch (STRID[0]) {
                        case '1': Pixel = new Solid(ID, P); break;
                        case '2': Pixel = new Liquid(ID, P); break;
                        case '3': Pixel = new Gas(ID, P); break;
                        case '4': Pixel = new Powder(ID, P); break;
                    }
                }
                Matrix.Set(P, Pixel);
            }
        }
    }

    // Return a list of all of the distinct points within a square along each point of a line
    public List<Vector2i> GetLinePoints(Vector2i start, Vector2i end, int size) {
        List<Vector2i> Points = new List<Vector2i>();

        start /= Engine.MatrixScale;
        end /= Engine.MatrixScale;

        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                Vector2i a = new Vector2i((start.X - size / 2) + x, (start.Y - size / 2) + y);
                Vector2i b = new Vector2i((end.X - size / 2) + x, (end.Y - size / 2) + y);

                int w = b.X - a.X;
                int h = b.Y - a.Y;
                Vector2i d1 = Vector2i.Zero;
                Vector2i d2 = Vector2i.Zero;

                if (w < 0) d1.X = -1; else if (w > 0) d1.X = 1;
                if (h < 0) d1.Y = -1; else if (h > 0) d1.Y = 1;
                if (w < 0) d2.X = -1; else if (w > 0) d2.X = 1;

                int longest  = Math.Abs(w);
                int shortest = Math.Abs(h);
                if (!(longest > shortest)) {
                    longest = Math.Abs(h);
                    shortest = Math.Abs(w);
                    if (h < 0) d2.Y = -1; else if (h > 0) d2.Y = 1;
                    d2.X = 0;
                }

                int numerator = longest >> 1;
                for (int i = 0; i <= longest; i++) {
                    Points.Add(new Vector2i(a.X, a.Y));
                    numerator += shortest;
                    if (!(numerator < longest)) {
                        numerator -= longest;
                        a.X += d1.X;
                        a.Y += d1.Y;
                    } else {
                        a.X += d2.X;
                        a.Y += d2.Y;
                    }
                }
            }
        }

        return Points.Distinct().ToList();
    }

    public void Update() {
        if (Painting)
            Paint();
    }

    public void Draw() {
        // Brush Indicator
        int Offset = BrushSize % 2 == 0 ? 0 : Engine.MatrixScale / 2;
        int MX = MousePos.X - ((BrushSize * Engine.MatrixScale) / 2) + Offset;
        int MY = MousePos.Y - ((BrushSize * Engine.MatrixScale) / 2) + Offset;

        var Col = Painting ? Color.RED : Color.WHITE;
        DrawRectangleLines(MX, MY, BrushSize * Engine.MatrixScale, BrushSize * Engine.MatrixScale, Col);
    }
}