using UnityEngine;


public class SphericalFlyerUnit : Unit
{
    public override string Name
    {
        get { return "Spherical Flyer"; }
    }

    private Stats _stats;
    public override Stats stats
    {
        get { return _stats; }
    }

    public override void Setup(HexCell cell)
    {
        base.Setup(cell);
        _stats = new Stats(6, 0);

        needChangeDirectionDuringMove = true;
    }
}
