using UnityEngine;


public class CubeUnit : Unit
{
    public override string Name
    {
        get { return "Cube Name"; }
    }

    private Stats _stats;
    public override Stats stats
    {
        get { return _stats; }
    }

    public override void Setup(HexCell cell)
    {
        base.Setup(cell);
        _stats = new Stats(9, 2);
    }
}
