using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Facebook.Unity.Gameroom;
using GameAnalyticsSDK.Setup;
//using System.Security.Cryptography;

namespace MyUI {
	public partial class GamePanel : MonoBehaviour, BasePanel
	{
		public Transform trans => transform;
        private Image[] snackIcons;
        [Header("零食存活图标")]
        public Sprite snack_Show ;
        public Sprite snack_Dead ;
        public Sprite snack_reward;
        [Header("使用零食按钮图标")]
        public Sprite freeSnack_sp;
        public Sprite moneySnack_sp;
        public Sprite adSnack_sp;
        private Image moneySnack_img;
        private Image adSnack_img;
        [Header("道具图标")]
        public Sprite[] propSps;
        [Header("钥匙图标")]
        public Sprite hasKeySp;
        public Sprite noKeySp;
        private Image[] keyImgs;
        [Header("地图类型图标")]
        public Sprite[] mapTypeSps;
        [Header("场景进度试用道具未选")]
        public Sprite[] sceneIconNoPassSps;
        [Header("场景进度试用道具已选")]
        public Sprite[] sceneIconHasPassSps;
        private List<Image> sceneIncoImgList; 
        //具体的零食
        private SkinInfo moneySnackInfo;
        private SkinInfo adSnackInfo;

        private MyTimer checkNet = new MyTimer(1);

        private BindableProperty<int> m_CoinValue;
        public BindableProperty<int> CoinValue
        {
            get
            {
                if (m_CoinValue == null)
                {
                    m_CoinValue = new BindableProperty<int>(PlayerPrefs.GetInt("Coin",0));
                    m_CoinValue.OnChange = () =>
                    {
                        PlayerPrefs.SetInt("Coin", m_CoinValue);
                        if (m_Txt_CoinValue)
                            m_Txt_CoinValue.text = m_CoinValue.ToString();
                    };
                }
                return m_CoinValue;
            }
        }

        public void OnEnter()
		{
            Messenger.AddListener<BaseControl, BaseControl>(ConstDefine.Listener.EatSnake, OnEatSnake);
            /**********评分界面***********/
            if (LevelScenes.level == 2 && !GameData.IsRateScore) {
                GameData.IsRateScore = true;
#if UNITY_EDITOR || UNITY_ANDROID
                UIPanelManager.Instance.PushPanel(UIPanelType.ScorePanel);
#else
                UnityEngine.iOS.Device.RequestStoreReview();
#endif
            }
            /****************************/
            
            if (!GameManager.Instance.hasShowReviveCountdown)
            {               
                ResetState();
                m_Btn_Play.gameObject.SetActive(true);
                m_Rect_DownGroup.gameObject.SetActive(true);
                if (GameData.Level >= 4 && !m_Btn_Skin.gameObject.activeSelf)
                {
                    m_Btn_Skin.gameObject.SetActive(true);
                }
                m_Btn_Setting.gameObject.SetActive(true);
                //初始化内购按钮
                m_Btn_NoAds.gameObject.SetActive(true);
                m_Btn_Store.gameObject.SetActive(true);
                if (GameData.AllInOneBundle)
                {
                    m_Btn_NoAds.gameObject.SetActive(false);
                    m_Btn_Store.gameObject.SetActive(false);
                }
                else if (GameData.NoAds)
                {
                    m_Btn_NoAds.gameObject.SetActive(false);
                }
                if(GameManager.abConfig.level_optimization==1 && GameManager.abConfig.bonus_level==1)
                    ShowProcessIcons(true);
                //禁用开始游戏时为了等AB初始化后在可以用
                m_Btn_Play.interactable = false;
                this.AttachTimer(0.2f, () => {
                    //延迟是为了刚开游戏时AB测试没有接收到数据
                    if (GameManager.abConfig.rv_optimized == 1)
                    {
                        if (GameManager.Instance.TryProp != EWeapon.NULL)
                        {
                            ShowUsePropPanel(GameManager.Instance.TryProp);
                        }
                        else if (!FindObjectOfType<GameEndPodium>() && LevelScenes.level >= 3)
                        {
                            ShowChosePanel();
                        }
                        else {
                            SetLocalPlayer.Instance.SetPlayer(null);
                        }
                    }
                    else
                    {
                        if (!FindObjectOfType<GameEndPodium>() && LevelScenes.level >= 3)
                        {
                            ShowChosePanel();
                        }
                        else {
                            SetLocalPlayer.Instance.SetPlayer(null);
                            GameManager.Instance.ResetGameTime();
                        }
                    }
                    m_Btn_Play.interactable = true;
                    if (LevelScenes.level>1 && !GameObject.FindObjectOfType<MuscleSnackControl>())
                        FllowObject.StartView(); });
            }
            else {
                if (GameManager.Instance.localPlayer.IsSnack)
                {
                    SetSnakeState(SnakeCount, true);
                    SnakeCount++;
                }
                else { 
                
                }
            }
            UpdateKeyUI();
            this.AttachTimer(0.1f, () =>
             {
                 if (GameManager.abConfig.rv_optimized == 0)
                 {
                     m_Trans_Keys.gameObject.SetActive(false);
                 }
                 else {
                     m_Trans_Keys.gameObject.SetActive(true);
                 }
             });
            gameObject.SetActive(true);
            GameManager.Instance.NowTime.OnChange = () =>
            {
                var time = GameManager.Instance.NowTime.Value;
                var timeInt = Mathf.CeilToInt(time);
                var fen = timeInt / 60;
                timeInt = timeInt - fen * 60;
                m_Txt_timeValue.text = $"{fen.ToString("00")}:{timeInt.ToString("00")}";
                m_Img_timeView.fillAmount = Mathf.Clamp01(time / GameManager.Instance.GameTimer.DurationTime );
            };
            GameManager.Instance.NowTime.Value += .1f ;
            GameManager.Instance.NowTime.Value -= .1f ;
            m_HGroup_PlayerGroup.gameObject.SetActive(true);  
        }

