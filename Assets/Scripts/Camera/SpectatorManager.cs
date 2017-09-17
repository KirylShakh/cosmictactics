using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorManager : MonoBehaviour {

	public float speed = 10f;
	public float controlDistance = 100f;

    public Unit sphereUnit;
    public Unit cubeUnit;
    public Unit sphericalFlyerUnit;

    private Rigidbody rb;
	private Vector3 velocity;

    private float delta = 0.5f;
    private float lastClickTime = -10f;

    private HexGrid hexGrid;

    void Start() {
		rb = GetComponent<Rigidbody>();
	}

	void Update() {
        HandleInput();
    }

	void FixedUpdate() {
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
	}

    private void HandleInput() {
        velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * speed;

        HexGrid grid = FindGrid();
        HexCell highlightedCell = FindHighlightedCell();
        HandleHighlighting(grid, highlightedCell);

        if (Input.GetMouseButtonDown(0)) {
            Select(grid, highlightedCell);
            CheckDoubleClick();
        }
        else if (Input.GetButton("Fire2")) {
            Act(grid, highlightedCell);
        }

        if (Input.GetKey("f")) {
            SpawnUnit(sphereUnit);
        }
        else if (Input.GetKey("g")) {
            SpawnUnit(cubeUnit);
        }
        else if (Input.GetKey("v")) {
            SpawnUnit(sphericalFlyerUnit);
        }
    }

    private HexCell FindHighlightedCell() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if ((Physics.Raycast(ray, out hit, controlDistance) && hit.transform.gameObject.CompareTag("Unit")) ||
            (Physics.Raycast(ray, out hit, controlDistance, LayerMask.GetMask("Floor")))) {

            HexGrid grid = FindGrid();
            if (grid) {
                return grid.FindCell(new Point(hit.point.x, hit.point.z));
            }
        }

        return null;
    }

    private void HandleHighlighting(HexGrid grid, HexCell cell) {
        if (cell && grid.highlightedCell != cell) {
            grid.HighlightCell(cell);
        }
    }

    private void CheckDoubleClick() {
        float timeDelta = Time.time - lastClickTime;

        if (timeDelta < delta) {
            Jump();
            lastClickTime = 0f;
        }
        else {
            lastClickTime = Time.time;
        }
    }

	private void Jump() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, controlDistance)) {
            if (hit.transform.gameObject.CompareTag("Unit")) {
                rb.MovePosition(hit.transform.position);
            }
        }
	}

    private void Select(HexGrid grid, HexCell cell) {
        if (cell && grid.selectedCell != cell) {
            grid.SelectCell(cell);
        }
    }

    private void SpawnUnit(Unit unit) {
        HexGrid grid = FindGrid();
        if (grid) {
            grid.Spawn(unit);
        }
    }

    private void Act(HexGrid grid, HexCell targetCell) {
        if (grid && grid.selectedCell && grid.selectedCell.occupied) {
            if (targetCell) {
                if (targetCell.occupied) {
                    targetCell.ResolveActBy(grid.selectedCell.occupier);
                }
                grid.MoveSelectedUnitTo(targetCell);
            }
        }
    }

    private HexGrid FindGrid() {
        if (!hexGrid) {
            GameObject gridGameObject = GameObject.FindWithTag("HexGrid");
            if (gridGameObject) {
                hexGrid = gridGameObject.GetComponent<HexGrid>();
            }
        }
        return hexGrid;
    }
}
