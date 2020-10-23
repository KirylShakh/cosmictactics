using UnityEngine;


public class SphericalFlyerUnit : Unit
{
    public override string Name
    {
        get { return "SphericalFlyerUnit Name"; }
    }

    private Stats _stats;
    public override Stats stats
    {
        get { return _stats; }
    }

    public override void Setup(HexCell cell)
    {
        base.Setup(cell);
        _stats = new Stats(13, 0);

        needChangeDirectionDuringMove = true;
    }
}
