using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AdieLab.PineMorphLab.Editor
{
    public static class PineMorphBuild
    {
        private const string ScenePath = "Assets/PineMorphLab/Scenes/PineMorphLab.unity";

        [MenuItem("Tools/PineMorph Lab/Build WebGL")]
        public static void BuildWebGl()
        {
            ConfigurePlayer();
            if (Directory.Exists("Builds/WebGL"))
            {
                Directory.Delete("Builds/WebGL", true);
            }
            Directory.CreateDirectory("Builds/WebGL");
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.template = "PROJECT:PineMorph";
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"PineMorph WebGL build failed: {report.summary.result}");
            }

            File.WriteAllText(Path.Combine("Builds/WebGL", "_headers"),
@"/*
  Cross-Origin-Opener-Policy: same-origin
  Cross-Origin-Embedder-Policy: require-corp
  X-Content-Type-Options: nosniff
");
            Debug.Log("PINEMORPH_WEBGL_BUILD_OK output=Builds/WebGL");
        }

        public static void BuildWebGlFromCommandLine()
        {
            BuildWebGl();
            EditorApplication.Exit(0);
        }

        [MenuItem("Tools/PineMorph Lab/Build Windows")]
        public static void BuildWindows()
        {
            ConfigurePlayer();
            Directory.CreateDirectory("Builds/PineMorphLab");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/PineMorphLab/PineMorphLab.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"PineMorph Windows build failed: {report.summary.result}");
            }

            Debug.Log("PINEMORPH_WINDOWS_BUILD_OK output=Builds/PineMorphLab/PineMorphLab.exe");
        }

        private static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "Adie Lab";
            PlayerSettings.productName = "PineMorph Lab";
            PlayerSettings.stripEngineCode = false;
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
        }
    }
}
