using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Events;
//using zFrame.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using LionStudios;
using MyUI;
using MoreMountains.NiceVibrations;
using LionStudios.Runtime.Sdks;
//using System.Configuration;
using Facebook.Unity;
//using UnityEngine.SocialPlatforms;
using LionStudios.GDPR;
using GameAnalyticsSDK;
using LionStudios.Debugging;
using GameAnalyticsSDK.Setup;
//using UnityEditor;
//using GameAnalyticsSDK.Setup;
//using Firebase;
//using Firebase.Analytics;

public class GameManager : Singleton<GameManager>
{
    #region 计时器

    public ScriptableBuff SpeedBuff;

    public float GameSpeed 
    { 
        get; 
        set; 
    } = 0;
    public MyTimer GameTimer { get; private set; }

    public BindableProperty<float> NowTime ;
    [HideInInspector]
    //单局内是否显示过加时按钮
    public bool hasShowAddTimeBtn;
    [HideInInspector]
    //单局内是否显示过复活倒计时
    public bool hasShowReviveCountdown;

    public static bool CanShake;
    [Header("试用道具")]
    public EWeapon TryProp = EWeapon.NULL;

    public void GameStart()
    {
        LevelScenes.LevelStartedEvent();
        GameSpeed = 1;  //点击开始时，打开计时
        foreach (var snake in SnackList)
        {
            //snake.agent.enabled = true;
            if (snake.IsLocalPlayer)
                continue;
            if (snake.IsSnack)
                snake.gameObject.AddComponent<SnackAI>();
            else {
                ChildAI ai =snake.gameObject.AddComponent<ChildAI>();
                ai.DelayToMove();
            }              
        }
        if(LevelScenes.level>1)
            FllowObject.MoveToTarget();
        Messenger.Broadcast(ConstDefine.Listener.StartGame);
        //GamePanel.SetSnakeCount();
    }

    public bool IsGameOver { get; private set; }
    private static bool hasInit = false;
    public static bool IsDebug;
    public static bool canRunGame = true;
    public ParentControl parentCtrl = null;
    private static bool hasDelayShowBanner = false;
    [Header("游戏时间")]
    public float gameTime=-1;
 
    //场景类型
    public static SceneType sceneType = SceneType.Level;

    [Header("吃脏脏零食会失败")]
    public bool eatDirtyDead = false;


