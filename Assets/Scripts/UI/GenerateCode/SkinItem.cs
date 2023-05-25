using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinItem : MonoBehaviour
{
    public enum State { 
    Lock,
    Noraml,
    Chose
    }
    public SkinInfo skinInfo;
    public Image itemImg;
    public Button itemBtn;
    private Image itemBgImg;
    public GameObject choseIcon;
    public GameObject adIcon;
    public GameObject coinIcon;
    public Text coinText;
    //[HideInInspector]
    public State state;
    private int hasLookAd;
    private MyTimer checkNet = new MyTimer(1);
    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init()
    {
        itemBgImg = itemBtn.GetComponent<Image>();
        itemImg.sprite = skinInfo.Icon;
        hasLookAd = 0;
        itemBtn.onClick.AddListener(OnGetBtn);
        UpdateState();
    }

    private void Update()
    {
        if (skinInfo != null && skinInfo.getWay == GetWay.WatchAd) {
            if (checkNet.IsFinish)
            {
                checkNet.ReStart();
                itemBtn.interactable = AdsManager.IsAdReady;
            }
            else
            {
                checkNet.OnUpdate(Time.deltaTime);
            }
        }      
    }


    public void SetState(State _state) {
        switch (_state) {
            case State.Lock:
                itemImg.color = Color.gray;
                itemBgImg.sprite = Resources.Load<Sprite>("Texture/LockSkinBg");
                choseIcon.SetActive(false);
                if (skinInfo.getWay == GetWay.CoinBuy)
                {
                    coinText.text = skinInfo.CoinCost.ToString();
                    coinIcon.SetActive(true);
                }
                else {
                    adIcon.SetActive(true);
                }
                break;
            case State.Noraml:
                itemImg.color = Color.white;
                itemBgImg.sprite = Resources.Load<Sprite>("Texture/NormalSkinBg");
                choseIcon.SetActive(false);
                adIcon.SetActive(false);
                coinIcon.SetActive(false);
                break;
            case State.Chose:
                itemImg.color = Color.white;
                itemBgImg.sprite = Resources.Load<Sprite>("Texture/ChoseSkinBg");
                choseIcon.SetActive(true);
                adIcon.SetActive(false);
                coinIcon.SetActive(false);               
                GameData.SetChoseSkinId(skinInfo.PlayerType, skinInfo.ID);
                Messenger.Broadcast(ConstDefine.Listener.ChoseSkinItem, this);
                break;
        }
        state = _state;
    }

    public void UpdateState() {
        if (GameData.GetHasSkinId(skinInfo.PlayerType, skinInfo.ID) == 0)
        {
            SetState(State.Lock);
        }
        else
        {
            if (GameData.GetChoseSkinId(skinInfo.PlayerType) == skinInfo.ID)
            {
                SetState(State.Chose);
            }
            else
            {
                SetState(State.Noraml);
            }
        }
    }
    //点击按钮
    void OnGetBtn() {
        switch (state) {
            case State.Lock:
                if (skinInfo.getWay == GetWay.CoinBuy)
                {
                    if (GameData.Coin >= skinInfo.CoinCost)
                    {
                        Messenger.Broadcast(ConstDefine.Listener.AddCoinValue, -skinInfo.CoinCost);
                        SkinManager.Instance.SaveNewSkin(skinInfo.PlayerType, skinInfo.ID);
                        //GameData.SetHasSkinId(skinInfo.PlayerType, skinInfo.ID, 1);
                        //SkinManager.Instance.GetHasNoSkinList((int)skinInfo.PlayerType).Remove(skinInfo.ID);
                        SetState(State.Chose);
                    }
                    else {
                        Messenger.Broadcast(ConstDefine.Listener.NotEnough);
                    }
                }
                else {
                    AdsManager.RewardAction = () =>
                    {
                        hasLookAd++;
                        if (hasLookAd >= skinInfo.LookAdCount) {
                            SkinManager.Instance.SaveNewSkin(skinInfo.PlayerType, skinInfo.ID);
                            SetState(State.Chose);
                        }
                    };
                    AdsManager.ShowRewardedAd("Skin_Unlock_Skin");
                }
                break;
            case State.Noraml:                
                SetState(State.Chose);
                break;
        }
    }
}
