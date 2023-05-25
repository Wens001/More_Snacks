using UnityEngine;
using UnityEngine.UI;

//自动生成于：5/18/2021 6:45:23 PM
namespace MyUI
{

	public partial class SkinPanel
	{

		private Text m_Txt_coin;
		private Button m_Btn_Return;
		private Button m_Btn_kid;
		private Button m_Btn_snack;
		private ScrollRect m_SRect_Kid;
		private ScrollRect m_SRect_Snack;
		private RectTransform m_Trans_NotEnough;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Txt_coin = autoBindTool.GetBindComponent<Text>(0);
			m_Btn_Return = autoBindTool.GetBindComponent<Button>(1);
			m_Btn_kid = autoBindTool.GetBindComponent<Button>(2);
			m_Btn_snack = autoBindTool.GetBindComponent<Button>(3);
			m_SRect_Kid = autoBindTool.GetBindComponent<ScrollRect>(4);
			m_SRect_Snack = autoBindTool.GetBindComponent<ScrollRect>(5);
			m_Trans_NotEnough = autoBindTool.GetBindComponent<RectTransform>(6);
		}
	}
}
