using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

abstract public class Unit : MonoBehaviour {

    abstract public string Name { get; }
    abstract public float centerHeight { get; }
    private Text statsComponent;

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

    public virtual void ActOn(Unit unit) {
        Destroy(unit.gameObject, 0.0f);
    }

    public void ShowStats() {
        Text stats = FindStatsComponent();
        if (stats)
        { 
            stats.text = Name;
            stats.enabled = true;
        }
         
    }

    public void HideStats() {
        Text stats = FindStatsComponent();
        if (stats)
        {
            stats.enabled = false;
        }
    }

    private Text FindStatsComponent() {
        if (!statsComponent)
        {
            GameObject stats = GameObject.FindGameObjectWithTag("Name text");
            if (stats)
            {
                statsComponent = stats.GetComponent<Text>();
            }
        }
        return statsComponent;
    }
}
