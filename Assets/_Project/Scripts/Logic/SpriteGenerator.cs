using UnityEngine;

public static class SpriteGenerator
{
    public static Sprite CreateCircle(int radius, Color color)
    {
        int size = radius * 2;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = dist <= radius ? color : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }

    public static Sprite CreateRect(int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);
        tex.filterMode = FilterMode.Point;

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, width, height), Vector2.one * 0.5f, Mathf.Max(width, height));
    }

    public static Sprite CreateDiamond(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        Color[] pixels = new Color[size * size];
        int half = size / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = Mathf.Abs(x - half);
                int dy = Mathf.Abs(y - half);
                pixels[y * size + x] = (dx + dy <= half) ? color : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }

    public static Sprite CreateTriangle(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            float widthAtY = (float)y / size;
            int halfWidth = Mathf.RoundToInt(widthAtY * size / 2);
            int center = size / 2;

            for (int x = 0; x < size; x++)
            {
                pixels[y * size + x] = (x >= center - halfWidth && x <= center + halfWidth) ? color : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }
}
