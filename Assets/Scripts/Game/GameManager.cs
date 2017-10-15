using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public Text roundText;
    public Unit sphereUnit;
    public Unit cubeUnit;
    public Unit sphericalFlyerUnit;

    public Material[] teamMaterials;

    public float controlDistance = 100f;
    public float switchingRoundsDelay = 2.0f;

    private HexGrid hexGrid;
    private List<Unit> units1;
    private Units units;
    private int round;
    private bool switchingRounds;
    private float switchingRoundsDeltaTime;

    private int[] teams = { 0, 1 };
    private int[] initiatives = { 0, 1, 2 };

    // Use this for initialization
    void Start() {
        units1 = new List<Unit>();

        units = new Units();
        round = 0;
        PlayRoundZero();
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

    private void SpawnUnit(Unit unitClass, int team = 0) {
        HexGrid grid = FindGrid();
        if (grid) {
            Unit unit = grid.Spawn(unitClass);

            if (unit) {
                units.Add(team, unit);
                unit.SetTeam(team, teamMaterials[team]);
            }
        }
    }

    private void SpawnUnitAt(Unit unitClass, Hex hex, int team = 0) {
        HexGrid grid = FindGrid();
        if (grid) {
            Unit unit = grid.SpawnAt(unitClass, hex);

            if (unit) {
                units.Add(team, unit);
                unit.SetTeam(team, teamMaterials[team]);
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

    private void PlayRoundZero() {
        // spawn blue team
        SpawnUnitAt(sphereUnit, new Hex(-3, 0), 0);
        SpawnUnitAt(sphericalFlyerUnit, new Hex(-3, 2), 0);
        SpawnUnitAt(cubeUnit, new Hex(-5, 2), 0);

        // spawn red team
        SpawnUnitAt(sphereUnit, new Hex(3, 0), 1);
        SpawnUnitAt(sphericalFlyerUnit, new Hex(3, 2), 1);
        SpawnUnitAt(cubeUnit, new Hex(5, 2), 1);

        EndRound();
    }

    private void EndRound() {
        switchingRounds = true;

        foreach (Unit unit in units1) {
            unit.RoundEnds();
        }

        round++;
        roundText.text = "Round " + round.ToString();
        roundText.enabled = true;

        switchingRoundsDeltaTime = 0.0f;
    }

    private void StartRound() {
        foreach (Unit unit in units1) {
            unit.RoundStarts();
        }

        roundText.enabled = false;
        switchingRounds = false;
    }
}

public class Units {

    private IDictionary<int, IDictionary<int, List<Unit> > > units;

    public Units() {
        units = new Dictionary<int, IDictionary<int, List<Unit> > >();
    }

    public void Add(int team, Unit unit) {
        if (!units.ContainsKey(team)) {
            units.Add(team, new Dictionary<int, List<Unit> >());
        }

        int initiative = unit.stats.initiative;
        if (!units[team].ContainsKey(initiative)) {
            units[team].Add(initiative, new List<Unit>());
        }

        if (!units[team][initiative].Contains(unit)) {
            units[team][initiative].Add(unit);
        }
    }
}