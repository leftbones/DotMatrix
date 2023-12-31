namespace DotMatrix;

class System<T> where T : Token {
    // protected static List<T> Tokens = new List<T>();
    public static List<T> Tokens = new List<T>();

    // Add a Token to the System's Token registry
    public static void Register(T token) {
        Tokens.Add(token);
    }

    // Remove a Token from the System's Token registry
    public static void Deregister(T token) {
        Tokens.Remove(token);
    }

    // Update all Tokens in the registry
    public static void Update(float delta) {
        for (int i = Tokens.Count - 1; i >= 0; i--) {
            var T = Tokens[i];

            if (T.Entity is not null && T.Entity.Disabled) {
                Deregister(T);
                continue;
            }

            T.Update(delta);
        }
    }
}

class ControlSystem: System<Control> { }
class RenderSystem : System<Render> { }
class PixelMapSystem : System<PixelMap> { }
class Box2DSystem : System<Box2D> { }