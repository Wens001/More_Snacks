using DG.Tweening;
using GameAnalyticsSDK.Setup;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ESpecialPanel { 
    Challenge_Start,
    Challenge_Win,
    Challenge_Fail,
    CoinLv_Start,
    CoinLv_Play,
    CoinLv_End,
    MuscleSnack_Start
}

namespace MyUI {
    public partial class ChallengePanel : MonoBehaviour, BasePanel
    {
        public Transform trans => transform;
        public Sprite[] envSps;
        private GraphicRaycaster raycaster;
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
                }
                return m_CoinValue;
            }
        }

        public void OnEnter()
        {
            raycaster.enabled = true;
            gameObject.SetActive(true);
        }

        public void OnExit()
        {
            ShowJoinChallengePanel(false);
            ShowWinPanel(false);
            ShowFailPanel(false);
            gameObject.SetActive(false);
        }

        public void OnPause()
        {
            
        }

        public void OnResume()
        {
            
        }

        void Awake() {
            GetBindComponents(gameObject);
            TryGetComponent(out raycaster);
            m_Txt_CoinValue.text = GameData.Coin.ToString();
            //开始前
            m_Txt_challengeReward.text = (ConstDefine.ChallengeCost * 2).ToString();
            m_Btn_JoinChallenge.GetComponentInChildren<Text>().text = ConstDefine.ChallengeCost.ToString();
            m_Btn_JoinChallenge.onClick.AddListener(() =>
            {
                if (GameData.Coin >= ConstDefine.ChallengeCost)
                {
                    Messenger.Broadcast<int>(ConstDefine.Listener.AddCoinValue, -ConstDefine.ChallengeCost);
                }
                else {
                    ShowNotEnough();
                }
                ShowJoinChallengePanel(false);
                LevelScenes.LoadScene();
            });
            m_Btn_NoChallenge.onClick.AddListener(() =>
            {
                ShowJoinChallengePanel(false);
                LoadNextScene();
            });
            //胜利
            m_Btn_GetIt.onClick.AddListener(() =>
            {
                Messenger.Broadcast<int>(ConstDefine.Listener.AddCoinValue, ConstDefine.ChallengeCost * 2);
                LoadNextScene();
            });
            m_Btn_GetDouble.onClick.AddListener(() =>
            {
                AdsManager.RewardAction = () =>
                {
                    raycaster.enabled = false;
                    //Messenger.Broadcast<int>(ConstDefine.Listener.AddCoinValue, ConstDefine.ChallengeCost * 4);                   
                    UITools.Instance.FlyCoin(ConstDefine.ChallengeCost * 4, 8, trans, m_Btn_GetDouble.transform.position, m_Btn_CoinImage.transform.position, () =>
                    {
                        this.AttachTimer(1, () =>
                        {
                            LoadNextScene();
                        });
                    });
                };
                AdsManager.ShowRewardedAd("More_ChallengeReward");
            });

            //失败
            m_Btn_NoThank.onClick.AddListener(() =>
            {
                LoadNextScene();
            });
            m_Btn_Restart.onClick.AddListener(() =>
            {
                AdsManager.RewardAction = () =>
                {
                    LevelScenes.LoadScene();
                };
                AdsManager.ShowRewardedAd("Restart Challenge");
            });
            Messenger.AddListener<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ShowPanel);
            Messenger.AddListener<int>(ConstDefine.Listener.AddCoinValue, UpdateCoinText);
        }

        void UpdateCoinText(int value=0) {
            m_Txt_CoinValue.text = GameData.Coin.ToString();
        }

        void LoadNextScene() {
            LevelScenes.AddLevel();
            if (GameManager.abConfig.level_optimization == 0)
            {
                while (MyMath.IsInArray(LevelScenes.randomLevel, ConstDefine.original_coinLv) || MyMath.IsInArray(LevelScenes.randomLevel, ConstDefine.original_challengeLv))
                {
                    LevelScenes.UpdateRandomLv();
                }
            }
            LevelScenes.LoadScene();
        }

        void ShowPanel(ESpecialPanel _lvName,bool _isShow) {
            switch (_lvName) {
                case ESpecialPanel.Challenge_Start:
                    ShowJoinChallengePanel(_isShow);
                    break;
                case ESpecialPanel.Challenge_Win:
                    ShowWinPanel(_isShow);
                    break;
                case ESpecialPanel.Challenge_Fail:
                    ShowFailPanel(_isShow);
                    break;
            }

        }

        //显示挑战关卡进入界面
        public void ShowJoinChallengePanel(bool _isShow)
        {
            if (_isShow)
            {
                //m_Img_scene.sprite = envSps[(int)LevelScenes.GetEnvType(LevelScenes.level)];
                UpdateCoinText();
                if (GameManager.abConfig.level_optimization == 0) {
                    m_Img_scene.sprite = envSps[Random.Range(0, (int)EnvType.Park+1)];
                }
                else
                    m_Img_scene.sprite = envSps[(int)System.Enum.Parse(typeof(EnvType), LevelScenes.LvInfoLDict[LevelScenes.level].Type)];
                m_Btn_NoChallenge.gameObject.SetActive(false);
                this.AttachTimer(ConstDefine.DelayShowTime, () => { m_Btn_NoChallenge.gameObject.SetActive(true); });
                m_Btn_JoinChallenge.transform.localScale = new Vector3(.8f, .8f, .8f);
                m_Btn_JoinChallenge.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), .8f).SetLoops(-1, LoopType.Yoyo);
            }
            else {
                m_Btn_JoinChallenge.transform.DOKill();
            }
            m_Trans_JoinChallenge.gameObject.SetActive(_isShow);
        }

        void ShowWinPanel(bool _isShow) {
            if (_isShow)
            {
                //UpdateCoinText();
                m_Btn_GetDouble.transform.localEulerAngles = new Vector3(0, 0, 10);
                m_Btn_GetDouble.transform.DORotate(Vector3.zero, .8f).SetLoops(-1, LoopType.Yoyo);
            }
            else {
                m_Btn_GetDouble.transform.DOKill();
            }
            m_Txt_reward.text = (ConstDefine.ChallengeCost * 2).ToString();
            m_Trans_WinChallenge.gameObject.SetActive(_isShow);
        }

        void ShowFailPanel(bool _isShow) {
            if (_isShow)
            {
                //UpdateCoinText();
                m_Btn_NoThank.gameObject.SetActive(false);
                this.AttachTimer(ConstDefine.DelayShowTime, () => { m_Btn_NoThank.gameObject.SetActive(true); });
                m_Btn_Restart.transform.localEulerAngles = new Vector3(0, 0, 10);
                m_Btn_Restart.transform.DORotate(Vector3.zero, .8f).SetLoops(-1, LoopType.Yoyo);
            }
            else {
                m_Btn_Restart.transform.DOKill();
            }
            m_Trans_EndChallenge.gameObject.SetActive(_isShow);
        }

        void Update() {           
            if (checkNet.IsFinish)
            {
                checkNet.ReStart();
                m_Btn_Restart.interactable = AdsManager.IsAdReady;
            }
            else
            {
                checkNet.OnUpdate(Time.deltaTime);
            }
        }

        void ShowNotEnough()
        {
            m_Trans_NotEnough.anchoredPosition = new Vector2(0, -1280);
            m_Trans_NotEnough.DOKill();
            m_Trans_NotEnough.DOAnchorPosY(0, 1).SetEase(Ease.Linear).onComplete = () =>
            {
                this.AttachTimer(0.3f, () => { m_Trans_NotEnough.anchoredPosition = new Vector2(0, -1280); });
            };
        }
    }
}

