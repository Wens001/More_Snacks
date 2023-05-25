using UnityEngine;
using UnityEngine.UI;

//自动生成于：7/28/2021 5:24:03 PM
namespace MyUI
{

	public partial class ScorePanel
	{

		private RectTransform m_Trans_Bg;
		private RectTransform m_Trans_Stars;
		private Button m_Btn_Later;
		private Button m_Btn_Rate;
		private RectTransform m_Rect_Thank;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Trans_Bg = autoBindTool.GetBindComponent<RectTransform>(0);
			m_Trans_Stars = autoBindTool.GetBindComponent<RectTransform>(1);
			m_Btn_Later = autoBindTool.GetBindComponent<Button>(2);
			m_Btn_Rate = autoBindTool.GetBindComponent<Button>(3);
			m_Rect_Thank = autoBindTool.GetBindComponent<RectTransform>(4);
		}
	}
}
