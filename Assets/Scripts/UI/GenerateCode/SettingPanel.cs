using LionStudios.GDPR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUI{
    public partial class SettingPanel : MonoBehaviour, BasePanel
    {
        public static SettingPanel instance;
        public static bool isOpenGPDR = false;
        public Transform trans => transform;
        private bool CanCheckGDPR = true;
        private MyTimer CheckGDPRTime;

        public void OnEnter()
        {
            Time.timeScale = 0;
            m_Tog_Sound.isOn = PlayerPrefs.GetFloat("SoundVolume",1) <0.01f?false:true;
            m_Tog_Vibration.isOn = GameData.CanShake==0 ? false : true;
            gameObject.SetActive(true);
        }

        public void OnExit()
        {
            Time.timeScale = 1;
            isOpenGPDR = false;
            gameObject.SetActive(false);
        }

        public void OnPause()
        {

        }

        public void OnResume()
        {

        }

        void Awake()
        {
            instance = this;
            GetBindComponents(gameObject);
            m_Btn_Exit.onClick.AddListener(() => OnExit());
            m_Tog_Sound.onValueChanged.AddListener((isOn) => {
                AudioManager.Instance.ChangeSoundVolume(isOn ? 1 : 0);
            });
            m_Tog_Vibration.onValueChanged.AddListener((isOn) =>
            {
                GameData.CanShake = isOn ? 1 : 0;
                GameManager.CanShake = isOn;
            });
            m_Btn_Restore.onClick.AddListener(() => {
                AdsManager.Instance.ProductRestore();
            });
            m_Btn_Privacy.onClick.AddListener(() => {
                isOpenGPDR = true;
                LionGDPR.Show();               
            });
            bool IOS_isHighVersion = false;
            #if UNITY_IOS || UNITY_IPHONE
            if (MaxSdkUtils.CompareVersions(UnityEngine.iOS.Device.systemVersion, "14.5") != MaxSdkUtils.VersionComparisonResult.Lesser) {
                IOS_isHighVersion = true;
                CanCheckGDPR = false;
            }              
#endif
            m_Btn_Privacy.gameObject.SetActive(!IOS_isHighVersion && LionGDPR.Status == LionGDPR.UserStatus.Applies);
#if !UNITY_IOS
            m_Btn_Restore.gameObject.SetActive(false);
#endif
            m_Btn_Twitter.onClick.AddListener(() => {
                Application.OpenURL("https://twitter.com/LionStudiosCC");
            });
            m_Btn_Instagram.onClick.AddListener(() =>
            {
                Application.OpenURL("https://www.instagram.com/lionstudioscc/");
            });
            m_Btn_Facebook.onClick.AddListener(() =>
            {
                Application.OpenURL("https://www.facebook.com/lionstudios.cc/");
            });
            m_Btn_Youtube.onClick.AddListener(() =>
            {
                Application.OpenURL("https://www.youtube.com/lionstudioscc");
            });
            m_Btn_Lion.onClick.AddListener(() =>
            {
                Application.OpenURL("https://lionstudios.cc");
            });
            ConnectUsInit();
            CheckGDPRTime = new MyTimer(0.5f);
            DontDestroyOnLoad(gameObject);
        }

        void Update() {
            if (CanCheckGDPR) {
                CheckGDPRTime.OnUpdate(Time.unscaledDeltaTime);
                //Debug.Log("CheckGDPRTime:"+ CheckGDPRTime.timer);
                if (CheckGDPRTime.IsFinish) {
                    m_Btn_Privacy.gameObject.SetActive(LionGDPR.Status == LionGDPR.UserStatus.Applies);
                    if (LionGDPR.Status == LionGDPR.UserStatus.Applies) {
                        CanCheckGDPR = false;
                    }
                    CheckGDPRTime.ReStart();
                }
            }
        }

        private void ConnectUsInit()
        {
            var systemLangue = Application.systemLanguage;

            switch (systemLangue)
            {
                case SystemLanguage.Korean:
                    m_Txt_Connect.text = "우리와 연결하세요!";

                    break;
                case SystemLanguage.Japanese:
                    m_Txt_Connect.text = "私達と繋がりましょう！";

                    break;
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    m_Txt_Connect.text = "请联系我们！";

                    break;
                case SystemLanguage.ChineseTraditional:
                    m_Txt_Connect.text = "與我們連結！";

                    break;
                case SystemLanguage.Estonian:
                    m_Txt_Connect.text = "¡Síguenos en las redes!";
                    break;
                case SystemLanguage.German:
                    m_Txt_Connect.text = "Setze dich mit uns in Verbindung!";
                    break;
                case SystemLanguage.Russian:
                    m_Txt_Connect.text = "Свяжитесь с нами!";
                    break;
                default:
                    m_Txt_Connect.text = "Connect with us!";
                    break;
            }

        }
    }
}