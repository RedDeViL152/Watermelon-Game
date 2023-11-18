#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using MoreLinq;
using NoxLibrary;

public static class SpriteSheetImporter
{
    [MenuItem("Assets/Spritesheet/Create", false)]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Invoked by Unity")]
    private static void CreateSpritesheet()
    {
        foreach (Object obj in Selection.objects)
        {
            if (obj is Texture2D texture)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                string fileName = Path.GetFileName(assetPath);

                if (fileName.StartsWith("f_")) return;
                if (fileName.Split('.') is string[] dotValues)
                {
                    // Disregard normal maps
                    if (dotValues[0].EndsWith("_n")) return;
                    // Disregard ignore files
                    if (dotValues.Contains("ignore")) return;
                }
                Debug.LogError($"Creating spritesheet for {fileName}");

                if (!SpriteSheetInfo.TryLoadFrom(assetPath, out SpriteSheetInfo ssi)) continue;

                var factory = new SpriteDataProviderFactories();
                factory.Init();
                var dataProvider = factory.GetSpriteEditorDataProviderFromObject(obj);
                dataProvider.InitSpriteEditorDataProvider();
                var assetImporter = dataProvider.targetObject as AssetImporter;

                /* Use the data provider */
                if (assetImporter is TextureImporter textureImporter)
                {
                    SetImporterSettings(textureImporter, true);
                    SetSpritesheetData(texture, dataProvider, ssi);
                    // reset isReadableState
                    textureImporter.isReadable = false;
                }

                // Apply the changes made to the data provider
                dataProvider.Apply();

                // Reimport the asset to have the changes applied
                assetImporter.SaveAndReimport();
            }
        }
        
        static void SetImporterSettings(TextureImporter importer, bool isSpriteSheet)
        {
            #region Feel free to set this to whatever settings your project uses, or even comment lines out
            //importer.mipmapEnabled = true;
            importer.isReadable = true; //This will be used later to slice the sprite sheet then turned back to false, don't worry
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 8192;

            // Set other importer settings
            TextureImporterSettings dat = new TextureImporterSettings();
            importer.ReadTextureSettings(dat);
            dat.spriteMode = (int)SpriteImportMode.Multiple;
            dat.spriteMeshType = SpriteMeshType.FullRect;
            dat.spriteExtrude = 0;
            dat.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(dat);

            #endregion

            if (isSpriteSheet)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
            }