        //设置点击按钮是否显示（关掉可用于先播放开场动画后开启，点击屏幕开始游戏）
        public void SetPlayBtnState(bool _Flag) {
            m_Btn_Play.gameObject.SetActive(_Flag);
            m_Rect_DownGroup.gameObject.SetActive(_Flag);
        }

        private void Update()
        {
            if (LevelScenes.level <= 1 || GameManager.abConfig.level_optimization != 1 || GameManager.abConfig.bonus_level != 1)
            {
                if (GameEndPodium.Instance)
                {
                    m_Img_RunAway.gameObject.SetActive(false);
                    m_Img_EatThem.gameObject.SetActive(false);
                    m_Img_EatMoreSnacks.gameObject.SetActive(true);
                }
                else
                {
                    m_Img_EatMoreSnacks.gameObject.SetActive(false);
                    if (GameManager.Instance.localPlayer)
                    {
                        m_Img_RunAway.gameObject.SetActive(GameManager.Instance.localPlayer.IsSnack);
                        m_Img_EatThem.gameObject.SetActive(!GameManager.Instance.localPlayer.IsSnack);
                    }
                }
            }
            else { 
                if(m_Img_RunAway.gameObject.activeSelf)
                    m_Img_RunAway.gameObject.SetActive(false);
                if (m_Img_EatThem.gameObject.activeSelf)
                    m_Img_EatThem.gameObject.SetActive(false);
                if (m_Img_EatMoreSnacks.gameObject.activeSelf)
                    m_Img_EatMoreSnacks.gameObject.SetActive(false);
            }
            

            if (GameManager.Instance.NowTime.Value >= 10)
            {
                m_Txt_TimeRunOut.gameObject.SetActive(false);
                if ( m_Btn_AddTime.gameObject.activeSelf) {
                    m_Btn_AddTime.gameObject.SetActive(false);
                }
            }
            else
            {
                m_Txt_TimeRunOut.gameObject.SetActive(true);
                if (!GameManager.Instance.hasShowAddTimeBtn && !m_Btn_AddTime.gameObject.activeSelf && !GameManager.Instance.localPlayer.IsSnack && LevelScenes.level.Value > 1 && AdsManager.IsAdReady)
                {
                    GameManager.Instance.hasShowAddTimeBtn = true;
                    m_Btn_AddTime.gameObject.SetActive(true);
                }
            }

            m_Rect_Test.gameObject.SetActive(GameEndPodium.Instance);
            if (GameEndPodium.Instance)
                m_Txt_TestValue.text = GameManager.Instance.LiveSnakeCount().ToString();
            if (checkNet.IsFinish) {
                checkNet.ReStart();
                m_Btn_AddTime.interactable = AdsManager.IsAdReady;
                m_Btn_AdSnack.interactable = AdsManager.IsAdReady;
                m_Btn_StartWith.interactable = AdsManager.IsAdReady;
            }
            else
            {
                checkNet.OnUpdate(Time.deltaTime);
            }
        }

        public void OnExit()
		{
            Messenger.RemoveListener<BaseControl, BaseControl>(ConstDefine.Listener.EatSnake, OnEatSnake);
            gameObject.SetActive(false);
            m_Txt_timeValue.gameObject.SetActive(true);
            m_Img_image.gameObject.SetActive(true);
            CloseUsePropPanel(false);
            CloseChosePanel(false);
            CloseChoseSnackPanel();
        }

		public void OnPause()
		{
        
		}

		public void OnResume()
		{
        
		}

        private static List<Transform> m_HGroup_Childs;

        /// <summary>
        /// 设置无限时间
        /// </summary>
        private void SetInfiniteTime()
        {
            m_Txt_timeValue.gameObject.SetActive(false);
            m_Img_image.gameObject.SetActive(false);
            GameManager.Instance.MoveSpeed = 0;
            GameManager.Instance.GameTimer.timer = GameManager.Instance.GameTimer.DurationTime - 3;
        }

        private void HidePlayerGroup()
        {
            m_HGroup_PlayerGroup.gameObject.SetActive(false);
        }

