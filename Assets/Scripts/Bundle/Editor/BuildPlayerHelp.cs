using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class BuildPlayerHelp 
{

    private static string BundleVersionAdd(string version)
    {
        var sp = version.Split('.');
        if (sp.Length == 0)
        {
            Debug.LogError($"Version Error:{version}");
            return version;
        }
        var nums = new int[sp.Length];
        for (int i = 0; i < sp.Length; i++)
            nums[i] = int.Parse(sp[i]);
        for (int i = sp.Length - 1; i > 0; i--)
        {
            nums[i]++;
            if (nums[i] >= 100)
            {
                nums[i] = 0;
                nums[i - 1]++;
            }
        }
        string ver = nums[0].ToString();
        for (int i = 1; i < sp.Length; i++)
            ver = $"{ver}.{nums[i]}";
        return ver;
    }

    static string BuildName = "android";

#if UNITY_ANDROID
    static bool UseKeystore = true;
    static string keystorePass = @"123456";
    static string keyaliasName = @"aaa";
    static string keyaliasPass = @"123456";
#endif


    [MenuItem("Build/Android")]
    public static void BuildAndroid()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            return;
        }

#if UNITY_ANDROID
        if (UseKeystore)
        {
            PlayerSettings.Android.keyaliasName = keyaliasName;
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasPass = keyaliasPass;
        }

        PlayerSettings.Android.bundleVersionCode += 1;
        PlayerSettings.bundleVersion = BundleVersionAdd(PlayerSettings.bundleVersion);

        var levels = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
                continue;
            levels.Add(scene.path);
        }

        var report = BuildPipeline.BuildPlayer(levels.ToArray(), $"{BuildName}{PlayerSettings.bundleVersion}.apk", BuildTarget.Android, BuildOptions.None);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($@"Build Sucess
Output Path:{report.summary.outputPath}
Time:{report.summary.totalTime}
Size:{report.summary.totalSize}");
            Process.Start("explorer.exe", $"/Select, {report.summary.outputPath.Replace(@"/", @"\")}");
        }
        else
        {
            Debug.LogError("Build Fail");
        }
#endif 
    }


}
