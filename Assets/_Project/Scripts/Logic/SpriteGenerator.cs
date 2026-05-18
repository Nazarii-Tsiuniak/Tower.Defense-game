using UnityEngine;

public static class SpriteGenerator
{
    // ==================== UTILITY ====================
    public static Sprite CreateRect(int width, int height, Color fill, Color border)
    {
        var tex = new Texture2D(width, height);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                bool isBorder = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                tex.SetPixel(x, y, isBorder ? border : fill);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), width);
    }

    public static Sprite CreateCircle(int diameter, Color fill, Color border)
    {
        var tex = new Texture2D(diameter, diameter);
        tex.filterMode = FilterMode.Point;
        float r = diameter / 2f;
        for (int y = 0; y < diameter; y++)
            for (int x = 0; x < diameter; x++)
            {
                float dx = x - r + 0.5f;
                float dy = y - r + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < r - 1.5f)
                    tex.SetPixel(x, y, fill);
                else if (dist < r - 0.5f)
                    tex.SetPixel(x, y, border);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f), diameter);
    }

    static Color Lerp3(Color a, Color b, Color c, float t)
    {
        if (t < 0.5f) return Color.Lerp(a, b, t * 2f);
        return Color.Lerp(b, c, (t - 0.5f) * 2f);
    }

    // ==================== GRASS TILE ====================
    public static Sprite CreateGrassTile()
    {
        int s = 32;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;

        Color dark   = new Color(0.22f, 0.50f, 0.14f);
        Color mid    = new Color(0.30f, 0.62f, 0.20f);
        Color light  = new Color(0.38f, 0.72f, 0.26f);
        Color bright = new Color(0.48f, 0.80f, 0.30f);
        Color border = new Color(0.20f, 0.45f, 0.12f);

        var rng = new System.Random(42);

        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                // Edge darkening
                float edgeDist = Mathf.Min(x, y, s - 1 - x, s - 1 - y);
                bool isBorder = edgeDist == 0;

                if (isBorder)
                {
                    tex.SetPixel(x, y, border);
                    continue;
                }

                // Base gradient — slight radial brightness
                float cx = (x - s / 2f) / (s / 2f);
                float cy = (y - s / 2f) / (s / 2f);
                float radial = 1f - (cx * cx + cy * cy) * 0.3f;
                float noise = (float)rng.NextDouble();

                Color baseCol;
                if (noise < 0.05f)
                    baseCol = bright;
                else if (noise < 0.25f)
                    baseCol = light;
                else if (noise < 0.75f)
                    baseCol = mid;
                else
                    baseCol = dark;

                baseCol *= radial;
                baseCol.a = 1f;

                // Grass blade pattern — vertical streaks
                if (x % 4 == 1 && y > 2 && y < s - 2 && rng.NextDouble() < 0.4f)
                    baseCol = Color.Lerp(baseCol, bright, 0.4f);

                // Small flower spots
                if (rng.NextDouble() < 0.008f)
                    baseCol = new Color(0.9f, 0.8f, 0.2f);
                if (rng.NextDouble() < 0.005f)
                    baseCol = new Color(0.85f, 0.3f, 0.3f);

                // Subtle edge shadow
                if (edgeDist <= 2)
                    baseCol = Color.Lerp(baseCol, border, 0.3f);

                tex.SetPixel(x, y, baseCol);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    // ==================== PATH TILE (COBBLESTONE) ====================
    public static Sprite CreatePathTile()
    {
        int s = 32;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;

        Color grout   = new Color(0.35f, 0.28f, 0.18f);
        Color stone1  = new Color(0.68f, 0.58f, 0.40f);
        Color stone2  = new Color(0.75f, 0.65f, 0.48f);
        Color stoneHi = new Color(0.82f, 0.72f, 0.55f);
        Color stoneSh = new Color(0.55f, 0.45f, 0.32f);
        Color border  = new Color(0.42f, 0.34f, 0.22f);

        // Fill with grout first
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, grout);

        // Draw cobblestone pattern — irregular grid of stones
        var rng = new System.Random(99);
        int[][] stoneRows = {
            new int[] { 0, 6, 14, 22, 32 },
            new int[] { 0, 8, 18, 26, 32 },
            new int[] { 0, 5, 12, 21, 32 },
            new int[] { 0, 9, 16, 24, 32 }
        };
        int[] rowStarts = { 0, 8, 16, 24 };
        int rowHeight = 8;

        for (int row = 0; row < 4; row++)
        {
            int ry = rowStarts[row];
            int[] cols = stoneRows[row];
            for (int ci = 0; ci < cols.Length - 1; ci++)
            {
                int cx1 = cols[ci] + 1;
                int cx2 = cols[ci + 1] - 1;
                int cy1 = ry + 1;
                int cy2 = Mathf.Min(ry + rowHeight - 1, s - 1);

                Color sc = rng.NextDouble() < 0.5 ? stone1 : stone2;
                for (int py = cy1; py < cy2; py++)
                    for (int px = cx1; px < cx2; px++)
                    {
                        if (px < 0 || px >= s || py < 0 || py >= s) continue;
                        Color c = sc;
                        // Highlight top-left
                        if (py == cy1 || px == cx1)
                            c = Color.Lerp(c, stoneHi, 0.5f);
                        // Shadow bottom-right
                        if (py == cy2 - 1 || px == cx2 - 1)
                            c = Color.Lerp(c, stoneSh, 0.5f);
                        // Noise
                        c = Color.Lerp(c, sc, 0.7f + (float)rng.NextDouble() * 0.3f);
                        tex.SetPixel(px, py, c);
                    }
            }
        }

        // Border
        for (int i = 0; i < s; i++)
        {
            tex.SetPixel(i, 0, border);
            tex.SetPixel(i, s - 1, border);
            tex.SetPixel(0, i, border);
            tex.SetPixel(s - 1, i, border);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    // ==================== ENTRY MARKER (PORTAL) ====================
    public static Sprite CreateEntryMarker()
    {
        int s = 32;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;

        Color dark    = new Color(0.10f, 0.10f, 0.15f);
        Color stone   = new Color(0.40f, 0.40f, 0.45f);
        Color stoneHi = new Color(0.55f, 0.55f, 0.60f);
        Color glow    = new Color(0.2f, 0.9f, 0.3f, 0.8f);
        Color glowDim = new Color(0.15f, 0.6f, 0.2f, 0.5f);

        // Background
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, new Color(0.25f, 0.55f, 0.20f));

        // Portal arch
        float cx = s / 2f, cy = s / 2f;
        for (int y = 2; y < s - 2; y++)
            for (int x = 4; x < s - 4; x++)
            {
                float dx = (x - cx) / 10f;
                float dy = (y - cy) / 13f;
                float dist = dx * dx + dy * dy;

                if (dist < 0.6f)
                    tex.SetPixel(x, y, dark);
                else if (dist < 0.75f)
                    tex.SetPixel(x, y, glow);
                else if (dist < 0.85f)
                    tex.SetPixel(x, y, glowDim);
                else if (dist < 1.0f)
                    tex.SetPixel(x, y, stone);
                else if (dist < 1.15f)
                    tex.SetPixel(x, y, stoneHi);
            }

        // Arrow pointing right
        for (int i = -2; i <= 2; i++)
            tex.SetPixel(s / 2 + 2, s / 2 + i, glow);
        tex.SetPixel(s / 2 + 3, s / 2, glow);
        tex.SetPixel(s / 2 + 3, s / 2 - 1, glow);
        tex.SetPixel(s / 2 + 3, s / 2 + 1, glow);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    // ==================== BASE MARKER (CASTLE) ====================
    public static Sprite CreateBaseMarker()
    {
        int s = 32;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;

        Color ground  = new Color(0.25f, 0.55f, 0.20f);
        Color wall    = new Color(0.60f, 0.30f, 0.25f);
        Color wallDk  = new Color(0.45f, 0.20f, 0.18f);
        Color wallHi  = new Color(0.75f, 0.42f, 0.35f);
        Color roof    = new Color(0.55f, 0.22f, 0.18f);
        Color flag    = new Color(1.0f, 0.85f, 0.15f);
        Color door    = new Color(0.25f, 0.15f, 0.10f);

        // Background
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, ground);

        // Castle base
        for (int y = 3; y < 20; y++)
            for (int x = 6; x < 26; x++)
            {
                Color c = wall;
                if (x == 6 || x == 25) c = wallDk;
                if (y == 19) c = wallDk;
                if (y == 3) c = wallHi;
                tex.SetPixel(x, y, c);
            }

        // Battlements (crenellations) on top
        for (int bx = 6; bx < 26; bx += 4)
        {
            for (int by = 20; by < 24; by++)
                for (int bxx = bx; bxx < Mathf.Min(bx + 2, 26); bxx++)
                {
                    tex.SetPixel(bxx, by, wall);
                    if (by == 23) tex.SetPixel(bxx, by, wallHi);
                }
        }

        // Tower turrets
        for (int y = 18; y < 28; y++)
        {
            for (int x = 4; x < 9; x++) tex.SetPixel(x, y, wallDk);
            for (int x = 23; x < 28; x++) tex.SetPixel(x, y, wallDk);
        }
        // Turret tops
        for (int x = 3; x < 10; x++) tex.SetPixel(x, 28, roof);
        for (int x = 22; x < 29; x++) tex.SetPixel(x, 28, roof);
        for (int x = 4; x < 9; x++) { tex.SetPixel(x, 29, roof); tex.SetPixel(x, 30, roof); }
        for (int x = 23; x < 28; x++) { tex.SetPixel(x, 29, roof); tex.SetPixel(x, 30, roof); }

        // Door
        for (int y = 3; y < 12; y++)
            for (int x = 13; x < 19; x++)
                tex.SetPixel(x, y, door);
        // Door arch
        tex.SetPixel(13, 12, door); tex.SetPixel(14, 12, door);
        tex.SetPixel(15, 13, door); tex.SetPixel(16, 13, door);
        tex.SetPixel(17, 12, door); tex.SetPixel(18, 12, door);

        // Flag on right turret
        tex.SetPixel(25, 30, wallDk);
        tex.SetPixel(25, 31, wallDk);
        for (int fx = 26; fx < 30 && fx < s; fx++)
            for (int fy = 29; fy < 32 && fy < s; fy++)
                tex.SetPixel(fx, fy, flag);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    // ==================== TOWER SPRITES ====================
    public static Sprite CreateTowerSprite(string towerName)
    {
        int s = 48;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, Color.clear);

        switch (towerName)
        {
            case "Archer":  DrawArcherTower(tex, s);  break;
            case "Mage":    DrawMageTower(tex, s);    break;
            case "Freezer": DrawFreezerTower(tex, s); break;
            case "Cannon":  DrawCannonTower(tex, s);  break;
            default: DrawGenericTower(tex, s, Color.gray, Color.black); break;
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    // ── Shared helpers ─────────────────────────────────────────────────────
    static void FillRect(Texture2D tex, int x0, int y0, int x1, int y1, Color c)
    {
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    tex.SetPixel(x, y, c);
    }

    static void FillCircle(Texture2D tex, float cx, float cy, float r, Color inner, Color rim)
    {
        int x0 = Mathf.Max(0, (int)(cx - r - 1));
        int x1 = Mathf.Min(tex.width - 1, (int)(cx + r + 1));
        int y0 = Mathf.Max(0, (int)(cy - r - 1));
        int y1 = Mathf.Min(tex.height - 1, (int)(cy + r + 1));
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (d < r - 1f)       tex.SetPixel(x, y, inner);
                else if (d < r + 0.5f) tex.SetPixel(x, y, rim);
            }
    }

    // Stone brick helper: fills region with stone bricks pattern
    static void FillBricks(Texture2D tex, int x0, int y0, int x1, int y1,
        Color light, Color dark, Color grout, int brickH = 4)
    {
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                if (x < 0 || x >= tex.width || y < 0 || y >= tex.height) continue;
                bool isGroutH = (y - y0) % brickH == 0;
                int row = (y - y0) / brickH;
                int offset = (row % 2 == 0) ? 0 : brickH * 2;
                bool isGroutV = (x - x0 + offset) % (brickH * 4) == 0;
                if (isGroutH || isGroutV)
                    tex.SetPixel(x, y, grout);
                else
                {
                    // slight shading: top-left of each brick is lighter
                    bool topEdge = (y - y0) % brickH == 1;
                    bool leftEdge = (x - x0 + offset) % (brickH * 4) == 1;
                    tex.SetPixel(x, y, (topEdge || leftEdge) ? light : dark);
                }
            }
    }

    // Crenellation (battlements) across the top of a tower
    static void DrawBattlements(Texture2D tex, int x0, int x1, int y, Color stone, Color dark)
    {
        // Draw parapet base
        for (int x = x0; x <= x1; x++)
            if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                tex.SetPixel(x, y, stone);
        // Merlons (raised parts) every 3px, gap every 3px
        int w = x1 - x0 + 1;
        for (int i = 0; i < w; i++)
        {
            int block = i / 3;
            if (block % 2 == 0)
            {
                int px = x0 + i;
                for (int dy = 1; dy <= 3; dy++)
                {
                    int py = y + dy;
                    if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                        tex.SetPixel(px, py, (dy == 3) ? dark : stone);
                }
            }
        }
    }

    // ── Archer Tower (48x48): green stone keep with wooden archer platform ──
    static void DrawArcherTower(Texture2D tex, int s)
    {
        Color stone    = new Color(0.56f, 0.56f, 0.52f);
        Color stoneLt  = new Color(0.70f, 0.70f, 0.65f);
        Color stoneDk  = new Color(0.38f, 0.38f, 0.35f);
        Color grout    = new Color(0.28f, 0.28f, 0.26f);
        Color wood     = new Color(0.55f, 0.36f, 0.16f);
        Color woodDk   = new Color(0.38f, 0.25f, 0.10f);
        Color bowYell  = new Color(0.90f, 0.80f, 0.45f);
        Color moss     = new Color(0.28f, 0.52f, 0.18f);

        // Base platform (wide)
        FillRect(tex, 4,  0, 43, 5, stoneDk);
        FillRect(tex, 6,  1, 41, 4, stone);

        // Tower body with bricks
        FillBricks(tex, 13, 5, 34, 30, stoneLt, stone, grout, 4);

        // Moss patches on lower bricks
        for (int x = 14; x <= 33; x += 5)
            for (int y = 5; y <= 10; y++)
                if ((x + y) % 3 == 0) tex.SetPixel(x, y, moss);

        // Arrow slit window
        FillRect(tex, 22, 14, 25, 22, new Color(0.08f, 0.08f, 0.10f));
        tex.SetPixel(23, 14, stoneDk); tex.SetPixel(24, 14, stoneDk);

        // Wooden archer platform
        for (int y = 30; y <= 33; y++)
            for (int x = 10; x <= 37; x++)
                tex.SetPixel(x, y, (y == 30 || x == 10 || x == 37) ? woodDk : wood);
        // Wood planks texture
        for (int x = 12; x <= 36; x += 4)
            for (int y = 31; y <= 32; y++)
                tex.SetPixel(x, y, woodDk);

        // Battlements on top of platform
        DrawBattlements(tex, 10, 37, 33, stone, stoneDk);

        // Archer figure (silhouette)
        Color body = new Color(0.22f, 0.18f, 0.14f);
        Color skin  = new Color(0.85f, 0.68f, 0.50f);
        Color cloak = new Color(0.35f, 0.22f, 0.10f);
        // Legs
        FillRect(tex, 20, 38, 21, 41, body);
        FillRect(tex, 23, 38, 24, 41, body);
        // Torso
        FillRect(tex, 19, 42, 25, 46, cloak);
        // Head
        FillCircle(tex, 22.5f, 47.5f, 2.5f, skin, body);
        // Bow arm
        FillRect(tex, 26, 43, 28, 44, body);
        // Bow (vertical arc)
        tex.SetPixel(29, 41, bowYell); tex.SetPixel(30, 42, bowYell);
        tex.SetPixel(30, 43, bowYell); tex.SetPixel(30, 44, bowYell);
        tex.SetPixel(29, 45, bowYell);
        // String
        tex.SetPixel(28, 41, new Color(0.9f, 0.9f, 0.7f));
        tex.SetPixel(28, 42, new Color(0.9f, 0.9f, 0.7f));
        tex.SetPixel(28, 43, new Color(0.9f, 0.9f, 0.7f));
        tex.SetPixel(28, 44, new Color(0.9f, 0.9f, 0.7f));
        tex.SetPixel(28, 45, new Color(0.9f, 0.9f, 0.7f));
        // Arrow
        for (int x = 26; x <= 34; x++) tex.SetPixel(x, 43, new Color(0.75f, 0.62f, 0.28f));
        tex.SetPixel(34, 42, new Color(0.75f, 0.28f, 0.18f));
        tex.SetPixel(34, 44, new Color(0.75f, 0.28f, 0.18f));
        tex.SetPixel(35, 43, new Color(0.80f, 0.72f, 0.60f));
    }

    // ── Mage Tower (48x48): tall purple spire with magic orb ──
    static void DrawMageTower(Texture2D tex, int s)
    {
        Color base1  = new Color(0.48f, 0.20f, 0.70f);
        Color base2  = new Color(0.35f, 0.14f, 0.55f);
        Color baseDk = new Color(0.22f, 0.08f, 0.38f);
        Color grout  = new Color(0.18f, 0.06f, 0.28f);
        Color rune   = new Color(0.85f, 0.55f, 1.00f);
        Color orbC   = new Color(0.92f, 0.55f, 1.00f);
        Color orbHi  = new Color(1.00f, 0.88f, 1.00f);
        Color gold   = new Color(1.00f, 0.82f, 0.25f);
        Color stoneLt= new Color(0.68f, 0.62f, 0.72f);

        // Base platform with golden trim
        FillRect(tex, 4,  0, 43, 4, baseDk);
        FillRect(tex, 6,  1, 41, 3, stoneLt);
        for (int x = 6; x <= 41; x += 3) tex.SetPixel(x, 2, gold);

        // Tower body — narrowing upward (tapering spire)
        for (int y = 4; y < 36; y++)
        {
            float t = (y - 4f) / 32f;
            int halfW = (int)Mathf.Lerp(10, 5, t);
            int cx = 23;
            for (int x = cx - halfW; x <= cx + halfW; x++)
            {
                bool edge = (x == cx - halfW || x == cx + halfW);
                bool groutV = (x - cx + halfW) % 5 == 0;
                bool groutH = (y - 4) % 5 == 0;
                Color c = (groutH || groutV || edge) ? grout : ((x + y) % 3 == 0 ? base2 : base1);
                if (x >= 0 && x < s && y >= 0 && y < s) tex.SetPixel(x, y, c);
            }
        }

        // Rune glyphs on tower face
        // Rune 1 (cross)
        tex.SetPixel(23, 12, rune); tex.SetPixel(22, 13, rune); tex.SetPixel(23, 13, rune);
        tex.SetPixel(24, 13, rune); tex.SetPixel(23, 14, rune);
        // Rune 2 (diamond)
        tex.SetPixel(23, 20, rune); tex.SetPixel(22, 21, rune); tex.SetPixel(24, 21, rune);
        tex.SetPixel(23, 22, rune);

        // Gold ring trim mid-tower
        for (int x = 14; x <= 32; x++) tex.SetPixel(x, 25, gold);

        // Conical spire top
        for (int y = 36; y < 44; y++)
        {
            float t = (y - 36f) / 8f;
            int halfW = (int)Mathf.Lerp(6, 0, t);
            int cx = 23;
            for (int x = cx - halfW; x <= cx + halfW; x++)
                if (x >= 0 && x < s && y >= 0 && y < s)
                    tex.SetPixel(x, y, (y % 2 == 0) ? base1 : baseDk);
        }
        // Spire tip gold cap
        tex.SetPixel(23, 44, gold); tex.SetPixel(23, 45, gold);
        tex.SetPixel(22, 44, gold); tex.SetPixel(24, 44, gold);

        // Magic orb — floating beside tower top
        FillCircle(tex, 36f, 40f, 5f, orbC, baseDk);
        tex.SetPixel(34, 42, orbHi); tex.SetPixel(33, 41, orbHi);
        // Orb glow aura
        for (int y = 34; y <= 46; y++)
            for (int x = 30; x <= 42; x++)
            {
                float d = Mathf.Sqrt((x - 36f) * (x - 36f) + (y - 40f) * (y - 40f));
                if (d > 5f && d < 7f && tex.GetPixel(x, y).a < 0.1f)
                    tex.SetPixel(x, y, new Color(rune.r, rune.g, rune.b, 0.35f));
            }
        // Connecting magic arc from tower to orb
        tex.SetPixel(29, 38, rune); tex.SetPixel(30, 39, rune); tex.SetPixel(31, 40, rune);
    }

    // ── Freezer Tower (48x48): icy blue crystal fortress ──
    static void DrawFreezerTower(Texture2D tex, int s)
    {
        Color base1  = new Color(0.38f, 0.68f, 0.90f);
        Color base2  = new Color(0.26f, 0.52f, 0.75f);
        Color baseDk = new Color(0.18f, 0.36f, 0.58f);
        Color grout  = new Color(0.14f, 0.28f, 0.45f);
        Color ice    = new Color(0.82f, 0.94f, 1.00f);
        Color iceHi  = new Color(0.96f, 0.99f, 1.00f);
        Color iceDk  = new Color(0.55f, 0.75f, 0.90f);
        Color snowCap= new Color(0.92f, 0.96f, 1.00f);

        // Base platform
        FillRect(tex, 4,  0, 43, 5, baseDk);
        FillRect(tex, 6,  1, 41, 4, new Color(0.45f, 0.55f, 0.60f));
        // Icicles hanging from base
        for (int x = 8; x <= 40; x += 5)
        {
            int h = 2 + (x % 3);
            for (int y = 0; y <= h; y++) tex.SetPixel(x, y, ice);
            tex.SetPixel(x, 0, iceHi);
        }

        // Tower body with ice-brick pattern
        FillBricks(tex, 12, 5, 35, 28, base1, base2, grout, 4);

        // Ice encrustation overlay
        var rng = new System.Random(77);
        for (int y = 5; y <= 28; y++)
            for (int x = 12; x <= 35; x++)
                if (rng.NextDouble() < 0.08f)
                    tex.SetPixel(x, y, ice);

        // Frost window
        FillRect(tex, 21, 13, 26, 22, new Color(0.06f, 0.10f, 0.20f));
        // Ice cross on window
        for (int i = 0; i < 4; i++) { tex.SetPixel(23 + i - 1, 17, iceHi); tex.SetPixel(23, 15 + i, iceHi); }

        // Battlements with snow cap
        DrawBattlements(tex, 10, 37, 28, base2, baseDk);
        for (int x = 10; x <= 37; x++)
        {
            tex.SetPixel(x, 28, snowCap);
            if ((x % 3 == 0) && x + 1 < s) tex.SetPixel(x, 29, snowCap);
        }

        // Main crystal cluster on top (4 crystals)
        int[] cxs = { 18, 22, 26, 31 };
        int[] cys = { 33, 36, 34, 32 };
        int[] chs = { 8,  11,  9,  7  };
        for (int ci = 0; ci < 4; ci++)
        {
            int cx = cxs[ci], cy = cys[ci], ch = chs[ci];
            // Crystal body (hexagonal, 3px wide)
            for (int y = cy; y <= cy + ch; y++)
            {
                float t = (float)(y - cy) / ch;
                int hw = t < 0.5f ? (int)(t * 4) : (int)((1 - t) * 4);
                hw = Mathf.Max(1, hw);
                for (int dx = -hw; dx <= hw; dx++)
                {
                    int px = cx + dx;
                    if (px >= 0 && px < s && y >= 0 && y < s)
                        tex.SetPixel(px, y, Color.Lerp(ice, base1, Mathf.Abs(dx) / (float)hw * 0.5f));
                }
            }
            // Highlight edge
            tex.SetPixel(cx - 1, cy + ch / 2, iceHi);
            // Tip
            tex.SetPixel(cx, cy + ch + 1, iceHi);
        }
        // Sparkle dots
        int[] sx = { 15, 33, 24, 20, 29 };
        int[] sy = { 40, 38, 47, 44, 42 };
        for (int i = 0; i < sx.Length; i++)
            if (sx[i] < s && sy[i] < s) tex.SetPixel(sx[i], sy[i], iceHi);
    }

    // ── Cannon Tower (48x48): heavy stone fortress with mounted cannon ──
    static void DrawCannonTower(Texture2D tex, int s)
    {
        Color base1  = new Color(0.55f, 0.48f, 0.38f);
        Color base2  = new Color(0.42f, 0.36f, 0.28f);
        Color baseDk = new Color(0.28f, 0.24f, 0.18f);
        Color grout  = new Color(0.22f, 0.18f, 0.12f);
        Color metal  = new Color(0.32f, 0.32f, 0.35f);
        Color metalHi= new Color(0.52f, 0.52f, 0.58f);
        Color metalDk= new Color(0.18f, 0.18f, 0.20f);
        Color wood   = new Color(0.50f, 0.32f, 0.14f);
        Color woodDk = new Color(0.35f, 0.22f, 0.09f);
        Color smoke  = new Color(0.60f, 0.58f, 0.55f, 0.5f);

        // Thick base
        FillRect(tex, 2, 0, 45, 7, baseDk);
        FillBricks(tex, 4, 1, 43, 6, base1, base2, grout, 3);

        // Main tower body (wide and squat)
        FillBricks(tex, 8, 7, 39, 28, base1, base2, grout, 4);

        // Side buttresses (structural pillars)
        FillBricks(tex, 4,  4, 11, 26, base2, baseDk, grout, 3);
        FillBricks(tex, 36, 4, 43, 26, base2, baseDk, grout, 3);

        // Decorative stone ring
        for (int x = 8; x <= 39; x++) tex.SetPixel(x, 20, new Color(0.62f, 0.55f, 0.42f));
        for (int x = 4; x <= 43; x++) tex.SetPixel(x, 26, new Color(0.62f, 0.55f, 0.42f));

        // Battlements
        DrawBattlements(tex, 8, 39, 28, base1, baseDk);

        // Cannon carriage (wooden wheels + frame)
        FillRect(tex, 10, 31, 38, 37, wood);
        for (int x = 10; x <= 38; x++)
        { tex.SetPixel(x, 31, woodDk); tex.SetPixel(x, 37, woodDk); }
        // Wheel left
        FillCircle(tex, 14f, 40f, 5f, wood, woodDk);
        tex.SetPixel(14, 40, woodDk); // hub
        // Spokes
        for (int a = 0; a < 4; a++)
        {
            float angle = a * Mathf.PI / 4f;
            for (int r = 1; r <= 4; r++)
            {
                int wx = 14 + Mathf.RoundToInt(Mathf.Cos(angle) * r);
                int wy = 40 + Mathf.RoundToInt(Mathf.Sin(angle) * r);
                if (wx >= 0 && wx < s && wy >= 0 && wy < s) tex.SetPixel(wx, wy, woodDk);
            }
        }
        // Wheel right
        FillCircle(tex, 34f, 40f, 5f, wood, woodDk);
        tex.SetPixel(34, 40, woodDk);
        for (int a = 0; a < 4; a++)
        {
            float angle = a * Mathf.PI / 4f;
            for (int r = 1; r <= 4; r++)
            {
                int wx = 34 + Mathf.RoundToInt(Mathf.Cos(angle) * r);
                int wy = 40 + Mathf.RoundToInt(Mathf.Sin(angle) * r);
                if (wx >= 0 && wx < s && wy >= 0 && wy < s) tex.SetPixel(wx, wy, woodDk);
            }
        }

        // Cannon barrel (pointing right-up at ~30°)
        for (int i = 0; i < 12; i++)
        {
            int bx = 22 + i;
            int by = 34 - i / 3;
            for (int d = -2; d <= 2; d++)
            {
                int py = by + d;
                if (bx >= 0 && bx < s && py >= 0 && py < s)
                {
                    Color bc = (d == -2 || d == 2) ? metalDk : metal;
                    if (d == -2) bc = metalHi;
                    tex.SetPixel(bx, py, bc);
                }
            }
        }
        // Muzzle flash hint
        tex.SetPixel(34, 30, smoke); tex.SetPixel(35, 31, smoke);
        tex.SetPixel(33, 29, smoke); tex.SetPixel(35, 29, smoke);
        // Cannonball in barrel
        FillCircle(tex, 26f, 33f, 2f, metalDk, metal);
    }

    static void DrawGenericTower(Texture2D tex, int s, Color fill, Color border)
    {
        FillBricks(tex, 4, 0, s - 5, s / 2, fill, Color.Lerp(fill, border, 0.5f),
            border, 4);
        DrawBattlements(tex, 4, s - 5, s / 2, fill, border);
    }

    static void DrawPlatform(Texture2D tex, int s, Color stone, Color stoneDk)
    {
        for (int y = 0; y < 5; y++)
            for (int x = 4; x < s - 4; x++)
            {
                Color c = stone;
                if (y == 0 || x == 4 || x == s - 5) c = stoneDk;
                if (y == 4) c = Color.Lerp(stone, stoneDk, 0.3f);
                tex.SetPixel(x, y, c);
            }
    }

    // ==================== ENEMY SPRITES ====================
    public static Sprite CreateEnemySprite(string enemyName)
    {
        int s = 40;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, Color.clear);

        switch (enemyName)
        {
            case "Goblin": DrawGoblin(tex, s); break;
            case "Orc":    DrawOrc(tex, s);    break;
            case "Ghost":  DrawGhost(tex, s);  break;
            default:       DrawDefaultEnemy(tex, s); break;
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.3f), s);
    }

    static void DrawGoblin(Texture2D tex, int s)
    {
        Color body    = new Color(0.30f, 0.72f, 0.22f);
        Color bodyDk  = new Color(0.20f, 0.52f, 0.14f);
        Color bodySh  = new Color(0.16f, 0.40f, 0.10f);
        Color skin    = new Color(0.45f, 0.80f, 0.30f);
        Color eye     = new Color(0.95f, 0.85f, 0.20f);
        Color eyePup  = new Color(0.08f, 0.08f, 0.08f);
        Color mouth   = new Color(0.12f, 0.30f, 0.08f);
        Color tooth   = new Color(0.95f, 0.92f, 0.85f);
        Color leather = new Color(0.48f, 0.30f, 0.12f);
        Color leatherDk=new Color(0.32f, 0.20f, 0.08f);
        Color shadow  = new Color(0f, 0f, 0f, 0.25f);

        // Drop shadow
        for (int x = 6; x <= 30; x++) { tex.SetPixel(x, 3, shadow); tex.SetPixel(x + 1, 3, shadow); }

        // Legs (short and stumpy)
        FillRect(tex, 10, 4, 14, 12, bodyDk);
        FillRect(tex, 17, 4, 21, 12, bodyDk);
        // Feet
        FillRect(tex,  8, 4, 15,  7, bodySh);
        FillRect(tex, 16, 4, 23,  7, bodySh);

        // Body (pear shaped — wider at belly)
        FillCircle(tex, 16f, 17f, 9f, body, bodyDk);
        // Belly highlight
        FillCircle(tex, 15f, 18f, 5f, skin, body);

        // Leather belt
        FillRect(tex, 8, 13, 24, 15, leather);
        tex.SetPixel(15, 14, leatherDk); tex.SetPixel(16, 14, leatherDk); // buckle

        // Stubby arms
        FillRect(tex, 3, 16, 8,  22, bodyDk);   // left arm
        FillRect(tex, 24, 16, 29, 22, bodyDk);  // right arm
        // Hands
        FillCircle(tex, 5f, 23f, 2.5f, body, bodyDk);
        FillCircle(tex, 27f, 23f, 2.5f, body, bodyDk);
        // Claws
        tex.SetPixel(3,  24, bodySh); tex.SetPixel(5, 25, bodySh); tex.SetPixel(7, 24, bodySh);
        tex.SetPixel(25, 24, bodySh); tex.SetPixel(27, 25, bodySh); tex.SetPixel(29, 24, bodySh);

        // Neck
        FillRect(tex, 13, 24, 19, 27, skin);

        // Head (round-ish)
        FillCircle(tex, 16f, 32f, 8f, body, bodyDk);

        // Pointy ears
        int[] earXL = { 6, 5, 4 }; int[] earYL = { 32, 33, 34 };
        int[] earXR = { 26, 27, 28 }; int[] earYR = { 32, 33, 34 };
        for (int i = 0; i < 3; i++) { tex.SetPixel(earXL[i], earYL[i], body); tex.SetPixel(earXR[i], earYR[i], body); }
        // Ear inner
        tex.SetPixel(6, 33, skin); tex.SetPixel(26, 33, skin);

        // Eyes (big, gleaming)
        FillCircle(tex, 12f, 33f, 3f, eye, bodyDk);
        FillCircle(tex, 20f, 33f, 3f, eye, bodyDk);
        tex.SetPixel(12, 33, eyePup); tex.SetPixel(13, 33, eyePup);
        tex.SetPixel(20, 33, eyePup); tex.SetPixel(21, 33, eyePup);
        // Eye shine
        tex.SetPixel(11, 35, new Color(1f, 1f, 1f)); tex.SetPixel(19, 35, new Color(1f, 1f, 1f));

        // Big nose
        FillCircle(tex, 16f, 30f, 2f, bodyDk, bodySh);
        tex.SetPixel(15, 30, bodySh); tex.SetPixel(17, 30, bodySh); // nostrils

        // Wide grin
        for (int x = 11; x <= 21; x++) tex.SetPixel(x, 27, mouth);
        tex.SetPixel(12, 26, mouth); tex.SetPixel(20, 26, mouth); // corners
        // Teeth (two big ones)
        tex.SetPixel(14, 27, tooth); tex.SetPixel(15, 27, tooth);
        tex.SetPixel(18, 27, tooth); tex.SetPixel(19, 27, tooth);
        // Upper teeth protruding
        tex.SetPixel(14, 28, tooth); tex.SetPixel(18, 28, tooth);

        // Small horns / hair spikes
        tex.SetPixel(11, 38, bodySh); tex.SetPixel(10, 39, bodySh);
        tex.SetPixel(16, 39, bodySh); tex.SetPixel(16, 40, bodySh);
        tex.SetPixel(21, 38, bodySh); tex.SetPixel(22, 39, bodySh);

        // Weapon: tiny dagger in right hand
        Color blade = new Color(0.75f, 0.75f, 0.80f);
        Color hilt  = new Color(0.55f, 0.35f, 0.14f);
        FillRect(tex, 30, 17, 31, 22, blade);
        FillRect(tex, 29, 22, 32, 24, hilt);
        tex.SetPixel(30, 16, blade); // tip
    }

    static void DrawOrc(Texture2D tex, int s)
    {
        Color body    = new Color(0.42f, 0.55f, 0.28f);
        Color bodyDk  = new Color(0.30f, 0.40f, 0.18f);
        Color bodySh  = new Color(0.22f, 0.30f, 0.12f);
        Color skin    = new Color(0.52f, 0.65f, 0.35f);
        Color eye     = new Color(0.90f, 0.25f, 0.10f);
        Color eyeWht  = new Color(0.90f, 0.85f, 0.75f);
        Color armor   = new Color(0.48f, 0.48f, 0.52f);
        Color armorDk = new Color(0.30f, 0.30f, 0.34f);
        Color armorHi = new Color(0.66f, 0.66f, 0.72f);
        Color leather = new Color(0.45f, 0.28f, 0.10f);
        Color tusk    = new Color(0.95f, 0.92f, 0.78f);
        Color shadow  = new Color(0f, 0f, 0f, 0.30f);

        // Drop shadow
        for (int x = 4; x <= 36; x++) tex.SetPixel(x, 3, shadow);

        // Massive legs
        FillRect(tex, 7,  4, 14, 14, bodyDk);
        FillRect(tex, 17, 4, 24, 14, bodyDk);
        // Greaves (leg armor)
        FillRect(tex, 7,  4, 14, 10, armorDk);
        FillRect(tex, 17, 4, 24, 10, armorDk);
        for (int x = 7; x <= 14; x++) tex.SetPixel(x, 10, armorHi);
        for (int x = 17; x <= 24; x++) tex.SetPixel(x, 10, armorHi);
        // Boots
        FillRect(tex, 5,  4, 16,  7, bodySh);
        FillRect(tex, 15, 4, 26,  7, bodySh);

        // Huge body
        FillCircle(tex, 16f, 19f, 12f, body, bodyDk);

        // Chest plate armor
        FillRect(tex, 7, 15, 25, 24, armorDk);
        // Armor highlight lines
        for (int y = 16; y <= 23; y += 3) for (int x = 8; x <= 24; x++) tex.SetPixel(x, y, armor);
        for (int x = 8; x <= 24; x++) tex.SetPixel(x, 15, armorHi);
        tex.SetPixel(15, 20, armorHi); tex.SetPixel(16, 20, armorHi); // center boss

        // Shoulder pads
        FillCircle(tex,  5f, 20f, 4f, armor, armorDk);
        FillCircle(tex, 27f, 20f, 4f, armor, armorDk);
        tex.SetPixel(4,  21, armorHi); tex.SetPixel(26, 21, armorHi);
        // Spikes on shoulders
        tex.SetPixel(2, 22, armorDk); tex.SetPixel(1, 23, armorDk);
        tex.SetPixel(29, 22, armorDk); tex.SetPixel(30, 23, armorDk);

        // Arms (massive)
        FillRect(tex, 1, 18, 6, 28, body);
        FillRect(tex, 26, 18, 31, 28, body);
        // Fists
        FillCircle(tex, 3.5f, 30f, 3.5f, bodyDk, bodySh);
        FillCircle(tex, 28.5f, 30f, 3.5f, bodyDk, bodySh);
        // Knuckles
        for (int i = 0; i < 3; i++) tex.SetPixel(2 + i, 32, bodySh);
        for (int i = 0; i < 3; i++) tex.SetPixel(27 + i, 32, bodySh);

        // Neck
        FillRect(tex, 12, 24, 20, 27, skin);

        // Big ugly head
        FillCircle(tex, 16f, 33f, 9f, body, bodyDk);

        // Helmet (iron cap)
        FillRect(tex, 10, 34, 22, 40, armorDk);
        FillRect(tex,  9, 39, 23, 41, armorDk);
        for (int x = 9; x <= 23; x++) tex.SetPixel(x, 41, armorHi);
        // Helmet crest (red)
        Color crest = new Color(0.80f, 0.18f, 0.12f);
        for (int x = 14; x <= 18; x++) for (int y = 40; y <= 43; y++) tex.SetPixel(x, y, crest);

        // Angry eyes
        FillCircle(tex, 12f, 34f, 2.5f, eyeWht, bodyDk);
        FillCircle(tex, 20f, 34f, 2.5f, eyeWht, bodyDk);
        tex.SetPixel(12, 34, eye); tex.SetPixel(13, 34, eye);
        tex.SetPixel(20, 34, eye); tex.SetPixel(21, 34, eye);
        // Eyebrow scar
        FillRect(tex, 10, 36, 15, 37, bodyDk);
        FillRect(tex, 17, 36, 22, 37, bodyDk);

        // Broad flat nose
        FillRect(tex, 14, 31, 18, 33, bodyDk);
        tex.SetPixel(14, 31, bodySh); tex.SetPixel(18, 31, bodySh); // nostrils

        // Snarling mouth
        FillRect(tex, 11, 27, 21, 29, bodySh);
        // Tusks (two big)
        for (int y = 25; y <= 29; y++) { tex.SetPixel(12, y, tusk); tex.SetPixel(20, y, tusk); }
        tex.SetPixel(12, 24, tusk); tex.SetPixel(20, 24, tusk); // tips

        // Weapon: large axe
        Color axeMetal = new Color(0.62f, 0.62f, 0.68f);
        Color axeHi    = new Color(0.80f, 0.80f, 0.88f);
        Color haft     = new Color(0.48f, 0.30f, 0.12f);
        // Haft
        for (int y = 10; y <= 30; y++) tex.SetPixel(35, y, haft);
        // Blade
        FillRect(tex, 33, 22, 39, 30, axeMetal);
        for (int y = 22; y <= 30; y++) tex.SetPixel(33, y, axeHi); // edge
        tex.SetPixel(32, 22, axeHi); tex.SetPixel(32, 30, axeHi);  // tip points
        // Axe top spike
        for (int y = 30; y <= 33; y++) tex.SetPixel(35 + (y - 30), y, axeMetal);
    }

    static void DrawGhost(Texture2D tex, int s)
    {
        Color body    = new Color(0.78f, 0.82f, 0.95f, 0.88f);
        Color bodyDk  = new Color(0.55f, 0.60f, 0.80f, 0.75f);
        Color bodyLt  = new Color(0.92f, 0.94f, 1.00f, 0.90f);
        Color glow    = new Color(0.65f, 0.75f, 1.00f, 0.45f);
        Color glowOut = new Color(0.45f, 0.55f, 0.90f, 0.20f);
        Color eyeBlue = new Color(0.25f, 0.45f, 1.00f);
        Color eyeWht  = new Color(0.90f, 0.92f, 1.00f);
        Color chain   = new Color(0.60f, 0.60f, 0.68f, 0.80f);
        Color chainDk = new Color(0.35f, 0.35f, 0.42f, 0.80f);

        // Outer glow aura
        FillCircle(tex, 20f, 24f, 16f, Color.clear, glowOut);
        FillCircle(tex, 20f, 24f, 14f, Color.clear, glow);

        // Ghostly tail / bottom (wavy)
        var rng2 = new System.Random(55);
        for (int y = 4; y <= 12; y++)
        {
            float wave = Mathf.Sin(y * 0.8f) * 3f;
            int x0 = 10 + (int)(wave);
            int x1 = 30 + (int)(wave);
            for (int x = x0; x <= x1; x++)
                if (x >= 0 && x < s)
                    tex.SetPixel(x, y, Color.Lerp(bodyDk, Color.clear, (12f - y) / 8f));
        }

        // Main ghostly body (teardrop shape)
        for (int y = 10; y <= 34; y++)
        {
            float t = (float)(y - 10) / 24f;
            float w = t < 0.5f ? Mathf.Lerp(5f, 11f, t * 2f) : Mathf.Lerp(11f, 8f, (t - 0.5f) * 2f);
            int cx = 20;
            for (int x = cx - (int)w; x <= cx + (int)w; x++)
            {
                if (x < 0 || x >= s) continue;
                float edge = (Mathf.Abs(x - cx)) / w;
                Color c = Color.Lerp(bodyLt, bodyDk, edge * edge);
                tex.SetPixel(x, y, c);
            }
        }

        // Robe folds
        for (int y = 12; y <= 28; y += 4)
        {
            tex.SetPixel(12, y, bodyDk);
            tex.SetPixel(28, y, bodyDk);
            tex.SetPixel(13, y + 2, bodyDk);
            tex.SetPixel(27, y + 2, bodyDk);
        }

        // Ghostly arms / tendrils
        // Left tendril
        for (int i = 0; i < 8; i++)
        {
            int ax = 10 - i;
            int ay = 22 + i;
            if (ax >= 0 && ay < s) tex.SetPixel(ax, ay, Color.Lerp(body, Color.clear, i / 8f));
        }
        // Right tendril
        for (int i = 0; i < 8; i++)
        {
            int ax = 30 + i;
            int ay = 22 + i;
            if (ax < s && ay < s) tex.SetPixel(ax, ay, Color.Lerp(body, Color.clear, i / 8f));
        }

        // Hood / head
        FillCircle(tex, 20f, 32f, 9f, body, bodyDk);
        // Hood shadow
        FillCircle(tex, 20f, 30f, 6f, bodyLt, body);

        // Hollow glowing eyes
        FillCircle(tex, 16f, 32f, 3f, eyeBlue, bodyDk);
        FillCircle(tex, 24f, 32f, 3f, eyeBlue, bodyDk);
        // Eye glow aura
        FillCircle(tex, 16f, 32f, 4.5f, Color.clear, new Color(eyeBlue.r, eyeBlue.g, eyeBlue.b, 0.4f));
        FillCircle(tex, 24f, 32f, 4.5f, Color.clear, new Color(eyeBlue.r, eyeBlue.g, eyeBlue.b, 0.4f));
        // Eye center slit
        tex.SetPixel(15, 32, eyeWht); tex.SetPixel(16, 32, new Color(0f, 0f, 0f, 0.9f));
        tex.SetPixel(23, 32, eyeWht); tex.SetPixel(24, 32, new Color(0f, 0f, 0f, 0.9f));

        // Mouth (ominous jagged)
        for (int x = 16; x <= 24; x++) tex.SetPixel(x, 28, bodyDk);
        tex.SetPixel(16, 27, bodyDk); tex.SetPixel(18, 29, bodyDk);
        tex.SetPixel(20, 27, bodyDk); tex.SetPixel(22, 29, bodyDk); tex.SetPixel(24, 27, bodyDk);

        // Spectral chain (rattling effect)
        for (int i = 0; i < 6; i++)
        {
            int cx2 = 20 + (int)(Mathf.Sin(i * 1.5f) * 3);
            int cy2 = 6 + i * 2;
            if (cy2 < s) tex.SetPixel(cx2, cy2, chain);
            if (cy2 - 1 >= 0 && cy2 + 1 < s)
            {
                tex.SetPixel(cx2 - 1, cy2, chainDk);
                tex.SetPixel(cx2 + 1, cy2, chain);
            }
        }
    }

    static void DrawDefaultEnemy(Texture2D tex, int s)
    {
        FillCircle(tex, s / 2f, s / 2f, s / 2f - 2f, Color.red, new Color(0.5f, 0f, 0f));
    }

    // ==================== DEATH PARTICLE SPRITE ====================
    // Returns a small colored particle sprite for death effects
    public static Sprite CreateParticleSprite(Color color, int size = 6)
    {
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        float cx = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cx) * (y - cx));
                if (d < cx - 0.5f)
                    tex.SetPixel(x, y, Color.Lerp(color, Color.white, 0.4f));
                else if (d < cx + 0.5f)
                    tex.SetPixel(x, y, color);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    // ==================== PROJECTILE ====================
    public static Sprite CreateProjectileSprite(Color color)
    {
        int s = 10;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        float cx = s / 2f;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - cx + 0.5f;
                float dy = y - cx + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < 3f)
                    tex.SetPixel(x, y, Color.Lerp(color, Color.white, 0.3f));
                else if (dist < 4f)
                    tex.SetPixel(x, y, color);
                else if (dist < 4.8f)
                    tex.SetPixel(x, y, color * 0.6f);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    // ==================== HEALTH BARS ====================
    public static Sprite CreateHealthBarBG()
    {
        int w = 24, h = 4;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool border = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                tex.SetPixel(x, y, border ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.2f, 0.2f, 0.2f));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 24);
    }

    public static Sprite CreateHealthBarFill()
    {
        int w = 22, h = 2;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, new Color(0.2f, 0.9f, 0.2f));
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 0.5f), 24);
    }

    // ==================== RANGE INDICATOR ====================
    public static Sprite CreateRangeIndicator()
    {
        int s = 64;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Bilinear;
        float r = s / 2f;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - r + 0.5f;
                float dy = y - r + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < r - 2f && dist > r - 4f)
                    tex.SetPixel(x, y, new Color(1f, 1f, 0.5f, 0.35f));
                else if (dist < r - 2f)
                    tex.SetPixel(x, y, new Color(1f, 1f, 0.5f, 0.06f));
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    // ==================== DECORATIONS ====================
    public static Sprite CreateTreeSprite()
    {
        int s = 32;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, Color.clear);

        Color trunk  = new Color(0.42f, 0.28f, 0.14f);
        Color trunkH = new Color(0.55f, 0.38f, 0.20f);
        Color leaf1  = new Color(0.15f, 0.48f, 0.10f);
        Color leaf2  = new Color(0.22f, 0.58f, 0.16f);
        Color leaf3  = new Color(0.28f, 0.68f, 0.22f);
        Color leafDk = new Color(0.10f, 0.35f, 0.08f);

        // Trunk with highlight
        for (int y = 0; y < 12; y++)
            for (int x = 14; x <= 18; x++)
            {
                Color c = trunk;
                if (x == 15 || x == 16) c = trunkH;
                tex.SetPixel(x, y, c);
            }

        // Foliage — layered circles for depth
        var rng = new System.Random(77);
        float[][] circles = { new float[] { 16, 18, 7 }, new float[] { 13, 22, 5 }, new float[] { 19, 21, 5 } };
        foreach (var circle in circles)
        {
            float ccx = circle[0], ccy = circle[1], cr = circle[2];
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float dx = x - ccx;
                    float dy = y - ccy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist < cr - 1)
                    {
                        double r = rng.NextDouble();
                        Color c = r < 0.3 ? leaf1 : r < 0.7 ? leaf2 : leaf3;
                        // Lighting — lighter on top
                        if (dy < -cr * 0.3f) c = Color.Lerp(c, leaf3, 0.3f);
                        if (dy > cr * 0.3f) c = Color.Lerp(c, leafDk, 0.3f);
                        tex.SetPixel(x, y, c);
                    }
                    else if (dist < cr)
                    {
                        tex.SetPixel(x, y, leafDk);
                    }
                }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    public static Sprite CreateBushSprite()
    {
        int s = 20;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, Color.clear);

        Color bush1  = new Color(0.22f, 0.52f, 0.15f);
        Color bush2  = new Color(0.30f, 0.65f, 0.22f);
        Color bushDk = new Color(0.15f, 0.38f, 0.10f);
        Color berry  = new Color(0.85f, 0.20f, 0.20f);

        var rng = new System.Random(55);
        float cx = 10, cy = 9;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - cx;
                float dy = (y - cy) * 1.4f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < 7)
                {
                    double r = rng.NextDouble();
                    Color c = r < 0.4 ? bush1 : bush2;
                    if (y < cy - 2) c = Color.Lerp(c, bush2, 0.3f);
                    if (y > cy + 2) c = Color.Lerp(c, bushDk, 0.3f);
                    tex.SetPixel(x, y, c);
                }
                else if (dist < 8)
                    tex.SetPixel(x, y, bushDk);
            }

        // Berries
        tex.SetPixel(7, 10, berry); tex.SetPixel(13, 8, berry); tex.SetPixel(10, 12, berry);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    public static Sprite CreateFlowerSprite()
    {
        int s = 12;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, Color.clear);

        Color stem = new Color(0.2f, 0.5f, 0.15f);
        tex.SetPixel(5, 0, stem); tex.SetPixel(5, 1, stem); tex.SetPixel(5, 2, stem);
        tex.SetPixel(6, 0, stem); tex.SetPixel(6, 1, stem); tex.SetPixel(6, 2, stem);
        // Leaf
        tex.SetPixel(4, 1, stem); tex.SetPixel(7, 2, stem);

        Color petal  = new Color(0.95f, 0.45f, 0.50f);
        Color center = new Color(1.0f, 0.90f, 0.2f);
        // Petals
        tex.SetPixel(5, 5, center); tex.SetPixel(6, 5, center);
        tex.SetPixel(5, 6, center); tex.SetPixel(6, 6, center);
        tex.SetPixel(4, 5, petal); tex.SetPixel(7, 5, petal);
        tex.SetPixel(5, 4, petal); tex.SetPixel(6, 7, petal);
        tex.SetPixel(4, 6, petal); tex.SetPixel(7, 6, petal);
        tex.SetPixel(5, 7, petal); tex.SetPixel(6, 4, petal);
        tex.SetPixel(3, 5, petal); tex.SetPixel(8, 6, petal);
        tex.SetPixel(5, 3, petal); tex.SetPixel(6, 8, petal);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    public static Sprite CreateRockSprite()
    {
        int s = 16;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, Color.clear);

        Color rock   = new Color(0.50f, 0.48f, 0.45f);
        Color rockDk = new Color(0.38f, 0.36f, 0.33f);
        Color rockHi = new Color(0.62f, 0.60f, 0.58f);

        var rng = new System.Random(33);
        float cx = 8, cy = 6;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = (x - cx) * 1.0f;
                float dy = (y - cy) * 1.4f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < 5)
                {
                    Color c = rng.NextDouble() < 0.3 ? rockDk : rock;
                    if (y > cy) c = Color.Lerp(c, rockDk, 0.3f);
                    if (y < cy - 1) c = Color.Lerp(c, rockHi, 0.3f);
                    tex.SetPixel(x, y, c);
                }
                else if (dist < 6)
                    tex.SetPixel(x, y, rockDk);
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    public static Sprite CreateWaterTile()
    {
        int s = 32;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;

        Color water1 = new Color(0.18f, 0.35f, 0.65f);
        Color water2 = new Color(0.22f, 0.42f, 0.72f);
        Color wave   = new Color(0.35f, 0.55f, 0.82f);
        Color shore  = new Color(0.55f, 0.50f, 0.35f);

        var rng = new System.Random(44);
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float wavePattern = Mathf.Sin(x * 0.5f + y * 0.3f) * 0.5f + 0.5f;
                Color c = Color.Lerp(water1, water2, wavePattern);
                // Wave highlights
                if (wavePattern > 0.8f) c = Color.Lerp(c, wave, 0.5f);
                // Shore effect on edges
                float edgeDist = Mathf.Min(x, y, s - 1 - x, s - 1 - y);
                if (edgeDist < 3) c = Color.Lerp(c, shore, (3 - edgeDist) / 3f * 0.5f);
                tex.SetPixel(x, y, c);
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    public static Sprite CreateMapBorder()
    {
        int s = 32;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;

        Color dark  = new Color(0.12f, 0.15f, 0.10f);
        Color stone = new Color(0.35f, 0.32f, 0.28f);
        Color moss  = new Color(0.20f, 0.35f, 0.15f);

        var rng = new System.Random(66);
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                double r = rng.NextDouble();
                Color c;
                if (r < 0.4f) c = dark;
                else if (r < 0.8f) c = stone;
                else c = moss;
                tex.SetPixel(x, y, c);
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    // ==================== BRIDGE TILE (wooden planks over river) ====================
    public static Sprite CreateBridgeTile()
    {
        int s = 32;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;

        // Transparent base
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, Color.clear);

        Color plank     = new Color(0.62f, 0.42f, 0.20f);
        Color plankDk   = new Color(0.45f, 0.30f, 0.13f);
        Color plankHi   = new Color(0.78f, 0.55f, 0.28f);
        Color rail      = new Color(0.38f, 0.24f, 0.10f);
        Color rope      = new Color(0.68f, 0.55f, 0.30f);

        // Horizontal planks (bridge goes left-right)
        int[] plankYStarts = { 5, 9, 13, 17, 21, 25 };
        foreach (int py in plankYStarts)
        {
            for (int x = 2; x < s - 2; x++)
            {
                for (int y = py; y < py + 3 && y < s; y++)
                {
                    Color c = plank;
                    if (y == py) c = plankHi;
                    if (y == py + 2) c = plankDk;
                    if (x == 2 || x == s - 3) c = rail;
                    tex.SetPixel(x, y, c);
                }
            }
        }

        // Side rails (vertical beams)
        for (int y = 3; y < s - 3; y++)
        {
            tex.SetPixel(2,  y, rail);
            tex.SetPixel(3,  y, rail);
            tex.SetPixel(s - 3, y, rail);
            tex.SetPixel(s - 4, y, rail);
        }

        // Rope / cable lines along top and bottom edges
        for (int x = 4; x < s - 4; x++)
        {
            tex.SetPixel(x, 2, rope);
            tex.SetPixel(x, s - 3, rope);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }
}