        void Awake()
		{
			GetBindComponents(gameObject);
            m_Txt_CoinValue.text = GameData.Coin.ToString();
            m_HGroup_Childs = new List<Transform>();           
            MaxSnackCount = m_HGroup_PlayerGroup.transform.childCount;
            snackIcons = new Image[MaxSnackCount];
            moneySnack_img = m_Btn_MoneySnack.GetComponent<Image>();
            adSnack_img = m_Btn_AdSnack.GetComponent<Image>();
            keyImgs = m_Trans_Keys.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < MaxSnackCount; i++) {
                m_HGroup_Childs.Add(m_HGroup_PlayerGroup.transform.GetChild(i));
                snackIcons[i] = m_HGroup_Childs[i].GetComponent<Image>();
            }              
            m_Btn_Play.onClick.AddListener(
                    () => StartGame()
            );
            m_Btn_CoinImage.onClick.AddListener( ()=> {
                if (GameManager.IsDebug) {                   
                    if (SettingCanvas.Instance == null) {
                        GameObject setCanva;
                        setCanva = Instantiate(Resources.Load<GameObject>("UIPanel/SettingCanvas"));
                    }
                    SettingCanvas.Instance.gameObject.SetActive(true);
                }                   
            } );
            m_Btn_NoAds.onClick.AddListener(() =>
            {
                AdsManager.BuyPorduct(ShopProductNames.No_Ads);
            });
            m_Btn_Store.onClick.AddListener(() =>
            {
                UIPanelManager.Instance.PushPanel(UIPanelType.StorePanel);
            });
            m_Img_RunAway.transform.DOScale(1.1f, 1f).SetLoops(-1,LoopType.Yoyo);
            m_Img_EatThem.transform.DOScale(1.1f, 1f).SetLoops(-1,LoopType.Yoyo);
            m_Img_EatMoreSnacks.transform.DOScale(1.1f, 1f).SetLoops(-1,LoopType.Yoyo);
            m_Btn_AddTime.transform.DOScale(1.2f, .6f).SetLoops(-1, LoopType.Yoyo);
            
            Messenger.AddListener(ConstDefine.Listener.InfiniteTime, SetInfiniteTime);
            Messenger.AddListener(ConstDefine.Listener.HidePlayerGroup, HidePlayerGroup);
            Messenger.AddListener(ConstDefine.Listener.SetPlayerGroup, SetSnakeCount);
            Messenger.AddListener<Vector3>(ConstDefine.Listener.GetKey, GetKey);
            Messenger.AddListener(ConstDefine.Listener.BugNoAds, OnBuyNoAds);
            Messenger.AddListener(ConstDefine.Listener.BugAllInOne, OnBuyAllInOne);
            Messenger.AddListener<bool>(ConstDefine.Listener.SetGamePlayBtn, SetPlayBtnState);

            Messenger.AddListener<int>(ConstDefine.Listener.AddCoinValue, (value)=> { CoinValue.Value += value; });
            m_Txt_TimeRunOut.transform.DOScale(1.2f, .6f).SetLoops(-1, LoopType.Yoyo);
            m_Btn_AddTime.onClick.AddListener(()=>{
                Time.timeScale = 0;
                AdsManager.RewardAction = () =>
                {
                    AddGameTime();
                    Time.timeScale = 1;
                };
                AdsManager.NoRewardAction = () =>
                {
                    Time.timeScale = 1;
                };               
                m_Btn_AddTime.gameObject.SetActive(false);
                AdsManager.ShowRewardedAd("Addtime");
            });
            m_Btn_Skin.onClick.AddListener(() =>{                
                GameManager.sceneType = SceneType.SkinScene;
                LevelScenes.LoadScene(ConstDefine.specialScene);
                CloseChosePanel(false);
            });
            m_Btn_Setting.onClick.AddListener(() => {
                if(SettingPanel.instance==null)
                    Instantiate(Resources.Load<GameObject>("UIPanel/SettingPanel"));
                SettingPanel.instance.OnEnter();
            });
            //初始化选人界面
            InitChosePanel();
            //初始化试用道具界面
            InitUsePropPanel();

            sceneIncoImgList = new List<Image>();
            var processTra = m_HGroup_Process.transform;
            var processIconLength = processTra.childCount;
            for (int i = 0; i < processIconLength; i++) {
                sceneIncoImgList.Add(processTra.GetChild(i).GetComponent<Image>());
            }
        }

        void StartGame() {
            if (!GameManager.canRunGame)
                return;
            m_Btn_Play.gameObject.SetActive(false);
            m_Rect_DownGroup.gameObject.SetActive(false);
            m_Btn_Skin.gameObject.SetActive(false);
            m_Btn_Setting.gameObject.SetActive(false);
            m_Btn_NoAds.gameObject.SetActive(false);
            m_Btn_Store.gameObject.SetActive(false);
            JoystickPanel.Instance.gameObject.SetActive(true);
            FllowObject.Get(0).speed = 20f;
            GameManager.Instance.GameStart();
            ShowProcessIcons(false);
        }

