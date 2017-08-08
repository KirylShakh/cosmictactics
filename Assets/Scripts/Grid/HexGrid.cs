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
        if (selectedCell) {
            selectedCell.Unselect();
        }

        Hex hex = p.ToHex(layout);

        HexCell cell = cells[hex.ToString()];
        if (cell) {
            cell.Select();
            selectedCell = cell;
        }
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
