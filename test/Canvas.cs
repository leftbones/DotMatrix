using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

// TODO: Add "freeze" mode to brush painting (painted Pixels are static until mouse button is released)

class Canvas {
    public Engine Engine { get; private set; }
    public Pepper Pepper { get { return Engine.Pepper; } }

    public int ID { get; set; }     = 3;
    public int BrushSize { get; set; }   = 10;

    public Vector2i WindowSize { get { return Engine.WindowSize; } }
    public Vector2i MatrixSize { get { return Engine.Matrix.Size; } }

    public Vector2i MousePos { get; set; }
    public Vector2i MousePrev { get; set; }

    public bool Painting { get; set; } = false;
    public bool Erasing { get; set; } = false;

    public Container Toolbar { get; private set; }
    public List<Container> Menus { get; private set; }

    public Container SceneMenu { get; private set; }
    public Container BrushMenu { get; private set; }
    public Container CheatsMenu { get; private set; }
    public Container DebugMenu { get; private set; }

    // Tools + Properties
    public bool DrawChunks          = false;
    public bool DrawDirtyRects      = false;
    public bool DrawActiveOverlay   = false;

    public Canvas(Engine engine) {
        Engine = engine;

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

        CheatsMenu = new Container(
            parent: Engine.Interface,
            position: new Vector2i(210, 25),
            activated: false
        );

        DebugMenu = new Container(
            parent: Engine.Interface,
            position: new Vector2i(315, 25),
            activated: false
        );

        Engine.Interface.AddContainer(Toolbar);
        Engine.Interface.AddContainer(SceneMenu);
        Engine.Interface.AddContainer(BrushMenu);
        Engine.Interface.AddContainer(CheatsMenu);
        Engine.Interface.AddContainer(DebugMenu);

        Menus = new List<Container>();
        Menus.Add(SceneMenu);
        Menus.Add(BrushMenu);
        Menus.Add(CheatsMenu);
        Menus.Add(DebugMenu);

        // Toolbar
        Toolbar.AddWidget(new Button(Toolbar, "Scene", () => { ChangeMenu(SceneMenu); }, new Vector2i(100, 20)));
        Toolbar.AddWidget(new Button(Toolbar, "Brush", () => { ChangeMenu(BrushMenu); }, new Vector2i(100, 20)));
        Toolbar.AddWidget(new Button(Toolbar, "Cheats", () => { ChangeMenu(CheatsMenu); }, new Vector2i(100, 20)));
        Toolbar.AddWidget(new Button(Toolbar, "Debug", () => { ChangeMenu(DebugMenu); }, new Vector2i(100, 20)));

        // Scene Menu
        SceneMenu.AddWidget(new Button(SceneMenu, "Save", () => { SaveScene(); ChangeMenu(); }, new Vector2i(100, 20), background: false));
        SceneMenu.AddWidget(new Button(SceneMenu, "Load", () => { LoadScene(); ChangeMenu(); }, new Vector2i(100, 20), background: false));

        // Brush Menu
        BrushMenu.AddWidget(new Button(BrushMenu, "Stone", () => { ID = 0; ChangeMenu(); }, new Vector2i(100, 20), background: false));
        BrushMenu.AddWidget(new Button(BrushMenu, "Water", () => { ID = 1; ChangeMenu(); }, new Vector2i(100, 20), background: false));
        BrushMenu.AddWidget(new Button(BrushMenu, "Smoke", () => { ID = 2; ChangeMenu(); }, new Vector2i(100, 20), background: false));
        BrushMenu.AddWidget(new Button(BrushMenu, "Sand", () => { ID = 3; ChangeMenu(); }, new Vector2i(100, 20), background: false));

        // Cheats Menu
        CheatsMenu.AddWidget(new Label(CheatsMenu, "(dust)", new Vector2i(100, 20)));

        // Debug Menu
        DebugMenu.AddWidget(new Button(DebugMenu, "Active Overlay", () => { DrawActiveOverlay = !DrawActiveOverlay; ChangeMenu(); }, new Vector2i(150, 20), background: false));
        DebugMenu.AddWidget(new Button(DebugMenu, "Chunk Borders", () => { DrawChunks = !DrawChunks; ChangeMenu(); }, new Vector2i(150, 20), background: false));
        DebugMenu.AddWidget(new Button(DebugMenu, "Dirty Rects", () => { DrawDirtyRects = !DrawDirtyRects; ChangeMenu(); }, new Vector2i(150, 20), background: false));

        // Finish
        Pepper.Log(LogType.OTHER, LogLevel.MESSAGE, "Canvas initialized.");
    }

    public void ChangeMenu(Container? menu=null) {
        foreach (var M in Menus) {
            if (M == menu) M.Toggle();
            else if (M.Active) M.Toggle();
        }
    }

    public void SaveScene() {

    }

    public void LoadScene() {

    }

    public void Paint() {
        var LinePoints = GetLinePoints(MousePrev, MousePos, BrushSize);
        var PointCache = new List<Vector2i>();

        foreach (var Point in LinePoints) {
            if (!Erasing && ID > 0 && RNG.Roll(0.95)) continue;

            if (PointCache.Contains(Point)) continue;
            PointCache.Add(Point);

            if (Engine.Matrix.InBounds(Point)) {
                var Pixel = new Pixel();
                if (!Erasing) {
                    if (ID == 0) Pixel = new Solid(Point);
                    if (ID == 1) Pixel = new Liquid(Point);
                    if (ID == 2) Pixel = new Gas(Point);
                    if (ID == 3) Pixel = new Powder(Point);
                }
                Engine.Matrix.Set(Point, Pixel);
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