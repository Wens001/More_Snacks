using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLB_Samples;

namespace MyUI {
    public partial class GetNewPropPanel : MonoBehaviour, BasePanel
    {
        public Transform trans => transform;
        public GameObject hammerPrefab;
        public GameObject pepperPrefab;
        public Sprite hammerSp;
        public Sprite pepperSp;
        private GameObject showProp;
        private Transform propTra;
        public float rotateSp = 100;
        public Space space;
        private Canvas canvas;
        public GameObject lightPrefab;
        private GameObject highLight;
        private MyTimer checkNet;

        public void OnEnter()
        {
            GameManager.canRunGame = false;
            if (canvas == null)
            {
                TryGetComponent(out canvas);
            }
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 5;
            gameObject.SetActive(true);
            //延迟2秒出现按钮
            this.AttachTimer(ConstDefine.DelayShowTime, () =>
            {
                m_Btn_lose.gameObject.SetActive(true);
            });
            if (GameData.Level == 7)
            {
                showProp = Instantiate(pepperPrefab);
                propTra = showProp.transform;
                propTra.position = new Vector3(0, 1, 1.85f);
                propTra.localEulerAngles = new Vector3(0, 0, 30);
                propTra.localScale = Vector3.one;
                space = Space.World;
                m_Img_PropName.sprite = pepperSp;
            }
            else if(GameData.Level == 6)
            { 
                showProp = Instantiate(hammerPrefab);
                propTra = showProp.transform;
                propTra.position = new Vector3(0.14f, 0.56f, 1.5f);
                propTra.localEulerAngles = new Vector3(0, 0, 30);
                propTra.localScale = Vector3.one;
                space = Space.Self;
                m_Img_PropName.sprite = hammerSp;
            }
            m_Img_PropName.SetNativeSize();
            propTra.GetComponent<Rotater>().enabled = false;
            showProp.GetComponent<TriggerBuff>().NoDestroy();
            propTra.Find("Circle_white").gameObject.SetActive(false);
            highLight = Instantiate(lightPrefab);
        }

        public void OnExit()
        {
            m_Btn_lose.gameObject.SetActive(false);           
            gameObject.SetActive(false);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Time.timeScale = 1;
            GameManager.canRunGame = true;
        }

        public void OnPause()
        {

        }

        public void OnResume()
        {

        }
        void Awake() {
            GetBindComponents(gameObject);
            m_Btn_get.onClick.AddListener(() =>
            {
                AdsManager.RewardAction = () =>
                {
                    UIPanelManager.Instance.PopPanel();
                    //GameData.SkinProgress 为 0说明跳转进来前皮肤已满
                    if (GameData.SkinProgress == 0) {
                        GetNewSkin();
                    }
                    else
                        LevelScenes.LoadScene();
                };
                AdsManager.ShowRewardedAd("Get_item");
            });
            m_Btn_lose.onClick.AddListener(() =>
            {
                UIPanelManager.Instance.PopPanel();
                if (GameData.SkinProgress == 0)
                {
                    GetNewSkin();
                }
                else
                    LevelScenes.LoadScene();
            });
            checkNet = new MyTimer(1);
        }

        void GetNewSkin() {
            //DestroyImmediate(showProp);
            Destroy(showProp);
            Destroy(highLight);
            GameManager.sceneType = SceneType.GetNewSkin;
            UIPanelManager.Instance.PushPanel(UIPanelType.GetNewSkinPanel);
        }

        void Update() {
            if(propTra)
                propTra.Rotate(Vector3.up * rotateSp * Time.unscaledDeltaTime, space);
            if (checkNet.IsFinish)
            {
                m_Btn_get.interactable = AdsManager.IsAdReady;
                checkNet.ReStart();
            }
            else {
                checkNet.OnUpdate(Time.unscaledDeltaTime);
            }
        }

    }

}