    public void GameWin(string _anim="Win")
    {
        if (IsGameOver)
            return;
        IsGameOver = true;

        if (GameEndPodium.Instance)
        {
            this.AttachTimer(1.5f, () => GameEndPodium.Instance.DelaySetGameEnd(true));
            return;
        }
        
        JoystickPanel.Instance.gameObject.SetActive(false);
        
        localPlayer.transform.eulerAngles = Vector3.zero;
        this.AttachTimer(0.6f,()=>
        {
            if (_anim == "Win")
            {
                localPlayer.SetWinAnim();
            }
            else {
                localPlayer.SetAnimTrigger(_anim,false);
            }
        });
        //localPlayer.SetWinAnim();
        if (localPlayer != childPlayer) {
            childPlayer.agent.enabled = false;
        }
        if (FindObjectOfType<MuscleSnackControl>())
        {
            this.AttachTimer(2.2f, () =>
            {
                //UIPanelManager.Instance.PushPanel(UIPanelType.CoinLvPanel);
                Messenger.Broadcast<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ESpecialPanel.CoinLv_End, true); 
            });
        }
        else
        {
            for (int i = 0; i < 5; i++)
                UIPanelManager.Instance.PopPanel();
            this.AttachTimer(2.2f, () => UIPanelManager.Instance.PushPanel(UIPanelType.WinPanel));
        }
        FllowObject.Get(0).FailEndCamPoint();
    }


    public void GameFaild(string _anim = "")
    {
        if (IsGameOver)
            return;
        IsGameOver = true;

        if (GameEndPodium.Instance)
        {
            this.AttachTimer(1.5f, () => GameEndPodium.Instance.DelaySetGameEnd(false));
            return;
        }

        JoystickPanel.Instance.gameObject.SetActive(false);

        if (FindObjectOfType<MuscleSnackControl>()) {
            this.AttachTimer(1.5f, () => Messenger.Broadcast<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ESpecialPanel.CoinLv_End, true));
            return;
        }

        for (int i = 0; i < 5; i++)
            UIPanelManager.Instance.PopPanel();
        if (!string.IsNullOrEmpty(_anim)) {
            this.AttachTimer(0.1f, () => localPlayer.SetAnimTrigger(_anim,false));
        }
        if (LevelScenes.level >= 5) {
            FllowObject.Get(0).FailEndCamPoint();
        }
        this.AttachTimer(1.2f, () => UIPanelManager.Instance.PushPanel(UIPanelType.LosePanel));
    }
    //复活
    void ReviveListener(ChildOrSnack playerType,EWeapon weaponType =  EWeapon.NULL)
    {
        if (playerType == ChildOrSnack.Snack)
        {
            localPlayer.IsInvincible = true;
            localPlayer.BeEat = false;
            localPlayer.gameObject.SetActive(true);
            childPlayer.agent.enabled = false;
            childPlayer.RandomPos();
            this.AttachTimer(2, () =>
            {
                localPlayer.IsInvincible = false;
                childPlayer.agent.enabled = true;
            });           
        }
        else {
            //localPlayer.agent.enabled = true;
            localPlayer.SetAnimTrigger("StopSpank",true);
            var control = ((ChildControl)localPlayer);
            control.WeapomType.Value = weaponType;
            if (control.IsHammer.Value)
                control.IsHammer.Value = false;
            control.IsHammer.Value = true;
            GameManager.Instance.GameTimer.DurationTime += 15;
            if (parentCtrl) {
                parentCtrl.SetAnimTrigger("StopSpank",false);
                //parentCtrl.agent.enabled = false;
                this.AttachTimer(2, () => parentCtrl.agent.enabled = true);
            }

        }
        IsGameOver = false;
        JoystickPanel.Instance.gameObject.SetActive(true);
        foreach (BaseControl snack in SnackList)
        {
            if (snack.IsSnack && !snack.IsLocalPlayer && snack.gameObject.activeSelf)
            {
                snack.SetAction();
            }
        }
    }

    public float MoveSpeed = 1f;
    private bool canShow = false;
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
            if (GameSpeed > 1) {
                GameSpeed -= 1;
                if (GameSpeed < 1)
                {
                    GameSpeed = 1;
                }
            }                     
        }
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            GameSpeed += 1;
            if (GameSpeed >5)
            {
                GameSpeed = 5;
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameManager.Instance.GameTimer.DurationTime += 60;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameData.Coin += 100;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            GameData.PrizeKey++;
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            ModelShow ms = SkinManager.Instance.ShowSkinModel(SkinManager.Instance.allSnackSkinInfo[0], 0).GetComponent<ModelShow>();
            ms.ShowPropAnim( EWeapon.Pepper);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ModelShow ms = SkinManager.Instance.ShowSkinModel(SkinManager.Instance.allSnackSkinInfo[0], 0).GetComponent<ModelShow>();
            ms.ShowPropAnim(EWeapon.Pepper);
        }