        /// <summary>
        /// 设置零食数量
        /// </summary>
        /// <param name="count"></param>
        public void SetSnakeCount()
        {
            //改前的MaxSnackCount为固定数字4
            int size = GameManager.Instance.SnackList.Count - 1;
            if (size <= 0)
                return;
            for (int i = MaxSnackCount-1; i > size-1; i--)
                m_HGroup_Childs[i].gameObject.SetActive(false);
            SnakeCount = size;//Mathf.Clamp(0, 4, size + 1);
            if (GameManager.abConfig.level_difficulty == 1) {
                for (int i = size-1; i >= 0; i--) {
                    m_HGroup_Childs[i].SetAsLastSibling();
                    var icon = m_HGroup_Childs[i].GetChild(0);
                    if (i == 0 && size>=3)
                    {
                        snackIcons[i].sprite = snack_reward;
                        snackIcons[i].rectTransform.sizeDelta = new Vector2(90, 90);
                        m_HGroup_Childs[i].GetChild(0).gameObject.SetActive(true);
                        icon.gameObject.SetActive(true);
                    }
                    else {
                        snackIcons[i].sprite = snack_Show;
                        snackIcons[i].rectTransform.sizeDelta = new Vector2(73, 73);                       
                        if (icon.gameObject.activeSelf)
                            icon.gameObject.SetActive(false);
                    }                  
                }              
                return;
            }
        }
        private static int MaxSnackCount;
        private static int SnakeCount = 4;
        private void SetSnakeState(int index,bool isShow)
        {
            if (index < 0)
                return;
            var img = snackIcons[index];
            img.sprite = isShow ? snack_Show : snack_Dead;
        }

        private void ResetState()
        {
            for (int i = 0; i < MaxSnackCount; i++)//4
            {
                m_HGroup_Childs[i].gameObject.SetActive(true);
                SetSnakeState(i, true);
            }
        }


        private void OnEatSnake(BaseControl killer, BaseControl control)
        {
            SnakeCount--;
            SetSnakeState(SnakeCount, false);
            if (GameManager.abConfig.level_difficulty == 1 && SnakeCount==0)
            {
                Messenger.Broadcast<int>(ConstDefine.Listener.AddCoinValue, 50);
            }
        }

        void AddGameTime() {
            GameManager.Instance.GameTimer.DurationTime += 30;
            Time.timeScale = 1;
        }

        void UpdateKeyUI()
        {           
            int hasKey = GameData.PrizeKey;
            for (int i = 0; i < 3; i++) {
                if (i < hasKey)
                    keyImgs[i].sprite = hasKeySp;
                else
                    keyImgs[i].sprite = noKeySp;
            }
        }
        void GetKey(Vector3 Pos) {
            GetKeyAnim(Pos);
        }

        void GetKeyAnim(Vector3 worldPos) {
            Vector2 screenPosition = FllowObject.cam.WorldToScreenPoint(worldPos);
            Vector2 uiPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)trans, screenPosition, null,out uiPos)) {
                RectTransform key_tra = Instantiate(Resources.Load<RectTransform>("UI/Image"),trans);
                key_tra.anchoredPosition = uiPos;
                key_tra.DOMove(keyImgs[Mathf.Clamp(GameData.PrizeKey, 0, 3) - 1].transform.position,1).SetEase( Ease.InExpo).
                    onComplete=()=> {
                        var keyId = Mathf.Clamp(GameData.PrizeKey - 1, 0, 2);
                        keyImgs[keyId].sprite = hasKeySp;
                        keyImgs[keyId].transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.5f).SetLoops(2, LoopType.Yoyo);
                        Destroy(key_tra.gameObject);
                };
            }
        }

        void OnBuyNoAds() {
            m_Btn_NoAds.gameObject.SetActive(false);
            AdsManager.HideBanner();
            AdsManager.HideCrossPromo();
        }
        void OnBuyAllInOne() {
            m_Btn_NoAds.gameObject.SetActive(false);
            m_Btn_Store.gameObject.SetActive(false);
            AdsManager.HideBanner();
        }

