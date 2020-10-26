using System.Collections.Generic;
using UnityEngine;


public class HexGrid : MonoBehaviour
{
    public int gridXSize = 2;
    public int gridYSize = 2;
    public HexCell hexCell;

    public Layout layout = new Layout(true, new Point(1f, 1f), new Point(0f, 0f));
    public IDictionary<string, HexCell> cells = new Dictionary<string, HexCell>();
    public HexCell selectedCell;
    public HexCell highlightedCell;

    private List<HexCell> highlightedPath = new List<HexCell>();

    // Use this for initialization
    void Start()
    {
        Generate();
    }

    // Update is called once per frame
    void Update() {}

    public void HighlightCell(Point p) => HighlightCell(FindCell(p));

    public void HighlightCell(HexCell cell)
    {
        if (!cell) return;

        ClearHighlighting();

        if (IsSomeoneSelected())
        {
            highlightedPath = FindPath(selectedCell, cell);
            HighlightPath();

            if (cell.occupied)
            {
                cell.HighlightActable();
            }
        }
        else
        {
            cell.Highlight();
        }

        highlightedCell = cell;
    }

    private bool IsSomeoneSelected() => selectedCell && selectedCell.occupied;

    private void HighlightPath()
    {
        if (highlightedPath.Count > selectedCell.occupier.stats.move + 1)
        {
            highlightedPath = highlightedPath.GetRange(0, selectedCell.occupier.stats.move + 1);
        }

        foreach (HexCell pathCell in highlightedPath)
        {
            pathCell.Highlight();
        }
    }

    public void SelectCell(Hex hex) => SelectCell(FindCell(hex));

    public void SelectCell(HexCell cell)
    {
        ClearHighlighting();

        if (selectedCell)
        {
            selectedCell.ClearHighlighting();
            if (selectedCell.occupied)
            {
                selectedCell.occupier.HideStats();
            }

        }

        if (cell)
        {
            highlightedPath.Clear();
            selectedCell = cell;
            selectedCell.ClearHighlighting(true);

            if (selectedCell.occupied)
            {
                selectedCell.occupier.ShowStats();
            }
        }
    }

    public void ManageHighlightActivated(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            HexCell cell = FindCell(unit.hex);
            cell?.ClearHighlighting();
        }
    }

    public bool LaunchSelectedUnitAction(HexCell destination)
    {
        if (!destination || destination.occupied || selectedCell.occupier.IsMoving) return false;


        return true;
    }

    public List<HexCell> GetPathToTarget(HexCell destination)
    {
        List<HexCell> path;
        if (highlightedPath.Count < 2 || highlightedPath[highlightedPath.Count - 1] != destination)
        {
            path = FindPath(selectedCell, destination);

            if (path.Count > selectedCell.occupier.stats.move + 1)
            {
                path = path.GetRange(0, selectedCell.occupier.stats.move + 1);
            }
        }
        else
        {
            path = new List<HexCell>(highlightedPath.ToArray());
        }
        return path;
    }

    public HexCell FindCell(Point p) => FindCell(p.ToHex(layout));

    public HexCell FindCell(Hex hex) => cells.ContainsKey(hex.ToString()) ? cells[hex.ToString()] : null;

    public List<HexCell> FindPath(HexCell start, HexCell destination)
    {
        var path = new List<HexCell>();

        var frontier = new PriorityQueue<Hex>();
        frontier.Enqueue(start.hex, 0);

        var cameFrom = new Dictionary<Hex, Hex>
        {
            [start.hex] = null
        };

        var costSoFar = new Dictionary<Hex, float>
        {
            [start.hex] = 0
        };

        while (frontier.Count() != 0)
        {
            var current = frontier.Dequeue();
            if (current == destination.hex) break;

            foreach (var next in current.Neighbours())
            {
                if (!cells.ContainsKey(next.ToString()) || (cells[next.ToString()].occupied && next != destination.hex)) continue;

                float newCost = costSoFar[current] + MovementCost(current, next);
                if (!costSoFar.ContainsKey(next) || (newCost < costSoFar[next]))
                {
                    costSoFar[next] = newCost;
                    float priority = newCost + HeuristicDistance(next, destination.hex);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        var step = destination.hex;
        while (step != null)
        {
            path.Add(cells[step.ToString()]);
            step = cameFrom[step];
        }
        path.Reverse();

        return path;
    }

    private float MovementCost(Hex current, Hex next) => cells.ContainsKey(current.ToString()) && cells.ContainsKey(next.ToString()) ? cells[next.ToString()].cost : 0;

    private float HeuristicDistance(Hex from, Hex to) => (cells[to.ToString()].transform.position - cells[from.ToString()].transform.position).magnitude;

    private void Generate()
    {
        int qMax = gridXSize / 2;
        int rMax = gridYSize / 2;

        for (int i = -qMax; i < qMax; i++)
        {
            for (int j = -rMax; j < rMax; j++)
            {
                var hex = new Hex(i, j);
                var p = hex.ToPoint(layout);

                var cell = Instantiate(hexCell, new Vector3(p.x, 0.1f, p.y), Quaternion.identity);
                cell.Init(layout, p, hex);

                cells.Add(hex.ToString(), cell);
            }
        }
    }

    public void ClearHighlighting()
    {
        foreach (HexCell pathCell in highlightedPath)
        {
            pathCell.ClearHighlighting(false);
        }
        if (highlightedPath.Count > 0 && highlightedPath[0].Equals(selectedCell))
        {
            selectedCell.Select();
        }

        if (highlightedCell)
        {
            highlightedCell.ClearHighlighting(highlightedCell.Equals(selectedCell));
        }
    }

    public bool CanSpawn() => selectedCell && !selectedCell.occupied;

    public bool CanSpawnAt(Hex hex)
    {
        HexCell cell = cells.ContainsKey(hex.ToString()) ? cells[hex.ToString()] : null;
        return cell && !cell.occupied;
    }

    public Unit Spawn(Unit unit)
    {
        if (CanSpawn())
        {
            var spawnedUnit = SpawnUnitAtCell(unit, selectedCell);
            selectedCell.occupier.ShowStats();

            ClearHighlighting();
            if (highlightedCell)
            {
                HighlightCell(highlightedCell);
            }
            return spawnedUnit;
        }
        return null;
    }

    public Unit SpawnAt(Unit unit, Hex hex) => CanSpawnAt(hex) ? SpawnUnitAtCell(unit, cells[hex.ToString()]) : null;

    private Unit SpawnUnitAtCell(Unit unit, HexCell cell)
    {
        var pos = cell.transform.position;

        var wrapper = new GameObject("Unit Wrapper");
        wrapper.transform.position = new Vector3(pos.x, pos.y, pos.z);

        var spawnedUnit = Instantiate(unit, new Vector3(0, 0, 0), Quaternion.identity);
        spawnedUnit.transform.parent = wrapper.transform;

        spawnedUnit.Setup(cell);
        cell.Occupy(spawnedUnit);

        return spawnedUnit;
    }
}