#endif
        if (!canRunGame)
            return;
        if (IsGameOver == false)
            GameTimer.OnUpdate(Time.deltaTime * GameSpeed * MoveSpeed );

        NowTime.Value = GameTimer.DurationTime - GameTimer.timer;
        if (NowTime.Value <0.01f)
        {
            //enabled = false;
            if (IsGameOver)
                return;
            if (GameEndPodium.Instance)
            {
                if (KillSnackUI.killSnackUIList[0].KillSize >= KillSnackUI.killSnackUIList[1].KillSize)
                    GameWin();
                else
                    GameFaild();
                return;
            }

            if (childPlayer.IsLocalPlayer)
            {
                int size = 0;
                for (int i = 0; i < SnackList.Count; i++)
                    if (SnackList[i].IsSnack && SnackList[i].gameObject.activeSelf)
                        size++;
                bool isWin;
                if (abConfig.level_difficulty == 0)
                {
                    isWin = size == 0;
                }
                else
                {
                    isWin = size <= 1;
                }
                if (!isWin)
                    GameFaild();
                else
                    GameWin();
            }
            else
                GameWin();
        }
    }

    #endregion

    public BaseControl childPlayer { get; set; }
    public BaseControl localPlayer { get; set; }

    private List<BaseControl> m_SnackList;
    public List<BaseControl> SnackList
    {
        get
        {
            return m_SnackList ?? (m_SnackList = new List<BaseControl>());
        }
    }

    public int ChildCount
    {
        get
        {
            var size = 0;
            foreach (var item in SnackList)
            {
                if (!item.IsSnack)
                    size++;
            }
            return size;
        }
    }

    public List<GameObject> SnackPrefabs;

    public List<GameObject> ChildPrefabs;


    private Light m_light;
    private Color beginLightColor;

    private void InitLight()
    {
        if (m_light == null)
        {
            transform.Find("DLight").TryGetComponent(out m_light);
            beginLightColor = m_light.color;
        }
    }

    private Light GetLight()
    {
        InitLight();
        return m_light;
    }

    public void LightColorDT(Color col, float timer)
    {
        InitLight();
        m_light.DOKill();
        m_light.DOColor(col, timer);
    }

    public void LightDefaultColorDT(float timer)
    {
        InitLight();
        m_light.DOKill();
        m_light.DOColor(beginLightColor, timer);
    }


    public GameObject InstanceSnackIndex(int index)
    {
        if (index >= SnackPrefabs.Count)
            throw new Exception($"out of {SnackPrefabs} index");
        return Instantiate(SnackPrefabs[index]);
    }

    public GameObject InstanceSnackRandom()
    {
        var temp = UnityEngine.Random.Range(0, SnackPrefabs.Count);
        return Instantiate(SnackPrefabs[temp]);
    }

    public GameObject InstanceChildIndex(int index)
    {
        if (index >= ChildPrefabs.Count)
            throw new Exception($"out of {ChildPrefabs} index");
        return Instantiate(ChildPrefabs[index]);
    }
    public GameObject InstanceChildRandom()
    {
        var temp = UnityEngine.Random.Range(0, ChildPrefabs.Count);
        return Instantiate(ChildPrefabs[temp]);
    }


    #region LionKit SDK Init

    /// <summary>
    /// 场景加载前运行。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnGameStart()
    {
#if UNITY_EDITOR
        int curLv;
        if (int.TryParse(SceneManager.GetActiveScene().name.Remove(0, 5), out curLv))
        {
            Debug.Log("start lv:" + curLv);
            LevelScenes.level.Value = curLv;
        }
        else {
            int.TryParse(SceneManager.GetActiveScene().name.Remove(0, 6), out curLv);
            Debug.Log("start lv:" + curLv);
            LevelScenes.level.Value = curLv;
        }
#endif
        // do MaxSdk.OnSdkInitializedEvent subscription here (SDK params)
        IOSCompareVersion();
        LionKit.OnInitialized += () =>
        {
            // do Facebook init
            // check Firebase dependencies
            // do Facebook, Firebase event subscription
            //FB.Init();  
            AdsManager.Instance.InitAdjust();
            AdsManager.InitFirebase();
            AdsManager.Instance.RewardCallBackInit();
            ABTestInit();
            AdsManager.Instance.InitGA();
            AdsManager.Instance.InitIAP();
            AdsManager.HasInitLionSdk = true;
        };
    }
    #endregion

    private void Awake()
    {
        _ = LocalPlayerSetting.Instance;
        var level = LevelScenes.GetLevelIndex(LevelScenes.level);

        var time = (level <= 2 && level >= 1) ? (ConstDefine.GuideTimes[level - 1]) : (ConstDefine.GameTime);
        if (LevelScenes.GetLevelIndex(LevelScenes.level) == 10)
            time = 30f;

        GameTimer = new MyTimer(time);
        NowTime = new BindableProperty<float>(time);
        NowTime.Value = GameTimer.DurationTime - GameTimer.timer;
        hasShowAddTimeBtn = false;
        hasShowReviveCountdown = false;

        //初始化皮肤管理器
        _ =SkinManager.Instance;

        for (int i = 0; i < 10; i++)
            UIPanelManager.Instance.PopPanel();

        UIPanelManager.Instance.PushPanel(UIPanelType.JoystickPanel);
        JoystickPanel.Instance.gameObject.SetActive(false);
        //if (LevelScenes.IsLoopLevel() && FindObjectOfType<ChoicePlayerManager>())
        //    UIPanelManager.Instance.PushPanel(UIPanelType.TapToPlayPanel);
        //else
        //    UIPanelManager.Instance.PushPanel(UIPanelType.GamePanel);
        switch (sceneType)
        {
            case SceneType.Level:
                //if (LevelScenes.IsLoopLevel() && FindObjectOfType<ChoicePlayerManager>())
                //    UIPanelManager.Instance.PushPanel(UIPanelType.TapToPlayPanel);
                //else
                if(!FindObjectOfType<ColosseumMgr>())
                    UIPanelManager.Instance.PushPanel(UIPanelType.GamePanel);
                break;
            case SceneType.SkinScene:
                UIPanelManager.Instance.PushPanel(UIPanelType.SkinPanel);
                break;
            case SceneType.GetNewSkin:
                if (SceneManager.GetActiveScene().name.Contains("level")) {
                    IsGameOver = true;
                    UIPanelManager.Instance.PushPanel(UIPanelType.WinPanel);
                }                   
                else
                    UIPanelManager.Instance.PushPanel(UIPanelType.GetNewSkinPanel);
                break;
            case SceneType.GetNewProp:
                if (SceneManager.GetActiveScene().name.Contains("level")) {
                    IsGameOver = true;
                    UIPanelManager.Instance.PushPanel(UIPanelType.WinPanel);
                }                   
                else
                    UIPanelManager.Instance.PushPanel(UIPanelType.GetNewPropPanel);
                break;
            case SceneType.DisCountStore:
                UIPanelManager.Instance.PushPanel(UIPanelType.BundleSalePanel);
                break;
        }

        
        Messenger.AddListener<GameObject>(ConstDefine.Listener.ChargeObject, OnChargeObject);
        Messenger.AddListener<ChildOrSnack,EWeapon>(ConstDefine.Listener.Revive, ReviveListener);

        if (!hasInit) {
            hasInit = true;
            FB.Init();
            AdsManager.Instance.CheckFireBase();
            //IOSCompareVersion();
            if (maxSDKConfig != null) {
                this.AttachTimer(0.2f, () => { OpenADTrack(maxSDKConfig); });
            }
            DebugModeConfig();        
            AdsManager.ShowAds.Value = GameData.Ads == 1 ? true : false;
            GDPRInit();
            Analytics.OnLogEvent += (LionGameEvent gameEvent) =>
            {
                if (FB.IsInitialized)
                {
                    FB.LogAppEvent(gameEvent.eventName, parameters: gameEvent.eventParams);
                    //Debug.Log("触发FB_log —— " + gameEvent.eventName);
                }
                else
                {
                    //Debug.LogError("触发FB_log失败 —— 未初始化； 信息：" + gameEvent.eventName);
                }
            };
            this.AttachTimer(0.2f,()=> {
                if(IsDebug) 
                    MaxSdk.ShowMediationDebugger();
            });
            CanShake = GameData.CanShake == 1 ? true : false;
        }        
    }

    private async void Start()
    {
        await new WaitForEndOfFrame();
        await new WaitForEndOfFrame();
        if (LevelScenes.level > 1) {
            if (!hasDelayShowBanner)
            {
                this.AttachTimer(1, AdsManager.ShowBanner);
                hasDelayShowBanner = true;
            }
            else
            {
                AdsManager.ShowBanner();
            }
        }
               
        Messenger.AddListener<BaseControl, BaseControl>(ConstDefine.Listener.EatSnake, EatSnackListener);
        float time;
        if (SnackList.Count > 6)
        {
            Messenger.Broadcast(ConstDefine.Listener.HidePlayerGroup);
            time = 30f;
            GameTimer.DurationTime = time;
            NowTime.Value = time;
        }
        else {
            //GamePanel.SetSnakeCount();
            Messenger.Broadcast(ConstDefine.Listener.SetPlayerGroup);
        }
        ResetGameTime();
    }

    public void ResetGameTime()
    {
        float time;
        #region ABtest赋值
        var level = LevelScenes.GetLevelIndex(LevelScenes.level);
        if (level <= 2 && level >= 1)
        {
            time = ConstDefine.GuideTimes[level - 1];
        }
        else
        {
            if (GameEndPodium.Instance != null)
            {
                time = 30;
            }
            else
            {
                if (!SetLocalPlayer.Instance.control.IsSnack)
                {
                    time = abConfig.childTime;
                }
                else
                {
                    time = abConfig.snackTime;
                }
            }
        }
        //if (LevelScenes.GetLevelIndex(LevelScenes.level) == 11)
        //    time = 30f;
        if (gameTime > 0) {
            time = gameTime;
        }
        GameTimer = new MyTimer(time);
        GameTimer.ReStart();
        NowTime.Value = GameTimer.DurationTime - GameTimer.timer;
        #endregion
    }

    private void OnDisable()
    {
        Messenger.RemoveListener<BaseControl, BaseControl>(ConstDefine.Listener.EatSnake, EatSnackListener);
        Messenger.RemoveListener<GameObject>(ConstDefine.Listener.ChargeObject, OnChargeObject);
        Messenger.RemoveListener<ChildOrSnack,EWeapon>(ConstDefine.Listener.Revive, ReviveListener);
        Destroy(gameObject);
    }

    /// <summary>
    /// 是否还存在零食
    /// </summary>
    /// <returns></returns>
    public bool HasSnack()
    {
        return LiveSnakeCount() != 0;
    }

    /// <summary>
    /// 存在零食
    /// </summary>
    /// <returns></returns>
    public int LiveSnakeCount()
    {
        int size = 0;
        for (int i = 0; i < SnackList.Count; i++)
        {
            if (SnackList[i].gameObject.activeSelf == false)
                continue;
            if (SnackList[i].IsSnack == false)
                continue;
            size++;
        }
        return size ;
    }

    public List<int> LiveSnakeList()
    {
        List<int> res = new List<int>();
        for (int i = 0; i < SnackList.Count; i++)
        {
            if (SnackList[i].gameObject.activeSelf == false)
                continue;
            if (SnackList[i].IsSnack == false)
                continue;
            res.Add(i);
        }
        return res;
    }

    /// <summary>
    /// 寻找附近最近的零食
    /// </summary>
    /// <param name="centen"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public BaseControl FindNearSnake(BaseControl centen, float radius)
    {
        int minIndex = -1;
        float minDistance = Mathf.Infinity;
        var snakeList = SnackList;
        for (int i = 0; i < snakeList.Count; i++)
        {
            if (snakeList[i].gameObject == centen.gameObject || snakeList[i].IsShow() == false
                || snakeList[i].IsSnack == false || snakeList[i].IsInvincible)//加多个零食无敌时不搜索（例如零食跳马桶时，不可寻）
                continue;
            var dis = Vector3.Distance(snakeList[i].transform.position, centen.transform.position);
            if (Vector3.Distance(snakeList[i].transform.position, centen.transform.position) > radius)
                continue;
            if (dis > minDistance)
                continue;
            minIndex = i;
            minDistance = dis;
        }
        if (minIndex != -1)
            return snakeList[minIndex];
        return null;
    }


    private void EatSnackListener(BaseControl killer,BaseControl control)
    {
        
        if (localPlayer == null)
            return;
        if(CanShake)
            MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
        if (ColosseumMgr.Instance != null)
            return;
        
        if (control.IsLocalPlayer)
        {
            GameFaild();
            return;
        }
        if (localPlayer.IsSnack == false && HasSnack() == false)
        {
            if (GameEndPodium.Instance)
            {
                if (KillSnackUI.killSnackUIList[0].KillSize >= KillSnackUI.killSnackUIList[1].KillSize)
                    GameWin();
                else
                    GameFaild();
                return;
            }

            GameWin();
            return;
        }
    }

    //void ChildLayghAtFather() {
    //    if (door) { 
    //    door
    //    }
    //}

    private void OnChargeObject(GameObject obj)
    {
        if (CanShake)
            MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
    }
    #region ios14.5需要开广告权限
    static MaxSdkBase.SdkConfiguration maxSDKConfig = null;
    static void IOSCompareVersion()
    {
#if UNITY_IPHONE || UNITY_IOS
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            var result = MaxSdkUtils.CompareVersions(UnityEngine.iOS.Device.systemVersion, "14.5");
            if (result != MaxSdkUtils.VersionComparisonResult.Lesser)
            {
                // Note that App transparency tracking authorization can be checked via `sdkConfiguration.AppTrackingStatus` for Unity Editor and iOS targets
                // 1. Set Facebook ATE flag here, THEN
                //FB.Mobile.SetAdvertiserTrackingEnabled(sdkConfiguration.AppTrackingStatus == MaxSdkBase.AppTrackingStatus.Authorized);             
                //this.AttachTimer(1, () =>
                // {
                //     AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(sdkConfiguration.AppTrackingStatus == MaxSdkBase.AppTrackingStatus.Authorized);
                //     Debug.Log("打开数据追踪");
                // });
                SetMaxSDKConfig(sdkConfiguration);
            }
        };
