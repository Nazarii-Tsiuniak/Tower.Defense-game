using UnityEngine;

public static class SpriteGenerator
{
    // ───────── Cartoon outline thickness (pixels) ─────────
    private const int Outline = 2;

    // ───────── Colour helpers ─────────
    static Color Darken(Color c, float amount = 0.35f) =>
        new Color(c.r * (1f - amount), c.g * (1f - amount), c.b * (1f - amount), c.a);

    static Color Lighten(Color c, float amount = 0.3f) =>
        new Color(Mathf.Min(1f, c.r + amount), Mathf.Min(1f, c.g + amount), Mathf.Min(1f, c.b + amount), c.a);

    static Color Blend(Color a, Color b, float t) => Color.Lerp(a, b, t);

    // ───────── Low-level pixel helpers ─────────
    static void FillCircle(Color[] px, int w, int h, int cx, int cy, int r, Color c)
    {
        for (int y = Mathf.Max(0, cy - r); y < Mathf.Min(h, cy + r); y++)
            for (int x = Mathf.Max(0, cx - r); x < Mathf.Min(w, cx + r); x++)
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                    px[y * w + x] = c;
    }

    static void FillRect(Color[] px, int w, int h, int x0, int y0, int x1, int y1, Color c)
    {
        for (int y = Mathf.Max(0, y0); y < Mathf.Min(h, y1); y++)
            for (int x = Mathf.Max(0, x0); x < Mathf.Min(w, x1); x++)
                px[y * w + x] = c;
    }

    static void FillEllipse(Color[] px, int w, int h, int cx, int cy, int rx, int ry, Color c)
    {
        for (int y = Mathf.Max(0, cy - ry); y < Mathf.Min(h, cy + ry); y++)
            for (int x = Mathf.Max(0, cx - rx); x < Mathf.Min(w, cx + rx); x++)
            {
                float dx = (float)(x - cx) / rx;
                float dy = (float)(y - cy) / ry;
                if (dx * dx + dy * dy <= 1f)
                    px[y * w + x] = c;
            }
    }

