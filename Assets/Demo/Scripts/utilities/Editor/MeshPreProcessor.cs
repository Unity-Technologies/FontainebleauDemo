using UnityEngine;
using UnityEditor;

// Automatically convert any texture file with "_bumpmap"
// in its file name into a normal map.

class MyMeshPostprocessor : AssetPostprocessor
{



    void OnPreprocessModel()
    {

        ModelImporter modelImporter = (ModelImporter)assetImporter;


        // Game Assets --------------------------------------------------------------------
        if (assetPath.Contains(".fbx") || assetPath.Contains(".FBX"))
        {
            modelImporter.importMaterials = true;
            modelImporter.materialName = ModelImporterMaterialName.BasedOnMaterialName;
            modelImporter.materialSearch = ModelImporterMaterialSearch.Everywhere;

        }

    }
}
