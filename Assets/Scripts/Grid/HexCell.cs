using System.Collections.Generic;
using UnityEngine;


public class HexCell : MonoBehaviour
{

    public Layout gridLayout;
    public float floorY = 0.1f;
    public Point point;
    public Hex hex;
    public float cost = 1.0f;

    public bool occupied = false;
    public Unit occupier;

    private LineRenderer lr;
    private Color defaultLRColor;
    private float defaultWidth;
    
    void Start ()
    {
        lr = GetComponent<LineRenderer>();
        defaultLRColor = lr.material.color;
        defaultWidth = lr.widthMultiplier;

        Vector3[] positions = HexCornerVertices();
        lr.positionCount = positions.Length;
        lr.SetPositions(positions);
    }

    void Update () {}

    public void Init(Layout l, Point p, Hex h)
    {
        (gridLayout, point, hex) = (l, p, h);
    }

    public void Highlight()
    {
        lr.material.color = Color.cyan;
        lr.widthMultiplier = defaultWidth * 2.5f;
    }

    public void ClearHighlighting(bool selected = false)
    {
        if (selected)
        {
            Select();
        }
        else if (occupied && occupier.canBeActivated && occupier.canAct && !occupier.isMoving)
        {
            HighlightActivated();
        }
        else
        {
            Unselect();
        }
    }

    public void Select() => (lr.material.color, lr.widthMultiplier) = (Color.green, defaultWidth * 2);

    public void Unselect() => (lr.material.color, lr.widthMultiplier) = (defaultLRColor, defaultWidth);

    public void HighlightActivated() => (lr.material.color, lr.widthMultiplier) = (Color.yellow, defaultWidth * 3);

    public void HighlightActable() => (lr.material.color, lr.widthMultiplier) = (Color.red, defaultWidth * 3);

    public void Occupy(Unit unit)
    {
        occupier = unit;
        occupied = true;
    }

    public void UnitLeaves()
    {
        occupier = null;
        occupied = false;
    }

    public bool ResolveActBy(Unit unit)
    {
        if (occupied && occupier != unit && !occupier.isMoving)
        {
            unit.ActOn(occupier);
            UnitLeaves();
            return true;
        }
        return false;
    }

    private Vector3[] HexCornerVertices()
    {
        var vertices = new List<Vector3>();

        var corners = gridLayout.PolygonCorners(hex);
        for (int i = 0; i < corners.Length; i++)
        {
            vertices.Add(new Vector3(corners[i].x, floorY, corners[i].y));
        }
        vertices.Add(new Vector3(corners[0].x, floorY, corners[0].y)); // loop vertices so that last one is also the first one: more convinient to draw hex

        return vertices.ToArray();
    }
}