    static Sprite ToSprite(Texture2D tex, float ppu = 0f)
    {
        if (ppu <= 0f) ppu = Mathf.Max(tex.width, tex.height);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, ppu);
    }

    static Texture2D NewTex(int w, int h)
    {
        var t = new Texture2D(w, h);
        t.filterMode = FilterMode.Bilinear;
        return t;
    }

    // ═══════════════════════════════════════════════════════
    //  PUBLIC API — basic shapes (kept for backward compat)
    // ═══════════════════════════════════════════════════════

    public static Sprite CreateCircle(int radius, Color color)
    {
        int size = radius * 2;
        var tex = NewTex(size, size);
        Color[] px = new Color[size * size];
        FillCircle(px, size, size, radius, radius, radius, color);
        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex, size);
    }

    public static Sprite CreateRect(int width, int height, Color color)
    {
        var tex = NewTex(width, height);
        Color[] px = new Color[width * height];
        for (int i = 0; i < px.Length; i++) px[i] = color;
        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex);
    }

    public static Sprite CreateDiamond(int size, Color color)
    {
        var tex = NewTex(size, size);
        Color[] px = new Color[size * size];
        int half = size / 2;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int dx = Mathf.Abs(x - half);
                int dy = Mathf.Abs(y - half);
                px[y * size + x] = (dx + dy <= half) ? color : Color.clear;
            }
        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex, size);
    }

    public static Sprite CreateTriangle(int size, Color color)
    {
        var tex = NewTex(size, size);
        Color[] px = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            float wAtY = (float)y / size;
            int hw = Mathf.RoundToInt(wAtY * size / 2);
            int cx = size / 2;
            for (int x = 0; x < size; x++)
                px[y * size + x] = (x >= cx - hw && x <= cx + hw) ? color : Color.clear;
        }
        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex, size);
    }

    // ═══════════════════════════════════════════════════════
    //  CARTOON SPRITES
    // ═══════════════════════════════════════════════════════

    /// <summary>Circle with dark outline and top-down gradient.</summary>
    public static Sprite CreateCartoonCircle(int radius, Color fill)
    {
        int size = radius * 2;
        var tex = NewTex(size, size);
        Color[] px = new Color[size * size];
        Color outline = Darken(fill, 0.55f);
        int cx = radius, cy = radius;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist <= radius)
                {
                    if (dist > radius - Outline)
                        px[y * size + x] = outline;
                    else
                    {
                        float t = (float)y / size;
                        px[y * size + x] = Blend(Lighten(fill, 0.2f), Darken(fill, 0.15f), t);
                    }
                }
            }
        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex, size);
    }

    /// <summary>Rounded rectangle with outline — cartoon panel/platform.</summary>
    public static Sprite CreateCartoonRoundedRect(int w, int h, int r, Color fill)
    {
        var tex = NewTex(w, h);
        Color[] px = new Color[w * h];
        Color outline = Darken(fill, 0.5f);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                // Determine nearest corner distance for rounding
                int cx = x < r ? r : (x >= w - r ? w - r - 1 : x);
                int cy = y < r ? r : (y >= h - r ? h - r - 1 : y);
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                bool inside = (cx == x && cy == y) || dist <= r;

                if (inside)
                {
                    bool border = (x < Outline || x >= w - Outline || y < Outline || y >= h - Outline);
                    if (!border && (cx != x || cy != y))
                        border = dist > r - Outline;
                    if (border)
                        px[y * w + x] = outline;
                    else
                    {
                        float t = (float)y / h;
                        px[y * w + x] = Blend(Lighten(fill, 0.15f), Darken(fill, 0.1f), t);
                    }
                }
            }
        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex);
    }

    // ─────────── Background ───────────

    /// <summary>Grass field with subtle colour noise.</summary>
    public static Sprite CreateGrassBackground(int w, int h)
    {
        var tex = NewTex(w, h);
        Color[] px = new Color[w * h];
        Color baseGreen = new Color(0.36f, 0.68f, 0.30f);

        // Deterministic seed for reproducibility
        Random.State prev = Random.state;
        Random.InitState(42);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float n = Random.Range(-0.06f, 0.06f);
                // Vertical gradient: slightly darker at bottom
                float gy = (float)y / h * 0.08f;
                px[y * w + x] = new Color(
                    baseGreen.r + n - gy,
                    baseGreen.g + n - gy,
                    baseGreen.b + n * 0.5f - gy, 1f);
            }

        // Scatter lighter "grass tuft" dots
        for (int i = 0; i < w * h / 60; i++)
        {
            int gx = Random.Range(2, w - 2);
            int tuftY = Random.Range(2, h - 2);
            Color tuft = new Color(0.45f, 0.78f, 0.35f, 1f);
            px[tuftY * w + gx] = tuft;
            if (gx + 1 < w) px[tuftY * w + gx + 1] = tuft;
            if (tuftY + 1 < h) px[(tuftY + 1) * w + gx] = tuft;
        }

        Random.state = prev;

        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex);
    }

    // ─────────── Path ───────────

    /// <summary>Dirt path tile with border and gravel specks.</summary>
    public static Sprite CreatePathTile(int w, int h)
    {
        var tex = NewTex(w, h);
        Color[] px = new Color[w * h];
        Color dirt = new Color(0.72f, 0.55f, 0.34f);
        Color borderCol = new Color(0.42f, 0.30f, 0.18f);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool border = y < Outline + 1 || y >= h - Outline - 1;
                px[y * w + x] = border ? borderCol : dirt;
            }

        // Gravel specks
        Random.State prev = Random.state;
        Random.InitState(99);
        for (int i = 0; i < w * h / 30; i++)
        {
            int sx = Random.Range(1, w - 1);
            int sy = Random.Range(Outline + 2, h - Outline - 2);
            float v = Random.Range(-0.08f, 0.08f);
            px[sy * w + sx] = new Color(dirt.r + v, dirt.g + v, dirt.b + v, 1f);
        }
        Random.state = prev;

        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex);
    }

    // ─────────── Decorations ───────────

    /// <summary>Cartoon tree: brown trunk + green canopy with outline.</summary>
    public static Sprite CreateTree(int size)
    {
        var tex = NewTex(size, size);
        Color[] px = new Color[size * size];
        Color trunk = new Color(0.45f, 0.28f, 0.12f);
        Color trunkOutline = Darken(trunk, 0.5f);
        Color leaf = new Color(0.22f, 0.62f, 0.22f);
        Color leafOutline = Darken(leaf, 0.5f);
        Color leafHighlight = Lighten(leaf, 0.25f);

        int cx = size / 2;
        // Trunk
        int tw = size / 6, th = size * 2 / 5;
        FillRect(px, size, size, cx - tw - 1, 0, cx + tw + 1, th + 1, trunkOutline);
        FillRect(px, size, size, cx - tw, 1, cx + tw, th, trunk);

        // Canopy (large circle)
        int cr = size * 3 / 8;
        int cy = th + cr - size / 12;
        FillCircle(px, size, size, cx, cy, cr + Outline, leafOutline);
        FillCircle(px, size, size, cx, cy, cr, leaf);
        // Highlight
        FillCircle(px, size, size, cx - cr / 4, cy + cr / 4, cr / 3, leafHighlight);

        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex, size);
    }

    /// <summary>Small cartoon bush.</summary>
    public static Sprite CreateBush(int size)
    {
        var tex = NewTex(size, size);
        Color[] px = new Color[size * size];
        Color leaf = new Color(0.25f, 0.58f, 0.20f);
        Color outline = Darken(leaf, 0.5f);
        Color highlight = Lighten(leaf, 0.2f);

        int cx = size / 2, cy = size / 2;
        int rx = size * 2 / 5, ry = size / 3;
        FillEllipse(px, size, size, cx, cy, rx + Outline, ry + Outline, outline);
        FillEllipse(px, size, size, cx, cy, rx, ry, leaf);
        FillEllipse(px, size, size, cx - rx / 4, cy + ry / 4, rx / 3, ry / 3, highlight);

        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex, size);
    }

    /// <summary>Small flower decoration.</summary>
    public static Sprite CreateFlower(int size, Color petalColor)
    {
        var tex = NewTex(size, size);
        Color[] px = new Color[size * size];
        int cx = size / 2, cy = size / 2;
        int pr = size / 5;

        // Stem
        FillRect(px, size, size, cx - 1, 0, cx + 1, cy - pr, new Color(0.2f, 0.5f, 0.15f));

        // Petals (4 directions + centre)
        FillCircle(px, size, size, cx, cy + pr, pr, petalColor);
        FillCircle(px, size, size, cx, cy - pr, pr, petalColor);
        FillCircle(px, size, size, cx + pr, cy, pr, petalColor);
        FillCircle(px, size, size, cx - pr, cy, pr, petalColor);
        // Centre
        FillCircle(px, size, size, cx, cy, pr - 1, new Color(1f, 0.9f, 0.3f));

        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex, size);
    }

    // ─────────── Stone platform for tower spot ───────────

    public static Sprite CreateStonePlatform(int size)
    {
        var tex = NewTex(size, size);
        Color[] px = new Color[size * size];
        Color stone = new Color(0.65f, 0.63f, 0.58f);
        Color outline = new Color(0.35f, 0.33f, 0.30f);
        Color highlight = new Color(0.78f, 0.76f, 0.70f);

        int cx = size / 2, cy = size / 2;
        int rx = size * 2 / 5, ry = size / 3;

        // Shadow
        FillEllipse(px, size, size, cx + 1, cy - 2, rx, ry, new Color(0f, 0f, 0f, 0.25f));
        // Outline
        FillEllipse(px, size, size, cx, cy, rx + Outline, ry + Outline, outline);
        // Main stone
        FillEllipse(px, size, size, cx, cy, rx, ry, stone);
        // Top highlight
        FillEllipse(px, size, size, cx, cy + ry / 4, rx * 2 / 3, ry / 2, highlight);

        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex, size);
    }

    // ─────────── Enemies ───────────

    /// <summary>Cartoon enemy: body circle + eyes + type features.</summary>
    public static Sprite CreateCartoonEnemy(int size, Color bodyColor, string type)
    {
        var tex = NewTex(size, size);
        Color[] px = new Color[size * size];
        Color outline = Darken(bodyColor, 0.55f);
        int cx = size / 2, cy = size / 2;
        int bodyR = size * 3 / 8;

        // Body outline + body with gradient
        FillCircle(px, size, size, cx, cy, bodyR + Outline, outline);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist <= bodyR)
                {
                    float t = (float)y / size;
                    px[y * size + x] = Blend(Lighten(bodyColor, 0.15f), Darken(bodyColor, 0.1f), t);
                }
            }

        // Eyes
        int eyeR = size / 10;
        int eyeSpacing = size / 6;
        int eyeY = cy + bodyR / 4;
        // White of eyes
        FillCircle(px, size, size, cx - eyeSpacing, eyeY, eyeR + 1, Color.white);
        FillCircle(px, size, size, cx + eyeSpacing, eyeY, eyeR + 1, Color.white);
        // Pupils
        FillCircle(px, size, size, cx - eyeSpacing + 1, eyeY, eyeR / 2 + 1, Color.black);
        FillCircle(px, size, size, cx + eyeSpacing + 1, eyeY, eyeR / 2 + 1, Color.black);

        // Type-specific features
        if (type == "orc")
        {
            // Horns
            int hornR = size / 12;
            FillCircle(px, size, size, cx - bodyR + hornR, cy + bodyR - hornR / 2, hornR, outline);
            FillCircle(px, size, size, cx + bodyR - hornR, cy + bodyR - hornR / 2, hornR, outline);
        }
        else if (type == "ghost")
        {
            // Make body semi-transparent by modifying alpha
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (dist <= bodyR && dist > bodyR - Outline * 2)
                        continue; // keep outline solid
                    if (px[y * size + x].a > 0f)
                        px[y * size + x] = new Color(px[y * size + x].r, px[y * size + x].g, px[y * size + x].b, 0.7f);
                }
            // Wavy bottom
            for (int x = cx - bodyR; x <= cx + bodyR; x++)
            {
                if (x < 0 || x >= size) continue;
                int waveY = cy - bodyR + (int)(Mathf.Sin(x * 0.5f) * 2);
                if (waveY >= 0 && waveY < size)
                    px[waveY * size + x] = outline;
            }
        }
        else if (type == "goblin")
        {
            // Pointed ears
            int earR = size / 10;
            FillCircle(px, size, size, cx - bodyR + 1, cy + bodyR / 3, earR, bodyColor);
            FillCircle(px, size, size, cx + bodyR - 1, cy + bodyR / 3, earR, bodyColor);
            // Mouth (small line)
            FillRect(px, size, size, cx - eyeSpacing, cy - bodyR / 5, cx + eyeSpacing, cy - bodyR / 5 + 2, outline);
        }

        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex, size);
    }

    // ─────────── Towers ───────────

    /// <summary>Cartoon tower sprite with base and top feature.</summary>
    public static Sprite CreateCartoonTower(int size, Color color, string type)
    {
        var tex = NewTex(size, size);
        Color[] px = new Color[size * size];
        Color outline = Darken(color, 0.55f);
        Color highlight = Lighten(color, 0.2f);

        int cx = size / 2;
        int baseW = size / 3;
        int baseH = size * 2 / 3;

        // Tower body — outlined rectangle with gradient
        FillRect(px, size, size, cx - baseW - Outline, 0, cx + baseW + Outline, baseH + Outline, outline);
        for (int y = 0; y < baseH; y++)
        {
            float t = (float)y / baseH;
            Color rowColor = Blend(Darken(color, 0.1f), Lighten(color, 0.1f), t);
            for (int x = cx - baseW; x < cx + baseW; x++)
                if (x >= 0 && x < size)
                    px[y * size + x] = rowColor;
        }

        // Battlements (crenellations) at top
        int bw = baseW * 2 / 3;
        int bh = size / 8;
        for (int i = -2; i <= 2; i += 2)
        {
            int bx = cx + i * (baseW / 2) - bw / 4;
            FillRect(px, size, size, bx - 1, baseH, bx + bw / 2 + 1, baseH + bh + 1, outline);
            FillRect(px, size, size, bx, baseH, bx + bw / 2, baseH + bh, highlight);
        }

        // Type-specific top decoration
        if (type == "archer")
        {
            // Arrow / cross-hair
            int ay = baseH - size / 5;
            FillRect(px, size, size, cx - 1, ay - size / 8, cx + 1, ay + size / 8, outline);
            FillRect(px, size, size, cx - size / 8, ay - 1, cx + size / 8, ay + 1, outline);
        }
        else if (type == "cannon")
        {
            // Cannon barrel
            int by = baseH / 2;
            FillRect(px, size, size, cx + baseW, by - 2, cx + baseW + size / 4, by + 3, outline);
            FillRect(px, size, size, cx + baseW + 1, by - 1, cx + baseW + size / 4 - 1, by + 2, new Color(0.4f, 0.4f, 0.42f));
        }
        else if (type == "mage")
        {
            // Magic orb on top
            int orbR = size / 8;
            int oy = baseH + bh + orbR;
            FillCircle(px, size, size, cx, oy, orbR + 1, outline);
            FillCircle(px, size, size, cx, oy, orbR, new Color(0.6f, 0.3f, 1f));
            FillCircle(px, size, size, cx - orbR / 3, oy + orbR / 3, orbR / 3, new Color(0.85f, 0.7f, 1f));
        }

        // Window / door
        FillRect(px, size, size, cx - baseW / 4, baseH / 4, cx + baseW / 4, baseH / 4 + baseH / 5, outline);

        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex, size);
    }

    // ─────────── Projectile ───────────

    /// <summary>Glowing projectile with bright centre.</summary>
    public static Sprite CreateCartoonProjectile(int radius, Color color)
    {
        int size = radius * 2;
        var tex = NewTex(size, size);
        Color[] px = new Color[size * size];
        int cx = radius, cy = radius;
        Color bright = Lighten(color, 0.4f);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist <= radius)
                {
                    float t = dist / radius;
                    Color c = Blend(bright, new Color(color.r, color.g, color.b, 0f), t);
                    px[y * size + x] = c;
                }
            }
        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex, size);
    }

    // ─────────── Health bar ───────────

    public static Sprite CreateHealthBarBG(int w, int h)
    {
        return CreateCartoonRoundedRect(w, h, 2, new Color(0.2f, 0.2f, 0.2f, 0.8f));
    }

    public static Sprite CreateHealthBarFill(int w, int h)
    {
        var tex = NewTex(w, h);
        Color[] px = new Color[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float t = (float)y / h;
                px[y * w + x] = Blend(new Color(0.2f, 0.75f, 0.2f), new Color(0.15f, 0.55f, 0.15f), t);
            }
        tex.SetPixels(px); tex.Apply();
        return ToSprite(tex);
    }
}
