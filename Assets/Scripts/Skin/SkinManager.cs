using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinManager : Singleton<SkinManager>
{
    //[HideInInspector]
    public int nextSkinType;
    public List<SkinInfo> allChildSkinInfo;
    public List<SkinInfo> allSnackSkinInfo;
    public List<int> hasNoChildSkin = new List<int>();
    public List<int> hasNoSnackSkin = new List<int>();

    private void Awake()
    {
        SkinInfo[] allskin = Resources.LoadAll<SkinInfo>("Skin");
        allChildSkinInfo = new List<SkinInfo>();
        allSnackSkinInfo = new List<SkinInfo>();
        for (int i = 0; i < allskin.Length; i++) {
            if (allskin[i].PlayerType == ChildOrSnack.Child)
            {
                allChildSkinInfo.Add(allskin[i]);
            }
            else {
                allSnackSkinInfo.Add(allskin[i]);
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        int skinlength = allChildSkinInfo.Count;
        for (int i = 0; i < skinlength; i++) {
            if (GameData.GetHasSkinId(ChildOrSnack.Child, allChildSkinInfo[i].ID) == 0) {
                if (i == 0) {
                    GameData.SetHasSkinId(ChildOrSnack.Child, i, 1);
                    continue;
                }
                hasNoChildSkin.Add(allChildSkinInfo[i].ID);
            }
        }
        skinlength = allSnackSkinInfo.Count;
        for (int i = 0; i < skinlength; i++)
        {
            if (GameData.GetHasSkinId(ChildOrSnack.Snack, allSnackSkinInfo[i].ID) == 0)
            {
                if (i == 0)
                {
                    GameData.SetHasSkinId(ChildOrSnack.Snack, i, 1);
                    continue;
                }
                hasNoSnackSkin.Add(allSnackSkinInfo[i].ID);
            }
        }
        //初始化时哪个类型缺的皮肤比较多就优先抽谁
        nextSkinType = hasNoChildSkin.Count >= hasNoSnackSkin.Count ? 0 : 1;
        //全部皮肤都有了就不抽了
        nextSkinType = GetHasNoSkinList(nextSkinType).Count > 0 ? nextSkinType : -1;
    }

    public void UpdateNextSkinType() {
        nextSkinType = (nextSkinType + 1) % 2;
        if (GetHasNoSkinList(nextSkinType).Count <= 0) {
            nextSkinType = (nextSkinType + 1) % 2;
            if (GetHasNoSkinList(nextSkinType).Count <= 0)
            {
                nextSkinType = -1;
            }
        }
    }
    public List<SkinInfo> GetAllSkinInfo(int _playerType) {
        if (_playerType != 0 && _playerType != 1)
        {
            Debug.Log("playerType 不合法");
            return null;
        }
        return _playerType == 0 ? allChildSkinInfo : allSnackSkinInfo;
    }

    public List<int> GetHasNoSkinList(int _playerType) {
        if (_playerType != 0 && _playerType != 1) {
            Debug.Log("playerType 不合法");
            return null;
        }
        return _playerType == 0 ? hasNoChildSkin : hasNoSnackSkin;
    }

    public GameObject GetChildModel(int _id) {
        return Instantiate(allChildSkinInfo[_id].SkinModel);
    }
    public GameObject GetSnackModel(int _id)
    {
        return Instantiate(allSnackSkinInfo[_id].SkinModel);
    }

    /// <summary>
    /// 当前选择的皮肤
    /// </summary>
    /// <param name="_type">阵营</param>
    /// <returns></returns>
    public SkinInfo GetChoseSkinInfo(ChildOrSnack _type) {
        return GetAllSkinInfo((int)_type)[GameData.GetChoseSkinId(_type)];
    }

    /// <summary>
    /// 保存皮肤数据
    /// </summary>
    /// <param name="_type"></param>
    /// <param name="_id"></param>
    public void SaveNewSkin(ChildOrSnack _type, int _id, bool isChose = false) {
        GameData.SetHasSkinId(_type, _id, 1);
        List<int> list = GetHasNoSkinList((int)_type);
        if (list.Contains(_id)) {
            list.Remove(_id);
        }
        if (isChose) {
            GameData.SetChoseSkinId(_type, _id);
        }
        if (list.Count == 0) {
            int otherType = _type == ChildOrSnack.Child ? 1 : 0;
            //当前类型皮肤全都获取了，看另一种是否也全齐，齐了就设置不再获取皮肤
            if (nextSkinType == (int)_type) {
                if (GetHasNoSkinList(otherType).Count > 0)
                {
                    nextSkinType = otherType;
                }
                else {
                    nextSkinType = -1;
                }               
            }
        }
    }

    //随机皮肤
    public int GetNewSkinId()
    {      
        if (nextSkinType == -1)
        {
            return -1;
        }
        else
        {
            List<int> curHasNoSkinList = GetHasNoSkinList(nextSkinType);
            if (curHasNoSkinList.Count > 0)
            {
                int newSkinId = curHasNoSkinList[Random.Range(0, curHasNoSkinList.Count)];
                //Debug.LogWarning("new skin id:"+ newSkinId);
                return newSkinId;
            }
            else
            {
                Debug.LogWarning((ChildOrSnack)nextSkinType + " 皮肤不足");
                return -1;
            }
        }
    }

    /// <summary>
    /// 展示角色皮肤模型
    /// </summary>
    /// <param name="info">角色信息</param>
    /// <param name="renderTexId">将使用render texture id</param>
    public GameObject ShowSkinModel(SkinInfo info,int renderTexId) {
        if (info == null || info.PlayerType == ChildOrSnack.NULL) {
            return null;
        }
        GameObject model =Instantiate(info.ShowSkinModel,new Vector3(20,20,20)*(renderTexId+1),Quaternion.identity) ;
        Camera renderCam = model.GetComponentInChildren<Camera>(true);
        renderCam.targetTexture = Resources.Load<RenderTexture>("RenderTexture/ModelTexture" + renderTexId);
        renderCam.gameObject.SetActive(true);
        return model;
    }

    //大礼包解锁皮肤
    public static void IAPforSkins() {
        //还没初始化时，应用于回购
        if (SkinManager.Instance == null)
        {
            for (int i = 0; i < 2; i++) {
                for (int index = 1; index <= 3; index++) {
                    GameData.SetHasSkinId((ChildOrSnack)i, index, 1);
                }
            }
        }
        else {
            for (int i = 0; i < 2; i++)
            {
                for (int index = 1; index <= 3; index++)
                {
                    SkinManager.Instance.SaveNewSkin((ChildOrSnack)i, index, false);
                    Messenger.Broadcast(ConstDefine.Listener.GetNewSkin, SkinManager.Instance.GetAllSkinInfo(i)[index]);
                }
            }     
        }
    }
}
