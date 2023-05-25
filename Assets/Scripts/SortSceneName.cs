using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class SortSceneName : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    [ContextMenu("Sort Scene")]
    public void SortScene()
    {
        int index = 1;//序号
        string[] paths = new string[] { "Assets/Scenes/New Folder" };     //指定场景所在的文件夹名称，如果是“Assets”,则代表工程里面所有的场景
        string[] sceneArr = AssetDatabase.FindAssets("t:Scene", paths);
        Debug.Log("sceneArr length:" + sceneArr.Length);

        for (int i = sceneArr.Length; i >= 1; i--) {
            var oldName = AssetDatabase.GUIDToAssetPath(sceneArr[i-1]);
            var newName = "level" + i;
            Debug.Log("old name:" + oldName + "\nnew name:" + newName);
            AssetDatabase.RenameAsset(oldName, newName);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif
