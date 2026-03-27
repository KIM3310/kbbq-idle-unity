#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        WriteBuildManifest(Path.Combine(docsDir, "Build"));

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

    private static void WriteBuildManifest(string docsBuildDir)
    {
        if (!Directory.Exists(docsBuildDir))
        {
            return;
        }

        var fileNames = Directory.GetFiles(docsBuildDir)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (fileNames.Count == 0)
        {
            return;
        }

        var buildName = DetectBuildBaseName(fileNames);
        var manifest = new BuildManifest
        {
            generatedAtUtc = DateTime.UtcNow.ToString("o"),
            buildName = buildName,
            dataUrl = PrefixBuildPath(PickFirst(fileNames, buildName, ".data", ".data.unityweb", ".data.br", ".data.gz")),
            frameworkUrl = PrefixBuildPath(PickFirst(fileNames, buildName, ".framework.js", ".framework.js.unityweb", ".framework.js.br", ".framework.js.gz")),
            codeUrl = PrefixBuildPath(PickFirst(fileNames, buildName, ".wasm", ".wasm.unityweb", ".wasm.br", ".wasm.gz")),
            symbolsUrl = PrefixBuildPath(PickFirst(fileNames, buildName, ".symbols.json", ".symbols.json.unityweb", ".symbols.json.br", ".symbols.json.gz")),
            loaderUrl = PrefixBuildPath(PickFirst(fileNames, buildName, ".loader.js")),
            companyName = string.IsNullOrWhiteSpace(PlayerSettings.companyName) ? "KBBQ" : PlayerSettings.companyName,
            productName = string.IsNullOrWhiteSpace(PlayerSettings.productName) ? "KBBQ Idle" : PlayerSettings.productName,
            productVersion = string.IsNullOrWhiteSpace(PlayerSettings.bundleVersion) ? "1.0" : PlayerSettings.bundleVersion,
            files = fileNames.Select(PrefixBuildPath).ToArray(),
        };

        var manifestPath = Path.Combine(docsBuildDir, "build-manifest.json");
        File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, prettyPrint: true));
    }

    private static string DetectBuildBaseName(List<string> fileNames)
    {
        var loader = fileNames.FirstOrDefault(name => name.EndsWith(".loader.js", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(loader))
        {
            return loader[..^(".loader.js".Length)];
        }

        var markers = new[]
        {
            ".data",
            ".data.unityweb",
            ".data.br",
            ".data.gz",
            ".framework.js",
            ".wasm",
        };

        for (var i = 0; i < fileNames.Count; i++)
        {
            var name = fileNames[i];
            for (var j = 0; j < markers.Length; j++)
            {
                var marker = markers[j];
                var index = name.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (index > 0)
                {
                    return name[..index];
                }
            }
        }

        return "KBBQIdleWebGL";
    }

    private static string PickFirst(List<string> fileNames, string buildName, params string[] suffixes)
    {
        var lookup = new HashSet<string>(fileNames, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < suffixes.Length; i++)
        {
            var exact = buildName + suffixes[i];
            if (lookup.Contains(exact))
            {
                return exact;
            }
        }

        for (var i = 0; i < suffixes.Length; i++)
        {
            var suffix = suffixes[i];
            for (var j = 0; j < fileNames.Count; j++)
            {
                var candidate = fileNames[j];
                if (candidate.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }
        }

        return string.Empty;
    }

    private static string PrefixBuildPath(string fileName)
    {
        return string.IsNullOrWhiteSpace(fileName) ? string.Empty : "Build/" + fileName;
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

    [Serializable]
    private class BuildManifest
    {
        public string generatedAtUtc;
        public string buildName;
        public string dataUrl;
        public string frameworkUrl;
        public string codeUrl;
        public string symbolsUrl;
        public string loaderUrl;
        public string companyName;
        public string productName;
        public string productVersion;
        public string[] files;
    }
}
#endif
