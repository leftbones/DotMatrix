namespace DotMatrix;

abstract class Test {
    public Engine Engine { get; private set; }
    public Matrix Matrix { get { return Engine.Matrix; } }
    public Camera Camera { get { return Engine.Camera; } }
    public RNG RNG { get { return Engine.Matrix.RNG; } }

    public int Duration { get; private set; }
    public bool Active { get; private set; }

    private int RunTime = 0;

    public Test(Engine engine, int duration) {
        Engine = engine;
        Duration = duration;
        Active = true;
    }

    public virtual void Tick(Engine E) {
        RunTime++;

        if (RunTime == Duration) {
            Active = false;
        }
    }
}