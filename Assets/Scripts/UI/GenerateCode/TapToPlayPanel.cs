using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
namespace MyUI
{
    public partial class TapToPlayPanel : MonoBehaviour ,BasePanel
    {
        public void OnEnter()
        {
            gameObject.SetActive(true);
            m_Txt_Text.transform.DOKill();
            m_Txt_Text.transform.DOScale(1.2f, .5f).SetLoops(-1, LoopType.Yoyo);
        }

        public void OnExit()
        {
            m_Txt_Text.transform.DOKill();
            gameObject.SetActive(false);
        }

        public void OnPause()
        {

        }

        public void OnResume()
        {

        }

        Transform BasePanel.trans { get => transform; }

        private void Awake()
        {
            GetBindComponents(gameObject);
            m_Btn_Play.onClick.AddListener(() => {
                UIPanelManager.Instance.PopPanel();
                ChoicePlayerManager.Instance?.StartGame();
            });
        }

        void Update()
        {

        }
    }
}
