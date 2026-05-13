using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    // Grid matches the visible cell grid on map.png: 22 cols x 12 rows, PPU=64
    // Cell size in world units = 1 (one cell = 1 world unit)
    // World origin at center: col 0..21 → x = -10.5..10.5, row 0..11 → y = 5.5..-5.5
    public const int Cols = 22;
    public const int Rows = 12;
    public const float CellSize = 1.0f;
    // OffsetX/Y: world position of cell(0,0) center
    public const float OffsetX = -10.5f;  // col 0 center
    public const float OffsetY =  5.5f;   // row 0 center (top)

    public enum CellType { Empty, Path, Tower }

    private CellType[,] grid = new CellType[Cols, Rows];

    // Path cells matching the sandy road on map.png (col, row), row 0 = top
    // Grid: 22 cols × 12 rows. Path read from the image:
    //  Cave at col~1,row~4 → right to col~3 → down to row~7 → left to col~1 → down to row~9
    //  → right to col~7 → up to row~4 → right through upper bridge (col~11) → col~12
    //  → down to row~9 → left through lower bridge (col~11) → right to castle col~20
    public static readonly Vector2Int[] PathCells = new Vector2Int[]
    {
        // Seg 1: right from cave
        new Vector2Int(1,4), new Vector2Int(2,4), new Vector2Int(3,4),
        // Seg 2: down
        new Vector2Int(3,5), new Vector2Int(3,6), new Vector2Int(3,7),
        // Seg 3: left
        new Vector2Int(2,7), new Vector2Int(1,7),
        // Seg 4: down
        new Vector2Int(1,8), new Vector2Int(1,9),
        // Seg 5: right
        new Vector2Int(2,9), new Vector2Int(3,9), new Vector2Int(4,9),
        new Vector2Int(5,9), new Vector2Int(6,9), new Vector2Int(7,9),
        // Seg 6: up
        new Vector2Int(7,8), new Vector2Int(7,7), new Vector2Int(7,6),
        new Vector2Int(7,5), new Vector2Int(7,4),
        // Seg 7: right to upper bridge
        new Vector2Int(8,4), new Vector2Int(9,4), new Vector2Int(10,4),
        new Vector2Int(11,4), new Vector2Int(12,4),
        // Seg 8: down
        new Vector2Int(12,5), new Vector2Int(12,6), new Vector2Int(12,7),
        new Vector2Int(12,8), new Vector2Int(12,9),
        // Seg 9: left through lower bridge
        new Vector2Int(11,9), new Vector2Int(10,9),
        // Seg 10: right to castle
        new Vector2Int(13,9), new Vector2Int(14,9), new Vector2Int(15,9),
        new Vector2Int(16,9), new Vector2Int(17,9), new Vector2Int(18,9),
        new Vector2Int(19,9), new Vector2Int(20,9)
    };

    // River cells: col 11, rows 5-8 (bridge at row 4 top and row 9 bottom)
    public static readonly Vector2Int[] WaterCells = new Vector2Int[]
    {
        new Vector2Int(11,5), new Vector2Int(11,6),
        new Vector2Int(11,7), new Vector2Int(11,8)
    };

    // Waypoints for EnemyMovement (turning points only)
    public static readonly Vector2Int[] WaypointCells = new Vector2Int[]
    {
        new Vector2Int(1,  4),  // cave exit
        new Vector2Int(3,  4),  // turn down
        new Vector2Int(3,  7),  // turn left
        new Vector2Int(1,  7),  // turn down
        new Vector2Int(1,  9),  // turn right
        new Vector2Int(7,  9),  // turn up
        new Vector2Int(7,  4),  // turn right
        new Vector2Int(12, 4),  // after upper bridge, turn down
        new Vector2Int(12, 9),  // turn right (lower)
        new Vector2Int(20, 9),  // castle
    };

    public Vector2Int EntryCell => new Vector2Int(1, 4);
    public Vector2Int BaseCell  => new Vector2Int(20, 9);

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

    // col 0 → x = OffsetX = -10.5,  row 0 → y = OffsetY = 5.5 (top)
    public static Vector3 CellToWorld(int col, int row)
    {
        return new Vector3(OffsetX + col * CellSize, OffsetY - row * CellSize, 0);
    }

    public static Vector3 CellToWorld(Vector2Int cell)
    {
        return CellToWorld(cell.x, cell.y);
    }

    public static Vector2Int WorldToCell(Vector3 worldPos)
    {
        int col = Mathf.RoundToInt((worldPos.x - OffsetX) / CellSize);
        int row = Mathf.RoundToInt((OffsetY - worldPos.y) / CellSize);
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
        // Spawn slightly left of cave cell so enemy walks into view
        Vector3 cave = CellToWorld(1, 4);
        return cave + Vector3.left * 1.5f;
    }
}
