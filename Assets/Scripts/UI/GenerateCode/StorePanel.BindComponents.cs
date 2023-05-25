using UnityEngine;
using UnityEngine.UI;

//自动生成于：7/30/2021 12:02:36 PM
namespace MyUI
{

	public partial class StorePanel
	{

		private CanvasGroup m_Group_Bg;
		private Image m_Img_ALLINONE;
		private Button m_Btn_ALLINONE;
		private Image m_Img_NoAds;
		private Button m_Btn_NoAds;
		private Button m_Btn_StoreBack;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Group_Bg = autoBindTool.GetBindComponent<CanvasGroup>(0);
			m_Img_ALLINONE = autoBindTool.GetBindComponent<Image>(1);
			m_Btn_ALLINONE = autoBindTool.GetBindComponent<Button>(2);
			m_Img_NoAds = autoBindTool.GetBindComponent<Image>(3);
			m_Btn_NoAds = autoBindTool.GetBindComponent<Button>(4);
			m_Btn_StoreBack = autoBindTool.GetBindComponent<Button>(5);
		}
	}
}
