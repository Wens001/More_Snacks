using UnityEngine;
using UnityEngine.UI;

//自动生成于：7/30/2021 3:21:39 PM
namespace MyUI
{

	public partial class SettingPanel
	{

		private Button m_Btn_Exit;
		private Toggle m_Tog_Sound;
		private Toggle m_Tog_Vibration;
		private Button m_Btn_Privacy;
		private Button m_Btn_Restore;
		private Text m_Txt_Connect;
		private Button m_Btn_Twitter;
		private Button m_Btn_Instagram;
		private Button m_Btn_Youtube;
		private Button m_Btn_Facebook;
		private Button m_Btn_Lion;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Btn_Exit = autoBindTool.GetBindComponent<Button>(0);
			m_Tog_Sound = autoBindTool.GetBindComponent<Toggle>(1);
			m_Tog_Vibration = autoBindTool.GetBindComponent<Toggle>(2);
			m_Btn_Privacy = autoBindTool.GetBindComponent<Button>(3);
			m_Btn_Restore = autoBindTool.GetBindComponent<Button>(4);
			m_Txt_Connect = autoBindTool.GetBindComponent<Text>(5);
			m_Btn_Twitter = autoBindTool.GetBindComponent<Button>(6);
			m_Btn_Instagram = autoBindTool.GetBindComponent<Button>(7);
			m_Btn_Youtube = autoBindTool.GetBindComponent<Button>(8);
			m_Btn_Facebook = autoBindTool.GetBindComponent<Button>(9);
			m_Btn_Lion = autoBindTool.GetBindComponent<Button>(10);
		}
	}
}
