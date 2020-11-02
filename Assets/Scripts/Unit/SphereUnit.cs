using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SphereUnit : Unit
{
    public override string Name
    {
        get { return "Sphere"; }
    }

    private Stats _stats;
    public override Stats stats
    {
        get { return _stats; }
    }

    public override void Setup(HexCell cell)
    {
        base.Setup(cell);
        _stats = new Stats(4, 1);
    }
}
