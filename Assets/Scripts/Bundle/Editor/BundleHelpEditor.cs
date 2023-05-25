using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
public class BundleHelpEditor
{
    [MenuItem("Bundle/Build/TempFolder")]
    private static void BulidAssetBundlesToTemp()
    {
        var options = BuildAssetBundleOptions.ChunkBasedCompression;
        string outPath = TempPath();
        DeleteFolder(outPath);
        BulidAssetBundles(outPath, options);
    }

    [MenuItem("Bundle/Copy/To StreamingAseets")]
    private static void CopyToStreaming()
    {
        var src = TempPath();
        var dst = StreamingAssetBundlePath();
        DeleteFolder(dst);
        CopyFolder(src, dst);
        AssetDatabase.Refresh();
    }

    [MenuItem("Bundle/Copy/To TempFolder")]
    private static void CopyToTempFolder()
    {
        var src = StreamingAssetBundlePath();
        var dst = TempPath();
        DeleteFolder(dst);
        CopyFolder(src, dst);
        AssetDatabase.Refresh();
    }

    private static string StreamingAssetBundlePath()
    {
        return BundleManager.StreamingAssetBundlePath(true);
    }

    private static string TempPath()
    {
        string outPath = Application.dataPath;
        outPath = outPath.Remove(outPath.Length - 6) + @"Bundle/";
#if UNITY_ANDROID
        outPath = outPath + "Android/";
#elif UNITY_IPHONE 
        outPath = outPath + "IOS/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
        outPath = outPath + "Win/";
#endif
        return outPath;
    }

    [MenuItem("Bundle/Build/StreamingAseets",priority =2)]
    private static void BulidAssetBundlesToStreamingAseets()
    {
        var outPath = StreamingAssetBundlePath();
        var options = BuildAssetBundleOptions.ChunkBasedCompression;
        BulidAssetBundles(outPath, options);
    }

    private static void BulidAssetBundles(string outPath, BuildAssetBundleOptions options)
    {
        CreateDirectory(outPath);
        DeleteFolder(outPath);
        var bundle = BuildAssetBundle(outPath, options);
        EncryptionBundle(bundle, outPath);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="outPath"></param>
    private static void CreateDirectory(string outPath)
    {
        if (string.IsNullOrEmpty(outPath))
            return;
        if (!Directory.Exists(outPath))
            Directory.CreateDirectory(outPath);
    }

    /// <summary>
    /// 构建Bundle
    /// </summary>
    /// <param name="outPath"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    private static AssetBundleManifest BuildAssetBundle(string outPath, BuildAssetBundleOptions options)
    {
#if UNITY_ANDROID
        var bundle = BuildPipeline.BuildAssetBundles(outPath,options,BuildTarget.Android);
        Debug.Log("Android平台打包成功");
#elif UNITY_IPHONE
        var bundle = BuildPipeline.BuildAssetBundles(outPath, options, BuildTarget.iOS);
        Debug.Log("IOS平台打包成功");
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
        var bundle = BuildPipeline.BuildAssetBundles(outPath, options,
            BuildTarget.StandaloneWindows);
        Debug.Log("PC平台打包成功");
#endif
        return bundle;
    }

    /// <summary>
    /// Bundle加密
    /// </summary>
    /// <param name="bundle"></param>
    /// <param name="outPath"></param>
    private static void EncryptionBundle(AssetBundleManifest bundle,string outPath)
    {
        if (bundle == null)
            return;
        foreach (var name in bundle.GetAllAssetBundles())
        {
            var uniqueSalt = Encoding.UTF8.GetBytes(name);
            var data = File.ReadAllBytes(Path.Combine(outPath, name));
            using (var myStream = new MyStream(Path.Combine(outPath, name), FileMode.Create))
            {
                myStream.Write(data, 0, data.Length);
            }
        }
    }

    [MenuItem("Bundle/Clear/StreamingAseets")]
    public static void DeleteStreamingAssetsPath()
    {
        var path = StreamingAssetBundlePath();
        DeleteFolder(path);
        AssetDatabase.Refresh();
    }

    [MenuItem("Bundle/Clear/TempFolder")]
    public static void DeleteTempAssetsPath()
    {
        var path = TempPath();
        DeleteFolder(path);
    }

    /// <summary>
    /// 删除文件夹
    /// </summary>
    /// <param name="path"></param>
    public static void DeleteFolder(string path)
    {
        if (!Directory.Exists(path))
            return;
        foreach (string d in Directory.GetFileSystemEntries(path))
        {
            if (File.Exists(d))
            {
                FileInfo fi = new FileInfo(d);
                if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                    fi.Attributes = FileAttributes.Normal;
                File.Delete(d);//直接删除其中的文件  
            }
            else
            {
                DirectoryInfo d1 = new DirectoryInfo(d);
                if (d1.GetFiles().Length != 0)
                {
                    DeleteFolder(d1.FullName);
                }
                Directory.Delete(d);
            }
        }
    }

    [MenuItem("Bundle/Open Temp Path")]
    public static void OpenTempFolder()
    {
        var path = TempPath();
        CreateDirectory(path);
        EditorUtility.RevealInFinder(path);
    }

    /// <summary>
    /// 复制文件夹所有文件
    /// </summary>
    /// <param name="sourcePath">源目录</param>
    /// <param name="destPath">目的目录</param>
    public static void CopyFolder(string sourcePath, string destPath)
    {
        if (!Directory.Exists(sourcePath))
            return;
        if (!Directory.Exists(destPath))
            Directory.CreateDirectory(destPath);
        List<string> files = new List<string>(Directory.GetFiles(sourcePath));
        files.ForEach(c =>
        {
            string destFile = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
            File.Copy(c, destFile, true);//覆盖模式
        });
        List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));
        folders.ForEach(c =>
        {
            string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
            CopyFolder(c, destDir);
        });
    }
}