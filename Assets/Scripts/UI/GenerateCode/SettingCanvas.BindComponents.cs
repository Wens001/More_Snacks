using UnityEngine;
using UnityEngine.UI;

//自动生成于：8/3/2021 4:14:32 PM
namespace MyUI
{

	public partial class SettingCanvas
	{

		private Button m_Btn_Close;
		private InputField m_Input_Lv;
		private Button m_Btn_ALevel;
		private Button m_Btn_BLevel;
		private Text m_Txt_Level;
		private InputField m_Input_Coin;
		private Button m_Btn_ACoin;
		private Button m_Btn_BCoin;
		private RectTransform m_Trans_ABTest;
		private Button m_Btn_A;
		private Button m_Btn_B;
		private Button m_Btn_C;
		private Button m_Btn_D;
		private Button m_Btn_Clear;
		private Button m_Btn_Console;
		private Button m_Btn_ShowAd;
		private Button m_Btn_ShowAd2;
		private Text m_Txt_banner;
		private Button m_Btn_TestBanner;
		private Text m_Txt_TestBanner;
		private Button m_Btn_InitFb;
		private Button m_Btn_FbIsLoggedIn;
		private Button m_Btn_FbIsInit;
		private Text m_Txt_firebaseInfo;
		private Text m_Txt_idfa;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Btn_Close = autoBindTool.GetBindComponent<Button>(0);
			m_Input_Lv = autoBindTool.GetBindComponent<InputField>(1);
			m_Btn_ALevel = autoBindTool.GetBindComponent<Button>(2);
			m_Btn_BLevel = autoBindTool.GetBindComponent<Button>(3);
			m_Txt_Level = autoBindTool.GetBindComponent<Text>(4);
			m_Input_Coin = autoBindTool.GetBindComponent<InputField>(5);
			m_Btn_ACoin = autoBindTool.GetBindComponent<Button>(6);
			m_Btn_BCoin = autoBindTool.GetBindComponent<Button>(7);
			m_Trans_ABTest = autoBindTool.GetBindComponent<RectTransform>(8);
			m_Btn_A = autoBindTool.GetBindComponent<Button>(9);
			m_Btn_B = autoBindTool.GetBindComponent<Button>(10);
			m_Btn_C = autoBindTool.GetBindComponent<Button>(11);
			m_Btn_D = autoBindTool.GetBindComponent<Button>(12);
			m_Btn_Clear = autoBindTool.GetBindComponent<Button>(13);
			m_Btn_Console = autoBindTool.GetBindComponent<Button>(14);
			m_Btn_ShowAd = autoBindTool.GetBindComponent<Button>(15);
			m_Btn_ShowAd2 = autoBindTool.GetBindComponent<Button>(16);
			m_Txt_banner = autoBindTool.GetBindComponent<Text>(17);
			m_Btn_TestBanner = autoBindTool.GetBindComponent<Button>(18);
			m_Txt_TestBanner = autoBindTool.GetBindComponent<Text>(19);
			m_Btn_InitFb = autoBindTool.GetBindComponent<Button>(20);
			m_Btn_FbIsLoggedIn = autoBindTool.GetBindComponent<Button>(21);
			m_Btn_FbIsInit = autoBindTool.GetBindComponent<Button>(22);
			m_Txt_firebaseInfo = autoBindTool.GetBindComponent<Text>(23);
			m_Txt_idfa = autoBindTool.GetBindComponent<Text>(24);
		}
	}
}
