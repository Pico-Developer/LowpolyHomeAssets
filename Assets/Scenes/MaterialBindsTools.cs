using System.IO;
using UnityEditor;
using UnityEngine;

public class MaterialBindsTools : EditorWindow
{
    [MenuItem("Assets/Test", false, 1)]
    static void BindTexture()
    {
        Object[] gameObjects = Selection.objects;
        string[] strs = Selection.assetGUIDs;

        bool success = false;

        for (int i = 0, len = strs.Length; i < len; i++)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(strs[i]);

            if (!assetPath.ToLower().EndsWith(".mat")) continue;

            var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            var tex = mat.mainTexture;

            if (!tex) continue;

            var mainTexPath = AssetDatabase.GetAssetPath(mat.mainTexture);

            if (!mainTexPath.ToLower().Contains("_albedo")) continue;

            var dir = Path.GetDirectoryName(mainTexPath);
            dir = "Assets\\Scenes\\Tex";
            var fileName = Path.GetFileNameWithoutExtension(mainTexPath);
            var extension = Path.GetExtension(mainTexPath);

            var albedoPath = dir + Path.DirectorySeparatorChar + fileName + extension;
            var albedoTex = AssetDatabase.LoadAssetAtPath<Texture>(albedoPath);
            if (albedoTex)
            {
                mat.mainTexture = albedoTex;
            }


            var aoPath = dir + Path.DirectorySeparatorChar +
                         fileName.Replace("_Albedo", "_AO") +
                         extension;


            var aoTex = AssetDatabase.LoadAssetAtPath<Texture>(aoPath);

            if (aoTex)
            {
                mat.EnableKeyword("_OCCLUSIONMAP");
                mat.SetTexture("_OcclusionMap", aoTex);
            }

            var emissionPath = dir + Path.DirectorySeparatorChar +
                               fileName.Replace("_Albedo", "_Emission") +
                               extension;
            var emissionTex = AssetDatabase.LoadAssetAtPath<Texture>(emissionPath);

            if (emissionTex)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetTexture("_EmissionMap", emissionTex);
                mat.SetColor("_EmissionColor", Color.gray);
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
                mat.SetTexture("_EmissionMap", null);
                mat.SetColor("_EmissionColor", Color.black);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }

            success = true;

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        if (success)
        {
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.Log("Must Choose Materials Files.");
        }
    }
}