using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex {

    public int q, r, s;
    public float qf, rf, sf;

    public Hex(int _q, int _r, int _s) {
        q = _q;
        r = _r;
        s = _s;

        SetFractional();
        Validate();
    }

    public Hex(int q, int r) : this(q, r, -q - r) { }

    public Hex(float _q, float _r, float _s) {
        qf = _q;
        rf = _r;
        sf = _s;

        Round(_q, _r, _s);
        Validate();
    }

    public Hex(float q, float r) : this(q, r, -q - r) { }

    public Hex MultiplyBy(int k) {
        return new Hex(q * k, r * k);
    }

    public int Length() {
        return (Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(s)) / 2;
    }

    public int DistanceTo(Hex h) {
        return (h - this).Length();
    }

    public Point ToPoint(Layout layout) {
        Orientation m = layout.orientation;
        float x = (m.f0 * q + m.f1 * r) * layout.size.x;
        float y = (m.f2 * q + m.f3 * r) * layout.size.y;
        return new Point(x + layout.origin.x, y + layout.origin.y);
    }

    // Operators and such override

    public override string ToString() {
        return string.Format("Hex({0}, {1}, {2})", q, r, s);
    }

    public static bool operator ==(Hex a, Hex b) {
        if ((object)a == null && (object)b == null) return true;
        if ((object)a == null || (object)b == null) return false;
        return (a.q == b.q) && (a.r == b.r) && (a.s == b.s);
    }

    public static bool operator !=(Hex a, Hex b) {
        return !(a == b);
    }

    public override bool Equals(object obj) {
        if (obj == null) return false;

        Hex h = obj as Hex;
        if ((object)h == null) return false;

        return this == h;
    }

    public override int GetHashCode() {
        return (q.ToString() + r.ToString() + s.ToString()).GetHashCode();
    }

    public static Hex operator +(Hex a, Hex b) {
        return new Hex(a.q + b.q, a.r + b.r);
    }

    public static Hex operator -(Hex a, Hex b) {
        return new Hex(a.q - b.q, a.r - b.r);
    }

    // private

    private void Round(float _q, float _r, float _s) {
        q = Mathf.RoundToInt(_q);
        r = Mathf.RoundToInt(_r);
        s = Mathf.RoundToInt(_s);

        float qDiff = Mathf.Abs(q - _q);
        float rDiff = Mathf.Abs(r - _r);
        float sDiff = Mathf.Abs(s - _s);

        if (qDiff > rDiff && qDiff > sDiff) {
            q = -r - s;
        }
        else if (rDiff > sDiff) {
            r = -q - s;
        }
        else {
            s = -q - r;
        }
    }

    private void SetFractional() {
        qf = q;
        rf = r;
        sf = s;
    }

    public void Validate() {
        if (q + r + s != 0) Debug.Log(string.Format("Invalid {0}", this));
    }

    // Starting from 'tr' clockwise
    static public Hex[] directions = new Hex[6] { new Hex(1, 0, -1), new Hex(1, -1, 0), new Hex(0, -1, 1), new Hex(-1, 0, 1), new Hex(-1, 1, 0), new Hex(0, 1, -1) };

    public Hex[] Neighbours() {
        Hex[] neighbours = new Hex[6];
        for (int i = 0; i < 6; i++) {
            neighbours[i] = this + Hex.directions[i];
        }
        return neighbours;
    }

    /*
     public Hex Direction(int direction) {
        int normalized = (6 + (direction % 6)) % 6;
        
        return directions[normalized];
     }

     public Hex Neighbour(int direction) {
        return this + this.Direction(direction);
     }
    */
}

public class Orientation {
    public float f0, f1, f2, f3;
    public float b0, b1, b2, b3;
    public float startAngle; // in multiples of 60

    public Orientation(float _f0, float _f1, float _f2, float _f3,
                float _b0, float _b1, float _b2, float _b3,
                float _angle) {

        f0 = _f0;
        f1 = _f1;
        f2 = _f2;
        f3 = _f3;

        b0 = _b0;
        b1 = _b1;
        b2 = _b2;
        b3 = _b3;

        startAngle = _angle;
    }
}

public class Point {
    public float x, y;

    public Point(float _x, float _y) {
        x = _x;
        y = _y;
    }

    public Hex ToHex(Layout l) {
        Point pt = new Point((x - l.origin.x) / l.size.x, (y - l.origin.y) / l.size.y);

        float q = l.orientation.b0 * pt.x + l.orientation.b1 * pt.y;
        float r = l.orientation.b2 * pt.x + l.orientation.b3 * pt.y;

        return new Hex(q, r, -q - r);
    }
}

public class Layout {

    Orientation pointyOrientation = new Orientation(Mathf.Sqrt(3.0f), Mathf.Sqrt(3.0f) / 2f, 0f, 3f / 2f,
                                                    Mathf.Sqrt(3.0f) / 3f, -1f / 3f, 0f, 2f / 3f,
                                                    0.5f);

    Orientation flatOrientation = new Orientation(3f / 2f, 0f, Mathf.Sqrt(3.0f) / 2f, Mathf.Sqrt(3.0f),
                                                  2f / 3f, 0f, -1f / 3f, Mathf.Sqrt(3.0f) / 3f,
                                                  0f);

    public Orientation orientation;
    public Point size;
    public Point origin;

    public Layout(bool _pointyOrientation, Point _size, Point _origin) {
        orientation = _pointyOrientation ? pointyOrientation : flatOrientation;
        size = _size;
        origin = _origin;
    }

    public Point Corner(int corner) {
        float angle = 2.0f * Mathf.PI * (orientation.startAngle + corner) / 6;
        return new Point(size.x * Mathf.Cos(angle), size.y * Mathf.Sin(angle));
    }

    public Point[] PolygonCorners(Hex h) {
        const int cornersCount = 6;

        Point[] corners = new Point[cornersCount];
        Point center = h.ToPoint(this);

        for (int i = 0; i < cornersCount; i++) {
            Point corner = Corner(i);
            corners[i] = new Point(center.x + corner.x, center.y + corner.y);
        }

        return corners;
    }
}
