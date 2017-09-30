using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public Text roundText;
    public Unit sphereUnit;
    public Unit cubeUnit;
    public Unit sphericalFlyerUnit;

    public float controlDistance = 100f;
    public float switchingRoundsDelay = 2.0f;

    private HexGrid hexGrid;
    private List<Unit> units;
    private int round;
    private bool switchingRounds;
    private float switchingRoundsDeltaTime;

    // Use this for initialization
    void Start() {
        units = new List<Unit>();
        round = 0;
        EndRound();
    }
	
    // Update is called once per frame
    void Update() {
        HandleInput();

        if (switchingRounds) {
            switchingRoundsDeltaTime += Time.deltaTime;
            if (switchingRoundsDeltaTime >= switchingRoundsDelay) {
                StartRound();
            }
        }
    }

    private void HandleInput() {

        if (switchingRounds) return;

        HexGrid grid = FindGrid();
        HexCell highlightedCell = FindHighlightedCell();
        HandleHighlighting(grid, highlightedCell);
        
        if (Input.GetMouseButtonDown(0)) {
            Select(grid, highlightedCell);
        }
        else if (Input.GetButton("Fire2")) {
            Act(grid, highlightedCell);
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            SpawnUnit(sphereUnit);
        }
        else if (Input.GetKeyDown(KeyCode.G)) {
            SpawnUnit(cubeUnit);
        }
        else if (Input.GetKeyDown(KeyCode.V)) {
            SpawnUnit(sphericalFlyerUnit);
        }
        else if (Input.GetKeyDown(KeyCode.Return)) {
            if (!switchingRounds) {
                EndRound();
                grid.ClearHighlighting();
            }
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

    private void SpawnUnit(Unit unit) {
        HexGrid grid = FindGrid();
        if (grid) {
            grid.Spawn(unit);
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

    private void Select(HexGrid grid, HexCell cell) {
        if (cell && grid.selectedCell != cell) {
            grid.SelectCell(cell);
        }
    }

    private void Act(HexGrid grid, HexCell targetCell) {
        if (grid && grid.selectedCell && grid.selectedCell.occupied && !grid.selectedCell.occupier.isMoving) {
            if (targetCell) {
                if (targetCell.occupied && !targetCell.occupier.isMoving) {
                    targetCell.ResolveActBy(grid.selectedCell.occupier);
                }
                grid.MoveSelectedUnitTo(targetCell);
            }
        }
    }

    private void EndRound() {
        switchingRounds = true;

        foreach (Unit unit in units) {
            unit.RoundEnds();
        }

        round++;
        roundText.text = "Round " + round.ToString();
        roundText.enabled = true;

        switchingRoundsDeltaTime = 0.0f;
    }

    private void StartRound() {
        foreach (Unit unit in units) {
            unit.RoundStarts();
        }

        roundText.enabled = false;
        switchingRounds = false;
    }
}
