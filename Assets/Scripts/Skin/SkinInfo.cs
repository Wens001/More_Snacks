using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skin", menuName = "Creat Skin")]
[SerializeField]
public class SkinInfo : ScriptableObject
{
    public string SkinName;
    public int ID;
    public ChildOrSnack PlayerType;

    [Header("获取方式")]
    public GetWay getWay;
    public int CoinCost;
    public int LookAdCount = 1;

    private Sprite icon;
    //public Sprite Icon { get => Resources.Load<Sprite>(string.Format("Icon/{0}{1}", PlayerType, ID)); }
    public Sprite Icon { 
        get{
            if (icon == null)
                icon = Resources.Load<Sprite>(string.Format("Icon/{0}{1}", PlayerType, ID.ToString("D2")));
            return icon;
        }
    }

    private GameObject skinModel;
    //public GameObject SkinModel { get => Resources.Load<GameObject>(string.Format("Model/{0}{1}", PlayerType,ID)); }
    public GameObject SkinModel
    {
        get
        {
            if (skinModel == null)
                skinModel = Resources.Load<GameObject>(string.Format("Model/{0}{1}", PlayerType, ID.ToString("D2")));
            return skinModel;
        }
    }

    private GameObject showSkinModel;
    public GameObject ShowSkinModel {
        get {
            if (showSkinModel == null)
                showSkinModel = Resources.Load<GameObject>(string.Format("ModelShow/{0}{1}", PlayerType, ID));
            return showSkinModel;
        }
    }

}

public enum GetWay
{
    WatchAd,
    CoinBuy
}
public enum ChildOrSnack { 
    NULL=-1,
    Child,
    Snack
}

