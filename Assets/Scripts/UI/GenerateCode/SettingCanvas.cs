using com.adjust.sdk;
using Facebook.Unity;
using IngameDebugConsole;
using LionStudios.Utility.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public enum EABGroup { 
A,
B,
C,
D
}

namespace MyUI {
	public partial class SettingCanvas : MonoBehaviour, BasePanel
	{
		public Transform trans => transform;
		public Toggle tog_ad;
		public GameObject debugConsolePrefab;
		private GameObject debugConsole;
		public static bool isTestBanner;
		private bool isShowBanner;
		private Button[] abBtns;
		private int choseABBtnId = 0;

		public void OnEnter()
		{
			gameObject.SetActive(true);
		}

		public void OnExit()
		{
			gameObject.SetActive(false);
		}

		public void OnPause()
		{
        
		}

		public void OnResume()
		{
        
		}

		public static SettingCanvas Instance { get; private set; }

		void Awake()
		{
			GetBindComponents(gameObject);
			DontDestroyOnLoad(gameObject);
			Instance = this;			
			abBtns = m_Trans_ABTest.GetComponentsInChildren<Button>();
			m_Input_Lv.onEndEdit.AddListener((lv) => { LevelScenes.level.Value = int.Parse(lv); LevelScenes.LoadScene(); });
			m_Btn_ALevel.onClick.AddListener( ()=> { if (LevelScenes.level >= 1) { m_Input_Lv.text = (--LevelScenes.level.Value).ToString(); LevelScenes.LoadScene(); } } );
			m_Btn_BLevel.onClick.AddListener( ()=> { if (LevelScenes.level <= 1000) { m_Input_Lv.text = (++LevelScenes.level.Value).ToString(); LevelScenes.LoadScene(); } } );
			m_Btn_Close.onClick.AddListener(() => { gameObject.SetActive(false); });
			m_Input_Coin.onEndEdit.AddListener((coin) => { m_Input_Coin.text = coin.ToString(); });
			m_Btn_ACoin.onClick.AddListener(() => { Messenger.Broadcast(ConstDefine.Listener.AddCoinValue, -int.Parse(m_Input_Coin.text)); });
			m_Btn_BCoin.onClick.AddListener(() => { Messenger.Broadcast(ConstDefine.Listener.AddCoinValue, int.Parse(m_Input_Coin.text)); });			
			tog_ad.onValueChanged.AddListener((isOn) => { AdsManager.ShowAds.Value = isOn;GameData.Ads = isOn ? 1 : 0; });
			//m_Btn_AB.onClick.AddListener(() => { GameManager.abConfig.ChangeGroup(); m_Txt_AB.text = GameManager.abConfig.abGroup.ToString(); });
			for (int i = 0; i < abBtns.Length; i++) {
				int curId = i;
				abBtns[i].onClick.AddListener(()=> {
					UpdateABTestState((EABGroup)curId);
					SetGameAB((EABGroup)curId);
				});
			}
			m_Btn_Clear.onClick.AddListener(() => { PlayerPrefs.DeleteAll();PlayerPrefs.Save(); });
			m_Btn_Console.onClick.AddListener(() => {
				if (debugConsole == null)
				{
					debugConsole = Instantiate(debugConsolePrefab);
				}
				else {
					debugConsole.SetActive(!debugConsole.activeSelf);
				}
				GameData.ShowDebug = debugConsole.activeSelf ? 1 : 0;
			});
			m_Btn_ShowAd.onClick.AddListener(() => {
                AdsManager.RewardAction = () =>
                {
					Debug.Log("广告回调成功");
                };
                AdsManager.ShowRewardedAd("TestShowAd");
            });
            m_Btn_ShowAd2.onClick.AddListener(() =>
            {
				//打点参数
				//            Dictionary<string, object> eventParams = new Dictionary<string, object>();
				//            eventParams["placement"] = "TestShowAd2";
				//            eventParams["level"] = LevelScenes.level;
				//            eventParams["ABtest"] = GameManager.abConfig.ab_experiment_group;
				//LionStudios.Analytics.Events.RewardVideoStart(eventParams);
				isShowBanner = !isShowBanner;
				if (isShowBanner)
				{					
					AdsManager.ShowBanner();
					Debug.Log("ShowBanner");

				}
				else {					
					AdsManager.HideBanner();
					Debug.Log("HideBanner");
				}
				UpdateShowBannerText();
			});
			m_Btn_TestBanner.onClick.AddListener(() => { isTestBanner = !isTestBanner; UpdateTestBannerText(); });
			m_Btn_InitFb.onClick.AddListener(() => { FB.Init();Debug.Log("Init Fb"); });
			m_Btn_FbIsLoggedIn.onClick.AddListener(() => {Debug.Log("Fb is loggedIn: "+FB.IsLoggedIn); });
			m_Btn_FbIsInit.onClick.AddListener(() => { Debug.Log("Fb is Init: " + FB.IsInitialized); });
			gameObject.SetActive(false);
		}

