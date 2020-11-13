using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    // UI
    public HUDPanelManager HUDManager;

    // base classes for units that can be spawn in this game
    public Unit sphereUnit;
    public Unit cubeUnit;
    public Unit sphericalFlyerUnit;

    // now is used to distinguish teams from each other
    public Material[] teamMaterials;

    // distance from camera to objects that can be targetable, clickable
    public float controlDistance = 100f;
    // delay between rounds
    public float switchingRoundsDelay = 2.0f;

    public HexGrid Grid;

    // collection for units that were spawned on current map
    private Units units = new Units();

    // current round
    private int round;
    // current round phase as per initiative (see below)
    private int roundPhase;
    private int actingTeam;

    // flag to stop the player controls when switch round procedure is in effect
    private bool switchingRounds;
    // delta for delay between rounds
    private float switchingRoundsDeltaTime;
    // flag when map conflict is resolved
    private bool fightEnded = false;

    // teams ids
    private readonly int[] teams = { 0, 1 };
    // units initiative classes, round is divided into phases, units take turns from both teams according to their initiatives
    private readonly int[] initiatives = { 0, 1, 2 };

    private bool actionInProcess = false;
    private bool paused = true;

    void Start()
    {
        HandleMainMenu();
        // init constants, spawn armies, start first round
        PlayRoundZero();
    }

    void Update()
    {
        HandleInput();

        if (switchingRounds)
        {
            switchingRoundsDeltaTime += Time.deltaTime;
            if (switchingRoundsDeltaTime >= switchingRoundsDelay)
            {
                StartRound();
            }
        }
    }

    private void HandleInput()
    {
        // Opening menu should be always possible
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleMainMenu();
        }

        // If game paused (menu is opened) allow nothing more
        if (paused) return;

        // Highligtning cell should be possible always if menu is not opened
        HexCell highlightedCell = FindHighlightedCell();
        HandleHighlighting(highlightedCell);

        // If switching round pause is in effect - process no more events
        if (switchingRounds) return;

        // Selection of the cell should be possible if no menu is opened or change round is in effect
        if (Input.GetMouseButtonDown(0))
        {
            Select(highlightedCell);
        }

        // If fight has ended - dont allow any more actions
        if (fightEnded) return;

        if (Input.GetButton("Fire2"))
        {
            Act(highlightedCell);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            SpawnUnit(sphereUnit, actingTeam);
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            SpawnUnit(cubeUnit, actingTeam);
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            SpawnUnit(sphericalFlyerUnit, actingTeam);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!switchingRounds)
            {
                EndRound();
                Grid.ClearHighlighting();
            }
        }
    }

    private void HandleMainMenu()
    {
        paused = !paused;
        HUDManager.ToggleMainMenu(paused);
        Time.timeScale = paused ? 0.0f : 1.0f;
    }

    private HexCell FindHighlightedCell()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if ((Physics.Raycast(ray, out hit, controlDistance) && hit.transform.gameObject.CompareTag("Unit")) ||
            (Physics.Raycast(ray, out hit, controlDistance, LayerMask.GetMask("Floor"))))
        {
            return Grid.FindCell(new Point(hit.point.x, hit.point.z));
        }

        return null;
    }

    private void HandleHighlighting(HexCell cell)
    {
        if (cell && Grid.highlightedCell != cell)
        {
            Grid.HighlightCell(cell);
        }
    }

    private void SpawnUnit(Unit unitClass, int team = 0) => SpawnUnitAt(unitClass, null, team);

    private void SpawnUnitAt(Unit unitClass, Hex hex, int team = 0)
    {
        Unit unit = hex is null ? Grid.Spawn(unitClass) : Grid.SpawnAt(unitClass, hex);
        if (unit)
        {
            units.Add(team, unit);
            unit.SetTeam(team, teamMaterials[team]);

            if (roundPhase == unit.stats.initiative && actingTeam == team)
            {
                unit.OnActivate();
            }
        }
    }

    private void Select(HexCell cell)
    {
        if (cell && Grid.selectedCell != cell)
        {
            Grid.SelectCell(cell);
        }
    }

    private void Act(HexCell targetCell)
    {
        if (actionInProcess || !targetCell || !IsThereValidActor()) return;

        var actingUnit = Grid.selectedCell.occupier;
        var pathToTarget = Grid.GetPathToTarget(targetCell);

        if (IsThereValidTarget(targetCell, pathToTarget) || IsThereValidMovePath(targetCell, pathToTarget))
        {
            actingUnit.MoveAction(pathToTarget);
            actionInProcess = true;
        }

        if (actionInProcess)
        {
            actingUnit.ActionFinishedEvent += OnActionFinished;
        }
    }

    private bool IsThereValidActor()
    {
        return Grid.selectedCell &&
                Grid.selectedCell.occupied &&
                !Grid.selectedCell.occupier.IsMoving &&
                Grid.selectedCell.occupier.canAct &&
                units[actingTeam, roundPhase].Contains(Grid.selectedCell.occupier);
    }

    private bool IsThereValidTarget(HexCell targetCell, List<HexCell> pathToTarget)
    {
        return targetCell != Grid.selectedCell &&
                targetCell.occupied &&
                !targetCell.occupier.IsMoving &&
                pathToTarget.Last() == targetCell &&
                targetCell.occupier != Grid.selectedCell.occupier;
    }

    private bool IsThereValidMovePath(HexCell targetCell, List<HexCell> pathToTarget)
    {
        return targetCell != Grid.selectedCell &&
                !targetCell.occupied &&
                pathToTarget.Count > 0;
    }

    private void OnActionFinished(Unit unit)
    {
        actionInProcess = false;
        unit.ActionFinishedEvent -= OnActionFinished;

        Grid.SelectCell(unit.hex);
        PassToNextTeam();
    }

    private void PlayRoundZero()
    {
        // init constants
        round = 0;

        StartCoroutine(SpawnTestUnits());
    }

    private IEnumerator SpawnTestUnits()
    {
        // spawn blue team
        SpawnUnitAt(sphereUnit, new Hex(-3, 0), 0);
        yield return new WaitForSeconds(0.5f);
        SpawnUnitAt(sphericalFlyerUnit, new Hex(-3, 2), 0);
        yield return new WaitForSeconds(0.5f);
        SpawnUnitAt(cubeUnit, new Hex(-5, 2), 0);
        yield return new WaitForSeconds(0.5f);

        // spawn red team
        SpawnUnitAt(sphereUnit, new Hex(3, 0), 1);
        yield return new WaitForSeconds(0.5f);
        SpawnUnitAt(sphericalFlyerUnit, new Hex(3, 2), 1);
        yield return new WaitForSeconds(0.5f);
        SpawnUnitAt(cubeUnit, new Hex(5, 2), 1);
        yield return new WaitForSeconds(0.5f);

        EndRound();
    }

    private void EndRound()
    {
        units.Map("RoundEnds");

        if (!FightEnded())
        {
            NextRound();
        }
    }

    private bool FightEnded()
    {
        if (units.OnlySingleTeamRemains() || units.NoTeamsRemains())
        {
            int winningTeam = units.NoTeamsRemains() ? -1 : units.FirstTeam();
            HUDManager.FightEnded(winningTeam);
            return true;
        }
        return false;
    }

    private void NextRound()
    {
        switchingRounds = true;

        round++;
        HUDManager.NextRound(round);

        switchingRoundsDeltaTime = 0.0f;
    }

    private void StartRound()
    {
        units.Map("RoundStarts");

        HUDManager.StartRound();
        switchingRounds = false;

        roundPhase = 0;
        ResetActedTeams();

        HUDManager.UpdatePhaseInfo(actingTeam, round, roundPhase);
    }

    private void NextPhase()
    {
        roundPhase++;
        if (roundPhase >= initiatives.Length)
        {
            EndRound();
        }
        else
        {
            ResetActedTeams();
            if (AllTeamsActed())
            {
                NextPhase();
            }
            HUDManager.UpdatePhaseInfo(actingTeam, round, roundPhase);
        }
    }

    private void ResetActedTeams()
    {
        actingTeam = 0;
        units.Map(actingTeam, roundPhase, "OnActivate");
        Grid.ManageHighlightActivated(units[actingTeam, roundPhase]);

        if (units.AllActed(actingTeam, roundPhase))
        {
            PassToNextTeam();
        }
    }

    private void PassToNextTeam()
    {
        if (AllTeamsActed())
        {
            NextPhase();
            return;
        }

        units.Map(actingTeam, roundPhase, "OnDeactivate");
        Grid.ManageHighlightActivated(units[actingTeam, roundPhase]);

        NextTeam();
        while (units.AllActed(actingTeam, roundPhase))
        {
            NextTeam();
        }

        units.Map(actingTeam, roundPhase, "OnActivate");
        Grid.ManageHighlightActivated(units[actingTeam, roundPhase]);

        HUDManager.UpdatePhaseInfo(actingTeam, round, roundPhase);
    }

    private void NextTeam() => actingTeam = (actingTeam + 1 < teams.Length) ? actingTeam + 1 : 0;

    private bool AllTeamsActed()
    {
        foreach (int team in teams)
        {
            if (!units.AllActed(team, roundPhase)) return false;
        }
        return true;
    }
}


