using UnityEngine;
using UnityEngine.UI;

//自动生成于：7/28/2021 4:25:08 PM
namespace MyUI
{

	public partial class WinPanel
	{

		private Button m_Btn_CoinImage;
		private Text m_Txt_CoinValue;
		private Image m_Img_youwin;
		private Button m_Btn_RePlay;
		private Button m_Btn_Next;
		private Button m_Btn_Ads;
		private Text m_Txt_Money;
		private Image m_Img_Black;
		private Image m_Img_Jianying;
		private Text m_Txt_SkinProgress;
		private Image m_Img_NewSkin;
		private Image m_Img_needle;
		private RectTransform m_Trans_PrizePanel;
		private RawImage m_RImg_skin;
		private CanvasGroup m_Group_ManyMoney;
		private RectTransform m_Trans_Boxs;
		private RectTransform m_Trans_Keys;
		private Button m_Btn_AddKey;
		private Button m_Btn_NoThank;
		private Image m_Img_CrossPromo;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Btn_CoinImage = autoBindTool.GetBindComponent<Button>(0);
			m_Txt_CoinValue = autoBindTool.GetBindComponent<Text>(1);
			m_Img_youwin = autoBindTool.GetBindComponent<Image>(2);
			m_Btn_RePlay = autoBindTool.GetBindComponent<Button>(3);
			m_Btn_Next = autoBindTool.GetBindComponent<Button>(4);
			m_Btn_Ads = autoBindTool.GetBindComponent<Button>(5);
			m_Txt_Money = autoBindTool.GetBindComponent<Text>(6);
			m_Img_Black = autoBindTool.GetBindComponent<Image>(7);
			m_Img_Jianying = autoBindTool.GetBindComponent<Image>(8);
			m_Txt_SkinProgress = autoBindTool.GetBindComponent<Text>(9);
			m_Img_NewSkin = autoBindTool.GetBindComponent<Image>(10);
			m_Img_needle = autoBindTool.GetBindComponent<Image>(11);
			m_Trans_PrizePanel = autoBindTool.GetBindComponent<RectTransform>(12);
			m_RImg_skin = autoBindTool.GetBindComponent<RawImage>(13);
			m_Group_ManyMoney = autoBindTool.GetBindComponent<CanvasGroup>(14);
			m_Trans_Boxs = autoBindTool.GetBindComponent<RectTransform>(15);
			m_Trans_Keys = autoBindTool.GetBindComponent<RectTransform>(16);
			m_Btn_AddKey = autoBindTool.GetBindComponent<Button>(17);
			m_Btn_NoThank = autoBindTool.GetBindComponent<Button>(18);
			m_Img_CrossPromo = autoBindTool.GetBindComponent<Image>(19);
		}
	}
}
