using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

abstract public class Unit : MonoBehaviour {

    abstract public string Name { get; }
    abstract public float centerHeight { get; }
    private Text statsComponent;

    public bool isMoving = false;
    public float velocityMultiplier = 10.0f;
    private Vector3 velocity;
    private List<HexCell> movePath;
    private int pathCellIndex = 0;
    private float movePrecision = 0.2f;

    protected Rigidbody rb;

    // Use this for initialization
    void Start() {

	}

    // Update is called once per frame
    void Update() {
		
	}

    protected void FixedUpdate() {
        if (isMoving) {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
    }

    public void MoveTo(HexCell cell) {
        Vector3 cellPos = cell.transform.position;
        transform.position = new Vector3(cellPos.x, cellPos.y + centerHeight, cellPos.z);
    }

    public virtual void ActOn(Unit unit) {
        Destroy(unit.gameObject, 0.0f);
    }

    public virtual void MoveAlong(List<HexCell> path) {
        isMoving = true;
        movePath = path;
        pathCellIndex = 1;

        movePath[0].UnitLeaves();
        movePath[pathCellIndex].Occupy(this);
    }

    protected void RecalculateMovement() {
        if (isMoving) {
            if ((pathCellIndex >= movePath.Count - 1) && isNearCell(movePath[pathCellIndex])) {
                MoveTo(movePath[pathCellIndex]);
                isMoving = false;
            }
            else {
                if (isNearCell(movePath[pathCellIndex])) {
                    movePath[pathCellIndex].UnitLeaves();
                    pathCellIndex++;
                    movePath[pathCellIndex].Occupy(this);
                }

                velocity = DirectionTo(movePath[pathCellIndex]).normalized * velocityMultiplier;
            }
        }
    }

    protected Vector3 DirectionTo(HexCell cell) {
        Vector3 hexedPosition = new Vector3(transform.position.x, transform.position.y - centerHeight, transform.position.z);
        return cell.transform.position - hexedPosition;
    }

    protected bool isNearCell(HexCell cell) {
        return DirectionTo(cell).magnitude <= movePrecision;
    }

    public void ShowStats() {
        Text stats = FindStatsComponent();
        if (stats) { 
            stats.text = Name;
            stats.enabled = true;
        }
         
    }

    public void HideStats() {
        Text stats = FindStatsComponent();
        if (stats) {
            stats.enabled = false;
        }
    }

    private Text FindStatsComponent() {
        if (!statsComponent) {
            GameObject stats = GameObject.FindGameObjectWithTag("Name text");
            if (stats) {
                statsComponent = stats.GetComponent<Text>();
            }
        }
        return statsComponent;
    }
}
