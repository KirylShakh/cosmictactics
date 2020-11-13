using System.Collections.Generic;
using System.Linq;
using UnityEngine;


abstract public class Unit : MonoBehaviour
{
    abstract public string Name { get; }
    abstract public Stats stats { get; }

    public List<BaseUnitStatus> activeStatuses;

    public bool IsMoving = false;
    public bool IsAttacking = false;
    public bool IsTargetDying = false;

    public float velocity = 10.0f;
    private List<HexCell> movePath;
    private int pathCellIndex = 0;
    private float movePrecision = 0.2f;
    protected bool needChangeDirectionDuringMove = false;

    public Hex hex;

    protected Rigidbody rb;
    protected Renderer rd;
    protected Animator animator;

    public bool canAct;
    public bool canBeActivated;

    public int team;
    protected Material teamMaterial;

    public delegate void UnitDestroyed(Unit unit);
    public event UnitDestroyed UnitDestroyedEvent;

    public delegate void ActionFinished(Unit unit);
    public event ActionFinished ActionFinishedEvent;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rd = GetComponent<Renderer>();
        rd.material = teamMaterial;

        animator = GetComponent<Animator>();

        activeStatuses = new List<BaseUnitStatus>();
	}

    // Update is called once per frame
    void Update()
    {
        if (IsMoving)
        {
            RecalculateMovement();
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

    public void Die(Unit from)
    {
        animator.SetBool("IsDying", true);
    }

    public void ApplyStatus(BaseUnitStatus status)
    {
        Stats modStats = status.StatsEffects();

        if (modStats.move != -1) {}
    }

    // Animation Event Handler for a moment in animation when actual "hit" is played
    public void AnimationStrikeApplied()
    {
        var targetUnit = movePath[pathCellIndex].occupier;
        targetUnit.UnitDestroyedEvent += OnUnitKilled;
        targetUnit.Die(this);

        IsTargetDying = true;
    }

    // Animation Event Handler when attack animation finishes its cycle
    public void AnimationAttackCompleted()
    {
        IsAttacking = false;
        animator.SetBool("IsAttacking", false);
    }

    // Animation Event Handler when dying animation finishes its cycle
    public void AnimationDyingCompleted()
    {
        UnitDestroyedEvent(this);

        Destroy(transform.parent.gameObject, 0.0f);
        Destroy(gameObject, 0.0f);
    }

    private void OnUnitKilled(Unit unit)
    {
        IsTargetDying = false;
        unit.UnitDestroyedEvent -= OnUnitKilled;
    }

    // Method for both "move and attack" and "move" actions
    public void MoveAction(List<HexCell> pathToTarget)
    {
        IsMoving = true;
        animator.SetBool("IsMoving", true);

        movePath = pathToTarget;
        pathCellIndex = 0; // Unit should proceed to attacking if it's target is adjacent to it
        if (!IsReadyToAttack())
        {
            pathCellIndex += 1;
        }

        canAct = false;
    }

    protected void RecalculateMovement()
    {
        if (IsDestinationReached())
        {
            transform.parent.position = movePath[pathCellIndex].transform.position;
            hex = movePath[pathCellIndex].hex;
            OccupyNextCell();

            IsMoving = false;
            animator.SetBool("IsMoving", false);
            ActionFinishedEvent(this);
        }
        else if (IsReadyToAttack())
        {
            if (pathCellIndex != 0)
            {
                OccupyNextCell();
            }
            else
            {
                pathCellIndex += 1;
            }

            IsAttacking = true;
            animator.SetBool("IsAttacking", true);
        }
        else if (IsReadyToMove())
        {
            if (IsNearCell(movePath[pathCellIndex]))
            {
                OccupyNextCell();
            }

            Vector3 cellPos = movePath[pathCellIndex].transform.position;
            transform.parent.position = Vector3.MoveTowards(transform.parent.position, cellPos, velocity * Time.deltaTime);

            if (needChangeDirectionDuringMove)
            {
                transform.parent.LookAt(cellPos);
            }
        }
    }

    protected bool IsDestinationReached() => (pathCellIndex >= movePath.Count - 1) && IsNearCell(movePath[pathCellIndex]);

    protected bool IsReadyToAttack() => IsNearCell(movePath[pathCellIndex]) && movePath[pathCellIndex + 1].occupied;

    protected bool IsReadyToMove() => !IsAttacking && !IsTargetDying;

    protected Vector3 DirectionTo(HexCell cell) => cell.transform.position - transform.parent.position;

    protected bool IsNearCell(HexCell cell) => DirectionTo(cell).magnitude <= movePrecision;

    private void OccupyNextCell()
    {
        movePath[pathCellIndex - 1].UnitLeaves();
        movePath[pathCellIndex].Occupy(this);
        pathCellIndex++;
    }

    public void SetTeam(int _team, Material material) {
        team = _team;
        teamMaterial = material;
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