#region 选人界面
        void InitChosePanel() {
            m_Btn_child.onClick.AddListener(() => { ChosePlayer(ChildOrSnack.Child); });
            m_Btn_snack.onClick.AddListener(() => { ChosePlayer(ChildOrSnack.Snack); });
            m_Btn_FreeSnack.onClick.AddListener(() => { ClickOneSnackBtn(0); });
            m_Btn_MoneySnack.onClick.AddListener(() => { ClickOneSnackBtn(1); });
            m_Btn_AdSnack.onClick.AddListener(() => { ClickOneSnackBtn(2); });
            m_Btn_child.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), .8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
            m_Btn_snack.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), .8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
            m_Img_ChoseSide.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), .8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
            var res = 1.0f * Screen.width / Screen.height;
            if (res < 0.5f)
            {
                RectTransform choseSnackPanel = m_Group_ChoseSnack.GetComponent<RectTransform>();
                Vector2 firstVec2 = new Vector2(-300, 270);
                foreach (RectTransform child in choseSnackPanel) {
                    child.anchoredPosition = firstVec2;
                    firstVec2.x += 300;
                }
            }
        }

        //显示选阵营界面
        void ShowChosePanel() {
            if (SetLocalPlayer.Instance.isUseThisModel)
            {
                return;
            }
            UpdateChoseImg();
            m_Group_ChosePlayer.alpha = 1;
            m_Group_ChosePlayer.blocksRaycasts = true;
            m_Rect_DownGroup.gameObject.SetActive(false);
        }
        //关闭选阵营界面
        void CloseChosePanel(bool canShowDownGroup = true) {
            m_Group_ChosePlayer.alpha = 0;
            m_Group_ChosePlayer.blocksRaycasts = false;
            m_Rect_DownGroup.gameObject.SetActive(canShowDownGroup);
        }
        //更新阵营角色UI
        void UpdateChoseImg() {
            m_Img_child.sprite = SkinManager.Instance.allChildSkinInfo[GameData.GetChoseSkinId(ChildOrSnack.Child)].Icon;
            m_Img_snack.sprite = SkinManager.Instance.allSnackSkinInfo[GameData.GetChoseSkinId(ChildOrSnack.Snack)].Icon;
        }
        //选阵营（选到小孩，直接开始游戏，选零食阵营再选）
        void ChosePlayer(ChildOrSnack _type)
        {
            if (!GameManager.canRunGame)
                return;
            if (_type == ChildOrSnack.Child)
            {
                ChosePlayer(SkinManager.Instance.GetChoseSkinInfo(_type));
            }
            else
            {
                if (GameManager.abConfig.rv_optimized == 0)
                {
                    ChosePlayer(SkinManager.Instance.GetChoseSkinInfo(_type));
                }
                else {
                    ShowChoseSnackPanel();
                }
                
            }
        }
        //显示选具体零食界面，（选择零食阵营后还要再选具体零食）
        void ShowChoseSnackPanel() {
            CloseChosePanel(false);
#region 更新零食UI          
            var nohasSnackList = GetNewList(SkinManager.Instance.GetHasNoSkinList((int)ChildOrSnack.Snack));
            var curSnackSkinId = GameData.GetChoseSkinId(ChildOrSnack.Snack);
            var nextSnackId = 0;
            m_Img_FreeSnack.sprite = SkinManager.Instance.allSnackSkinInfo[GameData.GetChoseSkinId(ChildOrSnack.Snack)].Icon;
            if (nohasSnackList.Count >= 2)
            {
                //中间的零食
                nextSnackId = nohasSnackList[Random.Range(0, nohasSnackList.Count)];
                moneySnackInfo = SkinManager.Instance.allSnackSkinInfo[nextSnackId];
                m_Img_MoneySnack.sprite = moneySnackInfo.Icon;
                Debug.Log("money Snack Id:" + moneySnackInfo.SkinName);
                nohasSnackList.Remove(nextSnackId);
                //右边的零食
                nextSnackId = nohasSnackList[Random.Range(0, nohasSnackList.Count)];
                adSnackInfo = SkinManager.Instance.allSnackSkinInfo[nextSnackId];
                m_Img_AdSnack.sprite = adSnackInfo.Icon;
                Debug.Log("ad Snack Id:" + adSnackInfo.SkinName);
                moneySnack_img.sprite = moneySnack_sp;
                adSnack_img.sprite = adSnack_sp;
            }
            else if (nohasSnackList.Count > 0)
            {
                //中间的零食               
                nextSnackId = Random.Range(0, SkinManager.Instance.allSnackSkinInfo.Count);
                while (nextSnackId == curSnackSkinId || nohasSnackList.Contains(nextSnackId))
                {
                    nextSnackId = Random.Range(0, SkinManager.Instance.allSnackSkinInfo.Count);
                }
                moneySnackInfo = SkinManager.Instance.allSnackSkinInfo[nextSnackId];
                m_Img_MoneySnack.sprite = moneySnackInfo.Icon;
                Debug.Log("money Snack Id:" + moneySnackInfo.SkinName);
                nohasSnackList.Remove(nextSnackId);
                //右边的零食
                nextSnackId = nohasSnackList[0];
                adSnackInfo = SkinManager.Instance.allSnackSkinInfo[nextSnackId];
                m_Img_AdSnack.sprite = adSnackInfo.Icon;
                Debug.Log("ad Snack Id:" + adSnackInfo.SkinName);
                moneySnack_img.sprite = freeSnack_sp;
                adSnack_img.sprite = adSnack_sp;
            }
            else {
                var allSnackSkin = new List<int>();
                int maxskin = SkinManager.Instance.allSnackSkinInfo.Count;
                for (int i = 0; i < maxskin; i++) {
                    if (i != curSnackSkinId) {
                        allSnackSkin.Add(i);
                    }
                }
                //中间的零食
                nextSnackId = allSnackSkin[Random.Range(0, allSnackSkin.Count)];
                moneySnackInfo = SkinManager.Instance.allSnackSkinInfo[nextSnackId];
                m_Img_MoneySnack.sprite = moneySnackInfo.Icon;
                Debug.Log("money Snack Id:" + moneySnackInfo.SkinName);
                allSnackSkin.Remove(nextSnackId);
                //右边的零食
                nextSnackId = allSnackSkin[Random.Range(0, allSnackSkin.Count)];
                adSnackInfo = SkinManager.Instance.allSnackSkinInfo[nextSnackId];
                m_Img_AdSnack.sprite = adSnackInfo.Icon;
                Debug.Log("ad Snack Id:" + adSnackInfo.SkinName);
                moneySnack_img.sprite = freeSnack_sp;
                adSnack_img.sprite = freeSnack_sp;
            }
            m_Group_ChoseSnack.gameObject.SetActive(true);
            nohasSnackList = null;
#endregion
        }

        void CloseChoseSnackPanel()
        {
            m_Group_ChoseSnack.gameObject.SetActive(false);
        }
        /// <summary>
        /// 点击具体零食
        /// </summary>
        /// <param name="snackBtn">0:免费按钮，1：金币按钮，2：广告按钮</param>
        void ClickOneSnackBtn(int snackBtn)
        {
            if (snackBtn == 0)
            {
                ChosePlayer(SkinManager.Instance.GetChoseSkinInfo(ChildOrSnack.Snack));
            }
            else if (snackBtn == 1)
            {
                //该皮肤没有，需要金币试用
                if (SkinManager.Instance.hasNoSnackSkin.Contains(moneySnackInfo.ID))
                {
                    if (GameData.Coin >= 100)
                    {
                        Messenger.Broadcast<int>(ConstDefine.Listener.AddCoinValue, -100);
                    }
                    else {
                        return;
                    }
                }
                ChosePlayer(moneySnackInfo); ;                
            }
            else {
                if (SkinManager.Instance.hasNoSnackSkin.Contains(adSnackInfo.ID))
                {
                    AdsManager.RewardAction = () =>
                    {
                        ChosePlayer(adSnackInfo);
                        CloseChoseSnackPanel();
                    };
                    //AdsManager.SetRewardAction(
                    //    () =>{
                    //        ChosePlayer(adSnackInfo);
                    //        CloseChoseSnackPanel();
                    //    }, m_Btn_AdSnack);
                    AdsManager.ShowRewardedAd("TryOutSnack");
                    return;
                }
                else {
                    ChosePlayer(adSnackInfo);
                }
            }            
            CloseChoseSnackPanel();                      
        }
        
        
        //选择具体人物
        void ChosePlayer(SkinInfo _info) {
            if (_info == null) {
                Debug.LogWarning("选择具体人物出错, _info = null");
                return;
            }
            if (_info.PlayerType == ChildOrSnack.NULL)
            {
                Debug.LogWarning("选择具体人物出错, _info.PlayerType = ChildOrSnack.NULL");
                return;
            }
            SetLocalPlayer.Instance.SetPlayer(_info);
            GameManager.Instance.ResetGameTime();
            CloseChosePanel(true);
        }
