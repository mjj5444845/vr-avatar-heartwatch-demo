using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class Quest3SetupChecklist
{
    [MenuItem("VR Avatar Demo/Quest 3 Setup Checklist")]
    public static void ShowChecklist()
    {
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
        string manifestPath = Path.Combine(projectRoot, "Packages", "manifest.json");
        string manifest = File.Exists(manifestPath) ? File.ReadAllText(manifestPath) : "";
        bool hasOpenXr = manifest.Contains("com.unity.xr.openxr");
        bool hasMetaOpenXr = manifest.Contains("com.unity.xr.meta-openxr");
        bool hasXri = manifest.Contains("com.unity.xr.interaction.toolkit");
        bool hasAndroidSupport = Directory.Exists(Path.Combine(EditorApplication.applicationContentsPath, "../PlaybackEngines/AndroidPlayer"));

        StringBuilder report = new StringBuilder();
        report.AppendLine("Quest 3 setup status");
        report.AppendLine();
        report.AppendLine($"OpenXR package: {(hasOpenXr ? "OK" : "Missing")}");
        report.AppendLine($"Unity OpenXR Meta package: {(hasMetaOpenXr ? "OK" : "Missing")}");
        report.AppendLine($"XR Interaction Toolkit: {(hasXri ? "OK" : "Missing")}");
        report.AppendLine($"Android Build Support: {(hasAndroidSupport ? "OK" : "Missing")}");
        report.AppendLine();
        report.AppendLine("Manual steps still required:");
        report.AppendLine("1. Install Android Build Support, Android SDK/NDK Tools, and OpenJDK from Unity Hub.");
        report.AppendLine("2. Import Meta XR SDK from Package Manager > My Assets after adding it to your Unity account.");
        report.AppendLine("3. Import your purchased assets from Package Manager > My Assets.");
        report.AppendLine("4. Enable Android OpenXR under Project Settings > XR Plug-in Management.");
        report.AppendLine("5. Enable Meta Quest feature group under OpenXR settings.");
        report.AppendLine("6. Use Build And Run with Quest 3 connected and USB debugging allowed.");

        string reportPath = Path.Combine(Application.dataPath, "Quest3SetupReport.txt");
        File.WriteAllText(reportPath, report.ToString());
        AssetDatabase.Refresh();

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Quest 3 Setup Checklist", report.ToString(), "OK");
        }
    }
}