            // Save Importer settings
            importer.SaveAndReimport();
        }

        
        static void SetSpritesheetData(Texture2D texture, ISpriteEditorDataProvider dataProvider, SpriteSheetInfo info)
        {
            // Identify flags
            bool fnbEnabled = false;

            // Acquire spriteRect data from the provider
            int counter = 0;
            List<SpriteRect> spriteRects = dataProvider.GetSpriteRects().ToList();

            // Set rows & columns for the looping limits
            int rows = (texture.height / info.SpriteSize.y).FloorToInt();
            int columns = (texture.width / info.SpriteSize.x).FloorToInt();
            // and begin the loop here
            for (int row = rows - 1; row >= 0; row--)
                for (int column = 0; column < columns; column++)
                {
                    Rect rect = new Rect(column * info.SpriteSize.x, row * info.SpriteSize.y, info.SpriteSize.x, info.SpriteSize.y);
                    
                    if (info.FirstNonBlank)
                    {
                        if (!fnbEnabled)
                        {
                            if (texture.IsBlank(rect)) continue;
                            else fnbEnabled = true;
                        }
                    }
                    else
                    if (info.DontBreakWhenBlank)
                    {
                        if (texture.IsBlank(rect)) continue;
                    }                    
                    else
                    if (texture.IsBlank(rect)) break;

                    if (counter >= spriteRects.Count)
                    {
                        spriteRects.Add(new SpriteRect()
                        {
                            name = info.GetFrameName(counter),
                            rect = rect,
                            alignment = SpriteAlignment.Custom,
                            pivot = (info.Pivot + (info.SpriteSize / 2f)) / info.SpriteSize, //Converts from Naming Convention format to Unity format
                            spriteID = GUID.Generate() // Generate a new GUID for this sprite data
                        });
                    }
                    else
                    {
                        spriteRects[counter].name = info.GetFrameName(counter);
                        spriteRects[counter].rect = rect;
                        spriteRects[counter].alignment = SpriteAlignment.Custom;
                        spriteRects[counter].pivot = (info.Pivot + (info.SpriteSize / 2f)) / info.SpriteSize; //Converts from Naming Convention format to Unity format
                    }
                    counter++;
                }
            
            // Initiate a removal of excess sprite rect data when the newly imported data is less
            while (counter < spriteRects.Count)
                spriteRects.RemoveAt(counter);

            // Write the updated data back to the data provider
            dataProvider.SetSpriteRects(spriteRects.ToArray());
            
            // #if UNITY_2021_2_OR_NEWER
            // Note: This section is only for Unity 2021.2 and newer
            // Get all the existing SpriteName & FileId pairs and look for the Sprite with the selected name
            var spriteNameFileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            var nameFileIdPairs = spriteNameFileIdDataProvider.GetNameFileIdPairs().ToList();

            // Clear list
            nameFileIdPairs.Clear();
            // Iterate and register the new Sprite Rect's name and GUID
            foreach (SpriteRect spriteRect in spriteRects)
                nameFileIdPairs.Add(new SpriteNameFileIdPair(spriteRect.name, spriteRect.spriteID));
            // Add back to the provider
            spriteNameFileIdDataProvider.SetNameFileIdPairs(nameFileIdPairs);
            // End of Unity 2021.2 and newer section
            // #endif
        }
    }
    [MenuItem("Assets/Spritesheet/Create", true)]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Invoked by Unity")]
    private static bool CreateSpritesheetValidation() => Selection.objects.All(o => AssetDatabase.GetAssetPath(o).EndsWith(".png", System.StringComparison.CurrentCultureIgnoreCase));

    [MenuItem("Assets/Spritesheet/Update Pivot", false)]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Invoked by Unity")]
    private static void UpdatePivotSpriteSheet()
    {
        foreach (Object obj in Selection.objects)
        {
            if (obj is Texture2D texture)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                string fileName = Path.GetFileName(assetPath);

                if (fileName.StartsWith("f_")) return;
                if (fileName.Split('.') is string[] dotValues)
                {
                    // Disregard normal maps
                    if (dotValues[0].EndsWith("_n")) return;
                    // Disregard ignore files
                    if (dotValues.Contains("ignore")) return;
                }
                Debug.LogError($"Updating pivot for for {fileName}");

                if (!SpriteSheetInfo.TryLoadFrom(assetPath, out SpriteSheetInfo ssi)) continue;

                var factory = new SpriteDataProviderFactories();
                factory.Init();
                var dataProvider = factory.GetSpriteEditorDataProviderFromObject(obj);
                dataProvider.InitSpriteEditorDataProvider();
                var assetImporter = dataProvider.targetObject as AssetImporter;

                /* Use the data provider */
                if (assetImporter is TextureImporter textureImporter)
                {
                    SetSpritesheetData(texture, dataProvider, ssi);
                }

                // Apply the changes made to the data provider
                dataProvider.Apply();

                // Reimport the asset to have the changes applied
                assetImporter.SaveAndReimport();
            }
        }
        static void SetSpritesheetData(Texture2D texture, ISpriteEditorDataProvider dataProvider, SpriteSheetInfo info)
        {
            // Get all the existing Sprites
            var spriteRects = dataProvider.GetSpriteRects();

            // Loop over all Sprites and update the pivots
            foreach (var rect in spriteRects)
            {
                rect.alignment = SpriteAlignment.Custom;
                rect.pivot = (info.Pivot + (info.SpriteSize / 2f)) / info.SpriteSize;
            }

            // Write the updated data back to the data provider
            dataProvider.SetSpriteRects(spriteRects);
        }
    }
    [MenuItem("Assets/Spritesheet/Update Pivot", true)]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Invoked by Unity")]
    private static bool UpdatePivotSpriteSheetValidation() => Selection.objects.All(o => AssetDatabase.GetAssetPath(o).EndsWith(".png", System.StringComparison.CurrentCultureIgnoreCase));

}

// public class SpritesheetImport : AssetPostprocessor
// {
//     protected void OnPreprocessTexture()
//     {
//         string fileName = Path.GetFileName(assetPath);

//         if (fileName.StartsWith("f_")) return;
//         if (fileName.Split('.') is string[] dotValues)
//         {
//             // Disregard normal maps
//             if (dotValues[0].EndsWith("_n")) return;
//             // Disregard ignore files
//             if (dotValues.Contains("ignore")) return;
//         }
//         if (assetImporter is TextureImporter importer)
//             SetImporterSettings(importer, SpriteSheetInfo.TryLoadFrom(fileName, out _));
//     }

//     private static void SetImporterSettings(TextureImporter importer, bool isSpriteSheet)
//     {
//         #region Feel free to set this to whatever settings your project uses, or even comment lines out
//         //importer.mipmapEnabled = true;
//         importer.isReadable = true; //This will be used later to slice the sprite sheet then turned back to false, don't worry
//         importer.filterMode = FilterMode.Point;
//         importer.textureCompression = TextureImporterCompression.Uncompressed;
//         importer.maxTextureSize = 8192;

//         // Set other importer settings
//         TextureImporterSettings dat = new TextureImporterSettings();
//         importer.ReadTextureSettings(dat);
//         dat.spriteMode = (int)SpriteImportMode.Multiple;
//         dat.spriteMeshType = SpriteMeshType.FullRect;
//         dat.spriteExtrude = 0;
//         dat.spriteGenerateFallbackPhysicsShape = false;
//         importer.SetTextureSettings(dat);

