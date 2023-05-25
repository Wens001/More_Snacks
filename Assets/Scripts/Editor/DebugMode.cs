using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class DebugMode : Editor
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [MenuItem("MyTools/游戏模式/DebugMode")]
    static void ChangeToDebugMode() {
        WriteTextFile(Path.Combine(Application.streamingAssetsPath, "DebugMode.txt"), "1");
        Debug.Log("Change to debugMode");
    }
    [MenuItem("MyTools/游戏模式/ReleaseMode")]
    static void ChangeToReleaseMode()
    {
        WriteTextFile(Path.Combine(Application.streamingAssetsPath, "DebugMode.txt"), "0");
        Debug.Log("Change to ReleaseMode");
        ChangeToNoSnadbox();
    }
    [MenuItem("MyTools/游戏模式/沙盒模式")]
    static void ChangeToSnadbox()
    {
        WriteTextFile(Path.Combine(Application.dataPath, "Resources/SandboxMode.txt"), "1");
        Debug.Log("沙盒模式");
    }
    [MenuItem("MyTools/游戏模式/非沙盒模式")]
    static void ChangeToNoSnadbox()
    {
        WriteTextFile(Path.Combine(Application.dataPath, "Resources/SandboxMode.txt"), "0");
        Debug.Log("非沙盒模式");
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    static void WriteTextFile(string filePath,string content)
    {
        StreamWriter sw = new StreamWriter(filePath);
        sw.WriteLine(content);
        sw.Flush();
        sw.Close();
    }
}
