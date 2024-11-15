using UnityEditor;
using UnityEngine;

public class CustomSpritePostProcessor : AssetPostprocessor
{
    void OnPostprocessTexture(Texture2D texture)
    {
        // Customize texture settings here
        if (assetPath.Contains("Game/Textures"))
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.textureType = TextureImporterType.Sprite;
            importer.mipmapEnabled = false;

            var andPlatform = importer.GetPlatformTextureSettings("Android");
            andPlatform.overridden = true;
            andPlatform.format = TextureImporterFormat.ETC2_RGBA8Crunched;
            andPlatform.maxTextureSize = 1024;
            andPlatform.compressionQuality = 50;
            andPlatform.crunchedCompression = true;
            importer.SetPlatformTextureSettings(andPlatform);

            var iPhonePlatform = importer.GetPlatformTextureSettings("iPhone");
            iPhonePlatform.overridden = true;
            iPhonePlatform.format = TextureImporterFormat.ETC2_RGBA8Crunched;
            iPhonePlatform.maxTextureSize = 1024;
            iPhonePlatform.compressionQuality = 50;
            iPhonePlatform.crunchedCompression = true;
            importer.SetPlatformTextureSettings(iPhonePlatform);

            importer.SaveAndReimport();
        }
    }
}