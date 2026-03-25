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
    // S-shaped path: entry at (0,5), exit/base at (11,5)
    public static readonly Vector2Int[] PathCells = new Vector2Int[]
    {
        // Segment 1: right along row 5
        new Vector2Int(0, 5), new Vector2Int(1, 5), new Vector2Int(2, 5), new Vector2Int(3, 5),
        // Segment 2: down from row 5 to row 1
        new Vector2Int(3, 4), new Vector2Int(3, 3), new Vector2Int(3, 2), new Vector2Int(3, 1),
        // Segment 3: right along row 1
        new Vector2Int(4, 1), new Vector2Int(5, 1), new Vector2Int(6, 1), new Vector2Int(7, 1),
        // Segment 4: up from row 1 to row 5
        new Vector2Int(7, 2), new Vector2Int(7, 3), new Vector2Int(7, 4), new Vector2Int(7, 5),
        // Segment 5: right along row 5 to base
        new Vector2Int(8, 5), new Vector2Int(9, 5), new Vector2Int(10, 5), new Vector2Int(11, 5)
    };

    // Waypoints: only turning points + entry/exit for enemy movement
    public static readonly Vector2Int[] WaypointCells = new Vector2Int[]
    {
        new Vector2Int(0, 5),   // entry
        new Vector2Int(3, 5),   // first turn
        new Vector2Int(3, 1),   // second turn
        new Vector2Int(7, 1),   // third turn
        new Vector2Int(7, 5),   // fourth turn
        new Vector2Int(11, 5)   // base/exit
    };

    public Vector2Int EntryCell => new Vector2Int(0, 5);
    public Vector2Int BaseCell => new Vector2Int(11, 5);

    private HashSet<Vector2Int> pathSet = new HashSet<Vector2Int>();

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

    public CellType GetCell(int col, int row)
    {
        if (!IsValidCell(col, row)) return CellType.Path;
        return grid[col, row];
    }

    public Vector3 GetEntryWorldPos()
    {
        return CellToWorld(EntryCell) + Vector3.left * 1.5f;
    }
}
