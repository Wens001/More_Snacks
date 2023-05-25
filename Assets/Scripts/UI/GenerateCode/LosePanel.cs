using HighlightingSystem.Demo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUI
{
    public partial class LosePanel : MonoBehaviour, BasePanel
    {
        public Transform trans => transform;
        private MyTimer countDownTime;
        private float nowTime;
        private EWeapon curProp;
        private GameObject showModel;
        private MyTimer checkNet = new MyTimer(1);

        public void OnEnter()
        {
            curProp = EWeapon.NULL;
            if (GameEndPodium.Instance)
            {
                m_Btn_RePlay.gameObject.SetActive(true);
            }
            else {
                //还没有显示倒数复活的话
                if (!GameManager.Instance.hasShowReviveCountdown)
                {
                    if (GameManager.Instance.localPlayer.IsSnack)
                    {
                        GameManager.Instance.hasShowReviveCountdown = true;
                        countDownTime = new MyTimer(5);
                        nowTime = 5;
                        m_Img_Countdown.gameObject.SetActive(true);
                        m_Btn_Revive.gameObject.SetActive(true);
                        m_Btn_RePlay.gameObject.SetActive(false);
                        this.AttachTimer(ConstDefine.DelayShowTime, () =>
                        {
                            m_Btn_no.gameObject.SetActive(true);
                        });
                    }
                    if (GameManager.abConfig.rv_optimized == 1 && !GameManager.Instance.localPlayer.IsSnack)
                    {
                        GameManager.Instance.hasShowReviveCountdown = true;
                        var weapon = Random.Range(0, (int)EWeapon.Pepper);
                        curProp = (EWeapon)weapon;
                        var skin = SkinManager.Instance.GetChoseSkinInfo(ChildOrSnack.Child);
                        showModel = SkinManager.Instance.ShowSkinModel(skin, 0);
                        ModelShow ms = showModel.GetComponent<ModelShow>();
                        ms.ShowPropAnim(curProp);
                        m_RImg_Prop.gameObject.SetActive(true);
                        m_Btn_Revive.gameObject.SetActive(true);
                        m_Btn_RePlay.gameObject.SetActive(false);
                        this.AttachTimer(ConstDefine.DelayShowTime, () =>
                        {
                            m_Btn_no.gameObject.SetActive(true);
                        });
                    }
                }
                else
                {
                    m_Img_Countdown.gameObject.SetActive(false);
                    m_RImg_Prop.gameObject.SetActive(false);
                    m_Btn_Revive.gameObject.SetActive(false);
                    m_Btn_RePlay.gameObject.SetActive(true);
                }
            }
                     
            m_Btn_no.gameObject.SetActive(false);
            gameObject.SetActive(true);
            LevelScenes.LevelFailedEvent();           
            //Time.timeScale = 0;
        }

        public void OnExit()
        {
            countDownTime = null;
            if (m_Img_Countdown.gameObject.activeSelf) {
                m_Img_Countdown.gameObject.SetActive(false);
                m_Btn_Revive.gameObject.SetActive(false);              
            }
            m_RImg_Prop.gameObject.SetActive(false);
            m_Btn_no.gameObject.SetActive(false);
            gameObject.SetActive(false);
            Time.timeScale = 1;
        }

        public void OnPause()
        {

        }

        public void OnResume()
        {

        }

        void Awake()
        {
            GetBindComponents(gameObject);

            m_Btn_RePlay.onClick.AddListener( ()=> {
                LevelScenes.LevelRestartEvent();
                AdsManager.ShowInterstitial();
                LevelScenes.LoadScene();
            } );
            m_Btn_Revive.onClick.AddListener(() => {
                AdsManager.RewardAction = () =>
                {
                    if (showModel != null) {
                        Destroy(showModel);
                    }
                    Messenger.Broadcast<ChildOrSnack, EWeapon>(ConstDefine.Listener.Revive, GameManager.Instance.localPlayer.IsSnack ? ChildOrSnack.Snack : ChildOrSnack.Child, curProp);
                    UIPanelManager.Instance.PopPanel();
                    UIPanelManager.Instance.PushPanel(UIPanelType.GamePanel);                   
                };
                AdsManager.ShowRewardedAd("Revive");
            });
            m_Btn_no.onClick.AddListener(() =>
            {
                LevelScenes.LevelRestartEvent();
                AdsManager.ShowInterstitial();
                LevelScenes.LoadScene();
            });
        }

        void Update() {
            if (countDownTime != null) {
                if (nowTime > 0.1f)
                {
                    countDownTime.OnUpdate(Time.unscaledDeltaTime * GameManager.Instance.GameSpeed);
                    nowTime = countDownTime.DurationTime - countDownTime.timer;
                    m_Img_white.fillAmount = nowTime / 5;
                    m_Txt_Countdown.text = Mathf.CeilToInt(nowTime).ToString();
                }
                else {
                    countDownTime = null;
                    m_Img_Countdown.gameObject.SetActive(false);
                    m_Btn_Revive.gameObject.SetActive(false);
                    m_Btn_RePlay.gameObject.SetActive(true);
                    m_Btn_no.gameObject.SetActive(false);
                }
            }
            if (checkNet.IsFinish)
            {
                checkNet.ReStart();
                m_Btn_Revive.interactable = AdsManager.IsAdReady;
            }
            else {
                checkNet.OnUpdate(Time.deltaTime);
            }
        }

    }

}