#endif
    }

    static void SetMaxSDKConfig(MaxSdkBase.SdkConfiguration sdkConfiguration) {
        maxSDKConfig = sdkConfiguration;
    }

    void OpenADTrack(MaxSdkBase.SdkConfiguration sdkConfiguration) {
#if UNITY_IPHONE || UNITY_IOS
        AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(sdkConfiguration.AppTrackingStatus == MaxSdkBase.AppTrackingStatus.Authorized);
        Debug.Log("打开数据追踪");
#endif
    }
#endregion

#region ABTest

    public static ABConfig abConfig = new ABConfig();
    //ABTest
    public static void ABTestInit()
    {
        abConfig = new ABConfig();
        //初始赋值
        if (GameData.ABGroup == -1)
        {
            AppLovin.WhenInitialized(() =>
            {
                AppLovin.LoadRemoteData(abConfig);
                Debug.LogWarning("AppLovin初始化回调 获取广告机制数据" +
                    $"\reward_level: {abConfig.reward_level}");
                //不需要这个了，用difficulty和Rv_optim分组，但是还是保留下来吧
                abConfig.abGroup = (ABGroup)Enum.Parse(typeof(ABGroup), abConfig.ab_experiment_group.Split('_')[0]);
                abConfig.SetGroup(abConfig.abGroup, abConfig.reward_level, true);
            });
        }
        else {
            abConfig.SetGroup((ABGroup)GameData.ABGroup,GameData.RewardLevel);
        }
        //Debug.Log("group:"+ abConfig.abGroup);
        //Debug.Log("ABTestInit");
    }
