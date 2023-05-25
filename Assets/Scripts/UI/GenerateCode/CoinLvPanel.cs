using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyUI
{
    public partial class CoinLvPanel : MonoBehaviour, BasePanel
    {
        enum CoinLvType
        {
            Colosseum,
            MuscleSnack,
        }
        
        public Transform trans => transform;
        private int score;
        private CoinLvType coinLvType;
        private MyTimer checkNet = new MyTimer(1);
        private BindableProperty<int> m_CoinValue;
        public BindableProperty<int> CoinValue
        {
            get
            {
                if (m_CoinValue == null)
                {
                    m_CoinValue = new BindableProperty<int>(PlayerPrefs.GetInt("Coin", 0));
                    m_CoinValue.OnChange = () =>
                    {
                        if (m_Txt_CoinValue)
                            m_Txt_CoinValue.text = m_CoinValue.ToString();
                    };
                    m_Txt_CoinValue.text = m_CoinValue.ToString();
                }
                return m_CoinValue;
            }
        }

        void UpdateCoinText(int _coin)
        {
            CoinValue.Value += _coin;
        }

        public void OnEnter()
        {
            score = 0;
            UpdateCoinText(0);
            Messenger.AddListener<int>(ConstDefine.Listener.AddCoinValue, UpdateCoinText);
            gameObject.SetActive(true);
        }

        public void OnExit()
        {
            score = 0;
            Messenger.RemoveListener<int>(ConstDefine.Listener.AddCoinValue, UpdateCoinText);
            ShowCoinLvStartPanel(false);
            ShowCoinLvPlayPanel(false);
            ShowCoinLvEndPanel(false);
            gameObject.SetActive(false);
        }

        public void OnPause()
        {
            
        }

        public void OnResume()
        {
            
        }

        // Start is called before the first frame update
        void Awake()
        {
            GetBindComponents(gameObject);
            Messenger.AddListener<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ShowPanel);
            Messenger.AddListener(ConstDefine.Listener.GetScoreReward, ShowGetMoneyUI);
            m_Btn_Join.onClick.AddListener(() =>
            {
                AdsManager.RewardAction = () =>
                {
                    GoinCoinLvScene();
                };
                var adsName = "";
                if (coinLvType == CoinLvType.Colosseum)
                    adsName = "Join_ColosseumLv";
                else
                    adsName = "Join_MuscleSnackLv";

                AdsManager.ShowRewardedAd(adsName);
            });
            m_Btn_NoJoin.onClick.AddListener(GotoNextScene);
            m_Btn_StartGame.onClick.AddListener(StartColosseum);
            m_Btn_GetIt.onClick.AddListener(()=>GotoNextScene());
            m_Btn_Restart.onClick.AddListener(() => {
                AdsManager.RewardAction = () =>
                {
                    LevelScenes.LevelRestartEvent();
                    LevelScenes.LoadScene();
                };
                AdsManager.ShowRewardedAd("Restart_CoinLv");               
            });
        }

        private void Update()
        {
            if (checkNet.IsFinish)
            {
                checkNet.ReStart();
                m_Btn_Join.interactable = AdsManager.IsAdReady;
                m_Btn_Restart.interactable = AdsManager.IsAdReady;
            }
            else {
                checkNet.OnUpdate(Time.deltaTime);
            }
        }

        void ShowPanel(ESpecialPanel _lvName, bool _isShow)
        {
            switch (_lvName)
            {
                case ESpecialPanel.CoinLv_Start:
                    ShowCoinLvStartPanel(_isShow);
                    break;              
                case ESpecialPanel.CoinLv_Play:
                    ShowCoinLvPlayPanel(_isShow);
                    break;
                case ESpecialPanel.CoinLv_End:
                    ShowCoinLvEndPanel(_isShow);
                    break;
                case ESpecialPanel.MuscleSnack_Start:
                    ShowMuscaleStartPanel(_isShow);
                    break;  
            }

        }
        //进入斗兽场界面
        void ShowCoinLvStartPanel(bool _isShow) {
            if (_isShow)
            {
                coinLvType = CoinLvType.Colosseum;
                ShowCoinLvAnim();
                m_Btn_NoJoin.gameObject.SetActive(false);
                this.AttachTimer(ConstDefine.DelayShowTime, () => m_Btn_NoJoin.gameObject.SetActive(true));
            }
            else {
                CloseCoinLvAnim();
            }
            m_Trans_JoinCoinLevel.gameObject.SetActive(_isShow);
        }
        //斗兽场游戏界面
        void ShowCoinLvPlayPanel(bool _isShow)
        {                     
            if (_isShow)
            {
                score = 0;
                m_Btn_StartGame.gameObject.SetActive(true);
            }
            m_Trans_CoinLevel.gameObject.SetActive(_isShow);
        }
        //斗兽场结算界面
        void ShowCoinLvEndPanel(bool _isShow)
        {         
            if (_isShow)
            {
                m_Txt_Reward.text = $"+{score}";
                m_Btn_StartGame.gameObject.SetActive(false);
                m_Trans_CoinLevel.gameObject.SetActive(false);
            }
            m_Trans_Win.gameObject.SetActive(_isShow);
        }
        //开始斗兽场
        void StartColosseum()
        {
            m_Btn_StartGame.gameObject.SetActive(false);
            ColosseumMgr.Instance.StartGame();
        }
        //斗兽场每次冲刺完获取奖励
        public void ShowGetMoneyUI()
        {
            int reward = 100;
            score += 100;
            Vector2 screenPosition = FllowObject.cam.WorldToScreenPoint(GameManager.Instance.localPlayer.transform.position + new Vector3(0, 0, -1));
            Vector2 uiPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)trans, screenPosition, null, out uiPos))
            {
                RectTransform coinText = Instantiate(m_Txt_GetCoinPrefab.rectTransform, trans);
                coinText.anchoredPosition = uiPos;
                coinText.GetComponent<Text>().text = $"+{reward}";
                coinText.DOMove(coinText.transform.position + Vector3.up * 150, 0.8f).
                    onComplete = () =>
                    {
                        Destroy(coinText.gameObject);
                    };
            }
        }
        void GoinCoinLvScene()
        {
            if (LevelScenes.IsLoopLevel())
            {
                LevelScenes.randomLevel = LevelScenes.level;
            }
            LevelScenes.LoadScene();
        }

        void GotoNextScene() {
            if (score > 0)
            {
                UITools.Instance.FlyCoin(score, 8, trans, m_Img_Coin.transform.position, m_Btn_CoinImage.transform.position, ()=>
                {
                    this.AttachTimer(1, () =>
                    {
                        LevelScenes.AddLevel();
                        //只有在未排序关卡数组里循环随机才有可能又随到特殊关卡
                        if (GameManager.abConfig.level_optimization == 0) {
                            while (MyMath.IsInArray(LevelScenes.randomLevel, ConstDefine.original_coinLv) || MyMath.IsInArray(LevelScenes.randomLevel, ConstDefine.original_challengeLv))
                            {
                                LevelScenes.UpdateRandomLv();
                            }
                        }
                        LevelScenes.LoadScene();
                    });
                });        
            }
            else {
                LevelScenes.AddLevel();
                //只有在未排序关卡数组里循环随机才有可能又随到特殊关卡
                if (GameManager.abConfig.level_optimization == 0)
                {
                    while (MyMath.IsInArray(LevelScenes.randomLevel, ConstDefine.original_coinLv) || MyMath.IsInArray(LevelScenes.randomLevel, ConstDefine.original_challengeLv))
                    {
                        LevelScenes.UpdateRandomLv();
                    }
                }
                LevelScenes.LoadScene();
            }           
        }

        private GameObject usePropModel;
        void ShowCoinLvAnim()
        {
            usePropModel = SkinManager.Instance.ShowSkinModel(SkinManager.Instance.allSnackSkinInfo[0], 0);
            ModelShow ms = usePropModel.GetComponent<ModelShow>();
            ms.ShowCoinLvAnim();
            m_RImg_Demo.DOColor(Color.white, 0.1f);
        }

        void CloseCoinLvAnim() {
            m_RImg_Demo.color = new Color(1, 1, 1, 0);
            if (usePropModel) {
                Destroy(usePropModel);
            }
        }
        
        /*************肌肉零食关卡***************/
        //显示是否进入肌肉零食关卡界面
        void ShowMuscaleStartPanel(bool _isShow) {
            if (_isShow)
            {
                coinLvType = CoinLvType.MuscleSnack;
                usePropModel = Instantiate(Resources.Load<GameObject>("ModelShow/MuscleSnack00"),new Vector3(20,20,20),Quaternion.identity);
                ModelShow ms = usePropModel.GetComponent<ModelShow>();
                ms.ShowMuscleSnack();
                
                m_RImg_Demo.DOColor(Color.white, 0.1f);
                m_Btn_NoJoin.gameObject.SetActive(false);
                this.AttachTimer(ConstDefine.DelayShowTime, () => m_Btn_NoJoin.gameObject.SetActive(true));
            }
            else {
                CloseCoinLvAnim();
            }
            m_Trans_JoinCoinLevel.gameObject.SetActive(_isShow);
        }
    }
}
