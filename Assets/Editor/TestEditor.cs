using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using RShell;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

public static class TestEditor
{
    [MenuItem("Test/CollectUnusedMeshComponents")]
    public static void CollectUnusedMeshComponents()
    {
        FindUnusedMeshAttr.Process(new[] { "Assets" });
    }
    
    [MenuItem("Test/CeckMeshSetting")]
    public static void CeckMeshSetting()
    {
        CheckMeshSetting.Process(new[] { "Assets" });
    }
}