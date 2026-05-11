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
    static string _charTexName;
    static string _towerTexName;
    static string _tileTexName;
    static string _propsTexName;

    static Texture2D LoadTex(ref Texture2D cache, ref string cachedFileName, string fileName)
    {
        if (cache != null && cachedFileName == fileName) return cache;

        cache = null;
        cachedFileName = fileName;

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
        var tex = LoadTex(ref _charTex, ref _charTexName, "characters");
        return Cut(tex, index, 0, CHAR_W, CHAR_H);
    }

    /// <summary>
    /// Load tower sprite.
    /// typeRow: 0=Stone/Archer  1=Wood/Cannon  2=Magic/Mage
    /// levelCol: 0=lvl1  1=lvl2  2=lvl3
    /// </summary>
    public static Sprite LoadTower(int typeRow, int levelCol = 0)
    {
        var tex = LoadTex(ref _towerTex, ref _towerTexName, "towers");
        return Cut(tex, levelCol, typeRow, TOWER_W, TOWER_H);
    }

    public static Sprite LoadTile(int col, int row)
    {
        var tex = LoadTex(ref _tileTex, ref _tileTexName, "tileset1");
        return Cut(tex, col, row, TILE_W, TILE_H);
    }

    public static Sprite LoadProp(int col, int row)
    {
        var tex = LoadTex(ref _propsTex, ref _propsTexName, "props");
        return Cut(tex, col, row, TILE_W, TILE_H);
    }

    static bool HasVisiblePixels(Texture2D tex, Rect rect)
    {
        try
        {
            var pixels = tex.GetPixels(
                Mathf.RoundToInt(rect.x),
                Mathf.RoundToInt(rect.y),
                Mathf.RoundToInt(rect.width),
                Mathf.RoundToInt(rect.height));
            int visible = 0;
            for (int i = 0; i < pixels.Length; i++)
                if (pixels[i].a > 0.1f) visible++;
            return visible > pixels.Length * 0.08f;
        }
        catch
        {
            // If texture is not readable, treat as usable to avoid suppressing valid sprites.
            return true;
        }
    }

    /// <summary>Loads visible props from props.png, up to maxSprites.</summary>
    public static Sprite[] LoadPropSprites(int maxSprites = 24)
    {
        var tex = LoadTex(ref _propsTex, ref _propsTexName, "props");
        if (tex == null) return new Sprite[0];

        int cols = tex.width / TILE_W;
        int rows = tex.height / TILE_H;
        var list = new System.Collections.Generic.List<Sprite>();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var s = Cut(tex, c, r, TILE_W, TILE_H);
                if (s == null) continue;
                if (!HasVisiblePixels(tex, s.rect)) continue;
                list.Add(s);
                if (list.Count >= maxSprites) return list.ToArray();
            }
        }
        return list.ToArray();
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
            // Prefer highest/tallest tower tier first, then gracefully fall back.
            case "Archer":  return LoadTower(0, 2) ?? LoadTower(0, 1) ?? LoadTower(0, 0);
            case "Cannon":  return LoadTower(1, 2) ?? LoadTower(1, 1) ?? LoadTower(1, 0);
            case "Mage":    return LoadTower(2, 2) ?? LoadTower(2, 1) ?? LoadTower(2, 0);
            case "Freezer": return LoadTower(2, 2) ?? LoadTower(2, 1) ?? LoadTower(2, 0);
            default:        return LoadTower(0, 2) ?? LoadTower(0, 1) ?? LoadTower(0, 0);
        }
    }

    static Sprite LoadCharacterFirst(params int[] indices)
    {
        for (int i = 0; i < indices.Length; i++)
        {
            var s = LoadCharacter(indices[i]);
            if (s != null) return s;
        }
        return null;
    }

    /// <summary>Small character sprite that stands on top of a tower.</summary>
    public static Sprite TowerUnitSprite(string towerName)
    {
        switch (towerName)
        {
            case "Archer":  return LoadCharacterFirst(3, 2, 0); // Archer/Knight fallback
            case "Mage":    return LoadCharacterFirst(4, 2, 3); // Mage may be absent in some sheets
            case "Freezer": return LoadCharacterFirst(1, 4, 2); // Slime/mage-like caster fallback
            case "Cannon":  return LoadCharacterFirst(2, 3, 0); // Knight/Archer fallback
            default:        return LoadCharacterFirst(2, 0);
        }
    }

    /// <summary>Call this to clear the texture cache (e.g. after moving files).</summary>
    public static void ClearCache()
    {
        _charTex  = null;
        _towerTex = null;
        _tileTex  = null;
        _propsTex = null;
        _charTexName = null;
        _towerTexName = null;
        _tileTexName = null;
        _propsTexName = null;
    }
}
