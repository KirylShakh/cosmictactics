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

    // Use this for initialization
    void Start () {
        layout = new Layout(true, new Point(1f, 1f), new Point(0f, 0f));
        cells = new Dictionary<string, HexCell>();

        Generate();
    }
	
    // Update is called once per frame
    void Update () {
        	
    }

    public void SelectCell(Point p) {
        SelectCell(FindCell(p));
    }

    public void SelectCell(HexCell cell) {
        if (selectedCell) {
            selectedCell.Unselect();
        }

        if (cell) {
            cell.Select();
            selectedCell = cell;
        }
    }

    public HexCell FindCell(Point p) {
        Hex hex = p.ToHex(layout);
        return cells[hex.ToString()];
    }

    public void MoveSelectedUnitTo(Point p) {
        HexCell destination = FindCell(p);
        if (!destination || destination.occupied) {
            return;
        }

        Unit unit = selectedCell.occupier;
        unit.MoveTo(destination);

        selectedCell.UnitLeaves();
        destination.Occupy(unit);
        SelectCell(destination);
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
