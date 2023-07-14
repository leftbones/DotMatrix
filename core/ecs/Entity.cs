namespace DotMatrix;

class Entity {
    public int ID { get; set; }
    public List<Token> Tokens { get; set; }         = new List<Token>();
    public bool Disabled { get; private set; }      = false;

    // Add a Token to this Entity (if it doesn't already exist)
    public void AddToken(Token token) {
        if (!Tokens.Contains(token)) {
            Tokens.Add(token);
            token.Entity = this;
        }
    }

    // Retrieve a Token from this Entity (if it exists)
    public T? GetToken<T>() where T : Token {
        foreach (var Token in Tokens) {
            if (Token.GetType().Equals(typeof(T)))
                return (T)Token;    
        }
        return null;
    }

    // Mark the Entity to be destroyed at the end of the current update
    public void Destroy() {
        Disabled = true;
    }
}