//         #endregion

//         if (isSpriteSheet)
//         {
//             importer.textureType = TextureImporterType.Sprite;
//             importer.spriteImportMode = SpriteImportMode.Multiple;
//         }
//     }

//     protected void OnPostprocessTexture(Texture2D texture)
//     {
//         string fileName = Path.GetFileName(assetPath);

//         if (fileName.Split('.') is string[] dotValues)
//         {
//             // Disregard normal maps
//             if (dotValues[0].EndsWith("_n")) return;
//             // Disregard ignore files
//             if (dotValues.Contains("ignore")) return;
//         }

//         if (!(assetImporter is TextureImporter importer)) return;
//         if (!SpriteSheetInfo.TryLoadFrom(assetPath, out SpriteSheetInfo ssi)) return;

//         var factory = new SpriteDataProviderFactories();
//         factory.Init();
//         var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
//         dataProvider.InitSpriteEditorDataProvider();

//         /* Use the data provider */
//         SetSpritesheetData(texture, dataProvider, ssi);
//         importer.isReadable = false;

//         // Apply the changes made to the data provider
//         dataProvider.Apply();

//         // Reimport the asset to have the changes applied
//         AssetDatabase.RenameAsset(assetPath, ssi.ToString());
//         AssetDatabase.ForceReserializeAssets(new List<string>() { assetPath });
//         AssetDatabase.RenameAsset(assetPath, ssi.ToString());
//         assetImporter.SaveAndReimport();
//     }

//     static void SetSpritesheetData(Texture2D texture, ISpriteEditorDataProvider dataProvider, SpriteSheetInfo info)
//     {
//         // Identify flags
//         bool fnbEnabled = false;

//         // Acquire spriteRect data from the provider
//         int counter = 0;
//         List<SpriteRect> spriteRects = dataProvider.GetSpriteRects().ToList();

//         // Set rows & columns for the looping limits
//         int rows = (texture.height / info.SpriteSize.y).FloorToInt();
//         int columns = (texture.width / info.SpriteSize.x).FloorToInt();
//         // and begin the loop here
//         for (int row = rows - 1; row >= 0; row--)
//             for (int column = 0; column < columns; column++)
//             {
//                 Rect rect = new Rect(column * info.SpriteSize.x, row * info.SpriteSize.y, info.SpriteSize.x, info.SpriteSize.y);
                
//                 if (info.FirstNonBlank)
//                 {
//                     if (!fnbEnabled)
//                     {
//                         if (texture.IsBlank(rect)) continue;
//                         else fnbEnabled = true;
//                     }
//                 }
//                 else
//                 if (info.DontBreakWhenBlank)
//                 {
//                     if (texture.IsBlank(rect)) continue;
//                 }                    
//                 else
//                 if (texture.IsBlank(rect)) break;

//                 if (counter >= spriteRects.Count)
//                 {
//                     spriteRects.Add(new SpriteRect()
//                     {
//                         name = info.GetFrameName(counter),
//                         rect = rect,
//                         alignment = SpriteAlignment.Custom,
//                         pivot = (info.Pivot + (info.SpriteSize / 2f)) / info.SpriteSize, //Converts from Naming Convention format to Unity format
//                         spriteID = GUID.Generate() // Generate a new GUID for this sprite data
//                     });
//                 }
//                 else
//                 {
//                     spriteRects[counter].name = info.GetFrameName(counter);
//                     spriteRects[counter].rect = rect;
//                     spriteRects[counter].alignment = SpriteAlignment.Custom;
//                     spriteRects[counter].pivot = (info.Pivot + (info.SpriteSize / 2f)) / info.SpriteSize; //Converts from Naming Convention format to Unity format
//                 }
//                 counter++;
//             }
        
//         // Initiate a removal of excess sprite rect data when the newly imported data is less
//         while (counter < spriteRects.Count)
//             spriteRects.RemoveAt(counter);
        
//         // Note: This section is only for Unity 2021.2 and newer
//         // Get all the existing SpriteName & FileId pairs and look for the Sprite with the selected name
//         var spriteNameFileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
//         var nameFileIdPairs = spriteNameFileIdDataProvider.GetNameFileIdPairs().ToList();

//         // Clear list
//         nameFileIdPairs.Clear();
//         // Iterate and register the new Sprite Rect's name and GUID
//         foreach (SpriteRect spriteRect in spriteRects)
//             nameFileIdPairs.Add(new SpriteNameFileIdPair(spriteRect.name, spriteRect.spriteID));
//         // Add back to the provider
//         spriteNameFileIdDataProvider.SetNameFileIdPairs(nameFileIdPairs);
//         // End of Unity 2021.2 and newer section

//         // Write the updated data back to the data provider
//         dataProvider.SetSpriteRects(spriteRects.ToArray());
//     }
// }
#endif