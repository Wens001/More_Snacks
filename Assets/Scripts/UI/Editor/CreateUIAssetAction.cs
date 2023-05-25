using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

/// <summary>
/// 定义一个创建资源的Action类并实现其Action方法
/// </summary>
class CreateUIAssetAction : EndNameEditAction
{


    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        //创建资源
        Object obj = CreateAssetFormTemplate(pathName, resourceFile);
        //高亮显示该资源
        ProjectWindowUtil.ShowCreatedAsset(obj);
    }

    internal static Object CreateAssetFormTemplate(string pathName, string resourceFile)
    {

        //获取要创建资源的绝对路径
        string fullName = Path.GetFullPath(pathName);
        //读取本地模版文件
        StreamReader reader = new StreamReader(resourceFile);
        string content = reader.ReadToEnd();
        reader.Close();

        //获取资源的文件名
        string fileName = Path.GetFileNameWithoutExtension(pathName);
        //替换默认的文件名
        content = content.Replace("#NAME#", fileName);

        content = content.Replace("#NAMESPACE#", AutoBindGlobalSetting.Instance.Namespace);


        //写入新文件
        StreamWriter writer = new StreamWriter(fullName, false, System.Text.Encoding.UTF8);
        writer.Write(content);
        writer.Close();

        //刷新本地资源
        AssetDatabase.ImportAsset(pathName);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
    }
}