﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour {

    public int gridXSize = 2;
    public int gridYSize = 2;
    public HexCell hexCell;

    public Layout layout;
    public IDictionary<string, HexCell> cells;
    public HexCell selectedCell;
    public HexCell highlightedCell;

    // Use this for initialization
    void Start () {
        layout = new Layout(true, new Point(1f, 1f), new Point(0f, 0f));
        cells = new Dictionary<string, HexCell>();

        Generate();
    }
	
    // Update is called once per frame
    void Update () {
        	
    }

    public void HighlightCell(Point p) {
        HighlightCell(FindCell(p));
    }

    public void HighlightCell(HexCell cell) {
        if (highlightedCell) {
            highlightedCell.ClearHighlighting(highlightedCell.Equals(selectedCell));
        }

        if (cell) {
            cell.Highlight();
            highlightedCell = cell;
        }
    }

    public void SelectCell(Point p) {
        SelectCell(FindCell(p));
    }

    public void SelectCell(HexCell cell) {
        if (selectedCell) {
            selectedCell.Unselect();
            if (selectedCell.occupied) {
                selectedCell.occupier.HideStats();
            }

        }

        if (cell) {
            cell.Select();
            selectedCell = cell;
            if (selectedCell.occupied) {
                selectedCell.occupier.ShowStats();
            }
        }
    }

    public void MoveSelectedUnitTo(HexCell destination) {
        if (!destination || destination.occupied || selectedCell.occupier.isMoving) {
            return;
        }

        List<HexCell> path = FindPath(selectedCell, destination);
        if (path.Count >= 2) {
            selectedCell.occupier.MoveAlong(path);
        }

        SelectCell(destination);
    }

    public void MoveSelectedUnitTo(Point p) {
        MoveSelectedUnitTo(FindCell(p));
    }

    public HexCell FindCell(Point p) {
        Hex hex = p.ToHex(layout);
        return cells.ContainsKey(hex.ToString()) ? cells[hex.ToString()] : null;
    }

    public List<HexCell> FindPath(HexCell start, HexCell destination) {
        List<HexCell> path = new List<HexCell>();

        PriorityQueue<Hex> frontier = new PriorityQueue<Hex>();
        frontier.Enqueue(start.hex, 0);

        IDictionary<Hex, Hex> cameFrom = new Dictionary<Hex, Hex>();
        cameFrom[start.hex] = null;

        IDictionary<Hex, float> costSoFar = new Dictionary<Hex, float>();
        costSoFar[start.hex] = 0;

        while (frontier.Count() != 0) {
            Hex current = frontier.Dequeue();
            if (current == destination.hex) break;

            foreach (Hex next in current.Neighbours()) {
                if (!cells.ContainsKey(next.ToString()) || cells[next.ToString()].occupied) continue;

                float newCost = costSoFar[current] + MovementCost(current, next);
                if (!costSoFar.ContainsKey(next) || (newCost < costSoFar[next])) {
                    costSoFar[next] = newCost;
                    float priority = newCost + HeuristicDistance(next, destination.hex);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        Hex step = destination.hex;
        while (step != null) {
            path.Add(cells[step.ToString()]);
            step = cameFrom[step];
        }
        path.Reverse();

        return path;
    }

    private float MovementCost(Hex current, Hex next) {
        if (cells.ContainsKey(current.ToString()) && cells.ContainsKey(next.ToString())) {
            return cells[next.ToString()].cost;
        }
        return 0;
    }

    private float HeuristicDistance(Hex from, Hex to) {
        return (cells[to.ToString()].transform.position - cells[from.ToString()].transform.position).magnitude;
    }

    private void Generate() {
        int qMax = gridXSize / 2;
        int rMax = gridYSize / 2;
        
        for (int i = -qMax; i < qMax; i++) {
            for (int j = -rMax; j < rMax; j++) {
                Hex hex = new Hex(i, j);
                Point p = hex.ToPoint(layout);

                HexCell cell = Instantiate(hexCell, new Vector3(p.x, 0.1f, p.y), Quaternion.identity);
                cell.Init(layout, p, hex);

                cells.Add(hex.ToString(), cell);
            }
        }
    }
}
