using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CheckMeshSetting
{
    public static void Process(string[] assetPaths)
    {
        var prefabs = CollectPrefabList(assetPaths);
        foreach (var prefab in prefabs)
        {
            if (ProcessPrefab(prefab))
            {
                EditorUtility.SetDirty(prefab);
                Debug.Log("Dirty: " + prefab.name);
            }
        }
    }
    
    private static bool ProcessPrefab(GameObject prefab)
    {
        var dirty = false;
        var renders = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var render in renders)
        {
            if(render.allowOcclusionWhenDynamic)
            {
                render.allowOcclusionWhenDynamic = false;
                dirty = true;
            }
        }

        return dirty;
    }
    
    private static List<GameObject> CollectPrefabList(string[] assetPaths)
    {
        var prefabs = AssetDatabase.FindAssets("t:Prefab", assetPaths).Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToList();
        return prefabs;
    }
}