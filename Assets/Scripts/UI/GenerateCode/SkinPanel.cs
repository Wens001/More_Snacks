//using AssetBundleBrowser.AssetBundleModel;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MyUI {
    public partial class SkinPanel : MonoBehaviour, BasePanel
    {
        public Transform trans => transform;
        private Image kidBtnImg;
        private Image snackBrnImg;
        private List<SkinItem> kidItemList;
        private List<SkinItem> snackItemList;

        private bool isShowKid;
        private SkinItem choseKidItem;
        private SkinItem choseSnackItem;
        private Dictionary<SkinInfo, GameObject> modelDict;
        private GameObject curModel;
        private Canvas canvas;
        [Serializable]
        public class SpBox{
            private Image img;
            public Sprite choseSp;
            public Sprite normalSp;
            private Text text;
            private UnityEngine.UI.Outline outLine;

            public void SetImg(Image _img, Text _text) {
                img = _img;
                text = _text;
                outLine = text.GetComponent<UnityEngine.UI.Outline>();
            }

            public void SetChose() {
                img.sprite = choseSp;
                Vector2 size = img.rectTransform.sizeDelta;
                size.y = 220;
                //img.rectTransform.sizeDelta = new Vector2(552, 220);
                img.rectTransform.sizeDelta = size;
                //Vector2 pos = img.rectTransform.anchoredPosition;
                //pos.y = 10;
                //img.rectTransform.anchoredPosition = pos;
                Vector2 pos = text.rectTransform.anchoredPosition;
                pos.y = 21.5f;
                text.rectTransform.anchoredPosition = pos;
                outLine.enabled = true;
            }
            public void SetNormal() {
                img.sprite = normalSp;
                Vector2 size = img.rectTransform.sizeDelta;
                size.y = 160;
                //img.rectTransform.sizeDelta = new Vector2(527, 160);//518,140
                img.rectTransform.sizeDelta = size;
                Vector2 pos = text.rectTransform.anchoredPosition;
                pos.y = 28.8f;
                //img.rectTransform.anchoredPosition = pos;
                text.rectTransform.anchoredPosition = pos;
                outLine.enabled = false;
            }
        }
        public SpBox kidSpBox;
        public SpBox snackSpBox;
        // Start is called before the first frame update
        private void Awake()
        {
            isShowKid = true;
            GetBindComponents(gameObject);
            kidBtnImg = m_Btn_kid.GetComponent<Image>();
            snackBrnImg = m_Btn_snack.GetComponent<Image>();
            m_Btn_kid.onClick.AddListener(ShowKidPanel);
            m_Btn_snack.onClick.AddListener(ShowSnackPanel);
            m_Btn_Return.onClick.AddListener(OnReturnBtnClick);
            modelDict = new Dictionary<SkinInfo, GameObject>();
            kidSpBox.SetImg(kidBtnImg, m_Btn_kid.GetComponentInChildren<Text>());
            snackSpBox.SetImg(snackBrnImg, snackBrnImg.GetComponentInChildren<Text>());
            Messenger.AddListener<SkinItem>(ConstDefine.Listener.ChoseSkinItem, OnChoseSkinItemBtn);
            Messenger.AddListener<SkinInfo>(ConstDefine.Listener.GetNewSkin, UpdatePanel);
            StartCoroutine(InitItem());
        }

        IEnumerator InitItem() {
            Transform parentTra = m_SRect_Kid.GetComponentInChildren<GridLayoutGroup>().transform;
            SkinItem itemPrefab = Resources.Load<SkinItem>("UI/SkinItem");
            kidItemList = new List<SkinItem>();
            snackItemList = new List<SkinItem>();
            for (int i = 0; i < SkinManager.Instance.allChildSkinInfo.Count; i++)
            {
                SkinItem item = Instantiate(itemPrefab, parentTra);
                item.transform.localScale = Vector3.one;
                item.skinInfo = SkinManager.Instance.allChildSkinInfo[i];
                item.Init();
                kidItemList.Add(item);
                yield return null;
            }
            parentTra = m_SRect_Snack.GetComponentInChildren<GridLayoutGroup>().transform;
            for (int i = 0; i < SkinManager.Instance.allSnackSkinInfo.Count; i++)
            {
                SkinItem item = Instantiate(itemPrefab, parentTra);
                item.transform.localScale = Vector3.one;
                item.skinInfo = SkinManager.Instance.allSnackSkinInfo[i];
                item.Init();
                snackItemList.Add(item);
                yield return null;
            }
        }


        public void OnEnter()
        {
            this.AttachTimer(0.2f, () => AdsManager.HideBanner());
            GameManager.canRunGame = false;
            Messenger.AddListener<int>(ConstDefine.Listener.AddCoinValue, UpdateCoin);
            Messenger.AddListener(ConstDefine.Listener.NotEnough, ShowNotEnough);
            if (canvas == null)
            {
                TryGetComponent(out canvas);
            }
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 5;
            m_Txt_coin.text = GameData.Coin.ToString();
            ShowKidPanel();
            gameObject.SetActive(true);
        }

        public void OnPause()
        {

        }

        public void OnResume()
        {

        }

        public void OnExit()
        {
            Messenger.RemoveListener<int>(ConstDefine.Listener.AddCoinValue, UpdateCoin);
            Messenger.RemoveListener(ConstDefine.Listener.NotEnough, ShowNotEnough);
            m_SRect_Kid.gameObject.SetActive(false);
            curModel = null;
            modelDict.Clear();
            gameObject.SetActive(false);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            GameManager.canRunGame = true;
        }

        void ShowKidPanel() {
            isShowKid = true;
            if (!m_SRect_Kid.gameObject.activeSelf)
            {
                kidSpBox.SetChose();
                snackSpBox.SetNormal();
                m_SRect_Kid.gameObject.SetActive(true);
                m_SRect_Snack.gameObject.SetActive(false);
                if(choseKidItem)
                    ShowModel(choseKidItem.skinInfo);
                else
                    ShowModel(SkinManager.Instance.allChildSkinInfo[GameData.GetChoseSkinId( ChildOrSnack.Child)]);
            }
        }
        void ShowSnackPanel()
        {
            isShowKid = false;
            if (!m_SRect_Snack.gameObject.activeSelf)
            {
                kidSpBox.SetNormal();
                snackSpBox.SetChose();
                m_SRect_Kid.gameObject.SetActive(false);
                m_SRect_Snack.gameObject.SetActive(true);                
                if (choseSnackItem)
                    ShowModel(choseSnackItem.skinInfo);
                else
                    ShowModel(SkinManager.Instance.allSnackSkinInfo[GameData.GetChoseSkinId(ChildOrSnack.Snack)]);
            }
        }
        //获取随机新皮肤时刷新皮肤界面
        void UpdatePanel(SkinInfo _info) {
            if (_info.PlayerType == ChildOrSnack.Child)
            {
                kidItemList[_info.ID].SetState(SkinItem.State.Chose);
            }
            else
            {
                snackItemList[_info.ID].SetState(SkinItem.State.Chose);
            }
        }

        void OnReturnBtnClick() {
            GameManager.sceneType = SceneType.Level;
            LevelScenes.LoadScene();
        }

        void OnChoseSkinItemBtn(SkinItem _item ) {
            if (_item.skinInfo.PlayerType == ChildOrSnack.Child)
            {
                if(choseKidItem && choseKidItem!=_item)
                    choseKidItem.SetState(SkinItem.State.Noraml);
                choseKidItem = _item;
                ShowModel(choseKidItem.skinInfo);
            }
            else {
                if(choseSnackItem && choseSnackItem != _item)
                    choseSnackItem.SetState(SkinItem.State.Noraml);
                choseSnackItem = _item;
                ShowModel(choseSnackItem.skinInfo);
            }
        }

        void ShowModel(SkinInfo _skin) {
            if (GameManager.sceneType !=  SceneType.SkinScene) {
                return;
            }
            if ((_skin.PlayerType == ChildOrSnack.Child ? true : false) != isShowKid)
            {
                return;
            }
            if (curModel != null) {
                curModel.SetActive(false);
            }
            if (!modelDict.ContainsKey(_skin)) {
                GameObject model = Instantiate(_skin.SkinModel);
                modelDict[_skin] = model;
                if(_skin.PlayerType == ChildOrSnack.Child)
                    model.transform.position = new Vector3(0, 0.5f, 0);
                else
                    model.transform.position = new Vector3(0, 0.8f, 0);

                model.transform.rotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
                model.GetComponent<NavMeshAgent>().enabled = false;
                model.GetComponent<BaseControl>().enabled = false;
            }
            curModel = modelDict[_skin];
            modelDict[_skin].SetActive(true);
        }

        void UpdateCoin(int _addCoin) {
            m_Txt_coin.text = GameData.Coin.ToString();
        }

        void ShowNotEnough() {
            m_Trans_NotEnough.anchoredPosition = new Vector2(0, -1280);
            m_Trans_NotEnough.DOKill();
            m_Trans_NotEnough.DOAnchorPosY(0, 1).SetEase(Ease.Linear).onComplete=()=> {
                this.AttachTimer(0.3f, () => { m_Trans_NotEnough.anchoredPosition = new Vector2(0, -1280); });                
            };
        }       
    }
}  