public class Units
{
    private readonly IDictionary<int, IDictionary<int, List<Unit>>> units = new Dictionary<int, IDictionary<int, List<Unit>>>();

    public void Add(int team, Unit unit)
    {
        if (!units.ContainsKey(team))
        {
            units.Add(team, new Dictionary<int, List<Unit>>());
        }

        int initiative = unit.stats.initiative;
        if (!units[team].ContainsKey(initiative))
        {
            units[team].Add(initiative, new List<Unit>());
        }

        if (!units[team][initiative].Contains(unit))
        {
            units[team][initiative].Add(unit);
        }

        unit.UnitDestroyedEvent += OnUnitDestroyed;
    }

    public void Map(string f)
    {
        foreach (KeyValuePair<int, IDictionary<int, List<Unit>>> teamUnits in units)
        {
            foreach (KeyValuePair<int, List<Unit>> initiativeUnits in teamUnits.Value)
            {
                foreach (Unit unit in initiativeUnits.Value)
                {
                    unit.GetType().GetMethod(f).Invoke(unit, new object[] { });
                }
            }
        }
    }

    public void Map(int team, int initiative, string f)
    {
        if (!units.ContainsKey(team) || !units[team].ContainsKey(initiative)) return;

        foreach (Unit unit in units[team][initiative])
        {
            unit.GetType().GetMethod(f).Invoke(unit, new object[] { });
        }
    }

    public bool AllActed(int team, int initiative)
    {
        foreach (Unit unit in this[team, initiative])
        {
            if (unit.canAct) return false;
        }
        return true;
    }

    // t - team, i - initiative
    public List<Unit> this[int t, int i] => (units.ContainsKey(t) && units[t].ContainsKey(i)) ? units[t][i] : new List<Unit>();

    public bool OnlySingleTeamRemains() => units.Keys.Count == 1;

    public bool NoTeamsRemains() => units.Keys.Count == 0;

    public int FirstTeam() => units.Keys.First();

    private void OnUnitDestroyed(Unit unit)
    {
        int team = unit.team;
        int initiative = unit.stats.initiative;

        units[team][initiative].Remove(unit);

        if (units[team][initiative].Count == 0)
        {
            units[team].Remove(initiative);
        }
        if (units[team].Count == 0)
        {
            units.Remove(team);
        }

        unit.UnitDestroyedEvent -= OnUnitDestroyed;
    }
}
