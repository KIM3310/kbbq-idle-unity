#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class KBBQWebGLBuild
{
    private const string ScenePath = "Assets/Scenes/Main.unity";

    [MenuItem("KBBQ/Build WebGL (docs)")]
    public static void BuildWebGLDocsMenu()
    {
        BuildWebGLDocs(interactive: true);
    }

    // For batchmode CLI:
    // Unity -quit -batchmode -projectPath <path> -executeMethod KBBQWebGLBuild.BuildWebGLDocsCLI
    public static void BuildWebGLDocsCLI()
    {
        BuildWebGLDocs(interactive: false);
    }

    private static void BuildWebGLDocs(bool interactive)
    {
        var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var outDir = Path.Combine(projectRoot, "docs");

        if (!File.Exists(Path.Combine(projectRoot, ScenePath)))
        {
            throw new System.Exception("Scene not found: " + ScenePath);
        }

        if (Directory.Exists(outDir))
        {
            // Delete old output to avoid stale files.
            Directory.Delete(outDir, recursive: true);
        }
        Directory.CreateDirectory(outDir);

        // Keep build settings simple and deterministic.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = outDir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception("WebGL build failed: " + summary.result);
        }

        Debug.Log($"WebGL build succeeded: {summary.totalSize / (1024f * 1024f):0.0} MB -> {outDir}");

        if (interactive)
        {
            EditorUtility.RevealInFinder(outDir);
        }
    }
}
#endif

