using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public Text roundText;
    public Text roundPhaseText;
    public Unit sphereUnit;
    public Unit cubeUnit;
    public Unit sphericalFlyerUnit;

    public Material[] teamMaterials;

    public float controlDistance = 100f;
    public float switchingRoundsDelay = 2.0f;

    private HexGrid hexGrid;
    private Units units;

    private int round;
    private int roundPhase;
    private int actingTeam;

    private bool switchingRounds;
    private float switchingRoundsDeltaTime;
    private bool fightEnded;

    private int[] teams = { 0, 1 };
    private int[] initiatives = { 0, 1, 2 };

    // Use this for initialization
    void Start() {
        units = new Units();
        fightEnded = false;
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

        if (switchingRounds || fightEnded) return;

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
            SpawnUnit(sphereUnit, actingTeam);
        }
        else if (Input.GetKeyDown(KeyCode.G)) {
            SpawnUnit(cubeUnit, actingTeam);
        }
        else if (Input.GetKeyDown(KeyCode.V)) {
            SpawnUnit(sphericalFlyerUnit, actingTeam);
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

                if (roundPhase == unit.stats.initiative && actingTeam == team) {
                    unit.OnActivate();
                }
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

                if (roundPhase == unit.stats.initiative && actingTeam == team) {
                    unit.OnActivate();
                }
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
        if (grid && grid.selectedCell && grid.selectedCell.occupied && !grid.selectedCell.occupier.isMoving && grid.selectedCell.occupier.canAct && units.Get(actingTeam, roundPhase).Contains(grid.selectedCell.occupier)) {
            if (targetCell) {
                bool acted = false;

                if (targetCell.occupied && !targetCell.occupier.isMoving && (targetCell.hex.DistanceTo(grid.selectedCell.hex) <= grid.selectedCell.occupier.stats.move)) {
                    acted = targetCell.ResolveActBy(grid.selectedCell.occupier);
                }
                bool moved = grid.MoveSelectedUnitTo(targetCell);

                if (acted || moved) {
                    NextTeam();
                }
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
        units.Map("RoundEnds");

        if (!FightEnded()) {
            NextRound();
        }
    }

    private bool FightEnded() {
        if (units.OnlySingleTeamRemains() || units.NoTeamsRemains()) {
            roundText.text = "Fight Ended";
            roundText.enabled = true;
            return true;
        }
        return false;
    }

    private void NextRound() {
        switchingRounds = true;

        round++;
        roundText.text = "Round " + round.ToString();
        roundText.enabled = true;

        switchingRoundsDeltaTime = 0.0f;
    }

    private void StartRound() {
        units.Map("RoundStarts");

        roundText.enabled = false;
        switchingRounds = false;

        roundPhase = 0;
        ResetActedTeams();

        UpdateRoundText();
    }

    private void NextPhase() {
        roundPhase++;
        if (roundPhase >= initiatives.Length) {
            EndRound();
        }
        else {
            ResetActedTeams();
            if (AllTeamsActed()) {
                NextPhase();
            }
            UpdateRoundText();
        }
    }

    private void ResetActedTeams() {
        actingTeam = 0;
        units.Map(actingTeam, roundPhase, "OnActivate");
        FindGrid().ManageHighlightActivated(units.Get(actingTeam, roundPhase));

        if (units.AllActed(actingTeam, roundPhase)) {
            NextTeam();
        }
    }

    private void NextTeam() {
        if (AllTeamsActed()) {
            NextPhase();
            return;
        }

        units.Map(actingTeam, roundPhase, "OnDeactivate");
        FindGrid().ManageHighlightActivated(units.Get(actingTeam, roundPhase));

        actingTeam = NextTeam(actingTeam);
        while (units.AllActed(actingTeam, roundPhase)) {
            actingTeam = NextTeam(actingTeam);
        }

        units.Map(actingTeam, roundPhase, "OnActivate");
        FindGrid().ManageHighlightActivated(units.Get(actingTeam, roundPhase));

        UpdateRoundText();
    }

    private int NextTeam(int team) {
        team++;
        if (team >= teams.Length) {
            team = 0;
        }
        return team;
    }

    private bool AllTeamsActed() {
        foreach (int team in teams) {
            if (!units.AllActed(team, roundPhase)) {
                return false;
            }
        }
        return true;
    }

    private void UpdateRoundText() {
        string team = actingTeam == 0 ? "Blue(0)" : "Red(1)";
        roundPhaseText.color = actingTeam == 0 ? Color.blue : Color.red;

        string roundPhaseStr = roundPhase == 0 ? "Light(0)" : (roundPhase == 1 ? "Medium(1)" : "Heavy(2)");
        roundPhaseText.text = "Round " + round.ToString() + ". Phase " + roundPhaseStr + ". Acting team " + team;
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

        unit.UnitDestroyedEvent += OnUnitDestroyed;
    }

    public void Map(string f) {
        foreach (KeyValuePair<int, IDictionary<int, List<Unit> > > teamUnits in units) {
            foreach (KeyValuePair<int, List<Unit> > initiativeUnits in teamUnits.Value) {
                foreach (Unit unit in initiativeUnits.Value) {
                    unit.GetType().GetMethod(f).Invoke(unit, new object[] { });
                }
            }
        }
    }

    public void Map(int team, int initiative, string f) {
        if (!units.ContainsKey(team) || !units[team].ContainsKey(initiative)) return;

        foreach (Unit unit in units[team][initiative]) {
            unit.GetType().GetMethod(f).Invoke(unit, new object[] { });
        }
    }

    public bool AllActed(int team, int initiative) {
        foreach (Unit unit in Get(team, initiative)) {
            if (unit.canAct) {
                return false;
            }
        }
        return true;
    }

    public List<Unit> Get(int team, int initiative) {
        if (units.ContainsKey(team) && units[team].ContainsKey(initiative)) {
            return units[team][initiative];
        }
        return new List<Unit>();
    }

    public bool OnlySingleTeamRemains() {
        return units.Keys.Count == 1;
    }

    public bool NoTeamsRemains() {
        return units.Keys.Count == 0;
    }

    private void OnUnitDestroyed(Unit unit) {
        int team = unit.team;
        int initiative = unit.stats.initiative;

        units[team][initiative].Remove(unit);

        if (units[team][initiative].Count == 0) {
            units[team].Remove(initiative);
        }
        if (units[team].Count == 0) {
            units.Remove(team);
        }
    }
}