		void OnEnable() {
			
			DebugLogManager debugLogManager = FindObjectOfType<DebugLogManager>();
			if (debugLogManager)
				debugConsole = debugLogManager.gameObject;
			m_Input_Lv.text = LevelScenes.level.Value.ToString();
			tog_ad.isOn = GameData.Ads == 1 ? true : false;
			UpdateABTestState(GetGameAB());
			UpdateShowBannerText();
			UpdateTestBannerText();
			UpdateIdfa();
		}

		void UpdateABTestState(EABGroup _type)
		{
			abBtns[choseABBtnId].image.color = Color.white;
			choseABBtnId = (int)_type;
			abBtns[choseABBtnId].image.color = Color.green;
		}

		string jsonPath;
		private Dictionary<string, object> firebasePkgJson;
		const string default_ver = "7.2.0";
		private void Update()
        {
			m_Txt_Level.text = LevelScenes.level.ToString();			
			m_Txt_firebaseInfo.text = "";
            if (firebasePkgJson == null)
            {
				m_Txt_firebaseInfo.text = "firebasePkgJson: null\n";

				string jsonPath = Path.GetFullPath(
                    Path.Combine("Packages", "com.google.firebase.app", "package.json"));
				
				if (File.Exists(jsonPath))
                {
					m_Txt_firebaseInfo.text += $"jsonPath: {jsonPath} ,have file!\n";
					firebasePkgJson =
                        MiniJson.Deserialize(File.ReadAllText(jsonPath)) as Dictionary<string, object>;
                }
                else
                {
					m_Txt_firebaseInfo.text += $"jsonPath: {jsonPath} ,have No file!\n";
					string path = Path.Combine(Application.dataPath, "Firebase", "Plugins");
					if (Directory.Exists(path))
					{
						m_Txt_firebaseInfo.text += $"path:{path} , have Dir!\n";
					}
					else {
						m_Txt_firebaseInfo.text += $"path:{path} , have No Dir!\n";
					}
                }
            }

            if (firebasePkgJson != null)
            {
				var version = firebasePkgJson.ContainsKey("version")
                    ? firebasePkgJson["version"] as string
                    : default_ver;
                m_Txt_firebaseInfo.text = "firebasePkgJson: have!\nversion:{version}\n";
            }
        }

		void UpdateShowBannerText()
		{
			m_Txt_banner.text = isShowBanner ? "ShowBanner" : "HideBanner";
		}
        void UpdateTestBannerText()
        {
            m_Txt_TestBanner.text = isTestBanner ? "On" : "Off";
        }
		void UpdateIdfa() {
			m_Txt_idfa.text ="IDFA : "+ Adjust.getIdfa();
		}

		EABGroup GetGameAB() {
            if (GameManager.abConfig.reward_level == 0)
            {
                return EABGroup.A;
            }
            else
            {
                return EABGroup.B;
            }
        }

		void SetGameAB(EABGroup group) {
			int muscleSnack = 0;
			switch (group) {
				case EABGroup.A:
					muscleSnack = 0;
					break;
				case EABGroup.B:
					muscleSnack = 1;
					break;
				case EABGroup.C:
					muscleSnack = 0;
					break;
				case EABGroup.D:
					muscleSnack = 1;
					break;
			}
			GameManager.abConfig.SetGroup(ABGroup.a, muscleSnack, true);
		}
    }
}