#endregion

#region 试用道具界面
        private GameObject usePropModel;

        void InitUsePropPanel() {
            m_Btn_StartWith.transform.DOScale(new Vector3(.8f, .8f, .8f), 1).SetLoops(-1, LoopType.Yoyo);
            m_Btn_StartWith.onClick.AddListener(() => { OnClickStartWith(GameManager.Instance.TryProp); });
            m_Btn_LoseProp.onClick.AddListener(() => { OnClickLostIt(); });
        }
        void ShowUsePropPanel(EWeapon weapon) {
            if (SetLocalPlayer.Instance.isUseThisModel) {
                return;
            }
            SkinInfo skin;
            if (weapon < 0)
            {
                CloseUsePropPanel(true);
                Debug.LogWarning("道具出错，直接关闭试用道具界面");
                return;
            }
            else if (weapon < EWeapon.Pepper)
            {
                skin = SkinManager.Instance.GetChoseSkinInfo(ChildOrSnack.Child);
            }
            else {
                skin = SkinManager.Instance.allSnackSkinInfo[0];
            }
            usePropModel = SkinManager.Instance.ShowSkinModel(skin, 0);
            ModelShow ms = usePropModel.GetComponent<ModelShow>();
            ms.ShowPropAnim(weapon);
            m_RImg_PropDemo.DOColor(Color.white,0.1f);
            m_Img_PropIcon.sprite = propSps[(int)weapon];
            m_Trans_UsePropPanel.gameObject.SetActive(true);
            m_Rect_DownGroup.gameObject.SetActive(false);
            this.AttachTimer(ConstDefine.DelayShowTime, () => m_Btn_LoseProp.gameObject.SetActive(true));
            
        }
        void CloseUsePropPanel(bool canShowDownGroup) {
            m_RImg_PropDemo.color = new Color(1, 1, 1, 0);
            m_Btn_LoseProp.gameObject.SetActive(false);
            m_Trans_UsePropPanel.gameObject.SetActive(false);
            m_Rect_DownGroup.gameObject.SetActive(canShowDownGroup);
            if(usePropModel)
                Destroy(usePropModel);
        }

        //点击试用道具
        void OnClickStartWith(EWeapon weapon)
        {
#region
            //AdsManager.RewardAction = () =>
            //{
            //    SkinInfo _info;
            //    if (weapon < EWeapon.Pepper)
            //    {
            //        _info = SkinManager.Instance.GetChoseSkinInfo(ChildOrSnack.Child);
            //    }
            //    else
            //    {
            //        _info = SkinManager.Instance.GetChoseSkinInfo(ChildOrSnack.Snack);
            //    }
            //    SetLocalPlayer.Instance.SetPlayer(_info);
            //    GameManager.Instance.ResetGameTime();
            //    if (weapon < EWeapon.Pepper)
            //    {
            //        ChildControl control = (ChildControl)GameManager.Instance.localPlayer;
            //        control.WeapomType.Value = weapon;
            //        if (control.IsHammer.Value)
            //            control.IsHammer.Value = false;
            //        control.IsHammer.Value = true;
            //    }
            //    else
            //    {
            //        SnackControl control = (SnackControl)GameManager.Instance.localPlayer;
            //        if (weapon == EWeapon.Pepper)
            //        {
            //            control.PlayerHealth = 2;
            //        }
            //        else
            //        {
            //            control.PlayerHealth++;
            //            control.SetDirtyMat();
            //        }
            //    }
            //    CloseUsePropPanel(true);
            //};
#endregion
            AdsManager.RewardAction =() =>
            {
                SkinInfo _info;
                if (weapon < EWeapon.Pepper)
                {
                    _info = SkinManager.Instance.GetChoseSkinInfo(ChildOrSnack.Child);
                }
                else
                {
                    _info = SkinManager.Instance.GetChoseSkinInfo(ChildOrSnack.Snack);
                }
                SetLocalPlayer.Instance.SetPlayer(_info);
                GameManager.Instance.ResetGameTime();
                if (weapon < EWeapon.Pepper)
                {
                    ChildControl control = (ChildControl)GameManager.Instance.localPlayer;
                    control.WeapomType.Value = weapon;
                    if (control.IsHammer.Value)
                        control.IsHammer.Value = false;
                    control.IsHammer.Value = true;
                }
                else
                {
                    SnackControl control = (SnackControl)GameManager.Instance.localPlayer;
                    if (weapon == EWeapon.Pepper)
                    {
                        control.PlayerHealth = 2;
                    }
                    else
                    {
                        control.PlayerHealth++;
                        control.SetDirtyMat();
                    }
                }
                CloseUsePropPanel(true);
            };
            AdsManager.ShowRewardedAd("Use_Prop");           
        }
        //不要试玩道具,进入选阵营
        void OnClickLostIt() {
            if (GameEndPodium.Instance)
            {
                SetLocalPlayer.Instance.SetPlayer(SkinManager.Instance.GetChoseSkinInfo(ChildOrSnack.Child));
                GameManager.Instance.ResetGameTime();
                CloseUsePropPanel(true);
            }
            else {
                CloseUsePropPanel(false);
                ShowChosePanel();
            }          
        }
