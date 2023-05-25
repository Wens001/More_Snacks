
/****************************************************
 * FileName:		AdsManager.cs
 * CompanyName:		
 * Author:			
 * Email:			
 * CreateTime:		2021-3-29-16:55:25
 * Version:			1.0
 * UnityVersion:	2019.4.9f1
 * Description:		Nothing
 * 
*****************************************************/
using UnityEngine;
using LionStudios;
using LionStudios.Ads;
using System;
using com.adjust.sdk;
using GameAnalyticsSDK;
using LionStudios.Runtime.Sdks;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Purchasing;
using com.adjust.sdk.purchase;
using LionStudios.Debugging;
using Firebase;
using Firebase.Analytics;

public class AdsManager : MonoBehaviour, IGameAnalyticsATTListener
{
    private static AdsManager _Instance;

    private static BindableProperty<bool> m_ShowAds;
    public static BindableProperty<bool> ShowAds
    {
        get
        {
            if (m_ShowAds == null)
            {
                m_ShowAds = new BindableProperty<bool>(true);
                m_ShowAds.OnChange = () => {
                    if (m_ShowAds.Value == false)
                        Banner.Hide();
                };
            }
            return m_ShowAds;
        }
    }

    public static AdsManager Instance
    {
        get
        {
            if (_Instance)
                return _Instance;
            var go = new GameObject() { name = "AdsManager" };
            DontDestroyOnLoad(go);
            _Instance = go.AddComponent<AdsManager>();
            return _Instance;
        }

    }

    public static bool HasInitLionSdk = false;

    #region ABTest
    public static string ABGroupName
    {
        get => GameManager.abConfig.ab_experiment_group;
    }
    #endregion   

