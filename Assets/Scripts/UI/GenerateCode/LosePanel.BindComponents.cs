using UnityEngine;
using UnityEngine.UI;

//自动生成于：7/12/2021 8:39:40 PM
namespace MyUI
{

	public partial class LosePanel
	{

		private Image m_Img_lose;
		private Button m_Btn_RePlay;
		private RawImage m_RImg_Prop;
		private Image m_Img_Countdown;
		private Image m_Img_white;
		private Text m_Txt_Countdown;
		private Button m_Btn_Revive;
		private Button m_Btn_no;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Img_lose = autoBindTool.GetBindComponent<Image>(0);
			m_Btn_RePlay = autoBindTool.GetBindComponent<Button>(1);
			m_RImg_Prop = autoBindTool.GetBindComponent<RawImage>(2);
			m_Img_Countdown = autoBindTool.GetBindComponent<Image>(3);
			m_Img_white = autoBindTool.GetBindComponent<Image>(4);
			m_Txt_Countdown = autoBindTool.GetBindComponent<Text>(5);
			m_Btn_Revive = autoBindTool.GetBindComponent<Button>(6);
			m_Btn_no = autoBindTool.GetBindComponent<Button>(7);
		}
	}
}
