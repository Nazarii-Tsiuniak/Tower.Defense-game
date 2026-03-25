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
        int s = 32;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;

        // Clear
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, Color.clear);

        switch (towerName)
        {
            case "Archer":
                DrawArcherTower(tex, s);
                break;
            case "Mage":
                DrawMageTower(tex, s);
                break;
            case "Freezer":
                DrawFreezerTower(tex, s);
                break;
            case "Cannon":
                DrawCannonTower(tex, s);
                break;
            default:
                DrawGenericTower(tex, s, Color.gray, Color.black);
                break;
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    static void DrawArcherTower(Texture2D tex, int s)
    {
        Color base1  = new Color(0.25f, 0.60f, 0.20f);
        Color base2  = new Color(0.18f, 0.48f, 0.15f);
        Color wood   = new Color(0.55f, 0.38f, 0.18f);
        Color bowStr = new Color(0.90f, 0.85f, 0.70f);

        // Stone platform
        DrawPlatform(tex, s, new Color(0.50f, 0.50f, 0.48f), new Color(0.38f, 0.38f, 0.36f));
        // Tower body
        for (int y = 4; y < 22; y++)
            for (int x = 10; x < 22; x++)
            {
                Color c = (x + y) % 3 == 0 ? base2 : base1;
                if (x == 10 || x == 21) c = base2;
                tex.SetPixel(x, y, c);
            }
        // Top platform
        for (int x = 8; x < 24; x++)
            for (int y = 22; y < 25; y++)
                tex.SetPixel(x, y, new Color(0.45f, 0.45f, 0.42f));
        // Bow
        tex.SetPixel(16, 26, wood); tex.SetPixel(16, 27, wood);
        tex.SetPixel(16, 28, wood); tex.SetPixel(16, 29, wood);
        tex.SetPixel(15, 25, wood); tex.SetPixel(17, 25, wood);
        tex.SetPixel(14, 26, wood); tex.SetPixel(18, 26, wood);
        tex.SetPixel(14, 29, wood); tex.SetPixel(18, 29, wood);
        tex.SetPixel(15, 30, wood); tex.SetPixel(17, 30, wood);
        // String
        for (int y = 26; y < 30; y++)
            tex.SetPixel(15, y, bowStr);
        // Arrow
        for (int x = 15; x < 22; x++)
            tex.SetPixel(x, 27, new Color(0.7f, 0.6f, 0.3f));
        tex.SetPixel(22, 28, new Color(0.7f, 0.3f, 0.2f));
        tex.SetPixel(22, 26, new Color(0.7f, 0.3f, 0.2f));
    }

    static void DrawMageTower(Texture2D tex, int s)
    {
        Color base1 = new Color(0.50f, 0.22f, 0.72f);
        Color base2 = new Color(0.38f, 0.15f, 0.58f);
        Color orb   = new Color(0.90f, 0.50f, 1.0f);
        Color orbGl = new Color(1.0f, 0.80f, 1.0f);

        DrawPlatform(tex, s, new Color(0.45f, 0.40f, 0.50f), new Color(0.32f, 0.28f, 0.38f));
        // Tower body — narrowing upward
        for (int y = 4; y < 24; y++)
        {
            float t = (y - 4f) / 20f;
            int halfW = (int)Mathf.Lerp(7, 4, t);
            int cx = s / 2;
            for (int x = cx - halfW; x <= cx + halfW; x++)
            {
                Color c = (x + y) % 4 == 0 ? base2 : base1;
                if (x == cx - halfW || x == cx + halfW) c = base2;
                tex.SetPixel(x, y, c);
            }
        }
        // Crystal orb on top
        float orbCx = 16, orbCy = 27;
        for (int y = 24; y < s; y++)
            for (int x = 12; x < 20; x++)
            {
                float dx = x - orbCx;
                float dy = y - orbCy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < 3f)
                    tex.SetPixel(x, y, orb);
                else if (dist < 3.8f)
                    tex.SetPixel(x, y, base2);
            }
        // Orb glow highlight
        tex.SetPixel(15, 28, orbGl);
        tex.SetPixel(14, 27, orbGl);
    }

    static void DrawFreezerTower(Texture2D tex, int s)
    {
        Color base1 = new Color(0.40f, 0.70f, 0.92f);
        Color base2 = new Color(0.28f, 0.55f, 0.78f);
        Color ice   = new Color(0.80f, 0.92f, 1.0f);
        Color iceGl = new Color(0.95f, 0.98f, 1.0f);

        DrawPlatform(tex, s, new Color(0.42f, 0.50f, 0.55f), new Color(0.30f, 0.38f, 0.42f));
        // Tower body
        for (int y = 4; y < 22; y++)
            for (int x = 10; x < 22; x++)
            {
                Color c = (x + y) % 3 == 0 ? base2 : base1;
                if (x == 10 || x == 21) c = base2;
                tex.SetPixel(x, y, c);
            }
        // Ice crystal top
        int cx = 16;
        // Diamond shape
        for (int dy = 0; dy < 8; dy++)
        {
            int halfW = dy < 4 ? dy : 7 - dy;
            for (int dx = -halfW; dx <= halfW; dx++)
            {
                int px = cx + dx;
                int py = 22 + dy;
                if (px >= 0 && px < s && py >= 0 && py < s)
                    tex.SetPixel(px, py, ice);
            }
        }
        // Ice sparkles
        tex.SetPixel(16, 28, iceGl); tex.SetPixel(15, 26, iceGl);
        tex.SetPixel(17, 25, iceGl);
        // Frost particles around
        tex.SetPixel(8, 20, ice); tex.SetPixel(24, 18, ice);
        tex.SetPixel(9, 14, ice); tex.SetPixel(23, 16, ice);
    }

    static void DrawCannonTower(Texture2D tex, int s)
    {
        Color base1  = new Color(0.72f, 0.45f, 0.18f);
        Color base2  = new Color(0.58f, 0.35f, 0.12f);
        Color metal  = new Color(0.35f, 0.35f, 0.38f);
        Color metalH = new Color(0.50f, 0.50f, 0.55f);

        DrawPlatform(tex, s, new Color(0.50f, 0.45f, 0.40f), new Color(0.38f, 0.33f, 0.28f));
        // Thick tower body
        for (int y = 4; y < 20; y++)
            for (int x = 8; x < 24; x++)
            {
                Color c = (x + y) % 3 == 0 ? base2 : base1;
                if (x == 8 || x == 23) c = base2;
                if (y == 4 || y == 19) c = base2;
                tex.SetPixel(x, y, c);
            }
        // Cannon barrel
        for (int y = 18; y < 28; y++)
            for (int x = 13; x < 20; x++)
            {
                Color c = metal;
                if (x == 13 || x == 19) c = new Color(0.25f, 0.25f, 0.28f);
                if (y == 27) c = metalH;
                tex.SetPixel(x, y, c);
            }
        // Cannon mouth
        for (int x = 14; x < 19; x++)
            tex.SetPixel(x, 28, new Color(0.15f, 0.15f, 0.18f));
        // Cannonball inside
        tex.SetPixel(16, 27, new Color(0.20f, 0.20f, 0.22f));
        tex.SetPixel(15, 27, new Color(0.20f, 0.20f, 0.22f));
        tex.SetPixel(17, 27, new Color(0.20f, 0.20f, 0.22f));
    }

    static void DrawGenericTower(Texture2D tex, int s, Color fill, Color border)
    {
        DrawPlatform(tex, s, new Color(0.50f, 0.50f, 0.48f), new Color(0.38f, 0.38f, 0.36f));
        float r = s / 2f;
        for (int y = 4; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - r + 0.5f;
                float dy = y - r + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < r - 3f) tex.SetPixel(x, y, fill);
                else if (dist < r - 1f) tex.SetPixel(x, y, border);
            }
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
        int s = 24;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;

        // Clear
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, Color.clear);

        switch (enemyName)
        {
            case "Goblin":
                DrawGoblin(tex, s);
                break;
            case "Orc":
                DrawOrc(tex, s);
                break;
            case "Ghost":
                DrawGhost(tex, s);
                break;
            default:
                DrawDefaultEnemy(tex, s);
                break;
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    static void DrawGoblin(Texture2D tex, int s)
    {
        Color body   = new Color(0.40f, 0.80f, 0.30f);
        Color bodyDk = new Color(0.28f, 0.60f, 0.20f);
        Color ear    = new Color(0.35f, 0.70f, 0.25f);

        // Body — smaller, faster looking
        float cx = s / 2f, cy = s / 2f - 1;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - cx;
                float dy = (y - cy) * 1.2f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < 7) tex.SetPixel(x, y, body);
                else if (dist < 8.5f) tex.SetPixel(x, y, bodyDk);
            }
        // Pointy ears
        tex.SetPixel(4, 16, ear); tex.SetPixel(3, 17, ear); tex.SetPixel(2, 18, ear);
        tex.SetPixel(19, 16, ear); tex.SetPixel(20, 17, ear); tex.SetPixel(21, 18, ear);
        // Eyes — big and mischievous
        tex.SetPixel(9, 14, Color.white); tex.SetPixel(10, 14, Color.white);
        tex.SetPixel(14, 14, Color.white); tex.SetPixel(15, 14, Color.white);
        tex.SetPixel(10, 14, Color.black); tex.SetPixel(15, 14, Color.black);
        // Grin
        tex.SetPixel(10, 10, bodyDk); tex.SetPixel(11, 9, bodyDk);
        tex.SetPixel(12, 9, bodyDk); tex.SetPixel(13, 9, bodyDk); tex.SetPixel(14, 10, bodyDk);
    }

    static void DrawOrc(Texture2D tex, int s)
    {
        Color body   = new Color(0.50f, 0.38f, 0.28f);
        Color bodyDk = new Color(0.38f, 0.28f, 0.20f);
        Color armor  = new Color(0.45f, 0.45f, 0.48f);

        // Larger body
        float cx = s / 2f, cy = s / 2f;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - cx;
                float dy = (y - cy) * 0.9f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < 9) tex.SetPixel(x, y, body);
                else if (dist < 10.5f) tex.SetPixel(x, y, bodyDk);
            }
        // Armor plate
        for (int y = 6; y < 12; y++)
            for (int x = 8; x < 16; x++)
                tex.SetPixel(x, y, armor);
        // Eyes — angry
        tex.SetPixel(9, 15, Color.white); tex.SetPixel(10, 15, Color.white);
        tex.SetPixel(14, 15, Color.white); tex.SetPixel(15, 15, Color.white);
        tex.SetPixel(10, 15, new Color(0.8f, 0.2f, 0.1f));
        tex.SetPixel(15, 15, new Color(0.8f, 0.2f, 0.1f));
        // Angry brow
        tex.SetPixel(8, 16, bodyDk); tex.SetPixel(9, 17, bodyDk);
        tex.SetPixel(16, 16, bodyDk); tex.SetPixel(15, 17, bodyDk);
        // Tusks
        tex.SetPixel(10, 11, Color.white); tex.SetPixel(14, 11, Color.white);
    }

    static void DrawGhost(Texture2D tex, int s)
    {
        Color body   = new Color(0.85f, 0.85f, 0.95f, 0.7f);
        Color bodyDk = new Color(0.65f, 0.65f, 0.78f, 0.6f);
        Color glow   = new Color(0.95f, 0.95f, 1.0f, 0.5f);

        // Ghost shape — rounded top, wavy bottom
        float cx = s / 2f, cy = s / 2f + 2;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - cx;
                float dy = (y - cy);
                if (y > cy) dy *= 0.7f; // Squish bottom
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Wavy bottom
                if (y < 6)
                {
                    float wave = Mathf.Sin(x * 1.2f) * 2f;
                    if (y < 4 + wave) continue;
                }

                if (dist < 8) tex.SetPixel(x, y, body);
                else if (dist < 9.5f) tex.SetPixel(x, y, bodyDk);
            }
        // Glowing eyes
        tex.SetPixel(9, 14, new Color(0.3f, 0.5f, 1.0f));
        tex.SetPixel(10, 14, new Color(0.3f, 0.5f, 1.0f));
        tex.SetPixel(14, 14, new Color(0.3f, 0.5f, 1.0f));
        tex.SetPixel(15, 14, new Color(0.3f, 0.5f, 1.0f));
        tex.SetPixel(10, 15, new Color(0.5f, 0.7f, 1.0f));
        tex.SetPixel(15, 15, new Color(0.5f, 0.7f, 1.0f));
        // Glow effect
        tex.SetPixel(12, 18, glow); tex.SetPixel(13, 19, glow);
    }

    static void DrawDefaultEnemy(Texture2D tex, int s)
    {
        float r = s / 2f;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - r + 0.5f;
                float dy = y - r + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < r - 2.5f) tex.SetPixel(x, y, Color.red);
                else if (dist < r - 1f) tex.SetPixel(x, y, new Color(0.6f, 0f, 0f));
            }
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
}
