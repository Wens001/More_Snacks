using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using LitJson;
public class UIPanelManager
{
    private static UIPanelManager _instance;
    private Transform m_CanvasTransform;
    private Transform CanvasTransform
    {
        get
        {
            if (m_CanvasTransform == null)
            {
                var newgo = new GameObject() { name = "CanvasTransform" };
                GameObject.DontDestroyOnLoad(newgo);
                m_CanvasTransform = newgo.transform;
            }
            return m_CanvasTransform;
        }
    }
    public static UIPanelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIPanelManager();
            }


            return _instance;
        }
    }

    private Dictionary<string, string> panelPathDict;
    private Dictionary<string, BasePanel> panelDict;
    private Stack<BasePanel> panelStack;

    private UIPanelManager()
    {
        ParseUIPanelTypeJson();
    }

    public BasePanel PushPanel(string panelType)
    {
        if (panelStack == null)
        {
            panelStack = new Stack<BasePanel>();
        }

        //停止上一个界面
        if (panelStack.Count > 0)
        {
            BasePanel topPanel = panelStack.Peek();
            topPanel.OnPause();
        }

        BasePanel panel = GetPanel(panelType);
        panelStack.Push(panel);
        panel.OnEnter();
        return panel;
    }

    public void PopPanel()
    {
        if (panelStack == null)
        {
            panelStack = new Stack<BasePanel>();
        }
        if (panelStack.Count <= 0)
        {
            return;
        }

        //退出栈顶面板
        BasePanel topPanel = panelStack.Pop();
        topPanel.OnExit();

        //恢复上一个面板
        if (panelStack.Count > 0)
        {
            BasePanel panel = panelStack.Peek();
            panel.OnResume();
        }

    }

    private BasePanel GetPanel(string panelType)
    {

        if (panelDict == null)
        {
            panelDict = new Dictionary<string, BasePanel>();
        }

        BasePanel panel = panelDict.GetValue(panelType);

        //如果没有实例化面板，寻找路径进行实例化，并且存储到已经实例化好的字典面板中
        if (panel == null)
        {
            string path = panelPathDict.GetValue(panelType);
            GameObject panelGo = GameObject.Instantiate(AssetsLoader.Load<GameObject>(path), CanvasTransform, false);
            panel = panelGo.GetComponent<BasePanel>();
            panelDict.Add(panelType, panel);
        }
        return panel;
    }

    //解析json文件
    private void ParseUIPanelTypeJson()
    {
        panelPathDict = new Dictionary<string, string>();
        TextAsset textUIPanelType = AssetsLoader.Load<TextAsset>("UIPanelTypeJson");
        UIPanelInfoList panelInfoList = JsonMapper.ToObject<UIPanelInfoList>(textUIPanelType.text);

        foreach (UIPanelInfo panelInfo in panelInfoList.panelInfoList)
        {
            panelPathDict.Add(panelInfo.panelType, panelInfo.path);
        }
    }
}
/*
 *      UIPanelManager panelManager = UIPanelManager.Instance;  //初始化
 *      panelManager.PushPanel(UIPanelType.MainMenu);           //入栈 (Panel继承BasePanel,Panel的Prefab位置保存在Json里)
 *      panelManager.PopPanel();                                //出栈
 * 
 */
