using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData 
{
    //金币
    public static int Coin {
        get {
            return PlayerPrefs.GetInt("Coin", 0);
        }
        set {
            PlayerPrefs.SetInt("Coin", value);
        }
    }
    //关卡
    public static int Level {
        get
        {
            return PlayerPrefs.GetInt("Level", 1);
        }
        set
        {
            PlayerPrefs.SetInt("Level", value);
        }
    }
    //随机关卡
    public static int RandomLevel
    {
        get
        {
            return PlayerPrefs.GetInt("RandomLevel", 1);
        }
        set
        {
            PlayerPrefs.SetInt("RandomLevel", value);
        }
    }
    //皮肤进度
    public static float SkinProgress {
        get
        {
            return PlayerPrefs.GetFloat("SkinProgress", 0);
        }
        set
        {
            PlayerPrefs.SetFloat("SkinProgress", value);
        }
    }
    //皮肤id，0位没有获得，1为已获得
    public static int GetHasSkinId(ChildOrSnack childOrSnack, int _id)
    {
        if (childOrSnack == ChildOrSnack.Child) {
            return PlayerPrefs.GetInt("ChildSkin" + _id, 0);
        }
        else
            return PlayerPrefs.GetInt("SnackSkin" + _id, 0);
    }
    public static void SetHasSkinId(ChildOrSnack childOrSnack, int _id,int _have)
    {
        if (childOrSnack == ChildOrSnack.Child)
            PlayerPrefs.SetInt("ChildSkin" + _id, _have);
        else
            PlayerPrefs.SetInt("SnackSkin" + _id, _have);
    }
    //当前皮肤id
    public static int GetChoseSkinId(ChildOrSnack childOrSnack)
    {
        if (childOrSnack == ChildOrSnack.Child)
            return PlayerPrefs.GetInt("ChoseChildSkinId", 0);
        else
            return PlayerPrefs.GetInt("ChoseSnackSkinId", 0);
    }
    public static void SetChoseSkinId(ChildOrSnack childOrSnack, int _value)
    {
        if (childOrSnack == ChildOrSnack.Child)
            PlayerPrefs.SetInt("ChoseChildSkinId", _value);
        else
            PlayerPrefs.SetInt("ChoseSnackSkinId", _value);
    }

    public static int AdSkinId {
        get
        {
            return PlayerPrefs.GetInt("AdSkinId", 1);
        }
        set
        {
            PlayerPrefs.SetInt("AdSkinId", value);
        }
    }
    /// <summary>
    /// AB组，-1表示没从服务器获取，0表示A组，1表示B组
    /// </summary>
    public static int ABGroup {
        get
        {
            return PlayerPrefs.GetInt("ABGroup", -1);
        }
        set
        {
            PlayerPrefs.SetInt("ABGroup", value);
        }
    }
    /// <summary>
    /// Ads开关，0为关，1为开
    /// </summary>
    public static int Ads {
        get {
            return PlayerPrefs.GetInt("Ads", 1);
        }
        set {
            PlayerPrefs.SetInt("Ads", value);
        }
    }
    public static int ShowDebug {
        get
        {
            return PlayerPrefs.GetInt("ShowDebug", 0);
        }
        set
        {
            PlayerPrefs.SetInt("ShowDebug", value);
        }
    }
    //是否可以震动
    public static int CanShake
    {
        get
        {
            return PlayerPrefs.GetInt("CanShake", 1);
        }
        set
        {
            PlayerPrefs.SetInt("CanShake", value);
        }
    }
    //宝箱钥匙数
    public static int PrizeKey {
        get {
            return PlayerPrefs.GetInt("PrizeKey", 0);
        }
        set {
            PlayerPrefs.SetInt("PrizeKey", value);
        }
    }
    //AB分组用的，游戏难度。现在不用此变量
    public static int Difficulty {
        get {
            return PlayerPrefs.GetInt("Difficulty", 0);
        }
        set {
            PlayerPrefs.SetInt("Difficulty", value);
        }
    }
    //AB分组用的。现在不用此变量
    public static int RvOptimized
    {
        get
        {
            return PlayerPrefs.GetInt("RvOptimized", 0);
        }
        set
        {
            PlayerPrefs.SetInt("RvOptimized", value);
        }
    }
    //AB分组用的，是否用排序后的场景。现在不用此变量
    public static int LevelOptimization
    {
        get
        {
            return PlayerPrefs.GetInt("LevelOptimization", 0);
        }
        set
        {
            PlayerPrefs.SetInt("LevelOptimization", value);
        }
    }
    //AB分组用的，是否有斗兽场金币奖励关卡。现在不用此变量
    public static int BonusLevel
    {
        get
        {
            return PlayerPrefs.GetInt("BonusLevel", 0);
        }
        set
        {
            PlayerPrefs.SetInt("BonusLevel", value);
        }
    }
    ////AB分组用的，是否有肌肉零食金币奖励关卡。
    public static int RewardLevel
    {
        get
        {
            return PlayerPrefs.GetInt("RewardLevel", 0);
        }
        set
        {
            PlayerPrefs.SetInt("RewardLevel", value);
        }
    }

    public static bool IsRateScore {
        get {
            return PlayerPrefs.GetInt("IsRateScore", 0) == 1 ? true : false;
        }
        set {
            PlayerPrefs.SetInt("IsRateScore", value ? 1 : 0);
        }
    }

    public static bool NoAds {
        get
        {
            return PlayerPrefs.GetInt("NoAds", 0) == 1 ? true : false;
        }
        set
        {
            PlayerPrefs.SetInt("NoAds", value ? 1 : 0);
        }
    }

    public static bool AllInOneBundle
    {
        get
        {
            return PlayerPrefs.GetInt("AllInOneBundle", 0) == 1 ? true : false;
        }
        set
        {
            PlayerPrefs.SetInt("AllInOneBundle", value ? 1 : 0);
        }
    }

    public static bool IsSandboxMode
    {
        get
        {
            var txt = Resources.Load<TextAsset>("SandboxMode");
            if (txt)
            {
                if (txt.text == "0")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else {
                return false;
            }
        }
    }
}
