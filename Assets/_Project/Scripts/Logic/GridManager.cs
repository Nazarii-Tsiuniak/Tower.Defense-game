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

    // Path cells derived by counting grid squares from the user's hand-drawn black line.
    // map.png 1407x768, PPU=64 → 22×12 grid, cell centers at pixel = (col+0.484)*64 x (row+0.5)*64
    //
    // Route (traced from user drawing):
    //  Cave (1,4) → RIGHT col 1-6, row 4
    //  → DOWN col 6, row 4-6
    //  → LEFT col 6-1, row 6
    //  → DOWN col 1, row 6-8
    //  → RIGHT col 1-9, row 8
    //  → UP col 9, row 8-4
    //  → RIGHT col 9-14, row 4  (crosses upper bridge at col 12-13)
    //  → DOWN col 14, row 4-8
    //  → RIGHT col 14-18, row 8  (castle entrance)
    public static readonly Vector2Int[] PathCells = new Vector2Int[]
    {
        // Seg 1: right from cave, row 4
        new Vector2Int(1,4), new Vector2Int(2,4), new Vector2Int(3,4),
        new Vector2Int(4,4), new Vector2Int(5,4), new Vector2Int(6,4),
        // Seg 2: down col 6
        new Vector2Int(6,5), new Vector2Int(6,6),
        // Seg 3: left row 6
        new Vector2Int(5,6), new Vector2Int(4,6), new Vector2Int(3,6),
        new Vector2Int(2,6), new Vector2Int(1,6),
        // Seg 4: down col 1
        new Vector2Int(1,7), new Vector2Int(1,8),
        // Seg 5: right row 8
        new Vector2Int(2,8), new Vector2Int(3,8), new Vector2Int(4,8),
        new Vector2Int(5,8), new Vector2Int(6,8), new Vector2Int(7,8),
        new Vector2Int(8,8), new Vector2Int(9,8),
        // Seg 6: up col 9
        new Vector2Int(9,7), new Vector2Int(9,6), new Vector2Int(9,5),
        new Vector2Int(9,4),
        // Seg 7: right row 4 (cols 9-14, crossing river bridge at col 13)
        new Vector2Int(10,4), new Vector2Int(11,4), new Vector2Int(12,4),
        new Vector2Int(13,4), new Vector2Int(14,4),
        // Seg 8: down col 14
        new Vector2Int(14,5), new Vector2Int(14,6), new Vector2Int(14,7),
        new Vector2Int(14,8),
        // Seg 9: right row 8 to castle
        new Vector2Int(15,8), new Vector2Int(16,8), new Vector2Int(17,8),
        new Vector2Int(18,8)
    };

    // River (col 13) — all rows except row 4 (upper bridge = path tile)
    public static readonly Vector2Int[] WaterCells = new Vector2Int[]
    {
        new Vector2Int(13,0),  new Vector2Int(13,1),  new Vector2Int(13,2),
        new Vector2Int(13,3),
        // row 4 is the bridge (PathCell), skip it
        new Vector2Int(13,5),  new Vector2Int(13,6),  new Vector2Int(13,7),
        new Vector2Int(13,8),  new Vector2Int(13,9),  new Vector2Int(13,10),
        new Vector2Int(13,11)
    };

    // Waypoints: turning-point corners of the route
    public static readonly Vector2Int[] WaypointCells = new Vector2Int[]
    {
        new Vector2Int(1,  4),  // cave exit
        new Vector2Int(6,  4),  // turn right→down
        new Vector2Int(6,  6),  // turn down→left
        new Vector2Int(1,  6),  // turn left→down
        new Vector2Int(1,  8),  // turn down→right
        new Vector2Int(9,  8),  // turn right→up
        new Vector2Int(9,  4),  // turn up→right (before upper bridge)
        new Vector2Int(14, 4),  // turn right→down (east of upper bridge)
        new Vector2Int(14, 8),  // turn down→right
        new Vector2Int(18, 8),  // castle entrance
    };

    public Vector2Int EntryCell => new Vector2Int(1, 4);
    public Vector2Int BaseCell  => new Vector2Int(18, 8);

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
