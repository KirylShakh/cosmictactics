﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeUnit : Unit {

    public override float centerHeight {
        get { return 0.5f; }
    }

    public override string Name {
        get { return "Cube Name"; }
    }

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    protected void Update () {
        RecalculateMovement();
	}
}
