using UnityEngine;
using UnityEngine.UI;

//自动生成于：8/20/2021 3:12:53 PM
namespace MyUI
{

	public partial class CoinLvPanel
	{

		private RectTransform m_Trans_JoinCoinLevel;
		private Button m_Btn_Join;
		private Button m_Btn_NoJoin;
		private RawImage m_RImg_Demo;
		private RectTransform m_Trans_CoinLevel;
		private Text m_Txt_GetCoinPrefab;
		private Button m_Btn_StartGame;
		private RectTransform m_Rect_DownGroup;
		private Image m_Img_RunAway;
		private Image m_Img_EatThem;
		private Image m_Img_EatMoreSnacks;
		private RectTransform m_Trans_Win;
		private Image m_Img_Coin;
		private Text m_Txt_Reward;
		private Button m_Btn_GetIt;
		private Button m_Btn_Restart;
		private Button m_Btn_CoinImage;
		private Text m_Txt_CoinValue;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Trans_JoinCoinLevel = autoBindTool.GetBindComponent<RectTransform>(0);
			m_Btn_Join = autoBindTool.GetBindComponent<Button>(1);
			m_Btn_NoJoin = autoBindTool.GetBindComponent<Button>(2);
			m_RImg_Demo = autoBindTool.GetBindComponent<RawImage>(3);
			m_Trans_CoinLevel = autoBindTool.GetBindComponent<RectTransform>(4);
			m_Txt_GetCoinPrefab = autoBindTool.GetBindComponent<Text>(5);
			m_Btn_StartGame = autoBindTool.GetBindComponent<Button>(6);
			m_Rect_DownGroup = autoBindTool.GetBindComponent<RectTransform>(7);
			m_Img_RunAway = autoBindTool.GetBindComponent<Image>(8);
			m_Img_EatThem = autoBindTool.GetBindComponent<Image>(9);
			m_Img_EatMoreSnacks = autoBindTool.GetBindComponent<Image>(10);
			m_Trans_Win = autoBindTool.GetBindComponent<RectTransform>(11);
			m_Img_Coin = autoBindTool.GetBindComponent<Image>(12);
			m_Txt_Reward = autoBindTool.GetBindComponent<Text>(13);
			m_Btn_GetIt = autoBindTool.GetBindComponent<Button>(14);
			m_Btn_Restart = autoBindTool.GetBindComponent<Button>(15);
			m_Btn_CoinImage = autoBindTool.GetBindComponent<Button>(16);
			m_Txt_CoinValue = autoBindTool.GetBindComponent<Text>(17);
		}
	}
}
