using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereUnit : Unit {

    public override float centerHeight {
        get { return 0.5f; }
    }

    public override string Name {
        get { return "Sphere Name"; }
    }

    private Stats _stats;
    public override Stats stats {
        get { return _stats; }
    }

    void Start () {
        rb = GetComponent<Rigidbody>();
        rd = GetComponent<Renderer>();

        rd.material = teamMaterial;
    }
	
    // Update is called once per frame
    protected void Update () {
        RecalculateMovement();
    }

    public override void Setup(HexCell cell) {
        base.Setup(cell);
        _stats = new Stats(4, 1);
    }
}
