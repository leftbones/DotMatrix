namespace DotMatrix;

class Token {
    public Entity? Entity;

    public virtual void Update(float delta) { }
    public virtual void Destroy() { }
}