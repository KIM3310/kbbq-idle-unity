using UnityEditor;
using System.Linq;

public class BuildScript
{
    public static void BuildWebGL()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = true;
        
        BuildPipeline.BuildPlayer(scenes, "docs", BuildTarget.WebGL, BuildOptions.None);
    }
}