#endregion

#region DebugMode

    private void DebugModeConfig()
    {
        var debugMode = PlayerPrefs.GetInt(ConstDefine.IsDebug, -1);

        if (debugMode == -1)
        {
            var path = "";
#if UNITY_EDITOR || UNITY_IPHONE
            path = "file://" + Application.streamingAssetsPath + "/DebugMode.txt";
#else
        path =  Application.streamingAssetsPath + "/DebugMode.txt";
#endif
            StartCoroutine(DebugFileRead(path));
        }
        else
        {
            IsDebug = debugMode == 1;
            Messenger.Broadcast(ConstDefine.Listener.ReadDebugMode);
        }
    }

    IEnumerator DebugFileRead(string path)
    {
        var request = UnityEngine.Networking.UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

        var debugMode = int.Parse(request.downloadHandler.text.Trim());
        IsDebug = debugMode == 1;
#if !UNITY_EDITOR
        PlayerPrefs.SetInt(ConstDefine.IsDebug, debugMode);
#endif
        Messenger.Broadcast(ConstDefine.Listener.ReadDebugMode);
    }
#endregion

#region GDPR
    private void GDPRInit()
    {
        LionGDPR.OnOpen += () =>
        {
            //Debug.Log("GDPR 当前状态：" + LionGDPR.Status);
            //Debug.Log("GDPR Completed：" + LionGDPR.Completed);
            //Debug.Log("GDPR AdConsentGranted：" + LionGDPR.AdConsentGranted);
            //Debug.Log("GDPR AnalyticsConsentGranted：" + LionGDPR.AnalyticsConsentGranted);
            //Debug.Log("GDPR Time:" + Time.time);
            //游戏一开始进入 GDPR.OnOpen 函数，并处于完成状态，分析均已同意则不再锁定（因为此情况下，lionGDPR并不会执行 OnClosed 函数）
            if ( LionGDPR.AdConsentGranted && LionGDPR.AnalyticsConsentGranted && !SettingPanel.isOpenGPDR)
            {
                canRunGame = true;
                Debug.Log("open gdpr but run game");
                return;
            }
            canRunGame = false;           
            Debug.Log("open gdpr & stop game");
        };

        LionGDPR.OnClosed += () =>
        {
            canRunGame = true;
            Debug.Log("close gdpr");
        };
    }

