using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace BoardRacing.Editor
{
    public static class BoardRacingAndroidBuild
    {
        public static void BuildDebug()
        {
            var outputDirectory = "Builds/Android";
            Directory.CreateDirectory(outputDirectory);

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.defaultcompany.boardracing");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/SampleScene.unity" },
                locationPathName = Path.Combine(outputDirectory, "BoardRacing.apk"),
                target = BuildTarget.Android,
                options = BuildOptions.Development
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"Android build failed: {report.summary.result}");
            }
        }
    }
}
