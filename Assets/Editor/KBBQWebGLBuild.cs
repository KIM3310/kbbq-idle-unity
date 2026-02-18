#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class KBBQWebGLBuild
{
    private const string ScenePath = "Assets/Scenes/Main.unity";

    [UnityEditor.MenuItem("KBBQ/Build WebGL (docs)")]
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
        var docsDir = Path.Combine(projectRoot, "docs");
        var tempOutDir = Path.Combine(projectRoot, "Temp", "KBBQIdleWebGL");

        if (!File.Exists(Path.Combine(projectRoot, ScenePath)))
        {
            throw new System.Exception("Scene not found: " + ScenePath);
        }

        if (Directory.Exists(tempOutDir))
        {
            Directory.Delete(tempOutDir, recursive: true);
        }
        Directory.CreateDirectory(tempOutDir);
        Directory.CreateDirectory(docsDir);

        // Cloudflare Pages compatibility: prefer uncompressed output for broader browser/server behavior.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = tempOutDir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception("WebGL build failed: " + summary.result);
        }

        var builtBuildDir = Path.Combine(tempOutDir, "Build");
        if (!Directory.Exists(builtBuildDir))
        {
            throw new System.Exception("WebGL build succeeded but Build directory was not found: " + builtBuildDir);
        }

        CopyDirectory(builtBuildDir, Path.Combine(docsDir, "Build"));

        var builtTemplateData = Path.Combine(tempOutDir, "TemplateData");
        if (Directory.Exists(builtTemplateData))
        {
            CopyDirectory(builtTemplateData, Path.Combine(docsDir, "TemplateData"));
        }

        Debug.Log($"WebGL build succeeded: {summary.totalSize / (1024f * 1024f):0.0} MB -> {docsDir}/Build");

        if (interactive)
        {
            EditorUtility.RevealInFinder(docsDir);
        }
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        if (Directory.Exists(destinationDir))
        {
            Directory.Delete(destinationDir, recursive: true);
        }

        Directory.CreateDirectory(destinationDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var name = Path.GetFileName(file);
            File.Copy(file, Path.Combine(destinationDir, name), overwrite: true);
        }

        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            var name = Path.GetFileName(directory);
            CopyDirectory(directory, Path.Combine(destinationDir, name));
        }
    }
}
#endif
