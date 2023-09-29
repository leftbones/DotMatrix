namespace DotMatrix;

/// <summary>
/// Receives and acts on player input, used for anything the player might be controlling
/// Has a special method `FireEvent` which takes a Key object and compares it to it's stored KeyEvents, firing any event that matches the Key
/// `FireEvent` is called during `Input.Update()` -- Control doesn't need a normal Update method
/// </summary>

class Control : Token {
    public Dictionary<int, Event> KeyEvents { get; private set; }

    public Control() : this(new()) { }
    public Control(Dictionary<int, Event> key_events) {
        KeyEvents = key_events;

        ControlSystem.Register(this);
    }

    public bool FireEvent(Key K) {
        foreach (var Data in KeyEvents) {
            if (K.Code == Data.Key && K.Type == Data.Value.Type) {
                Data.Value.Fire();
                return true;
            }
        }

        return false;
    }
}