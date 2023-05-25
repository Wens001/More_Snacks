using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
namespace MyUI {
	public partial class GuidePanel : MonoBehaviour, BasePanel
	{
		public Transform trans => transform;

		public void SetGuideString(int dataIndex)
        {
			HideAllString();

			if (dataIndex < 0 || dataIndex >= AllTextList.Count)
            {
				Debug.LogError("Out of AllTextList Range");
				return;
            }
			AllTextList[dataIndex].SetActive(true);
		}

		public void HideAllString()
        {
			for (int i = 0; i < AllTextList.Count; i++)
				AllTextList[i].SetActive(false);
		}

		public void OnEnter()
		{
			gameObject.SetActive(true);
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

		private List<GameObject> AllTextList;

		void Awake()
		{
			GetBindComponents(gameObject);
            AllTextList = new List<GameObject>
            {
                m_Img_CatchTheSnack.gameObject,
                m_Img_DontBeCaught.gameObject,
                m_Img_EscapeNow.gameObject,
                m_Img_HideUnderTheTable.gameObject,
                m_Img_KickOverTable.gameObject,
				m_Img_EscapeToTheExit.gameObject
            };

            for (int i = 0; i < AllTextList.Count; i++)
				AllTextList[i].transform.DOScale(.8f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
			HideAllString();
		}

    }
}