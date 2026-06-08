using System.IO;
using UnityEditor;

public static class DemoBuildPipeline
{
    private const string ScenePath = "Assets/Scenes/Quest3LivingRoomAvatar.unity";

    [MenuItem("VR Avatar Demo/Build/macOS Demo App")]
    public static void BuildMacOS()
    {
        BuildPlayer(
            BuildTarget.StandaloneOSX,
            "../../release/macOS/VRAvatarHeartWatch.app"
        );
    }

    [MenuItem("VR Avatar Demo/Build/Windows Demo App")]
    public static void BuildWindows()
    {
        BuildPlayer(
            BuildTarget.StandaloneWindows64,
            "../../release/Windows/VRAvatarHeartWatch.exe"
        );
    }

    [MenuItem("VR Avatar Demo/Build/Quest 3 Android APK")]
    public static void BuildQuest3Apk()
    {
        BuildPlayer(
            BuildTarget.Android,
            "../../release/Quest3/VRAvatarHeartWatch.apk"
        );
    }

    public static void BuildMacOSFromCommandLine()
    {
        BuildMacOS();
    }

    public static void BuildWindowsFromCommandLine()
    {
        BuildWindows();
    }

    public static void BuildQuest3FromCommandLine()
    {
        BuildQuest3Apk();
    }

    private static void BuildPlayer(BuildTarget target, string outputPath)
    {
        string fullOutputPath = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath));

        BuildPipeline.BuildPlayer(
            new[] { ScenePath },
            fullOutputPath,
            target,
            BuildOptions.None
        );
    }
}
