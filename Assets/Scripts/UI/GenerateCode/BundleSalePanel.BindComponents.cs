using UnityEngine;
using UnityEngine.UI;

//自动生成于：7/30/2021 11:56:47 AM
namespace MyUI
{

	public partial class BundleSalePanel
	{

		private CanvasGroup m_Group_Bg;
		private Button m_Btn_Buy;
		private Text m_Txt_Prize;
		private Button m_Btn_Lose;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Group_Bg = autoBindTool.GetBindComponent<CanvasGroup>(0);
			m_Btn_Buy = autoBindTool.GetBindComponent<Button>(1);
			m_Txt_Prize = autoBindTool.GetBindComponent<Text>(2);
			m_Btn_Lose = autoBindTool.GetBindComponent<Button>(3);
		}
	}
}
