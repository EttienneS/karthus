using UnityEditor;

public class Builder
{
    public static string[] Scenes = new[] { "Assets/scenes/Main.unity" };

    public static void BuildWindows()
    {
        BuildPipeline.BuildPlayer(Scenes, "Builds/Windows/x86/Ruby Pegasus.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
        BuildPipeline.BuildPlayer(Scenes, "Builds/Windows/x64/Ruby Pegasus.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
    }

    public static void BuildAndroid()
    {
        BuildPipeline.BuildPlayer(Scenes, "Builds/Android/Ruby Pegasus.apk", BuildTarget.Android, BuildOptions.None);
    }

    public static void BuildWeb()
    {
        BuildPipeline.BuildPlayer(Scenes, "Builds/WebGL/", BuildTarget.WebGL, BuildOptions.Development);
    }
}