    #region Banner
    public static void ShowBanner()
    {
        if (ShowAds == false)
            return;
        if (GameData.NoAds || GameData.AllInOneBundle)
            return;
        try
        {
            if (Banner.IsAdReady && !Banner.IsShowing)
            {
                //Debug.Log("请求banner");
                Banner.Show();
                SendAdEvent(GAAdAction.Show, GAAdType.Banner);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public static void HideBanner()
    {
        if (ShowAds == false)
            return;
        try
        {
            Banner.Hide();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    #endregion

    #region 插屏广告
    private static ShowAdRequest _InterRequest;
    private static ShowAdRequest InterstitialRequest
    {
        get
        {
            if (_InterRequest == null)
            {
                _InterRequest = new ShowAdRequest
                {
                    OnClicked = AdsActionLog,
                    OnDisplayed = AdsActionLog,
                    OnHidden = AdsActionLog,
                    OnFailedToDisplay = FailedToDisplay,
                };             
            }
            return _InterRequest;
        }
    }   

    private static void AdsActionLog(string ss)
    {
        ReStart();
    }
    private static void FailedToDisplay(string msg, int code)
    {
        Debug.LogError($"Ads Error:{msg},code:{code}");
    }  

    public static void ShowInterstitial()
    {
        if (ShowAds == false)
            return;
        if (GameData.NoAds || GameData.AllInOneBundle)
            return;
        if (LevelScenes.level > 0 && LevelScenes.level <= Interstitial.InterstitialStartLevel)
        {
            //Debug.Log($"Level Is {LevelScenes.level} , Play Level is {Interstitial.InterstitialStartLevel}");
            return;
        }
        if (LevelScenes.level < 2) {
            return;
        }
        if ( timer < Interstitial.MinInterstitialInterval)
        {
            Debug.Log("Interstitial 间隔不够");
            return;
        }
        if (!Interstitial.IsAdReady)
        {
            Debug.Log("Interstitial IsNotReady");
            MaxSdk.LoadInterstitial(LionSettings.AppLovin.InterstitialAdUnitId);
            return;
        }
        
        try
        {
            //InterstitialRequest.SetEventParam("experiment_group", GameManager.abConfig.ab_experiment_group);
            InterstitialRequest.SetEventParam("reward_level", GameManager.abConfig.reward_level);
            Interstitial.Show(InterstitialRequest, LevelScenes.level);
            SendAdEvent(GAAdAction.Show, GAAdType.Interstitial, LevelScenes.level.ToString());
            //Debug.Log("Show ADS Interstitial");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        ReStart();
    }
    #endregion

    #region 激励广告
    private static ShowAdRequest _RewardRequest;
    private static ShowAdRequest RewardedAdRequest
    {
        get
        {
            if (_RewardRequest == null)
            {
                _RewardRequest = new ShowAdRequest
                {
                    OnClicked = AdsActionLog,
                    OnDisplayed = AdsActionLog,//AdsActionLog
                    OnHidden = AdsActionLog,
                    OnFailedToDisplay = FailedToDisplay,
                    OnReceivedReward = SetReceivedReward
                };                
            }
            return _RewardRequest;
        }
    }

    public static Action RewardAction;
    public static Action NoRewardAction;
   

    private static void SetReceivedReward(string msg, MaxSdkBase.Reward reward)
    {
        RewardAction?.Invoke();
        //Debug.Log("reward:" + reward.ToString());
        ReStart();
    }
   
    public static void ShowRewardedAd(string msg = null)
    {
        if (ShowAds == false)
        {
            RewardAction();
            return;
        }
        if (!RewardedAd.IsAdReady)
        {
            Debug.Log("Rewarded IsNotReady");
            NoRewardAction?.Invoke();
            return;
        }
        //看要不要用在GA打点上
        string sendmsg = "";
        if (!string.IsNullOrEmpty(msg))
        {
            RewardedAdRequest.SetPlacement(msg);
            RewardedAdRequest.SetLevel(LevelScenes.level);
            RewardedAdRequest.SetEventParam("reward_level", GameManager.abConfig.reward_level);
            //RewardedAdRequest.SetEventParam("experiment_group", ABGroupName);

            RewardForMoreTime = msg.Equals("Addtime");
            sendmsg = $"{msg}\nlevel : {LevelScenes.level}\nexperiment_grou : {ABGroupName}";
        }
        try
        {
            RewardedAd.Show(RewardedAdRequest, LevelScenes.level);
            SendAdEvent(GAAdAction.Show, GAAdType.RewardedVideo, msg);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }      
    }

    //广告收入跟踪
    static void AdRevenueTracking(MaxSdkBase.AdInfo adInfo,string placement) {
        if (adInfo == null) {
            Debug.LogWarning("adInfo is null!");
            return;
        }
        try
        {
            // initialise with AppLovin MAX source
            AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAppLovinMAX);
            if (adjustAdRevenue == null) {
                Debug.LogWarning("adjustAdRevenue is null!");
                return;
            }
            // set revenue and currency
            adjustAdRevenue.setRevenue(adInfo.Revenue, "USD");
            // optional parameters
            adjustAdRevenue.setAdImpressionsCount(10);
            adjustAdRevenue.setAdRevenueNetwork(adInfo.NetworkName);
            adjustAdRevenue.setAdRevenueUnit("unit");
            adjustAdRevenue.setAdRevenuePlacement(placement);
            // track ad revenue
            Adjust.trackAdRevenue(adjustAdRevenue);
            //Debug.LogWarning("adjustAdRevenue没问题!");
        }
        catch (Exception e){
            Debug.LogWarning("广告收入跟踪有问题: "+e);
        }      
    }

    /// <summary>
    /// 激励广告是否准备好
    /// </summary>
    public static bool IsAdReady{
        get {
            return RewardedAd.IsAdReady;
        }
    }
    #endregion
    
    #region 交叉广告   
    public static void ShowCrossPromo()
    {
        if (ShowAds == false)
            return;
        if (GameData.NoAds || GameData.AllInOneBundle)
            return;
        try
        {
            CrossPromo.Show();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public static void HideCrossPromo() {
        if (ShowAds == false)
            return;
        try
        {
            CrossPromo.Hide();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    #endregion

    #region 内购

    public static Action BuySuccessEvent;
    //初始化内购
    public void InitIAP()
    {
        IAPManager.Instance.InitializeIAPManager(
            (IAPOperationStatus status, string message, List<StoreProduct> shopProducts) =>
            {
                if (status == IAPOperationStatus.Fail)
                {
                    Debug.LogError("内购初始化失败： " + message);

                }
                else
                {
                    Debug.Log("内购初始化成功： " + message);
                    CheckIAP();
                }
            });

    }
    //检测IAP情况
    static void CheckIAP() {
        if (!IAPManager.Instance.IsInitialized())
        {
            Debug.LogError("检测IAP，但IAP未初始化");
            return;
        }

        if (!GameData.NoAds)
        {
            GameData.NoAds = IAPManager.Instance.IsActive(ShopProductNames.No_Ads);
        }

        if (!GameData.AllInOneBundle)
        {
            var hasAll = IAPManager.Instance.IsActive(ShopProductNames.All_In_One_Bundle)
                            || IAPManager.Instance.IsActive(ShopProductNames.All_In_One_Bundle_sale);

            if (hasAll)
            {
                Messenger.Broadcast<int>(ConstDefine.Listener.AddCoinValue, ConstDefine.OneBundleCoin);
                Messenger.Broadcast(ConstDefine.Listener.BugAllInOne);
                SkinManager.IAPforSkins();
                GameData.AllInOneBundle = true;
            }
        }
    }

    //购买商品
    public static void BuyPorduct(ShopProductNames ProductName)
    {
        IAPManager.Instance.BuyProduct(ProductName,
            (IAPOperationStatus status, string message, StoreProduct product) =>
            {
                if (status == IAPOperationStatus.Success)
                {
                    OnGotProduct(ProductName);
                    BuySuccessEvent?.Invoke();
                }
                else
                {
                    Debug.LogError("购买失败。。。" + message);
                    BuySuccessEvent = null;
                }
            });
    }

    private static void OnGotProduct(ShopProductNames ProductName)
    {
        switch (ProductName)
        {
            case ShopProductNames.No_Ads:
                GameData.NoAds = true;
                Messenger.Broadcast(ConstDefine.Listener.BugNoAds);
                break;
            case ShopProductNames.All_In_One_Bundle:
            case ShopProductNames.All_In_One_Bundle_sale:
                if (!GameData.AllInOneBundle) {
                    Messenger.Broadcast<int>(ConstDefine.Listener.AddCoinValue, ConstDefine.OneBundleCoin);
                    Messenger.Broadcast(ConstDefine.Listener.BugAllInOne);
                    SkinManager.IAPforSkins();
                }               
                GameData.AllInOneBundle = true;
                break;
            default:
                break;
        }
    }

    //回购
    public void ProductRestore()
    {
        //only required for iOS App Store
        //restores previously bought products
        //this is also done automatically every time at initialize
        IAPManager.Instance.RestorePurchases(
            // automatically called after one product is restore, is the same with Buy Product callback
            (IAPOperationStatus status, string message, StoreProduct product) =>
            {
                if (status == IAPOperationStatus.Success)
                {
                    //消耗品不会回购
                    var ProductName = (ShopProductNames)Enum.Parse(typeof(ShopProductNames), product.productName);
                    OnGotProduct(ProductName);
                }
                else
                {
                    Debug.LogError("回购失败。。。");
                }
            });
    }

    /// <summary>
    /// 购买完成回调
    /// </summary>
    /// <param name="purchaseEventArgs"></param>
    public void OnPurchaseComplete(PurchaseEventArgs purchaseEventArgs,string productName)
    {
        Debug.LogError("购买结束, 执行unity purchase回调");

        ValidateAndTrackPurchase(productName,purchaseEventArgs, (validationState) =>
        {
            if (validationState == ADJPVerificationState.ADJPVerificationStatePassed)
            {
                //Debug.Log("Purchase is valid, do smth");
                Debug.LogError("购买结束, adjust回调时间执行中。。。");
            }
        });
    }

#region 额外的IAP查询功能
    //Get the amount of in game currency received for a product (use this to display the reward)
    //应该是消耗品的reward
    public static int GetProductReward(ShopProductNames product) {
        try
        {
            return IAPManager.Instance.GetValue(product);
        }
        catch (Exception e){
            Debug.LogError(e);
            return 0;
        }
    }

    // Get the price and currency code of the product as a string (use this to display the price)
    //商品价格，返回字符串
    public static string GetLocalizedPriceString(ShopProductNames product)
    {
        try
        {
            return IAPManager.Instance.GetLocalizedPriceString(product);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return "";
        }
    }

    //Get product price denominated in the local currency
    //商品价格
    // public static int GetProductPrice(ShopProductNames product)
    // {
    //     try
    //     {
    //         return IAPManager.Instance.GetPrice(product);
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogError(e);
    //         return -1;
    //     }
    // }

    // Get product currency in ISO 4217 format; e.g. GBP or USD
    //返回货币字符
    // public static string GetIsoCurrencyCode(ShopProductNames product)
    // {
    //     try
    //     {
    //         return IAPManager.Instance.GetIsoCurrencyCode(product);
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogError(e);
    //         return "";
    //     }
    // }

    // Get description from the store
    //从商店获取描述
    public static string GetLocalizedDescription(ShopProductNames product) {
        try
        {
            return IAPManager.Instance.GetLocalizedDescription(product);
        }
        catch (Exception e) {
            Debug.LogError(e);
            return "";
        }
    }

    // Get title from the store
    //从商店获取标题
    public static string GetLocalizedTitle(ShopProductNames product)
    {
        try
        {
            return IAPManager.Instance.GetLocalizedTitle(product);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return "";
        }
    }
#endregion

#endregion


    public static void ReStart()
    {
        timer = 0;
    }
   
#region GameAnalytics打点
    public void InitGA() {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            GameAnalytics.RequestTrackingAuthorization(this);
        }
        else
        {
            GameAnalytics.Initialize();
        }
        Debug.Log("GA init!");
        //Messenger.AddListener(ConstDefine.Listener.ReadDebugMode, GameAnalyticsMode);
    }
    
    //获取广告ID
    private static string GetAdUnitId(GAAdType _type)
    {
        if (_type == GAAdType.Interstitial)
        {
            return LionSettings.AppLovin.InterstitialAdUnitId;
        }
        else if (_type == GAAdType.RewardedVideo)
        {
            return LionSettings.AppLovin.RewardedAdUnitId;
        }
        else if (_type == GAAdType.Banner)
        {
            return LionSettings.AppLovin.BannerAdUnitId;
        }
        return null;
    }
    //GA广告打点
    public static void SendAdEvent(GAAdAction adAction, GAAdType adType, string adPlacement = "")
    {
        var adUnitId = GetAdUnitId(adType);
        if (adUnitId == null)
        {
            Debug.LogWarning("GA send warning , adType:" + adType);
            return;
        }

#if UNITY_EDITOR
        MaxSdkBase.AdInfo adinfo = MaxSdk.GetAdInfo(adUnitId);
#elif UNITY_ANDROID
        MaxSdkBase.AdInfo adinfo = MaxSdkAndroid.GetAdInfo(adUnitId);
#elif UNITY_IOS
        MaxSdkBase.AdInfo adinfo = MaxSdkiOS.GetAdInfo(adUnitId);
#endif
        string adNetworkName = "";
        if (adinfo != null)
        {
            adNetworkName = adinfo.NetworkName.ToLower();
        }
        GameAnalytics.NewAdEvent(adAction, adType, adNetworkName, adPlacement);
        //广告收入跟踪
#if UNITY_IOS
        AdRevenueTracking(adinfo, adPlacement);
#endif
    }

    //GA广告打点
    public static void SendBusinessEvent(string currency, int amount, string itemType, string itemId, string cartType, string receipt, string signature) {
#if UNITY_ANDROID
        GameAnalytics.NewBusinessEventGooglePlay(currency, amount, itemType, itemId, cartType, receipt, signature);
#elif UNITY_IOS
        GameAnalytics.NewBusinessEvent(currency, amount, itemType, itemId, cartType);      
#endif
    }

    public void GameAnalyticsATTListenerNotDetermined()
    {
        GameAnalytics.Initialize();
    }
    public void GameAnalyticsATTListenerRestricted()
    {
        GameAnalytics.Initialize();
    }
    public void GameAnalyticsATTListenerDenied()
    {
        GameAnalytics.Initialize();
    }
    public void GameAnalyticsATTListenerAuthorized()
    {
        GameAnalytics.Initialize();
    }

    void GameAnalyticsMode()
    {
        GameAnalytics.SettingsGA.SubmitErrors = GameManager.IsDebug;
        GameAnalytics.SettingsGA.SubmitFpsAverage = GameManager.IsDebug;
        GameAnalytics.SettingsGA.SubmitFpsCritical = GameManager.IsDebug;
    }
#endregion

    public static float timer;
    void Update()
    {
        if (LevelScenes.level <= Interstitial.InterstitialStartLevel)
            return;

        if (timer <= Interstitial.MinInterstitialInterval)
            timer += Time.deltaTime;
    }

#region Adjust

#if UNITY_IOS
    private string iap_purchase = "10j6eo";
    private string purchase_failed = "12a3zb";
    private string purchase_unknown = "7jixh2";
    private string purchase_notverified = "xjh2y2";
    private string adjustToken = "6yg61cepkqrk";


#else
    private string iap_purchase = "tyd0e6";
    private string purchase_failed = "t0cl80";
    private string purchase_unknown = "h0mzxi";
    private string purchase_notverified = "wcdc1g";
    private string adjustToken = "mhwoy0hcojcw";
#endif

    public void InitAdjust() {
        var adjustEnv = GameData.IsSandboxMode ? AdjustEnvironment.Sandbox : AdjustEnvironment.Production;
        var adjustPEnv = GameData.IsSandboxMode ? ADJPEnvironment.Sandbox : ADJPEnvironment.Production;
        var adjustLog = GameData.IsSandboxMode ? AdjustLogLevel.Verbose : AdjustLogLevel.Info;
        var adjustPLog = GameData.IsSandboxMode ? ADJPLogLevel.Verbose : ADJPLogLevel.Info;
        //adjust初始化
        var adjustConfig = new AdjustConfig(adjustToken, adjustEnv, true);
        adjustConfig.setLogLevel(adjustLog);
        adjustConfig.setSendInBackground(true);
        new GameObject("Adjust").AddComponent<com.adjust.sdk.Adjust>();
        com.adjust.sdk.Adjust.start(adjustConfig);
        
        //初始化Adjust内购 并验证
        var adjustPVConfig = new ADJPConfig(adjustToken, adjustPEnv);
        adjustPVConfig.SetLogLevel(adjustPLog);
        new GameObject("AdjustPurchase").AddComponent<AdjustPurchase>();
        AdjustPurchase.Init(adjustPVConfig);
        Debug.Log("adjust init!");
    }

    /// <summary>
    /// Adjust验证并追踪购买（打点？）
    /// </summary>
    /// <param name="purchaseEventArgs"></param>
    /// <param name="resultCallback"></param>
    public void ValidateAndTrackPurchase(string productName, PurchaseEventArgs purchaseEventArgs, Action<ADJPVerificationState> resultCallback)
    {
        var IPrice = purchaseEventArgs.purchasedProduct.metadata.localizedPrice;
        var price = decimal.ToDouble(IPrice);
        var currencyCode = purchaseEventArgs.purchasedProduct.metadata.isoCurrencyCode;
        var transactionID = purchaseEventArgs.purchasedProduct.transactionID;
        var productID = purchaseEventArgs.purchasedProduct.definition.id;
        var receiptDict = (Dictionary<string, object>)MiniJson.JsonDecode(purchaseEventArgs.purchasedProduct.receipt);
        var payload = (receiptDict != null && receiptDict.ContainsKey("Payload")) ? (string)receiptDict["Payload"] : "";

        if (GameData.ShowDebug == 1) {
            Debug.Log($"ProductName: {productName}\n" +
                $"IPrice: {IPrice}\n" +
                $"prize: {price}\n" +
                $"currencyCode: {currencyCode}\n" +
                $"transactionID: {transactionID}\n" +
                $"productID: {productID}\n" +
                $"storeSpecificId: {purchaseEventArgs.purchasedProduct.definition.storeSpecificId}\n" +
                $"receipt: {purchaseEventArgs.purchasedProduct.receipt}\n" +
                $"payload: {payload}\n");
        }       

        Action<ADJPVerificationInfo> verificationCb = (verificationInfo) =>
        {
            AdjustEvent adjustEvent = null;
            switch (verificationInfo.VerificationState)
            {
                case ADJPVerificationState.ADJPVerificationStatePassed:
                    adjustEvent = new AdjustEvent(iap_purchase);
                    adjustEvent.setRevenue(price, currencyCode);
                    adjustEvent.setTransactionId(transactionID); // in-SDK deduplication
                    adjustEvent.addCallbackParameter("productID", productID);
                    adjustEvent.addCallbackParameter("transactionID", transactionID);
                    com.adjust.sdk.Adjust.trackEvent(adjustEvent);
                    //GA购买打点
                    SendBusinessEvent(currencyCode, decimal.ToInt32(IPrice * 100), productName, productID, "", payload, "");
                    break;
                case ADJPVerificationState.ADJPVerificationStateFailed:
                    adjustEvent = new AdjustEvent(purchase_failed);
                    adjustEvent.addCallbackParameter("productID", productID);
                    adjustEvent.addCallbackParameter("transactionID", transactionID);
                    com.adjust.sdk.Adjust.trackEvent(adjustEvent);
                    break;
                case ADJPVerificationState.ADJPVerificationStateUnknown:
                    adjustEvent = new AdjustEvent(purchase_unknown);
                    adjustEvent.addCallbackParameter("productID", productID);
                    adjustEvent.addCallbackParameter("transactionID", transactionID);
                    com.adjust.sdk.Adjust.trackEvent(adjustEvent);
                    break;
                default:
                    adjustEvent = new AdjustEvent(purchase_notverified);
                    adjustEvent.addCallbackParameter("productID", productID);
                    adjustEvent.addCallbackParameter("transactionID", transactionID);
                    com.adjust.sdk.Adjust.trackEvent(adjustEvent);
                    break;
            }

            if (resultCallback != null)
            {
                resultCallback(verificationInfo.VerificationState.Value);
            }
        };

#if UNITY_IOS
        AdjustPurchase.VerifyPurchaseiOS(payload, transactionID, productID, verificationCb);
#elif UNITY_ANDROID
        var jsonDetailsDict = (!string.IsNullOrEmpty(payload)) ? (Dictionary<string, object>)MiniJson.JsonDecode(payload) : null;
        var json = (jsonDetailsDict != null && jsonDetailsDict.ContainsKey("json")) ? (string)jsonDetailsDict["json"] : "";
        var gpDetailsDict = (!string.IsNullOrEmpty(json)) ? (Dictionary<string, object>)MiniJson.JsonDecode(json) : null;
        var purchaseToken = (gpDetailsDict != null && gpDetailsDict.ContainsKey("purchaseToken")) ? (string)gpDetailsDict["purchaseToken"] : "";

        AdjustPurchase.VerifyPurchaseAndroid(productID, purchaseToken, "", verificationCb);
#endif
    }
#endregion

#region Firebase
    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    public static void InitFirebase() {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
    }
    public void CheckFireBase() {

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {

            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // subscribe to firebase events
                // subscribe here so avoid error if dependency check fails
                Debug.Log("firebase 检查依赖成功");
                //LionStudios.Analytics.OnLogEvent += LogFirebaseEvent;
                //LionStudios.Analytics.OnLogEventUA += LogUAFirebaseEvent;
            }
            else
            {
                Debug.LogError($"Firebase: Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }


    
#endregion

#region 激励相关回调

    public void RewardCallBackInit()
    {
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardOpen;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardAdsHide;
        //MaxSdkCallbacks.OnRewardedAdDisplayedEvent += OnRewardOpen;
        //MaxSdkCallbacks.OnRewardedAdHiddenEvent += OnRewardAdsHide;
    }


    // 当为了 加时 显示激励广告时，设置bool值暂停游戏计时，激励结束关闭bool值，打开计时。
    private static bool RewardForMoreTime = false;
    private void OnRewardOpen(string str,MaxSdkBase.AdInfo _info)
    {
        if (RewardForMoreTime)
        {
            GameManager.Instance.GameSpeed = 0;
        }
    }

    private void OnRewardAdsHide(string str, MaxSdkBase.AdInfo _info)
    {
        if (RewardForMoreTime)
        {
            RewardForMoreTime = false;
            GameManager.Instance.GameSpeed = 1; //结算时，可能会出现————先加载完关卡再调用该方法，导致游戏在一开始就在计时；因此增加额外的逻辑判断
        }  
    }
#endregion

    public  void ShowGetReview() { 
    
    }
}
