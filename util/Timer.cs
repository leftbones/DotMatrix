using Raylib_cs;
using static Raylib_cs.Raylib;

namespace DotMatrix;

class Timer {
    public double StartTime { get; private set; }
    public double Lifetime { get; private set; }
    public Action Action { get; private set; }
    public bool Repeat { get; private set; }

    public bool Active { get; private set; }

    public double Time { get { return Math.Min(GetTime() - StartTime, Lifetime); } }
    public bool Done { get { return GetTime() - StartTime >= Lifetime; } }

    public Timer(double lifetime, Action action, bool start=true, bool repeat=false, bool fire=false) {
        Lifetime = lifetime;
        Action = action;

        Active = false;
        if (start)
            Start();

        Repeat = repeat;

        if (fire)
            Fire();
    }

    // If active, check if the timer is done and fire the action when it is
    public void Tick() {
        if (!Active)
            return;

        if (Done)
            Fire();
    }

    // Start the timer (and reset it)
    public void Start() {
        StartTime = GetTime();
        Active = true;
    }

    // Stop the timer
    public void Stop() {
        Active = false;
    }

    // Toggle the timer on and off
    public void Toggle() {
        Active = !Active;
    }

    // Invoke the timer's action
    public void Fire() {
        Action.Invoke();
        if (Repeat) Start();
        else Stop();
    }
}