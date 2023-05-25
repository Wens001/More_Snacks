using DG.Tweening;
using GameAnalyticsSDK.Setup;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUI
{
    public partial class StorePanel : MonoBehaviour, BasePanel
    {
        public Transform trans => transform;

        public void OnEnter()
        {
            if (GameData.NoAds) {
                m_Img_NoAds.gameObject.SetActive(false);
            }
            m_Group_Bg.transform.localScale = Vector3.zero;
            gameObject.SetActive(true);
            m_Group_Bg.transform.DOScale(Vector3.one, ConstDefine.ShowPanelAnimTime).onComplete=()=> {
                m_Group_Bg.interactable = true;
            };
        }

        public void OnExit()
        {
            m_Group_Bg.interactable = false;
            m_Group_Bg.transform.DOScale(Vector3.zero, ConstDefine.ShowPanelAnimTime).onComplete = () =>
            {
                m_Group_Bg.transform.localScale = Vector3.zero;
                gameObject.SetActive(false);
            };
        }

        public void OnPause()
        {
            
        }

        public void OnResume()
        {
            
        }

        void Awake() {
            GetBindComponents(gameObject);
            m_Btn_ALLINONE.onClick.AddListener(()=> {             
                AdsManager.BuySuccessEvent = () =>
                {
                    UIPanelManager.Instance.PopPanel();
                };
                Debug.Log("onclick m_Btn_ALLINONE");
                AdsManager.BuyPorduct(ShopProductNames.All_In_One_Bundle);
            });
            m_Btn_NoAds.onClick.AddListener(() => {               
                AdsManager.BuySuccessEvent = () =>
                {
                    UIPanelManager.Instance.PopPanel();
                };
                Debug.Log("onclick m_Btn_NoAds");
                AdsManager.BuyPorduct(ShopProductNames.No_Ads);
            });
            m_Btn_StoreBack.onClick.AddListener(() => {               
                UIPanelManager.Instance.PopPanel();
            });
        }
    }
}