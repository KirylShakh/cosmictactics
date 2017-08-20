using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Unit : MonoBehaviour {

    abstract public float centerHeight { get; }

	// Use this for initialization
	void Start() {
		
	}
	
	// Update is called once per frame
	void Update() {
		
	}

    public void MoveTo(HexCell cell) {
        Vector3 cellPos = cell.transform.position;
        transform.position = new Vector3(cellPos.x, cellPos.y + centerHeight, cellPos.z);
    }
}
