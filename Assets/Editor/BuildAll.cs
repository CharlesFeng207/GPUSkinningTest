using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class BuildAll
{
    public static Tester Tester => GameObject.FindObjectOfType<Tester>();
    private static List<string> Builds = new List<string>();

    [MenuItem("BuildAll/Start &B")]
    public static void Start()
    {
        Builds.Clear();

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

        RunTest();
    }

    private static void Build(string name)
    {
        EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/TestSkinning.unity"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        PlayerSettings.applicationIdentifier = "com.gpuskin.test";
        PlayerSettings.productName = name;
        BuildPipeline.BuildPlayer(new[] { "Assets/TestSkinning.unity" }, Path.Combine("Build", name + ".apk"), BuildTarget.Android, BuildOptions.Development);
        Builds.Add(name);
    }

    private static void RunTest()
    {
        RunCmd($"python RunTest.py {string.Join(";", Builds)}");
    }

    private static void RunCmd(string cmd)
    {
        // Create a new process
        Process process = new Process();

        // Set the process start info
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = "/c " + cmd;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;

        // Start the process
        process.StartInfo = startInfo;
        process.Start();

        // Read the output and display it
        StringBuilder output = new StringBuilder();
        while (!process.StandardOutput.EndOfStream)
        {
            string line = process.StandardOutput.ReadLine();
            output.AppendLine(line);
        }

        // Close the process
        process.WaitForExit();
        process.Close();

        // Display the captured output
        Debug.Log(output.ToString());
    }

}
