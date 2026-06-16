using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Template.Editor
{
    /// <summary>
    /// Provides command-line build entry points for project automation.
    /// </summary>
    public static class CI
    {
        /// <summary>
        /// Builds the WebGL player from a Unity command-line executeMethod call.
        /// </summary>
        public static void BuildWebGL()
        {
            Build.WebGL();
        }

        /// <summary>
        /// Contains platform-specific build implementations.
        /// </summary>
        public static class Build
        {
            /// <summary>
            /// Builds all enabled scenes into a WebGL player.
            /// </summary>
            public static void WebGL()
            {
                var scenes = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray();

                var buildPath = GetCommandLineArgument("-buildPath") ?? "Build/WebGL";
                var report = BuildPipeline.BuildPlayer(
                    scenes, buildPath, BuildTarget.WebGL, BuildOptions.None);

                if (report.summary.result != BuildResult.Succeeded)
                    throw new Exception($"Build failed: {report.summary.result} " +
                                        $"Errors: {report.summary.totalErrors}");
            }
        }

        private static string GetCommandLineArgument(string argumentName)
        {
            var arguments = Environment.GetCommandLineArgs();
            for (var index = 0; index < arguments.Length - 1; index++)
            {
                if (arguments[index] == argumentName)
                {
                    return arguments[index + 1];
                }
            }

            return null;
        }
    }
}
