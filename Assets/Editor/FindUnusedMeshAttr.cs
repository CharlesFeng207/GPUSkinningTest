using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class FindUnusedMeshAttr
{
    private class MeshRefInfo
    {
        public Mesh Mesh;
        public GameObject Prefab;
        public Material Material;
    }

    [Serializable]
    public class Report
    {
        public ReportItem[] Items;
    }

    [Serializable]
    public class ReportItem
    {
        public string MeshName;
        public string AssetPath;
        public List<VertexAttribute> UnusedAttributes;
    }

    public static Report Process(string[] assetPaths)
    {
        var prefabs = CollectPrefabList(assetPaths);
        var meshRefInfos = new List<MeshRefInfo>();
        foreach (var prefab in prefabs)
        {
            CollectPrefabRefInfo(prefab, meshRefInfos);
        }

        var report = AnalyzeUnusedAttributes(meshRefInfos);
        var jsonStr = JsonUtility.ToJson(report, true);
        var outputPath = Path.Combine(Application.dataPath, "UnusedMeshComponents.json");
        File.WriteAllText(outputPath, jsonStr);
        Debug.Log($"Output to {outputPath}");
        Application.OpenURL(outputPath);
        return report;
    }
    
    private static List<GameObject> CollectPrefabList(string[] assetPaths)
    {
        var prefabs = AssetDatabase.FindAssets("t:Prefab", assetPaths).Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToList();
        return prefabs;
    }

    private static void CollectPrefabRefInfo(GameObject prefab, List<MeshRefInfo> meshRefInfos)
    {
        var renders = prefab.GetComponentsInChildren<Renderer>();
        foreach (var render in renders)
        {
            ExtractMeshAndMaterial(render, out var mesh, out var materials);
            if (mesh != null && materials != null && materials.Length > 0)
            {
                foreach (var material in materials)
                {
                    if (material == null)
                        continue;

                    meshRefInfos.Add(new MeshRefInfo()
                    {
                        Mesh = mesh,
                        Prefab = prefab,
                        Material = material,
                    });
                }
            }
        }
    }

    private static void ExtractMeshAndMaterial(Renderer renderer, out Mesh mesh, out Material[] materials)
    {
        if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
        {
            mesh = skinnedMeshRenderer.sharedMesh;
            materials = skinnedMeshRenderer.sharedMaterials;
        }
        else if (renderer is MeshRenderer meshRenderer)
        {
            mesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh;
            materials = meshRenderer.sharedMaterials;
        }
        else
        {
            mesh = null;
            materials = null;
        }
    }

    // todo: 动态解析shader获取需要的属性
    private static List<VertexAttribute> GetShaderNeedAttrList(Shader shader)
    {
        var result = new List<VertexAttribute>();
        if (shader == null)
            return result;

        switch (shader.name)
        {
            case "Unlit/MyUnlitShader":
                result.Add(VertexAttribute.Position);
                result.Add(VertexAttribute.TexCoord0);
                break;
            case "GPUSkinning/GPUSkinning_Unlit_Skin2":
                result.Add(VertexAttribute.Position);
                result.Add(VertexAttribute.TexCoord0);
                result.Add(VertexAttribute.TexCoord1);
                result.Add(VertexAttribute.TexCoord2);
                break;
            case "Standard (Specular setup)":
                break;
            default:
                Debug.LogWarning($"Unknown shader: {shader.name}");
                return null;
        }

        return result;
    }

    private static Report AnalyzeUnusedAttributes(List<MeshRefInfo> meshRefInfos)
    {
        var meshNeedAttributes = new Dictionary<Mesh, HashSet<VertexAttribute>>();
        var skippedMeshes = new HashSet<Mesh>();
        foreach (var meshRefInfo in meshRefInfos)
        {
            if (!meshNeedAttributes.ContainsKey(meshRefInfo.Mesh))
                meshNeedAttributes.Add(meshRefInfo.Mesh, new HashSet<VertexAttribute>());

            var needAttrList = GetShaderNeedAttrList(meshRefInfo.Material.shader);
            if (needAttrList == null)
            {
                if (!skippedMeshes.Contains(meshRefInfo.Mesh))
                {
                    Debug.LogWarning($"{AssetDatabase.GetAssetPath(meshRefInfo.Mesh)} skipped!");
                    skippedMeshes.Add(meshRefInfo.Mesh);
                }

                continue;
            }

            meshNeedAttributes[meshRefInfo.Mesh].UnionWith(needAttrList);
        }

        var result = new Report();
        var fileItems = new List<ReportItem>();
        foreach (var meshNeedAttribute in meshNeedAttributes)
        {
            if (skippedMeshes.Contains(meshNeedAttribute.Key))
                continue;

            List<VertexAttribute> exist = meshNeedAttribute.Key.GetVertexAttributes().Select(x => x.attribute).ToList();
            List<VertexAttribute> required = meshNeedAttribute.Value.ToList();
            List<VertexAttribute> unused = exist.Except(required).ToList();
            
            fileItems.Add(new ReportItem()
            {
                MeshName = meshNeedAttribute.Key.name,
                AssetPath = AssetDatabase.GetAssetPath(meshNeedAttribute.Key),
                UnusedAttributes = unused,
            });
        }
        
        result.Items = fileItems.ToArray();
        return result;
    }
}