#endregion

        //获取链表（要修改到链表，为了不影响引用的获取链表，所以新创一个）
        List<int> GetNewList(List<int> list)
        {
            List<int> newList = new List<int>();
            foreach (int child in list)
            {
                newList.Add(child);
            }
            return newList;
        }

        #region 场景进度图标

        void ShowProcessIcons(bool _isShow)
        {
            if (_isShow)
            {
                UpdateProcessIcon();
                m_HGroup_Process.GetComponent<RectTransform>().sizeDelta = new Vector2(GetProcessImgWidth(), 120);
            }
            else {
                m_Trans_choseArrow.DOKill();
            }
                
            m_Trans_Process.gameObject.SetActive(_isShow);
        }

        int curMapIndex = 0;
        int curMapSceneCount = 0;
        public void UpdateProcessIcon()
        {
            //加载的关卡下标
            var loadLvId = LevelScenes.GetLevelIndex(LevelScenes.level);
            EnvType curMap = 0;
            EnvType NextMap = 0;
            curMapIndex = 0;
            curMapSceneCount = 0;
            for (int i = ConstDefine.NextMapIndex.Length - 1; i >= 0; i--)
            {
                if (ConstDefine.NextMapIndex[i] <= loadLvId)
                {
                    curMapIndex = i;
                    curMap = (EnvType)System.Enum.Parse(typeof(EnvType), LevelScenes.LvInfoLDict[ConstDefine.NextMapIndex[i]].Type);
                    var nexti = i + 1 < ConstDefine.NextMapIndex.Length ? i + 1 : 0;
                    NextMap = (EnvType)System.Enum.Parse(typeof(EnvType), LevelScenes.LvInfoLDict[ConstDefine.NextMapIndex[nexti]].Type);
                    if (i < nexti)
                    {
                        curMapSceneCount = ConstDefine.NextMapIndex[nexti] - ConstDefine.NextMapIndex[i];
                    }
                    else
                    {
                        curMapSceneCount = LevelScenes.MaxLevel - ConstDefine.NextMapIndex[i];
                    }
                    //Debug.Log($"当前地图类型:{curMap}");
                    //Debug.Log($"下一个地图类型:{NextMap}");
                    //Debug.Log($"当前curMapIndex:{curMapIndex}");
                    //Debug.Log($"ConstDefine.NextMapIndex[curMapIndex]:{ConstDefine.NextMapIndex[curMapIndex]}");
                    //Debug.Log($"当前共有{curMapSceneCount}关");
                    //Debug.Log($"sceneIncoImgList.Count:{sceneIncoImgList.Count}");
                    break;
                }
            }
            var curObjLength = m_HGroup_Process.transform.childCount;
            if (curMapSceneCount + 2 <= curObjLength)
            {
                sceneIncoImgList[0].sprite = mapTypeSps[(int)curMap];
                sceneIncoImgList[curMapSceneCount + 1].sprite = mapTypeSps[(int)NextMap];
                sceneIncoImgList[curMapSceneCount + 1].gameObject.SetActive(true);
                sceneIncoImgList[curMapSceneCount + 1].SetNativeSize();
                var length = ConstDefine.NextMapIndex[curMapIndex] + curMapSceneCount;
                for (int i = ConstDefine.NextMapIndex[curMapIndex]; i < length; i++)
                {
                    var id = 1 + i - ConstDefine.NextMapIndex[curMapIndex];
                    if (i <= loadLvId)
                    {
                        if (LevelScenes.LvInfoLDict[i].SpScene == "Challenge")
                        {
                            sceneIncoImgList[id].sprite = sceneIconHasPassSps[6];
                        }
                        else if (LevelScenes.LvInfoLDict[i].SpScene == "Coin")
                        {
                            sceneIncoImgList[id].sprite = sceneIconHasPassSps[7];
                        }
                        else {
                            sceneIncoImgList[id].sprite = sceneIconHasPassSps[(int)System.Enum.Parse(typeof(EWeapon), LevelScenes.LvInfoLDict[i].Prop) + 1];
                        }
                        if (i == loadLvId) {
                            m_Trans_choseArrow.SetParent(sceneIncoImgList[id].transform);
                            m_Trans_choseArrow.localPosition = new Vector3(0, 80, 0);
                            m_Trans_choseArrow.DOLocalMoveY(150, 0.6f).SetLoops(-1, LoopType.Yoyo);
                        }
                    }
                    else {
                        if (LevelScenes.LvInfoLDict[i].SpScene == "Challenge")
                        {
                            sceneIncoImgList[id].sprite = sceneIconNoPassSps[6];
                        }
                        else if (LevelScenes.LvInfoLDict[i].SpScene == "Coin")
                        {
                            sceneIncoImgList[id].sprite = sceneIconNoPassSps[7];
                        }
                        else
                            sceneIncoImgList[id].sprite = sceneIconNoPassSps[(int)System.Enum.Parse(typeof(EWeapon), LevelScenes.LvInfoLDict[i].Prop)+1];
                    }                    
                    sceneIncoImgList[id].gameObject.SetActive(true);
                    sceneIncoImgList[id].SetNativeSize();
                }
                for(int i= curMapSceneCount+2; i< curObjLength; i++)
                {
                    sceneIncoImgList[i].gameObject.SetActive(false);
                }
            }
            //m_Trans_choseArrow.position = sceneIncoImgList[1 + LevelScenes.GetLevelIndex(LevelScenes.level) - ConstDefine.NextMapIndex[curMapIndex]].transform.position;
            //m_Trans_choseArrow.localPosition += new Vector3(0, 100, 0);
        }

        void AddSceneIconImg(int addNum)
        {
            Debug.Log($"补多{addNum}图片");
            var prefab = m_HGroup_Process.transform.GetChild(0);
            for (int i = 0; i < addNum; i++)
            {
                Transform newImg = Instantiate<Transform>(prefab, m_HGroup_Process.transform);
                sceneIncoImgList.Add(newImg.GetComponent<Image>());
            }
        }

        float GetProcessImgWidth() {
            var length = ConstDefine.NextMapIndex[curMapIndex] + curMapSceneCount;
            float width = 0;
            //当前关卡组图标长度
            for (int i = ConstDefine.NextMapIndex[curMapIndex]; i < length; i++) {
                if (LevelScenes.LvInfoLDict[i].SpScene == "Normal")
                {
                    if (LevelScenes.LvInfoLDict[i].Prop == "NULL")
                    {
                        width += 50;
                    }
                    else
                    {
                        width += 108;
                    }
                }
                else {
                    width += 108;
                }
                        
            }
            //加上头尾2个缩略图的长度
            width += 256;
            return width;
        }
        #endregion
    }
}