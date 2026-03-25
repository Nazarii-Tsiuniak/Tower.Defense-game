using UnityEngine;

public static class SpriteGenerator
{
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

    public static Sprite CreateGrassTile()
    {
        int s = 32;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        Color baseColor = new Color(0.35f, 0.65f, 0.25f);
        Color darkGrass = new Color(0.30f, 0.58f, 0.20f);
        Color borderColor = new Color(0.28f, 0.52f, 0.18f);
        var rng = new System.Random(42);
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                bool isBorder = x == 0 || x == s - 1 || y == 0 || y == s - 1;
                if (isBorder)
                    tex.SetPixel(x, y, borderColor);
                else
                    tex.SetPixel(x, y, rng.NextDouble() < 0.15 ? darkGrass : baseColor);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    public static Sprite CreatePathTile()
    {
        int s = 32;
        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        Color baseColor = new Color(0.72f, 0.62f, 0.42f);
        Color darkDirt = new Color(0.65f, 0.55f, 0.38f);
        Color borderColor = new Color(0.58f, 0.48f, 0.32f);
        var rng = new System.Random(99);
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                bool isBorder = x == 0 || x == s - 1 || y == 0 || y == s - 1;
                if (isBorder)
                    tex.SetPixel(x, y, borderColor);
                else
                    tex.SetPixel(x, y, rng.NextDouble() < 0.2 ? darkDirt : baseColor);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    public static Sprite CreateEntryMarker()
    {
        return CreateRect(32, 32, new Color(0.2f, 0.8f, 0.2f, 0.6f), new Color(0.1f, 0.6f, 0.1f));
    }

    public static Sprite CreateBaseMarker()
    {
        return CreateRect(32, 32, new Color(0.9f, 0.2f, 0.2f, 0.6f), new Color(0.7f, 0.1f, 0.1f));
    }

    public static Sprite CreateTowerSprite(string towerName)
    {
        int s = 32;
        Color fill, border;
        switch (towerName)
        {
            case "Archer":  fill = new Color(0.25f, 0.70f, 0.25f); border = new Color(0.15f, 0.50f, 0.15f); break;
            case "Mage":    fill = new Color(0.55f, 0.25f, 0.85f); border = new Color(0.35f, 0.15f, 0.65f); break;
            case "Freezer": fill = new Color(0.35f, 0.70f, 0.95f); border = new Color(0.20f, 0.50f, 0.75f); break;
            case "Cannon":  fill = new Color(0.85f, 0.50f, 0.15f); border = new Color(0.65f, 0.35f, 0.10f); break;
            default:        fill = Color.gray; border = Color.black; break;
        }

        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        float r = s / 2f;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - r + 0.5f;
                float dy = y - r + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < r - 3f)
                    tex.SetPixel(x, y, fill);
                else if (dist < r - 1f)
                    tex.SetPixel(x, y, border);
                else
                    tex.SetPixel(x, y, Color.clear);
            }

        // draw a simple letter in the center
        char letter = towerName[0];
        DrawLetter(tex, s / 2 - 3, s / 2 - 4, letter, Color.white);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    public static Sprite CreateEnemySprite(string enemyName)
    {
        int s = 24;
        Color fill, border;
        switch (enemyName)
        {
            case "Goblin": fill = new Color(0.40f, 0.80f, 0.30f); border = new Color(0.25f, 0.55f, 0.18f); break;
            case "Orc":    fill = new Color(0.55f, 0.40f, 0.30f); border = new Color(0.38f, 0.28f, 0.20f); break;
            case "Ghost":  fill = new Color(0.85f, 0.85f, 0.95f, 0.7f); border = new Color(0.60f, 0.60f, 0.70f, 0.8f); break;
            default:       fill = Color.red; border = Color.black; break;
        }

        var tex = new Texture2D(s, s);
        tex.filterMode = FilterMode.Point;
        float r = s / 2f;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = x - r + 0.5f;
                float dy = y - r + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < r - 2.5f)
                    tex.SetPixel(x, y, fill);
                else if (dist < r - 1f)
                    tex.SetPixel(x, y, border);
                else
                    tex.SetPixel(x, y, Color.clear);
            }

        // eyes
        tex.SetPixel(s / 2 - 3, s / 2 + 2, Color.black);
        tex.SetPixel(s / 2 + 2, s / 2 + 2, Color.black);
        tex.SetPixel(s / 2 - 3, s / 2 + 3, Color.black);
        tex.SetPixel(s / 2 + 2, s / 2 + 3, Color.black);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    public static Sprite CreateProjectileSprite(Color color)
    {
        return CreateCircle(8, color, color * 0.7f);
    }

    public static Sprite CreateHealthBarBG()
    {
        int w = 24, h = 4;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, new Color(0.2f, 0.2f, 0.2f));
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
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.3f));
                else if (dist < r - 2f)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.05f));
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
    }

    static void DrawLetter(Texture2D tex, int startX, int startY, char c, Color color)
    {
        bool[,] pixels = GetLetterPixels(c);
        if (pixels == null) return;
        int w = pixels.GetLength(0);
        int h = pixels.GetLength(1);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (pixels[x, y])
                    tex.SetPixel(startX + x, startY + (h - 1 - y), color);
    }

    static bool[,] GetLetterPixels(char c)
    {
        switch (c)
        {
            case 'A': return new bool[,] {
                {false,true,true,true,false},
                {true,false,false,false,true},
                {true,true,true,true,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {false,true,true,true,false},
                {false,false,true,false,false}
            };
            case 'M': return new bool[,] {
                {true,false,false,false,true},
                {true,true,false,true,true},
                {true,false,true,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true}
            };
            case 'F': return new bool[,] {
                {true,true,true,true,true},
                {true,false,false,false,false},
                {true,true,true,true,false},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,false,false,false,false}
            };
            case 'C': return new bool[,] {
                {false,true,true,true,false},
                {true,false,false,false,true},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,false,false,false,true},
                {false,true,true,true,false}
            };
            default: return null;
        }
    }
}