#endregion

}

public class ABConfig
{
    public string ab_experiment_group = "a_0514";
    public int level_difficulty = 1;
    public int rv_optimized = 1;
    public int level_optimization = 0;
    public int bonus_level = 1;
    public ABGroup abGroup = ABGroup.a;
    public int reward_level = 0;//肌肉零食关卡 0：无，1：有
    public int childTime ;
    public int snackTime ;
    public float aiSpeed ;
    public ABConfig() {
        SetGroup(abGroup,reward_level);
    }

    public void SetGroup(ABGroup _group,int _reward_level, bool _isSave =false) {
        abGroup = _group;
        reward_level = _reward_level;
        if (_group == ABGroup.a)
        {
            childTime = 30;
            snackTime = 20;
            aiSpeed = 0.7f;
        }
        else {
            childTime = 25;
            snackTime = 20;
            aiSpeed = 0.6f;
        }
        if (_isSave) {
            GameData.ABGroup = (int)_group;
            GameData.RewardLevel = _reward_level;
        }           
    }
}

public enum ABGroup
{
    a,
    b,
}

public enum SceneType { 
    Level,
    SkinScene,
    GetNewSkin,
    GetNewProp,
    DisCountStore
}
public enum EnvType {
    Hall,
    Bathroom, 
    Beach,
    Park
}
//public enum EWeapon
//{
//    NULL,
//    Hammer,
//    BaseBallBar,
//    Wrench,
//    Pepper, 
//    Closestool
//}
public class LevelScenes
{
    private static Dictionary<int, LvInfo> lvInfoLDict;
    public static Dictionary<int, LvInfo> LvInfoLDict {
        get {
            if (lvInfoLDict == null) {
                lvInfoLDict = new Dictionary<int, LvInfo>();
                TextAsset infos = AssetsLoader.Load<TextAsset>("LvInfo");
                LvInfoList infoList = LitJson.JsonMapper.ToObject<LvInfoList>(infos.text);

                foreach (LvInfo lvInfo in infoList.lvInfoList)
                {
                    lvInfoLDict.Add(lvInfo.Lv, lvInfo);
                }
            }
            return lvInfoLDict;
        }
    }

