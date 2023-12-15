namespace DotMatrix;

/// <summary>
/// Super basic benchmark testing. Subclasses of this class are given custom behavior in their constructor and Tick methods.
/// When the Engine has a Test loaded, it runs the Tick method of the test each (active) update, and the test is unloaded once complete.
/// Tests are ticked after Input but before everything else. Tests are paused when the simulation is paused.
/// Note that if base.Tick() is not called, the duration will never tick down.
/// </summary>

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

    public virtual void Tick() {
        RunTime++;

        if (RunTime == Duration) {
            Active = false;
        }
    }
}