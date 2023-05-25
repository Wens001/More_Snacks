using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class ScriptCreatorWindow : EditorWindow
{
    private static bool isfind = false;
    private static string path;

    [MenuItem("Assets/Create/UI/New C# Script", false, 1)]
    public static void ShowWindow()
    {
        if (!isfind)
        {
            isfind = true;
            string[] paths = AssetDatabase.FindAssets("UITemplateCode");
            if (paths.Length == 0)
                throw new System.Exception("Not Find UITemplateCode");
            path = AssetDatabase.GUIDToAssetPath(paths[0]);
        }

        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
        ScriptableObject.CreateInstance<CreateUIAssetAction>(),
        GetSelectedPath() + "/NewUIScript.cs", null,
        path);
    }


    private static string GetSelectedPath()
    {
        //默认路径为Assets
        string selectedPath = "Assets";

        //获取选中的资源
        Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

        //遍历选中的资源以返回路径
        foreach (Object obj in selection)
        {
            selectedPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(selectedPath) && File.Exists(selectedPath))
            {
                selectedPath = Path.GetDirectoryName(selectedPath);
                break;
            }
        }

        return selectedPath;
    }


}