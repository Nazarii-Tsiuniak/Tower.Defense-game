using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Loads sprites from the downloaded PNG spritesheets.
/// Files must be located in Assets/Resources/Sprites/ folder.
/// Run Tools > Setup Sprites from the Unity menu to copy them there.
/// </summary>
public static class SpriteLoader
{
    // ── Spritesheet dimensions (tweak if sprites look cropped) ──
    // characters.png: 5 chars in one row
    const int CHAR_W = 16;
    const int CHAR_H = 16;

    // towers.png: 3 cols (level 1-3)  x  3 rows (Stone, Wood, Magic)
    const int TOWER_W = 32;
    const int TOWER_H = 32;

    // tileset1.png: grid of tiles
    const int TILE_W = 16;
    const int TILE_H = 16;

    // ── Cache ────────────────────────────────────────────────────
    static Texture2D _charTex;
    static Texture2D _towerTex;
    static Texture2D _tileTex;
    static Texture2D _propsTex;

    static Texture2D LoadTex(ref Texture2D cache, string fileName)
    {
        if (cache != null) return cache;

        cache = Resources.Load<Texture2D>($"Sprites/{fileName}");
        if (cache == null)
            cache = Resources.Load<Texture2D>(fileName);

#if UNITY_EDITOR
        if (cache == null)
        {
            cache = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Sprites/{fileName}.png");
            if (cache == null)
                cache = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/{fileName}.png");
        }
#endif

        return cache;
    }

    // ── Generic sheet cutter ─────────────────────────────────────
    static Sprite Cut(Texture2D tex, int col, int row, int w, int h)
    {
        if (tex == null) return null;
        int x = col * w;
        // Unity Y axis goes bottom-up; row 0 = top row in the image
        int totalRows = tex.height / h;
        int y = (totalRows - 1 - row) * h;
        if (x + w > tex.width || y < 0) return null;
        return Sprite.Create(tex, new Rect(x, y, w, h),
            new Vector2(0.5f, 0.5f), Mathf.Max(w, h));
    }

    // ── Public API ───────────────────────────────────────────────

    /// <summary>Load one of the 5 characters (0=Goblin 1=Slime 2=Knight 3=Archer 4=Mage)</summary>
    public static Sprite LoadCharacter(int index)
    {
        var tex = LoadTex(ref _charTex, "characters");
        return Cut(tex, index, 0, CHAR_W, CHAR_H);
    }

    /// <summary>
    /// Load tower sprite.
    /// typeRow: 0=Stone/Archer  1=Wood/Cannon  2=Magic/Mage
    /// levelCol: 0=lvl1  1=lvl2  2=lvl3
    /// </summary>
    public static Sprite LoadTower(int typeRow, int levelCol = 0)
    {
        var tex = LoadTex(ref _towerTex, "towers");
        return Cut(tex, levelCol, typeRow, TOWER_W, TOWER_H);
    }

    public static Sprite LoadTile(int col, int row)
    {
        var tex = LoadTex(ref _tileTex, "tileset1");
        return Cut(tex, col, row, TILE_W, TILE_H);
    }

    /// <summary>Grass tile from tileset (col 0, row 0)</summary>
    public static Sprite LoadGrassTile()
    {
        return LoadTile(0, 0);
    }

    /// <summary>Path / cobblestone tile (col 1, row 0)</summary>
    public static Sprite LoadPathTile()
    {
        return LoadTile(1, 0);
    }

    // ── Named helpers used by SpriteGenerator ────────────────────

    public static Sprite EnemySprite(string name)
    {
        switch (name)
        {
            case "Goblin": return LoadCharacter(0);
            case "Ghost":  return LoadCharacter(1); // Slime as Ghost
            case "Orc":    return LoadCharacter(3); // Archer-style
            default:       return LoadCharacter(0);
        }
    }

    public static Sprite TowerSprite(string name)
    {
        switch (name)
        {
            case "Archer":  return LoadTower(0, 0);
            case "Cannon":  return LoadTower(1, 0);
            case "Mage":    return LoadTower(2, 0);
            case "Freezer": return LoadTower(2, 1);
            default:        return LoadTower(0, 0);
        }
    }

    /// <summary>Call this to clear the texture cache (e.g. after moving files).</summary>
    public static void ClearCache()
    {
        _charTex  = null;
        _towerTex = null;
        _tileTex  = null;
        _propsTex = null;
    }
}
