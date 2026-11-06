using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PinballRoguelite.Editor
{
    public static class WebGlBuild
    {
        private const string MainScenePath = "Assets/Scenes/Main.unity";
        private const string OutputFolder = "Builds/WebGL";

        [MenuItem("Pinball Roguelite/Build WebGL For GitHub Pages")]
        public static void BuildGithubPages()
        {
            ValidateWebGlBuildSupport();

            string outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", OutputFolder));
            if (Directory.Exists(outputPath) && Directory.GetFileSystemEntries(outputPath).Length > 0)
                throw new InvalidOperationException($"WebGL output already exists: {outputPath}. Clear only this publish folder before creating a fresh build.");

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { MainScenePath },
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"WebGL build failed: {report.summary.result}. Review the Unity build log for details.");

            File.WriteAllText(Path.Combine(outputPath, ".nojekyll"), string.Empty);
            Debug.Log($"PINBALL WEBGL BUILD PASS: {outputPath} ({report.summary.totalSize} bytes)");
        }

        public static void ValidateWebGlBuildSupport()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new BuildFailedException("Unity Web Build Support is not installed for this editor. In Unity Hub, add the WebGL Build Support module for Unity 6000.3.27f2, then run this build again.");
        }
    }
}
