using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace DotMatrix;

[System.Serializable]
public struct Quad : IEquatable<Quad> {
    public int U;
    public int D;
    public int L;
    public int R;

    public int X { get { return L + R; } }
    public int Y { get { return U + D; } }

    // Constructor
    public Quad(int u, int d, int l, int r) {
        U = u;
        D = d;
        L = l;
        R = r;
    }

    // Access
    public int this[int index] {
        get {
            if (index == 0) return U;
            else if (index == 1) return D;
            else if (index == 2) return L;
            else if (index == 3) return R;
            else throw new IndexOutOfRangeException($"Quad: index out of range, index {index}");
        }

        set {
            if (index == 0) U = value;
            else if (index == 1) D = value;
            else if (index == 2) L = value;
            else if (index == 3) R = value;
            else throw new IndexOutOfRangeException($"Quad: index out of range, index {index}");
        }
    }

    // Equals
    public override bool Equals([NotNullWhen(true)] object? obj) {
        return obj is Quad && base.Equals(obj);
    }

    public bool Equals(Quad other) {
        return U == other.U && D == other.D && L == other.L && R == other.R;
    }

    public static bool operator ==(Quad left, Quad right) {
        return left.Equals(right);
    }

    public static bool operator !=(Quad left, Quad right) {
        return !(left == right);
    }

    public override int GetHashCode() {
        return HashCode.Combine(U, D, L, R);
    }

    // Shorthand + Misc
    public static readonly Quad Zero = new Quad(0, 0, 0, 0);

    public int Sum() {
        return U + D + L + R;
    }
}