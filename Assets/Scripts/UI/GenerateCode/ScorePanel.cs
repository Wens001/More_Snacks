using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyUI
{
    public partial class ScorePanel : MonoBehaviour, BasePanel
    {
        public Transform trans => transform;
        public Sprite choseStarSp;
        public Sprite noStarSp;
        private int socre = 0;
        private Button[] starBtnArray;
        private Image[] starImgArray;
        public void OnEnter()
        {
            m_Trans_Bg.localScale = Vector3.zero;
            gameObject.SetActive(true);
            m_Trans_Bg.DOKill();
            m_Trans_Bg.DOScale(Vector3.one, ConstDefine.ShowPanelAnimTime);
            UpdateScore(socre);
        }

        public void OnExit()
        {           
            m_Trans_Bg.DOScale(Vector3.zero, ConstDefine.ShowPanelAnimTime).onComplete=()=> {
                m_Trans_Bg.localScale = Vector3.zero;
                m_Rect_Thank.anchoredPosition = new Vector2(0, -1280);
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
            starBtnArray = m_Trans_Stars.GetComponentsInChildren<Button>();
            starImgArray = m_Trans_Stars.GetComponentsInChildren<Image>();
            socre = 0;
            for (int i = 0; i < starBtnArray.Length; i++) {
                int _socre = i + 1;
                starBtnArray[i].onClick.AddListener(()=> {
                    UpdateScore(_socre);
                    socre = _socre;
                });
            }

            m_Btn_Rate.onClick.AddListener(OnRateBtn);
            m_Btn_Later.onClick.AddListener(HidePanel);
        }

        void UpdateScore(int _socre) {
            for (int i = 0; i < starBtnArray.Length; i++)
            {
                if (i < _socre)
                {
                    starImgArray[i].sprite = choseStarSp;
                }
                else {
                    starImgArray[i].sprite = noStarSp;
                }
            }
            if (_socre <= 0)
            {
                m_Btn_Rate.gameObject.SetActive(false);
            }
            else {
                m_Btn_Rate.gameObject.SetActive(true);
            }
        }

        void OnRateBtn() {
            if (socre >= 4)
            {
#if UNITY_ANDROID || UNITY_EDITOR
                Application.OpenURL("https://play.google.com/store/apps/details?id=com.casualgames.snack");
#endif
                HidePanel();
            }
            else {
                m_Rect_Thank.DOAnchorPosY(0, 0.5f).onComplete = () =>
                {
                    this.AttachTimer(1, HidePanel);
                };
            }
        }

        void HidePanel() {
            UIPanelManager.Instance.PopPanel();
        }

    }
}
