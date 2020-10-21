using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    // texts
    public Text roundText;
    public Text roundPhaseText;

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

    private HexGrid Grid { get; set; }

    // collection for units that were spawned on current map
    private Units units;

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
    private bool fightEnded;

    // teams ids
    private readonly int[] teams = { 0, 1 };
    // units initiative classes, round is divided into phases, units take turns from both teams according to their initiatives
    private readonly int[] initiatives = { 0, 1, 2 };

    void Start()
    {
        units = new Units();
        fightEnded = false;
        Grid = GameObject.FindWithTag("HexGrid")?.GetComponent<HexGrid>();
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
        if (switchingRounds || fightEnded) return;

        HexCell highlightedCell = FindHighlightedCell();
        HandleHighlighting(highlightedCell);
        
        if (Input.GetMouseButtonDown(0))
        {
            Select(highlightedCell);
        }
        else if (Input.GetButton("Fire2"))
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
        if (!targetCell) return;

        if  (Grid.selectedCell &&
            Grid.selectedCell.occupied &&
            !Grid.selectedCell.occupier.isMoving &&
            Grid.selectedCell.occupier.canAct &&
            units[actingTeam, roundPhase].Contains(Grid.selectedCell.occupier)) 
        {
            bool acted = false;

            if (targetCell.occupied &&
                !targetCell.occupier.isMoving &&
                (targetCell.hex.DistanceTo(Grid.selectedCell.hex) <= Grid.selectedCell.occupier.stats.move))
            {
                acted = targetCell.ResolveActBy(Grid.selectedCell.occupier);
            }
            bool moved = Grid.MoveSelectedUnitTo(targetCell);

            if (acted || moved)
            {
                PassToNextTeam();
            }
        }
    }

    private void PlayRoundZero()
    {
        // init constants        
        round = 0;

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
            roundText.text = "Fight Ended";
            roundText.enabled = true;
            return true;
        }
        return false;
    }

    private void NextRound()
    {
        switchingRounds = true;

        round++;
        roundText.text = "Round " + round.ToString();
        roundText.enabled = true;

        switchingRoundsDeltaTime = 0.0f;
    }

    private void StartRound()
    {
        units.Map("RoundStarts");

        roundText.enabled = false;
        switchingRounds = false;

        roundPhase = 0;
        ResetActedTeams();

        UpdateRoundText();
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
            UpdateRoundText();
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

        UpdateRoundText();
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

    private void UpdateRoundText()
    {
        string team = actingTeam == 0 ? "Blue(0)" : "Red(1)";
        roundPhaseText.color = actingTeam == 0 ? Color.blue : Color.red;

        string roundPhaseStr = roundPhase == 0 ? "Light(0)" : (roundPhase == 1 ? "Medium(1)" : "Heavy(2)");
        roundPhaseText.text = $"Round {round}. Phase {roundPhaseStr}. Acting team {team}";
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
    }
}