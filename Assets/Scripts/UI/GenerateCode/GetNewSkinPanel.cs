using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUI {
    public partial class GetNewSkinPanel : MonoBehaviour, BasePanel
    {
        Transform BasePanel.trans => transform;
        public int luckySkin;
        Canvas canvas;
        List<int> curHasNoSkinList;
        public GameObject lightPrefab;
        private int curSkinType;
        private MyTimer checkNet = new MyTimer(1);

        void BasePanel.OnEnter()
        {
            GameManager.canRunGame = false;
            if (canvas == null) {
                TryGetComponent(out canvas);
            }
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 5;
            curSkinType = SkinManager.Instance.nextSkinType;
            //curHasNoSkinList = SkinManager.Instance.GetHasNoSkinList(curSkinType);
            //luckySkin = GetNewSkinId();
            luckySkin = SkinManager.Instance.GetNewSkinId();
            gameObject.SetActive(true);
            //Time.timeScale = 0;
            if (luckySkin > -1) {
                ShowNewModel();
            }
            //延迟2秒出现按钮
            Timer.Register(ConstDefine.DelayShowTime, () =>
            {
                m_Btn_lose.gameObject.SetActive(true);
            });
            Instantiate(lightPrefab);
        }

        void BasePanel.OnExit()
        {
            m_Btn_lose.gameObject.SetActive(false);
            gameObject.SetActive(false);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Time.timeScale = 1;
            GameManager.canRunGame = true;
        }

        void BasePanel.OnPause()
        {
            
        }

        void BasePanel.OnResume()
        {
            
        }

        // Start is called before the first frame update
        void Awake() {
            GetBindComponents(gameObject);
            m_Btn_get.onClick.AddListener(() =>
            {
                AdsManager.RewardAction = () =>
                {
                    SkinManager.Instance.SaveNewSkin((ChildOrSnack)curSkinType, luckySkin,true);
                    Messenger.Broadcast(ConstDefine.Listener.GetNewSkin, SkinManager.Instance.GetAllSkinInfo(curSkinType)[luckySkin]);
                    //GameData.SetHasSkinId((ChildOrSnack)SkinManager.Instance.nextSkinType, luckySkin, 1);
                    //GameData.SetChoseSkinId((ChildOrSnack)SkinManager.Instance.nextSkinType, luckySkin);
                    //curHasNoSkinList.Remove(luckySkin);
                    SkinManager.Instance.UpdateNextSkinType();
                    UIPanelManager.Instance.PopPanel();
                    LevelScenes.LoadScene();
                };
                AdsManager.ShowRewardedAd("Level_Unlock_Skin");
            });
            m_Btn_lose.onClick.AddListener(() =>
            {
                Messenger.Broadcast(ConstDefine.Listener.LoseNewSkin);
                UIPanelManager.Instance.PopPanel();
                LevelScenes.LoadScene();
            });
        }

        //随机皮肤
        int GetNewSkinId() {
            if (curSkinType == -1)
            {
                return -1;
            }
            else {
                if (curHasNoSkinList.Count > 0) {
                    int newSkinId = curHasNoSkinList[Random.Range(0, curHasNoSkinList.Count)];
                    //Debug.LogWarning("new skin id:"+ newSkinId);
                    return newSkinId;
                }                   
                else{
                    Debug.LogWarning((ChildOrSnack)curSkinType + " 皮肤不足");
                    return -1;
                }
            }
        }

        void ShowNewModel() {
            NewModel();
        }

        GameObject NewModel() {
            GameObject model;
            if (curSkinType == (int)ChildOrSnack.Child)
            {
                model = SkinManager.Instance.GetChildModel(luckySkin);
                model.transform.position = Vector3.zero;
            }
            else {
                model = SkinManager.Instance.GetSnackModel(luckySkin);
                model.transform.position = new Vector3(0,0.4f,0);
            }
            Transform tra = model.transform;
            //tra.position = Vector3.zero;
            tra.localEulerAngles = Vector3.zero;
            tra.localScale = Vector3.one;
            return model;
        }

        void Update() {
            if (checkNet.IsFinish)
            {
                checkNet.ReStart();
                m_Btn_get.interactable = AdsManager.IsAdReady;
            }
            else {
                checkNet.OnUpdate(Time.unscaledDeltaTime);
            }
        }
    }
}