    public static BindableProperty<int> level
    {
        get
        {
            if (_level == null)
            {
                _level = new BindableProperty<int>(PlayerPrefs.GetInt("Level", 1));
                _level.OnChange += () => {
                    PlayerPrefs.SetInt("Level", level.Value); };
            }
            return _level;
        }
        set
        {
            level.Value = value;
        }
    }
    public static BindableProperty<int> randomLevel
    {
        get
        {
            if (_randomLevel == null)
            {
                _randomLevel = new BindableProperty<int>(GameData.RandomLevel);
                _randomLevel.OnChange += () =>
                {
                    GameData.RandomLevel = randomLevel.Value;
                };
            }
            return _randomLevel;
        }
        set
        {
            _randomLevel.Value = value;
        }
    }

    private static BindableProperty<int> _level;
    private static BindableProperty<int> _randomLevel;

    /// <summary>
    /// 当前关卡是否进入了循环
    /// </summary>
    /// <returns></returns>
    public static bool IsLoopLevel()
    {
        return level >= MaxLevel;//level >= MaxLevel - 1;
    }
    //更新循环时随机关卡（用于未排序的场景）
    public static void UpdateRandomLv()
    {
        randomLevel.Value = UnityEngine.Random.Range(3, MaxLevel);
    }
    //当前关卡的加载ID（1~max）
    public static int CurLoadLvId() {
        if (IsLoopLevel())
        {
            if (GameManager.abConfig.level_optimization == 0)
            {
                return randomLevel;
            }
            else {
                return GetLevelIndex(level);
            }
        }
        else {
            return level;
        }
    }
    
    private static bool isLoadLevel = false;
    

