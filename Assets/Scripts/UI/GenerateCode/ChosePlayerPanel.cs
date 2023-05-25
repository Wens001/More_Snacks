using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLB;

namespace MyUI
{
    public partial class ChosePlayerPanel : MonoBehaviour, BasePanel
    {
        public Transform trans => transform;

        void Awake() {
            GetBindComponents(gameObject);
            m_Btn_child.onClick.AddListener(() => { ChosePlayer(ChildOrSnack.Child);  });
            m_Btn_snack.onClick.AddListener(() => { ChosePlayer(ChildOrSnack.Snack); });
            m_Btn_child.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), .8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
            m_Btn_snack.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), .8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
        }

        void ChosePlayer(ChildOrSnack _type) {
            if (!GameManager.canRunGame)
                return;
            //SetLocalPlayer.Instance.SetPlayer(_type);
            UIPanelManager.Instance.PopPanel();
        }

        public void OnEnter()
        {
            gameObject.SetActive(true);
            m_Img_child.sprite = SkinManager.Instance.allChildSkinInfo[GameData.GetChoseSkinId(ChildOrSnack.Child)].Icon;
            m_Img_snack.sprite = SkinManager.Instance.allSnackSkinInfo[GameData.GetChoseSkinId(ChildOrSnack.Snack)].Icon;
            
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
    }
}
