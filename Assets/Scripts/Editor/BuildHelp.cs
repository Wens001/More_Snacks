#define LIONKIT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
#if LIONKIT
using LionStudios;
#endif

public class BuildHelp
{
    public int callbackOrder  { get{ return 0 ; }  }

    private static string BundleVersionAdd(string version)
    {
        var sp = version.Split('.');
        if (sp.Length <= 2)
            return "1.0.0";
        
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

#if LIONKIT
    //[MenuItem("MyTools/LionKit Init")]
    //private static void UpdateLionKitData()
    //{
    //    var settings = Resources.Load<LionSettings>("LionStudios.LionSettings");
    //    settings._AppLovin._Enabled = true;
    //    settings._AppLovin._SDKKey = "pIZT0gTB19HmoBhtMBRD2fXDkHJC9HryZOUa4yn552suDTlakAomrJzZbGmlTbg6_Ah46SACU05iTqHG_40rN-";
    //    settings._AppLovin._AndroidInterstitialAdUnitId = "d02f6e5b2bedc326";
    //    settings._AppLovin._AndroidBannerAdUnitId = "2256a2403df79c37";
    //    settings._AppLovin._AndroidRewardedAdUnitId = "c2826d834c1072c5";
    //    settings._AppLovin._AndroidAdMobAppId = "ca-app-pub-3555987499620362~4435059482";

    //    settings._AppLovin._iOSInterstitialAdUnitId = "d02f6e5b2bedc326";
    //    settings._AppLovin._iOSBannerAdUnitId = "2256a2403df79c37";
    //    settings._AppLovin._iOSRewardedAdUnitId = "c2826d834c1072c5";
    //    settings._AppLovin._iOSAdMobAppId = "ca-app-pub-3555987499620362~4435059482";

    //    settings._Facebook._Enabled = true;
    //    settings._Facebook._AppId = "816165869206726";
    //    settings._Facebook._AppName = "More Snacks!";

    //    settings._Adjust._Enabled = true;
    //    settings._Adjust._AndroidToken = "mhwoy0hcojcw";
    //    settings._Adjust._iOSToken = "6yg61cepkqrk";
    //    settings._Adjust._SandboxMode = Adjust.SandboxMode.Off;

    //    settings._Debugging._EnableDebugger = false;
    //}

    [MenuItem("MyTools/Keystore Init")]
    private static void UpdateKeystoreData()
    {
#if UNITY_ANDROID
        PlayerSettings.applicationIdentifier = "com.casualgames.snack";
        PlayerSettings.Android.keystorePass = "DefaultCompany";
        PlayerSettings.Android.keyaliasName = "aaa";
        PlayerSettings.Android.keyaliasPass = "DefaultCompany";
#endif
#if UNITY_IOS
        PlayerSettings.applicationIdentifier = "com.moresnacks.san";
#endif
    }


#endif

    [MenuItem("Builds/Build Android APK",priority = -1)]
    private static void OnBuildAndroid()
    {
        UpdateKeystoreData();
#if UNITY_ANDROID

        if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP
            && (int)PlayerSettings.Android.targetArchitectures >= 2)
        {
            PlayerSettings.Android.bundleVersionCode++;
            PlayerSettings.bundleVersion = BundleVersionAdd(PlayerSettings.bundleVersion);
        }

        var levels = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
                continue;
            levels.Add(scene.path);
        }

        var report = BuildPipeline.BuildPlayer(levels.ToArray(), 
            $"{Application.productName}_{PlayerSettings.bundleVersion}_{PlayerSettings.Android.bundleVersionCode}.apk",
            BuildTarget.Android, BuildOptions.None);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Process.Start("explorer.exe", $"/Select, {report.summary.outputPath.Replace(@"/", @"\")}");
        }
        else
        {
            Debug.LogError("Build Fail");
        }
#endif
    }
}