    public static async void LoadScene()
    {
        if (isLoadLevel)
            return;
        try
        {
            isLoadLevel = true;
            if (level < 0)
            {
#if UNITY_EDITOR
                await SceneManager.LoadSceneAsync($"Scenes/level{level}");
#else
                    await BundleManager.LoadScene("level.scene", $"level{level}");
#endif
            }
            else
            {
                if (IsLoopLevel())
                {
#if UNITY_EDITOR
                    await SceneManager.LoadSceneAsync($"Scenes/level{randomLevel}");
#else
                        await BundleManager.LoadScene("level.scene", $"level{randomLevel}");
#endif
                }
                else
                {
                    Debug.Log("load lv:" + level);
#if UNITY_EDITOR
                    await SceneManager.LoadSceneAsync($"Scenes/level{GetLevelIndex(level)}");
#else
                        await BundleManager.LoadScene("level.scene", $"level{GetLevelIndex(level)}");
#endif
                }
            }
        }
        catch (Exception)
        {
            isLoadLevel = false;
            throw;
        }
        isLoadLevel = false;
        //SceneManager.LoadScene(GetLevelIndex(level));
    }
    public static async void LoadScene(string _sceneName)
    {
        if (isLoadLevel)
            return;
        try
        {
            isLoadLevel = true;
#if UNITY_EDITOR
            await SceneManager.LoadSceneAsync(_sceneName);
#else
                    await BundleManager.LoadScene("level.scene", _sceneName);
#endif
        }
        catch (Exception)
        {
            isLoadLevel = false;
            throw;
        }
        isLoadLevel = false;
    }

    public static int MaxLevel
    {
        get
        {
            return 45;//41 project文件夹level最大编号+1
        }
    }

    /// <summary>
    /// 转换到循环关卡
    /// </summary>
    /// <returns></returns>
    public static int GetLevelIndex(int level)
    {
        int loopBeginIndex = 2;//5
        if (level < MaxLevel)
            return level;
        var t = level;
        t = (t - MaxLevel) % (MaxLevel - loopBeginIndex);
        return t + loopBeginIndex;
    }

    public static void AddLevel()
    {
        level.Value++;
        if (IsLoopLevel()) {
            if (GameManager.abConfig.level_optimization == 0)
            {
                UpdateRandomLv();
            }
        }
    }

    public static EnvType GetEnvType(int lv) {
        var _lv = lv % MaxLevel;
        _lv = _lv == 0 ? MaxLevel : _lv;

        if (_lv <= 8)
        {
            return EnvType.Hall;
        }
        else if (_lv <= 15)
        {
            return EnvType.Bathroom;
        }
        else if (_lv <= 23)
        {
            return EnvType.Beach;
        }
        else
            return EnvType.Park;
    }

    public static void LevelStartedEvent()
    {
        Dictionary<string, object> eventParams = new Dictionary<string, object>();
        eventParams["level"] = level.Value;
        //eventParams["ABtest"] = GameManager.abConfig.ab_experiment_group;
        eventParams["reward_level"] = GameManager.abConfig.reward_level;
        Analytics.Events.LevelStarted(eventParams);
        //GA打点
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "Level" + level.Value.ToString("D3"), 
            GameManager.Instance.localPlayer.IsSnack?"Snack":"Child");
#if UNITY_EDITOR
        Debug.Log("<color=green>LevelStartedEvent</color>");
#endif
    }

    public static void LevelCompleteEvent()
    {
        Dictionary<string, object> eventParams = new Dictionary<string, object>();
        eventParams["level"] = level.Value;
        eventParams["reward_level"] = GameManager.abConfig.reward_level;
        Analytics.Events.LevelComplete(eventParams);
        //GA打点
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete,"Level" + level.Value.ToString("D3"),
            GameManager.Instance.localPlayer.IsSnack ? "Snack" : "Child");
#if UNITY_EDITOR
        Debug.Log("<color=green>LevelCompleteEvent</color>");
#endif
    }

    public static void LevelFailedEvent()
    {
        Dictionary<string, object> eventParams = new Dictionary<string, object>();
        eventParams["level"] = level.Value;
        eventParams["reward_level"] = GameManager.abConfig.reward_level;
        Analytics.Events.LevelFailed(eventParams);
        //GA打点
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail,"Level" + level.Value.ToString("D3"),
            GameManager.Instance.localPlayer.IsSnack ? "Snack" : "Child");
#if UNITY_EDITOR
        Debug.Log("<color=green>LevelFailedEvent</color>");
#endif
    }

    public static void LevelRestartEvent()
    {
        Dictionary<string, object> eventParams = new Dictionary<string, object>();
        eventParams["level"] = level.Value;
        eventParams["reward_level"] = GameManager.abConfig.reward_level;
        Analytics.Events.LevelRestart(eventParams);
        //GA打点
        //GameAnalytics.NewProgressionEvent(GAProgressionStatus.Restart, "Level" + level.Value.ToString("D3"), 
        //    GameManager.Instance.localPlayer.IsSnack ? "Snack" : "Child");
    }
}
