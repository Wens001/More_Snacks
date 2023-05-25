using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
/*****************************************
文件:   AssetsExtension.cs
作者:   Siran
日期:   2021/7/1 11:15:10
功能:   资源Inspector面板拓展
*****************************************/
[HelpURL("https://github.com/Siran1994")]

[CustomEditor(typeof(UnityEditor.SceneAsset))]
public class SceneInspector : Editor
{
    public override void OnInspectorGUI()
    {
        string path = AssetDatabase.GetAssetPath(target);
        GUI.enabled = true;       
        if (path.EndsWith(".unity"))
        {
            if (GUILayout.Button("我是场景"))
            {
                List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
                bool hasExist = false;
                foreach (EditorBuildSettingsScene scene in scenes) {
                    if (scene.path == path) {
                        hasExist = true;
                        break;
                    }
                }
                if (!hasExist)
                {
                    scenes.Add(new EditorBuildSettingsScene(path, true));
                    EditorBuildSettings.scenes = scenes.ToArray();
                    Debug.Log("add scene!");
                }
                else {
                    Debug.LogWarning("can't add scene!");
                }

            }
        }
    }

    public static bool ExistSameScene(string path)
    {
        string[] TmpPath1 = path.Split('/');
       
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                string[] TmpPath2= scene.path.Split('/');
                if (TmpPath1[TmpPath1.Length - 1] == TmpPath2[TmpPath2.Length - 1])
                    return true;
            }
        }
        return false;
    }
}
[CustomEditor(typeof(UnityEditor.DefaultAsset))]
public class CustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        string path = AssetDatabase.GetAssetPath(target);
        GUI.enabled = true;       
        if (path.EndsWith(".txt"))
        {
            GUILayout.Button("我是文档");
        }
        else if (path.EndsWith(".abc"))
        {
            GUILayout.Button("我是abc");
        }       
        else if (path.EndsWith(""))//文件夹要放到最后，因为所有的结尾均为空
        {
            GUILayout.Button("我是文件夹");
        }
    }
}
