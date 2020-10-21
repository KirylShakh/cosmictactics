using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


abstract public class Unit : MonoBehaviour
{
    abstract public string Name { get; }
    abstract public float centerHeight { get; }
    abstract public Stats stats { get; }
    private Text statsUI;

    public List<BaseUnitStatus> activeStatuses;

    public bool isMoving = false;
    public float velocityMultiplier = 10.0f;
    private Vector3 velocity;
    private List<HexCell> movePath;
    private int pathCellIndex = 0;
    private float movePrecision = 0.2f;

    public Hex hex;

    protected Rigidbody rb;
    protected Renderer rd;

    public bool canAct;
    public bool canBeActivated;

    public int team;
    protected Material teamMaterial;

    public delegate void UnitDestroyed(Unit unit);
    public event UnitDestroyed UnitDestroyedEvent;

    // Use this for initialization
    void Start()
    {
        activeStatuses = new List<BaseUnitStatus>();
	}

    // Update is called once per frame
    void Update() {}

    protected void FixedUpdate()
    {
        if (isMoving)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
    }

    public virtual void RoundEnds() => canBeActivated = false;

    public virtual void RoundStarts() => canAct = true;

    public virtual void OnActivate() => canBeActivated = true;

    public virtual void OnDeactivate() => canBeActivated = false;

    protected void Init() {}

    public virtual void Setup(HexCell cell)
    {
        canAct = true;
        hex = cell.hex;
    }

    public void MoveTo(HexCell cell)
    {
        Vector3 cellPos = cell.transform.position;
        transform.position = new Vector3(cellPos.x, cellPos.y + centerHeight, cellPos.z);
        hex = cell.hex;
    }

    public virtual void ActOn(Unit unit)
    {
        unit.UnitDestroyedEvent(unit);
        Destroy(unit.gameObject, 0.0f);

        canAct = false;
    }

    public void ApplyStatus(BaseUnitStatus status)
    {
        Stats modStats = status.StatsEffects();

        if (modStats.move != -1) {}
    }

    public virtual void MoveAlong(List<HexCell> path)
    {
        isMoving = true;
        movePath = path;
        pathCellIndex = 1;

        foreach (HexCell step in path)
        {
            step.Occupy(this);
        }
        movePath[0].UnitLeaves();

        canAct = false;
    }

    protected void RecalculateMovement()
    {
        if (isMoving) 
        {
            if ((pathCellIndex >= movePath.Count - 1) && IsNearCell(movePath[pathCellIndex]))
            {
                MoveTo(movePath[pathCellIndex]);
                isMoving = false;
            }
            else
            {
                if (IsNearCell(movePath[pathCellIndex]))
                {
                    movePath[pathCellIndex].UnitLeaves();
                    pathCellIndex++;
                }

                velocity = DirectionTo(movePath[pathCellIndex]).normalized * velocityMultiplier;
            }
        }
    }

    protected Vector3 DirectionTo(HexCell cell)
    {
        Vector3 hexedPosition = new Vector3(transform.position.x, transform.position.y - centerHeight, transform.position.z);
        return cell.transform.position - hexedPosition;
    }

    protected bool IsNearCell(HexCell cell) => DirectionTo(cell).magnitude <= movePrecision;

    public void ShowStats()
    {
        Text _statsUI = FindStatsUIComponent();
        if (_statsUI)
        {
            _statsUI.text = Name;
            _statsUI.enabled = true;
        }
         
    }

    public void HideStats()
    {
        Text _statsUI = FindStatsUIComponent();
        if (_statsUI)
        {
            _statsUI.enabled = false;
        }
    }

    public void SetTeam(int _team, Material material) {
        team = _team;
        teamMaterial = material;
    }

    private Text FindStatsUIComponent()
    {
        if (!statsUI)
        {
            statsUI = GameObject.FindGameObjectWithTag("Name text")?.GetComponent<Text>();
        }
        return statsUI;
    }
}

public class Stats
{
    public int move;
    public int initiative;

    public Stats()
    {
        move = -1;
        initiative = -1;
    }

    public Stats(int _move, int _initiative)
    {
        move = _move;
        initiative = _initiative;
    }
}