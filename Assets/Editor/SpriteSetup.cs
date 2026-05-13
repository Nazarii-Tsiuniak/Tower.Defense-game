#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// One-click tool to copy the uploaded PNG spritesheets into
/// Assets/Resources/Sprites/ so that Resources.Load() can find them at runtime.
///
/// Usage: Unity menu -> Tools -> Setup Sprites
/// </summary>
public class SpriteSetup : EditorWindow
{
    static readonly string[] FILES = { "characters", "towers", "tileset1", "props" };
    const string SRC  = "Assets";
    const string DEST = "Assets/Resources/Sprites";

    [MenuItem("Tools/Setup Sprites")]
    static void Run()
    {
        // Create destination folder
        if (!Directory.Exists(DEST))
        {
            Directory.CreateDirectory(DEST);
            AssetDatabase.Refresh();
        }

        int copied = 0, skipped = 0;
        foreach (var f in FILES)
        {
            string src  = $"{SRC}/{f}.png";
            string dest = $"{DEST}/{f}.png";

            if (!File.Exists(src))
            {
                Debug.LogWarning($"[SpriteSetup] Source not found: {src}");
                skipped++;
                continue;
            }

            File.Copy(src, dest, overwrite: true);
            Debug.Log($"[SpriteSetup] Copied {src} -> {dest}");
            copied++;
        }

        AssetDatabase.Refresh();

        // Configure each texture as Sprite / Point filter
        foreach (var f in FILES)
        {
            string dest = $"{DEST}/{f}.png";
            if (!File.Exists(dest)) continue;

            var importer = AssetImporter.GetAtPath(dest) as TextureImporter;
            if (importer == null) continue;

            importer.textureType        = TextureImporterType.Sprite;
            importer.spriteImportMode   = SpriteImportMode.Single;
            importer.filterMode         = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;

            // Enable Read/Write so we can call Sprite.Create at runtime
            var settings = importer.GetDefaultPlatformTextureSettings();
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(settings);
            importer.isReadable = true;

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Setup Sprites",
            $"Done!\n\nCopied: {copied}\nSkipped: {skipped}\n\nSprites are now in:\n{DEST}",
            "OK");
    }
}
#endif
