using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Brush {
    public Engine Engine { get; private set; }

    public int ID { get; set; }     = 0;
    public int Size { get; set; }   = 10;

    public Vector2i WindowSize { get { return Engine.WindowSize; } }
    public Vector2i MatrixSize { get { return Engine.Matrix.Size; } }

    public Vector2i MousePos { get; set; }
    public Vector2i MousePrev { get; set; }

    public bool Painting { get; set; } = false;
    public bool Erasing { get; set; } = false;

    public Container Menu { get; private set; }

    public Brush(Engine engine) {
        Engine = engine;

        // Brush Menu
        Menu = new Container(
            parent: Engine.Interface,
            position: new Vector2i(5, 5)
            // position: new Vector2i(10, WindowSize.Y - ((45 * 4) - 5))
        );

        Menu.AddWidget(new Button(Menu, "Stone", () => { ID = 0; }, new Vector2i(80, 35)));
        Menu.AddWidget(new Button(Menu, "Water", () => { ID = 1; }, new Vector2i(80, 35)));
        Menu.AddWidget(new Button(Menu, "Smoke", () => { ID = 2; }, new Vector2i(80, 35)));
        Menu.AddWidget(new Button(Menu, "Sand", () => { ID = 3; }, new Vector2i(80, 35)));

        Engine.Interface.AddContainer(Menu);
    }

    public void Update() {
        if (Painting)
            Paint();
    }

    public void Paint() {
        var LinePoints = GetLinePoints(MousePrev, MousePos, Size);
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
}