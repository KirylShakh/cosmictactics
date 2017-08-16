using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorManager : MonoBehaviour {

	public float speed = 10f;
	public float controlDistance = 100f;

    public Unit sphereUnit;
    public Unit cubeUnit;

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

        HandleHighlighting();

        if (Input.GetMouseButtonDown(0)) {
            Select();
            CheckDoubleClick();
        }
        if (Input.GetButton("Fire2")) {
            AttemptUnitMove();
        }

        if (Input.GetKey("f")) {
            SpawnSphereUnit();
        }
        else if (Input.GetKey("g")) {
            SpawnCubeUnit();
        }
    }

    private void HandleHighlighting() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if ((Physics.Raycast(ray, out hit, controlDistance) && hit.transform.gameObject.CompareTag("Unit")) ||
            (Physics.Raycast(ray, out hit, controlDistance, LayerMask.GetMask("Floor")))) {

            HexGrid grid = FindGrid();
            if (grid) {
                grid.HighlightCell(new Point(hit.point.x, hit.point.z));
            }
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

    private void Select() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if ((Physics.Raycast(ray, out hit, controlDistance) && hit.transform.gameObject.CompareTag("Unit")) ||
            (Physics.Raycast(ray, out hit, controlDistance, LayerMask.GetMask("Floor")))) {

            HexGrid grid = FindGrid();
            if (grid) {
                grid.SelectCell(new Point(hit.point.x, hit.point.z));
            }
        }
    }

    private void SpawnSphereUnit() {
        SpawnUnit(sphereUnit);
    }

    private void SpawnCubeUnit() {
        SpawnUnit(cubeUnit);
    }

    private void SpawnUnit(Unit unit) {
        HexGrid grid = FindGrid();
        if (grid && grid.selectedCell && !grid.selectedCell.occupied) {
            Vector3 pos = grid.selectedCell.transform.position;
            Unit spawnedUnit = Instantiate(unit, new Vector3(pos.x, pos.y + 0.5f, pos.z), Quaternion.identity);
            grid.selectedCell.Occupy(spawnedUnit);
        }
    }

    private void AttemptUnitMove() {
        HexGrid grid = FindGrid();
        if (grid && grid.selectedCell && grid.selectedCell.occupied) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, controlDistance, LayerMask.GetMask("Floor"))) {
                grid.MoveSelectedUnitTo(new Point(hit.point.x, hit.point.z));
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
