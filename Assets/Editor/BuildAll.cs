using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildAll
{
    public static Tester Tester => GameObject.FindObjectOfType<Tester>();
    [MenuItem("BuildAll/Build")]
    public static void Build()
    {
        Tester.Count = 1000;
        Tester.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Adam.prefab");     
        PlayerSettings.gpuSkinning = false;           
        Build("Adam-CPUSkin");
        PlayerSettings.gpuSkinning = true;           
        Build("Adam-ComputeSkin");

        Tester.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AdamGPU.prefab");     
        Tester.EnableInstancing = false;
        Build("Adam-GPUSkin");
        Tester.EnableInstancing = true;
        Build("Adam-GPUSkin-Batched");

        Tester.Count = 100;
        Tester.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Mannequin.prefab");
        PlayerSettings.gpuSkinning = false;
        Build("Mannequin-CPUSkin");
        PlayerSettings.gpuSkinning = true;
        Build("Mannequin-ComputeSkin");

        Tester.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MannequinGPU.prefab");
        Tester.EnableInstancing = false;
        Build("Mannequin-GPUSkin");
        Tester.EnableInstancing = true;
        Build("Mannequin-GPUSkin-Batched");
    }

    private static void Build(string name)
    {
        EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<Object>("Assets/TestSkinning.unity"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        PlayerSettings.applicationIdentifier = "com.gpuskintest." + name.ToLower();
        PlayerSettings.productName = name;
        BuildPipeline.BuildPlayer(new[] { "Assets/TestSkinning.unity" }, Path.Combine("Build", name + ".apk"), BuildTarget.Android, BuildOptions.Development);
    }
}
