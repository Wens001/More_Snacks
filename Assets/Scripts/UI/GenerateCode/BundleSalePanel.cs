using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUI
{
    public partial class BundleSalePanel : MonoBehaviour,BasePanel
    {
        public Transform trans => transform;

        public void OnEnter()
        {
            //m_Btn_Lose.gameObject.SetActive(false);
            m_Group_Bg.transform.localScale = Vector3.zero;
            gameObject.SetActive(true);
            m_Group_Bg.transform.DOScale(Vector3.one, ConstDefine.ShowPanelAnimTime).onComplete = () =>
            {
                m_Group_Bg.interactable = true;
            };
            //this.AttachTimer(ConstDefine.DelayShowTime, () => { m_Btn_Lose.gameObject.SetActive(true); });
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
            m_Btn_Buy.onClick.AddListener(() =>
            {               
                AdsManager.BuySuccessEvent = () =>
                {
                    GameManager.sceneType = SceneType.Level;
                    LevelScenes.LoadScene();
                };
                AdsManager.BuyPorduct(ShopProductNames.All_In_One_Bundle_sale);
            });
            m_Btn_Lose.onClick.AddListener(() =>
            {
                GameManager.sceneType = SceneType.Level;
                LevelScenes.LoadScene();
            });
        }
    }
}

