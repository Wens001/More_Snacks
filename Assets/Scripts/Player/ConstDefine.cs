using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstDefine 
{
    public class Anim
    {
        public const string State = "State";
        public const string Speed = "Speed";
    }
    public class Mater
    {
        public const string _MainColor = "_MainColor";
    }
    public class Listener
    {
        public const string EatSnake = "EatSnake"; // 消灭目标 BaseControl
        public const string DiscoverSnake = "DiscoverSnake"; // 发现目标 BaseControl
        public const string ChargeState = "ChargeState"; // 掀物体状态改变 bool Vector3
        public const string ChargeValue = "ChargeValue"; // 掀物体值改变 float
        public const string ChargeObject = "ChargeObject"; // 掀起物体触发 GameObject
        public const string SnakeHide = "SnakeHide"; //零食消失 BaseControl
        public const string InfiniteTime = "InfiniteTime"; //无限时间
        public const string AddCoinValue = "AddCoinValue"; //增加金币
        public const string SetPlayerGroup = "SetPlayerGroup"; //设置零食图标数
        public const string HidePlayerGroup = "HidePlayerGroup"; //隐藏人数
        public const string ChoseSkinItem = "ChoseSkinItem"; //选中皮肤
        public const string Revive = "Revivie"; //复活
        public const string NotEnough = "NotEnough"; //不够钱
        public const string GetNewSkin = "GetNewSkin";//获取新皮肤（通关时进度达100%）
        public const string LoseNewSkin = "LoseNewSkin";//不要新皮肤（通关时进度达100%）
        public const string ReadDebugMode = "ReadDebugMode";//读取模式
        public const string StartGame = "StartGame";//选择人物后开始游戏
        public const string GetKey = "UpdateKey";//钥匙数量变化
        public const string BugNoAds = "BugNoAds";//购买去广告
        public const string BugAllInOne = "BugAllInOne";//购买大礼包
        public const string ShowSpecialPanel = "ShowSpecialPanel";//展示、隐藏特殊关卡界面
        public const string GetScoreReward = "GetScoreReward";//金币关卡奖励
        public const string CoinLvEnd = "CoinLvEnd";//斗兽场关卡结束
        public const string SetGamePlayBtn = "SetGamePlayBtn";//设置开始按钮状态
    }
    public static float[] GuideTimes = new float[] { 10, 25};//20,20,5,10
    public const float GameTime = 20;
    public const string specialScene = "GetNewSkin";
    public const string DiscountScene = "DiscountScene";
    public const string ColosseumScene = "Colosseum";
    public const string IsDebug = "IsDebug";
    public const float DelayShowTime = 3;
    public const float ShowPanelAnimTime = 0.2f;
    public const int OneBundleCoin = 10000;//大礼包金币
    public const int ChallengeCost = 100;//进入挑战关卡的花费
    public static int[] NextMapIndex = { 1, 9, 16, 24, 33};
    public static int[] original_coinLv = { 6, 15 };
    public static int[] original_muscleSnackLv = {8,19,24,30};
    public static int[] original_challengeLv = { 11, 17, 22, 27, 34, 37, 40, 43 };//10, 16, 20, 24, 30, 33, 36, 39
    public static int[] adjust_challengeLv = { 12, 17, 21, 25, 29, 33, 37, 40 };
}
