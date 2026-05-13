using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public const int Cols = 12;
    public const int Rows = 8;
    public const float OffsetX = -5.5f;
    public const float OffsetY = -3.5f;

    public enum CellType { Empty, Path, Tower }

    private CellType[,] grid = new CellType[Cols, Rows];

    // Path defined as cell coordinates (col, row)
    // True S-curve winding map matching the reference pixel-art image:
    //   Cave (top-left) → meanders down-left → crosses river (bridge) → Castle (bottom-right)
    //
    // Row layout (row 7=top, row 0=bottom):
    //  Row 6: Cave → → → →  (enter top-left, go right)
    //  Col 4: ↓↓              (turn down)
    //  Row 4: ← ← ←          (turn left — meander)
    //  Col 1: ↓↓              (turn down)
    //  Row 2: → → → → →       (turn right, go right at bottom)
    //  Col 6: ↑↑↑             (turn up)
    //  Row 5: →[BRIDGE]→→     (right, cross river via bridge at col 7)
    //  Col 9: ↓↓↓             (turn down)
    //  Row 2: → →  Castle     (right to castle, bottom-right)
    public static readonly Vector2Int[] PathCells = new Vector2Int[]
    {
        // Segment 1: right along row 6 (cave entry)
        new Vector2Int(0, 6), new Vector2Int(1, 6), new Vector2Int(2, 6),
        new Vector2Int(3, 6), new Vector2Int(4, 6),
        // Segment 2: down col 4
        new Vector2Int(4, 5), new Vector2Int(4, 4),
        // Segment 3: left along row 4 (meander left)
        new Vector2Int(3, 4), new Vector2Int(2, 4), new Vector2Int(1, 4),
        // Segment 4: down col 1
        new Vector2Int(1, 3), new Vector2Int(1, 2),
        // Segment 5: right along row 2
        new Vector2Int(2, 2), new Vector2Int(3, 2), new Vector2Int(4, 2),
        new Vector2Int(5, 2), new Vector2Int(6, 2),
        // Segment 6: up col 6
        new Vector2Int(6, 3), new Vector2Int(6, 4), new Vector2Int(6, 5),
        // Segment 7: right along row 5 + bridge over river at col 7
        new Vector2Int(7, 5), new Vector2Int(8, 5), new Vector2Int(9, 5),
        // Segment 8: down col 9
        new Vector2Int(9, 4), new Vector2Int(9, 3), new Vector2Int(9, 2),
        // Segment 9: right along row 2 to castle
        new Vector2Int(10, 2), new Vector2Int(11, 2)
    };

    // Waypoints: turning points + entry/exit for EnemyMovement
    public static readonly Vector2Int[] WaypointCells = new Vector2Int[]
    {
        new Vector2Int(0, 6),   // entry (cave)
        new Vector2Int(4, 6),   // turn: right → down
        new Vector2Int(4, 4),   // turn: down → left
        new Vector2Int(1, 4),   // turn: left → down
        new Vector2Int(1, 2),   // turn: down → right
        new Vector2Int(6, 2),   // turn: right → up
        new Vector2Int(6, 5),   // turn: up → right
        new Vector2Int(9, 5),   // turn: right → down (after bridge)
        new Vector2Int(9, 2),   // turn: down → right
        new Vector2Int(11, 2)   // base/exit (castle)
    };

    // River: col 7, full height EXCEPT row 5 (which is the bridge/path tile)
    public static readonly Vector2Int[] WaterCells = new Vector2Int[]
    {
        new Vector2Int(7, 0), new Vector2Int(7, 1), new Vector2Int(7, 2),
        new Vector2Int(7, 3), new Vector2Int(7, 4),
        new Vector2Int(7, 6), new Vector2Int(7, 7)
    };

    public Vector2Int EntryCell => new Vector2Int(0, 6);
    public Vector2Int BaseCell => new Vector2Int(11, 2);

    private HashSet<Vector2Int> pathSet = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> waterSet = new HashSet<Vector2Int>();

    void Awake()
    {
        Instance = this;
        InitGrid();
    }

    void InitGrid()
    {
        for (int c = 0; c < Cols; c++)
            for (int r = 0; r < Rows; r++)
                grid[c, r] = CellType.Empty;

        foreach (var cell in PathCells)
        {
            grid[cell.x, cell.y] = CellType.Path;
            pathSet.Add(cell);
        }

        foreach (var cell in WaterCells)
        {
            // Water cells cannot have towers (treat as Path for placement purposes)
            grid[cell.x, cell.y] = CellType.Path;
            waterSet.Add(cell);
        }
    }

    public static Vector3 CellToWorld(int col, int row)
    {
        return new Vector3(col + OffsetX, row + OffsetY, 0);
    }

    public static Vector3 CellToWorld(Vector2Int cell)
    {
        return CellToWorld(cell.x, cell.y);
    }

    public static Vector2Int WorldToCell(Vector3 worldPos)
    {
        int col = Mathf.RoundToInt(worldPos.x - OffsetX);
        int row = Mathf.RoundToInt(worldPos.y - OffsetY);
        return new Vector2Int(col, row);
    }

    public bool IsValidCell(int col, int row)
    {
        return col >= 0 && col < Cols && row >= 0 && row < Rows;
    }

    public bool CanPlaceTower(int col, int row)
    {
        if (!IsValidCell(col, row)) return false;
        return grid[col, row] == CellType.Empty;
    }

    public bool CanPlaceTower(Vector2Int cell)
    {
        return CanPlaceTower(cell.x, cell.y);
    }

    public void SetTower(int col, int row)
    {
        if (IsValidCell(col, row))
            grid[col, row] = CellType.Tower;
    }

    public void ResetTowers()
    {
        for (int c = 0; c < Cols; c++)
            for (int r = 0; r < Rows; r++)
                if (grid[c, r] == CellType.Tower)
                    grid[c, r] = CellType.Empty;
    }

    public bool IsPath(int col, int row)
    {
        return pathSet.Contains(new Vector2Int(col, row));
    }

    public bool IsWater(int col, int row)
    {
        return waterSet.Contains(new Vector2Int(col, row));
    }

    public CellType GetCell(int col, int row)
    {
        if (!IsValidCell(col, row)) return CellType.Path;
        return grid[col, row];
    }

    public Vector3 GetEntryWorldPos()
    {
        // Cave entrance on map.png — slightly left of first waypoint so enemy enters from outside
        return new Vector3(-7.2f, 0.72f, 0f);
    }
}
