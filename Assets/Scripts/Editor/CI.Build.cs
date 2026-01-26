using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class CI
{
    public static class Build
    {
        public static void WebGL()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            var buildPath = "Build/WebGL";
            var report = BuildPipeline.BuildPlayer(
                scenes, buildPath, BuildTarget.WebGL, BuildOptions.None);

            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception($"Build failed: {report.summary.result} " +
                                    $"Errors: {report.summary.totalErrors}");
        }
    }
}
