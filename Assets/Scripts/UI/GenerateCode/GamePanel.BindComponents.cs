using UnityEngine;
using UnityEngine.UI;

//自动生成于：8/18/2021 10:57:05 AM
namespace MyUI
{

	public partial class GamePanel
	{

		private Image m_Img_image;
		private Image m_Img_timeView;
		private Text m_Txt_timeValue;
		private HorizontalLayoutGroup m_HGroup_PlayerGroup;
		private Text m_Txt_TimeRunOut;
		private Button m_Btn_AddTime;
		private RectTransform m_Rect_DownGroup;
		private Image m_Img_RunAway;
		private Image m_Img_EatThem;
		private Image m_Img_EatMoreSnacks;
		private Button m_Btn_Play;
		private RectTransform m_Rect_Test;
		private Text m_Txt_TestValue;
		private CanvasGroup m_Group_ChosePlayer;
		private Image m_Img_child;
		private Button m_Btn_child;
		private Image m_Img_snack;
		private Button m_Btn_snack;
		private Image m_Img_ChoseSide;
		private CanvasGroup m_Group_ChoseSnack;
		private Image m_Img_FreeSnack;
		private Button m_Btn_FreeSnack;
		private Image m_Img_MoneySnack;
		private Button m_Btn_MoneySnack;
		private Image m_Img_AdSnack;
		private Button m_Btn_AdSnack;
		private Button m_Btn_CoinImage;
		private Text m_Txt_CoinValue;
		private RectTransform m_Trans_Keys;
		private Button m_Btn_Skin;
		private Button m_Btn_Setting;
		private RectTransform m_Trans_UsePropPanel;
		private RawImage m_RImg_PropDemo;
		private Button m_Btn_StartWith;
		private Image m_Img_PropIcon;
		private Button m_Btn_LoseProp;
		private Button m_Btn_NoAds;
		private Button m_Btn_Store;
		private RectTransform m_Trans_Process;
		private HorizontalLayoutGroup m_HGroup_Process;
		private RectTransform m_Trans_choseArrow;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Img_image = autoBindTool.GetBindComponent<Image>(0);
			m_Img_timeView = autoBindTool.GetBindComponent<Image>(1);
			m_Txt_timeValue = autoBindTool.GetBindComponent<Text>(2);
			m_HGroup_PlayerGroup = autoBindTool.GetBindComponent<HorizontalLayoutGroup>(3);
			m_Txt_TimeRunOut = autoBindTool.GetBindComponent<Text>(4);
			m_Btn_AddTime = autoBindTool.GetBindComponent<Button>(5);
			m_Rect_DownGroup = autoBindTool.GetBindComponent<RectTransform>(6);
			m_Img_RunAway = autoBindTool.GetBindComponent<Image>(7);
			m_Img_EatThem = autoBindTool.GetBindComponent<Image>(8);
			m_Img_EatMoreSnacks = autoBindTool.GetBindComponent<Image>(9);
			m_Btn_Play = autoBindTool.GetBindComponent<Button>(10);
			m_Rect_Test = autoBindTool.GetBindComponent<RectTransform>(11);
			m_Txt_TestValue = autoBindTool.GetBindComponent<Text>(12);
			m_Group_ChosePlayer = autoBindTool.GetBindComponent<CanvasGroup>(13);
			m_Img_child = autoBindTool.GetBindComponent<Image>(14);
			m_Btn_child = autoBindTool.GetBindComponent<Button>(15);
			m_Img_snack = autoBindTool.GetBindComponent<Image>(16);
			m_Btn_snack = autoBindTool.GetBindComponent<Button>(17);
			m_Img_ChoseSide = autoBindTool.GetBindComponent<Image>(18);
			m_Group_ChoseSnack = autoBindTool.GetBindComponent<CanvasGroup>(19);
			m_Img_FreeSnack = autoBindTool.GetBindComponent<Image>(20);
			m_Btn_FreeSnack = autoBindTool.GetBindComponent<Button>(21);
			m_Img_MoneySnack = autoBindTool.GetBindComponent<Image>(22);
			m_Btn_MoneySnack = autoBindTool.GetBindComponent<Button>(23);
			m_Img_AdSnack = autoBindTool.GetBindComponent<Image>(24);
			m_Btn_AdSnack = autoBindTool.GetBindComponent<Button>(25);
			m_Btn_CoinImage = autoBindTool.GetBindComponent<Button>(26);
			m_Txt_CoinValue = autoBindTool.GetBindComponent<Text>(27);
			m_Trans_Keys = autoBindTool.GetBindComponent<RectTransform>(28);
			m_Btn_Skin = autoBindTool.GetBindComponent<Button>(29);
			m_Btn_Setting = autoBindTool.GetBindComponent<Button>(30);
			m_Trans_UsePropPanel = autoBindTool.GetBindComponent<RectTransform>(31);
			m_RImg_PropDemo = autoBindTool.GetBindComponent<RawImage>(32);
			m_Btn_StartWith = autoBindTool.GetBindComponent<Button>(33);
			m_Img_PropIcon = autoBindTool.GetBindComponent<Image>(34);
			m_Btn_LoseProp = autoBindTool.GetBindComponent<Button>(35);
			m_Btn_NoAds = autoBindTool.GetBindComponent<Button>(36);
			m_Btn_Store = autoBindTool.GetBindComponent<Button>(37);
			m_Trans_Process = autoBindTool.GetBindComponent<RectTransform>(38);
			m_HGroup_Process = autoBindTool.GetBindComponent<HorizontalLayoutGroup>(39);
			m_Trans_choseArrow = autoBindTool.GetBindComponent<RectTransform>(40);
		}
	}
}
