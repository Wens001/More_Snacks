using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLocalPlayer : Singleton<SetLocalPlayer>
{
    public BaseControl control;
    public bool isUseThisModel = false;
    public bool IsCoerce = false;
    public bool UseFllowNow = true;
    private Transform control_parent;
    void Start()
    {
        if (control == null )//|| (LevelScenes.IsLoopLevel() && !IsCoerce)
            return;
        //if (GameEndPodium.Instance) {
        //    if (!isUseThisModel)
        //        ReplaceModel();
        //}
        control_parent = control.transform.parent;
        control.IsLocalPlayer.Value = true;

        if (UseFllowNow)
        {
            FllowObject.Get(0).target = control.transform;
            FllowObject.Get(0).FllowNow();
        }
        Shader.SetGlobalColor("_XRayColor", new Color(0, 1, 1, 0.375f));
    }

    /// <summary>
    /// 替换模型
    /// </summary>
    public virtual void ReplaceModel(SkinInfo _newModel=null)
    {        
        GameObject newModel;
        control.gameObject.SetActive(false);
        GameManager.Instance.SnackList.Remove(control);
        //默认没有的话，就自动替换选择皮肤的
        if (_newModel == null) {
            if (control.IsSnack)
                _newModel = SkinManager.Instance.allSnackSkinInfo[GameData.GetChoseSkinId(ChildOrSnack.Snack)];
            else
                _newModel = SkinManager.Instance.allChildSkinInfo[GameData.GetChoseSkinId(ChildOrSnack.Child)];
        }
        //替换模型
        if (_newModel.PlayerType == ChildOrSnack.Snack)
        {       
            newModel = Instantiate(_newModel.SkinModel);
            newModel.AddComponent<JumpToToilet>();
            newModel.GetComponent<BeEatEffect>().enabled = true;
        }
        else
        {
            newModel = Instantiate(_newModel.SkinModel);
        }
        newModel.transform.SetParent(control_parent);
        newModel.gameObject.SetActive(false);
        newModel.transform.position = control.transform.position;
        newModel.transform.rotation = control.transform.rotation;
        control = newModel.GetComponent<BaseControl>();
        newModel.gameObject.SetActive(true);
    }

    public void SetPlayer(SkinInfo childOrSnack =null)
    {
        if (isUseThisModel)
        {
            return;
        }
        control.IsLocalPlayer.Value = false;
        if (!isUseThisModel)
        {
            if (childOrSnack == null) {
                var curType = control.IsSnack ? ChildOrSnack.Snack : ChildOrSnack.Child;
                childOrSnack = SkinManager.Instance.GetChoseSkinInfo(curType);
            }
            if (childOrSnack.PlayerType == ChildOrSnack.Child)
            {
                ChildControl[] childs = control.transform.parent.GetComponentsInChildren<ChildControl>();
                if (childs.Length <= 0)
                    return;
                control = childs[0];
            }
            else
            {
                SnackControl[] snacks = control.transform.parent.GetComponentsInChildren<SnackControl>();
                if (snacks.Length <= 0)
                    return;
                control = snacks[Random.Range(0, snacks.Length)];
            }
            ReplaceModel(childOrSnack);
        }
        else {
            if (control.IsSnack) {
                control.gameObject.AddComponent<JumpToToilet>();
                control.GetComponent<BeEatEffect>().enabled = true;
            }
                
        }
        control.IsLocalPlayer.Value = true;
        if (UseFllowNow)
        {
            FllowObject.Get(0).target = control.transform;
            FllowObject.Get(0).FllowNow();
        }
        KillSnackUI.SetKillUIFllowObj(control.transform);
    }
}
