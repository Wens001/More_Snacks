using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace MyUI {
	public partial class WinPanel : MonoBehaviour, BasePanel
	{
		public struct PrizeItem {
			public Button boxBtn;
			public Transform coin;
			public Text coinTxt;
			public GameObject skinTex;

			public PrizeItem(Transform item) {
				boxBtn = item.GetComponentInChildren<Button>();
				coin = item.Find("CoinImg");
				coinTxt = coin.GetComponentInChildren<Text>();
				skinTex = item.Find("SkinTex").gameObject;
			}

			public void Reset() {
				boxBtn.transform.localScale = Vector3.one;
				coin.localScale = Vector3.zero;
				if (skinTex.activeSelf)
					skinTex.SetActive(false);
			}
			public void ShowGetCoin(int money) {
				coinTxt.text = money.ToString();
				coin.DOScale(Vector3.one, 0.6f);
			}
		}

		public Transform trans => transform;
		private GraphicRaycaster raycaster;
		//指针
		private Transform needleTra;
		private float rotateSp = 200;
		private Vector3 curEluer;
		private float angle;
		private bool canRotate = true;
		private int rotateDir = 1;

		//初始金币
		private readonly int startMoney = 50;
		//奖励倍数
		private int rewardRate;
		//最终奖励金币
		private int rewardMoney;

		//是否可以显示宝箱面板
		/*****正常玩游戏后，如果集满3个钥匙，弹出胜利面板后，优先弹宝箱面板，抽完奖再走下一步。
			  因为可能会跳转到新皮肤界面或者道具界面，再跳回来会显示胜利界面，因此此判断为了防止再次出现宝箱面板
		 *******/
		private bool canShowPrizePanel;
		private MyTimer checkNet = new MyTimer(1);

		public void OnEnter()
		{
			this.AttachTimer(0.5f, () => AdsManager.HideBanner());
			gameObject.SetActive(true);
			LevelGuide guide = FindObjectOfType<LevelGuide>();
			if (guide) {
				guide.gameObject.SetActive(false);
			}
			if (GameManager.sceneType != SceneType.Level)
			{
				if (GameManager.sceneType == SceneType.GetNewSkin)
				{
					if (m_Img_NewSkin.sprite == null)
					{
						m_Img_NewSkin.gameObject.SetActive(false);
						m_Img_Black.gameObject.SetActive(false);
						m_Txt_SkinProgress.gameObject.SetActive(false);
					}
					else
					{
						m_Img_NewSkin.gameObject.SetActive(true);
						m_Img_Black.gameObject.SetActive(false);
						m_Txt_SkinProgress.gameObject.SetActive(false);
					}
				}
				GameManager.sceneType = SceneType.Level;
				m_Btn_Next.gameObject.SetActive(true);
				m_Btn_RePlay.gameObject.SetActive(true);
				canShowPrizePanel = false;
				return;
			}
			else {
				canShowPrizePanel = true;
				AdsManager.ShowInterstitial();
				//LevelScenes.LevelCompleteEvent();
				this.AttachTimer(0.1f, () => LevelScenes.LevelCompleteEvent());
			}
			//Time.timeScale = 0;
			if (raycaster)
				raycaster.enabled = true;
			if (GameManager.abConfig.rv_optimized ==1 && canShowPrizePanel && GameData.PrizeKey >= 3)
			{
				ShowPrizePanel();
			}
			else {
				ShowWinContent();
                //交叉广告
                AdsManager.ShowCrossPromo();
            }			
		}

		public void OnExit()
		{
			AdsManager.HideCrossPromo();
			m_Img_NewSkin.gameObject.SetActive(false);
			m_Btn_Next.gameObject.SetActive(false);
			m_Btn_RePlay.gameObject.SetActive(false);
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
			TryGetComponent(out raycaster);
			needleTra = m_Img_needle.transform;
			m_Btn_RePlay.onClick.AddListener(() => {
				raycaster.enabled = false;
				LevelScenes.LevelRestartEvent();
				//AdsManager.ShowInterstitial();
				LevelScenes.LoadScene();
			});
			m_Btn_RePlay.gameObject.SetActive(false);
			m_Btn_Next.onClick.AddListener(() => {
				raycaster.enabled = false;
				Messenger.Broadcast<int>(ConstDefine.Listener.AddCoinValue, startMoney);
				//AdsManager.ShowInterstitial();
				OnNextLv();
			});
			m_Btn_Next.gameObject.SetActive(false);
			m_Btn_Ads.onClick.AddListener(() => {
				AdsManager.RewardAction = () => {
					raycaster.enabled = false;
					canRotate = false;
					//Debug.Log("转盘奖励："+rewardMoney);
					//Messenger.Broadcast<int>(ConstDefine.Listener.AddCoinValue, rewardMoney);
					//LevelScenes.AddLevel();
					UITools.Instance.FlyCoin(rewardMoney, 8, trans, m_Btn_Ads.transform.position, m_Btn_CoinImage.transform.position, () =>
					{
						this.AttachTimer(1,()=> {
							OnNextLv();
						} );
					});
				};
				AdsManager.ShowRewardedAd("More_Coin");
			});
			UpdateCoinUI();
			Messenger.AddListener<SkinInfo>(ConstDefine.Listener.GetNewSkin, UpdateNewSkinImg);
			Messenger.AddListener(ConstDefine.Listener.LoseNewSkin, LoseNewSkin);
			Messenger.AddListener<int>(ConstDefine.Listener.AddCoinValue, UpdateCoinUI);
			//宝箱面板
			InitPrizePanel();
		}

		//点击按钮进入下一关功能（多倍按钮和直接下一关按钮）
		void OnNextLv() {
            LevelScenes.AddLevel();
            var loadLv = LevelScenes.CurLoadLvId();
            if ((LevelScenes.level == 4 || LevelScenes.level == 11 || LevelScenes.level == 21) && !GameData.AllInOneBundle)
            {
                GameManager.sceneType = SceneType.DisCountStore;
                LevelScenes.LoadScene(ConstDefine.DiscountScene);
            }
            else
            {
                if (GameManager.abConfig.bonus_level == 0)
                {
                    //A,B组都跳过奖励关卡
                    if (MyMath.IsInArray(loadLv, ConstDefine.original_coinLv))
                    {
                        LevelScenes.AddLevel();
                    }
					//为了防止进入的是奖励关卡(上面已经更新关卡，但是在没排序的关卡组且是循环时，有可能又循环到奖励关卡)
					while (MyMath.IsInArray(LevelScenes.CurLoadLvId(), ConstDefine.original_coinLv)) {
						if (GameManager.abConfig.level_optimization == 0)
						{
							LevelScenes.UpdateRandomLv();
						}
						else {
							LevelScenes.AddLevel();
						}
					}
                    LevelScenes.LoadScene();
                }
                else
                {
                    if (GameManager.abConfig.level_optimization == 0)
                    {
                        //C组按原来顺序，可进入奖励关卡
                        if (MyMath.IsInArray(loadLv, ConstDefine.original_coinLv))
                        {
                            UIPanelManager.Instance.PushPanel(UIPanelType.CoinLvPanel);
                            Messenger.Broadcast<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ESpecialPanel.CoinLv_Start, true);
                        }
                        else if (MyMath.IsInArray(loadLv, ConstDefine.original_challengeLv))
                        {
                            UIPanelManager.Instance.PushPanel(UIPanelType.ChallengePanel);
                            Messenger.Broadcast<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ESpecialPanel.Challenge_Start, true);
                        }
                        else if(MyMath.IsInArray(loadLv, ConstDefine.original_muscleSnackLv))
						{
							if (GameManager.abConfig.reward_level == 0)
							{
								LevelScenes.AddLevel();
								LevelScenes.LoadScene();
							}
							else {
                                UIPanelManager.Instance.PushPanel(UIPanelType.CoinLvPanel);
                                Messenger.Broadcast<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ESpecialPanel.MuscleSnack_Start, true);
                            }								
                        }
                        else
                        {
                            LevelScenes.LoadScene();
                        }
                    }
                    else
                    {//D组按调整后顺序，可以进奖励关卡 
                        if (MyMath.IsInArray(loadLv, ConstDefine.original_coinLv))
                        {
                            UIPanelManager.Instance.PushPanel(UIPanelType.CoinLvPanel);
                            Messenger.Broadcast<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ESpecialPanel.CoinLv_Start, true);
                        }
                        else if (MyMath.IsInArray(loadLv, ConstDefine.adjust_challengeLv))
                        {
                            UIPanelManager.Instance.PushPanel(UIPanelType.ChallengePanel);
                            Messenger.Broadcast<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ESpecialPanel.Challenge_Start, true);
                        }
                        else
                        {
                            LevelScenes.LoadScene();
                        }
                    }
                }
            }
        }

		void ShowWinContent() {
			canRotate = true;
			rotateDir = 1;
			curEluer = Vector3.zero;
			angle = 0;
			//延迟3秒出现按钮
			Timer.Register(ConstDefine.DelayShowTime, () =>
			{
				m_Btn_Next.gameObject.SetActive(true);
				m_Btn_RePlay.gameObject.SetActive(true);
			});
			//换剪影
			if (SkinManager.Instance.nextSkinType > -1)
			{
				m_Img_Black.sprite = Resources.Load<Sprite>("Texture/jy_black" + SkinManager.Instance.nextSkinType);
				m_Img_Jianying.sprite = Resources.Load<Sprite>("Texture/jy_green" + SkinManager.Instance.nextSkinType);
				if (!m_Img_Black.gameObject.activeSelf)
				{
					m_Img_Black.gameObject.SetActive(true);
					m_Txt_SkinProgress.gameObject.SetActive(true);
				}
				//刷新皮肤进度
				UpdateSkin();
			}
			else
			{
				if (m_Img_Black.gameObject.activeSelf)
				{
					m_Img_Black.gameObject.SetActive(false);
					m_Txt_SkinProgress.gameObject.SetActive(false);
				}
			}
		}

		//要新皮肤的回调
		void UpdateNewSkinImg(SkinInfo _info) {
			Debug.Log($"胜利界面显示获取皮肤：{_info.SkinName}");
			m_Img_NewSkin.sprite = _info.Icon;
			if (_info.PlayerType == ChildOrSnack.Child)
				m_Img_NewSkin.rectTransform.sizeDelta = new Vector2(256, 256);
			else
				m_Img_NewSkin.rectTransform.sizeDelta = new Vector2(200, 200);
		}
		//不要新皮肤的回调
		void LoseNewSkin() {
			m_Img_NewSkin.sprite = null;
		}

		void Update() {
			GameManager.Instance.GameSpeed = 0;
			//转指针

			RotateNeedle();
#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.A))
			{
				UpdateSkin();
			}
#endif
            if (checkNet.IsFinish)
			{
				checkNet.ReStart();
				m_Btn_Ads.interactable = AdsManager.IsAdReady;
				m_Btn_AddKey.interactable = AdsManager.IsAdReady;
			}
			else {
				checkNet.OnUpdate(Time.deltaTime);
			}
		}
		//更新皮肤进度
		void UpdateSkin() {
			float progress = GameData.SkinProgress;
			m_Img_Jianying.fillAmount = progress;
			progress += 0.33f;
			if (progress > 0.95) {
				progress = 1;
			}
			GameData.SkinProgress = progress == 1 ? 0 : progress;
			m_Txt_SkinProgress.text = string.Format("NEW SKIN :{0:f0}%", progress * 100);
			m_Img_Jianying.DOFillAmount(progress, 0.5f).SetEase(Ease.Linear).onComplete += () => {
				//进入获取道具，如果皮肤也满了，那么退出获取道具界面后，进去获取皮肤界面
				if (GameData.Level == 6 || GameData.Level == 7) {
					GameManager.sceneType = SceneType.GetNewProp;
					LevelScenes.LoadScene(ConstDefine.specialScene);
					return;
				}
				if (progress == 1) {
					GameManager.sceneType = SceneType.GetNewSkin;
					LevelScenes.LoadScene(ConstDefine.specialScene);
				}
			};
		}

		//转指针
		void RotateNeedle() {
			if (canRotate)
			{
				curEluer.z += rotateSp * rotateDir * Time.unscaledDeltaTime;
				curEluer.z = Mathf.Clamp(curEluer.z, -90, 90);
				needleTra.localEulerAngles = curEluer;
				angle = Mathf.Abs(curEluer.z);
				if (angle >= 90)
				{
					angle = 90;
					rotateDir *= -1;
				}
				if (angle < 23)
				{
					rewardRate = 5;
				}
				else if (angle < 46)
				{
					rewardRate = 4;
				}
				else if (angle < 69)
				{
					rewardRate = 3;
				}
				else
				{
					rewardRate = 2;
				}
				rewardMoney = startMoney * rewardRate;
				m_Txt_Money.text = rewardMoney.ToString();
			}
		}

		void UpdateCoinUI(int _addCoin = 0) {
			DOTween.To(() => GameData.Coin-_addCoin, x => m_Txt_CoinValue.text = x.ToString(),GameData.Coin, 0.5f).SetEase(Ease.Linear);
			
		}

        /*****************宝箱界面*********************/
        public Sprite hasKeySp;
		public Sprite noKeySp;
		private PrizeItem[] prizeItems;
		private Image[] keys;
		private BindableProperty<int> keyNum;
		private int skinBoxId = 0;
		private int[] moneys = { 50, 50, 50, 150, 150, 200, 200, 300 };
		private List<int> moneyList;
		int luckySkinId;
		private bool firstDelayShow;
		private int remainBoxNum;
		private GameObject prizeModel;
		
		void InitPrizePanel() {
			prizeItems = new PrizeItem[m_Trans_Boxs.childCount];
            for (int i = 0; i < prizeItems.Length; i++)
            {
                var id = i;
				prizeItems[i] = new PrizeItem(m_Trans_Boxs.GetChild(i));
				prizeItems[i].boxBtn.onClick.AddListener(() => { OnClickBoxBtn(id); });
				BoxAnim(i);
			}
			keys = m_Trans_Keys.GetComponentsInChildren<Image>();
			keyNum = new BindableProperty<int>(m_Trans_Keys.childCount);
			keyNum.OnChange += () =>
            {
				for (int i = 0; i < keys.Length; i++) {
					if (i < keyNum)
					{
						keys[i].sprite = hasKeySp;
					}
					else {
						keys[i].sprite = noKeySp;
					}
				}
				GameData.PrizeKey = keyNum.Value;
			};           
			m_Btn_AddKey.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.6f).SetLoops(-1, LoopType.Yoyo);
			m_Btn_AddKey.onClick.AddListener(OnClickAddKey);
			m_Btn_NoThank.onClick.AddListener(ClosePrizePanel);
		}
        #region 宝箱动画
        void BoxAnim(int id)
		{
			prizeItems[id].boxBtn.transform.DOShakeRotation(1, 50).SetDelay(1).onComplete = () =>
			{
				BoxAnim(id);
			};
		}
		void CLoseBoxAnim(int id) {
			prizeItems[id].boxBtn.transform.DOKill();
		}
        #endregion

        void ShowPrizePanel() {
			if (prizeModel != null) {
				Destroy(prizeModel);
			}
            for (int i = 0; i < prizeItems.Length; i++)
            {
				prizeItems[i].Reset();
			}
            keyNum.Value = keys.Length;
			skinBoxId = Random.Range(0, prizeItems.Length);
			moneyList = moneys.ToList();
            var luckySkinType = SkinManager.Instance.nextSkinType;
            luckySkinId = SkinManager.Instance.GetNewSkinId();
			if (luckySkinId < 0)
			{
				m_RImg_skin.gameObject.SetActive(false);
				m_Group_ManyMoney.alpha = 1;
			}
			else {
				m_Group_ManyMoney.alpha = 0;
				prizeModel = SkinManager.Instance.ShowSkinModel(SkinManager.Instance.GetAllSkinInfo(luckySkinType)[luckySkinId], 0);
				m_RImg_skin.gameObject.SetActive(true);
			}
            m_Trans_PrizePanel.gameObject.SetActive(true);
			firstDelayShow = true;
			remainBoxNum = 9;
		}

		void ClosePrizePanel() {
			ShowDownBtns(false);
			m_Trans_PrizePanel.gameObject.SetActive(false);
			Destroy(prizeModel);
			ShowWinContent();
		}

        void OnClickBoxBtn(int id)
        {
			CLoseBoxAnim(id);
			if (keyNum <= 0) {
				return;
			}
            keyNum.Value--;
			remainBoxNum--;
			HideBox(id);			
			if (keyNum <= 0)
            {
                if (remainBoxNum > 0)
                {
					ShowDownBtns(true);
				}
				else
				{
					this.AttachTimer(2, ClosePrizePanel);
				}
            }
            if (skinBoxId == id)
            {               
				if (luckySkinId >= 0)
				{
					int luckySkinType = SkinManager.Instance.nextSkinType;
					prizeItems[id].skinTex.SetActive(true);
					Messenger.RemoveListener<SkinInfo>(ConstDefine.Listener.GetNewSkin, UpdateNewSkinImg);
					SkinManager.Instance.SaveNewSkin((ChildOrSnack)luckySkinType, luckySkinId, true);
					Messenger.Broadcast(ConstDefine.Listener.GetNewSkin, SkinManager.Instance.GetAllSkinInfo(luckySkinType)[luckySkinId]);
					Messenger.AddListener<SkinInfo>(ConstDefine.Listener.GetNewSkin, UpdateNewSkinImg);
					SkinManager.Instance.UpdateNextSkinType();
					return;
				}
				else {
					StartCoroutine(ShowGetMoneyAnim(id, 500));
					return;
				}
            }            
			var boxMoney = moneyList[Random.Range(0, moneyList.Count)];
			moneyList.Remove(boxMoney);
			StartCoroutine(ShowGetMoneyAnim(id, boxMoney));			
        }

        void HideBox(int id)
        {
            prizeItems[id].boxBtn.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        }
		//钥匙奖励金币动画
        IEnumerator ShowGetMoneyAnim(int id, int money)
        {
            int iconNum = money / 30;
            iconNum = Mathf.Clamp(iconNum, 4, 9);     
			prizeItems[id].ShowGetCoin(money);
            for (int i = 0; i < iconNum; i++)
            {
                int curId = i;
				Transform icon;
				icon = Instantiate(Resources.Load<Transform>("UI/CoinIcon"));
                icon.SetParent(trans);
                icon.position = prizeItems[id].boxBtn.transform.position;
                Vector3 pos = Random.insideUnitCircle;
                icon.DOMove(icon.position + pos * 180, 0.7f).onComplete = () =>
                {
                    icon.DOMove(m_Btn_CoinImage.transform.position, 0.6f).onComplete = () =>
                    {
                        if (curId == 0)
                        {
                            Messenger.Broadcast<int>(ConstDefine.Listener.AddCoinValue, money);
                        }
                        Destroy(icon.gameObject);
                    };
                };
                yield return null;
            }
        }

		void ShowDownBtns(bool _isShow) {
            m_Btn_AddKey.gameObject.SetActive(_isShow);
			if (firstDelayShow)
			{
				this.AttachTimer(ConstDefine.DelayShowTime, () => {
					if(m_Btn_AddKey.gameObject.activeSelf)
						m_Btn_NoThank.gameObject.SetActive(_isShow);
				});
				firstDelayShow = false;
			}
			else {
				m_Btn_NoThank.gameObject.SetActive(_isShow);
			}
        }

		void OnClickAddKey() {
            AdsManager.RewardAction = () =>
            {
				keyNum.Value = 3;
				ShowDownBtns(false);
            };
            AdsManager.ShowRewardedAd("AddKey");
        }
    }	
}