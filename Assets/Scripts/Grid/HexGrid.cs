using System.Collections;
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
        if (!destination || destination.occupied) {
            return;
        }

        Unit unit = selectedCell.occupier;
        unit.MoveTo(destination);

        selectedCell.UnitLeaves();
        destination.Occupy(unit);
        SelectCell(destination);
    }

    public void MoveSelectedUnitTo(Point p) {
        MoveSelectedUnitTo(FindCell(p));
    }

    public HexCell FindCell(Point p) {
        Hex hex = p.ToHex(layout);
        return cells.ContainsKey(hex.ToString()) ? cells[hex.ToString()] : null;
    }

    private void Generate() {
        int qMax = gridXSize / 2;
        int rMax = gridYSize / 2;
        
        for (int i = -qMax; i < qMax; i++) {
            for (int j = -rMax; j < rMax; j++) {
                Hex hex = new Hex(i, j);
                Point p = hex.ToPoint(layout);

                HexCell cell = Instantiate(hexCell, new Vector3(p.x, 0.1f, p.y), Quaternion.identity);
                cell.gridLayout = layout;

                cells.Add(hex.ToString(), cell);
            }
        }
    }
}
