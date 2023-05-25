using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class RefreshScene : Editor
{
    /// <summary>
    /// 构建场景设置为所有
    /// </summary>
    [MenuItem("Tools/BuildSettings/add All scenes to buildSettings(同步所有场景到SceneSetting文件)")]
    public static void AddAllScenesToBuildSettings()
    {
        HashSet<string> sceneNames = new HashSet<string>();


        string[] paths = new string[] { "Assets/Scenes" };     //指定场景所在的文件夹名称，如果是“Assets”,则代表工程里面所有的场景
        string[] sceneArr = AssetDatabase.FindAssets("t:Scene", paths);


        foreach (string scene in sceneArr)
        {
            sceneNames.Add(AssetDatabase.GUIDToAssetPath(scene));

        }
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

        foreach (string scensName in sceneNames)
        {

            scenes.Add(new EditorBuildSettingsScene(scensName, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Add scene ok");
    }

    /// <summary>
    /// 删除所有的构建场景
    /// </summary>
    [MenuItem("Tools/BuildSettings/DeleteScenesFormBuildSettings(删除所有构建场景)")]
    public static void DeleteScenesFormBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("del scene ok");
    }
}
