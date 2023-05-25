using UnityEngine;
using UnityEngine.UI;

//自动生成于：8/18/2021 2:39:55 PM
namespace MyUI
{

	public partial class ChallengePanel
	{

		private RectTransform m_Trans_JoinChallenge;
		private Image m_Img_scene;
		private Text m_Txt_challengeReward;
		private Button m_Btn_JoinChallenge;
		private Button m_Btn_NoChallenge;
		private RectTransform m_Trans_NotEnough;
		private RectTransform m_Trans_WinChallenge;
		private Text m_Txt_reward;
		private Button m_Btn_GetIt;
		private Button m_Btn_GetDouble;
		private RectTransform m_Trans_EndChallenge;
		private Button m_Btn_NoThank;
		private Button m_Btn_Restart;
		private Button m_Btn_CoinImage;
		private Text m_Txt_CoinValue;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Trans_JoinChallenge = autoBindTool.GetBindComponent<RectTransform>(0);
			m_Img_scene = autoBindTool.GetBindComponent<Image>(1);
			m_Txt_challengeReward = autoBindTool.GetBindComponent<Text>(2);
			m_Btn_JoinChallenge = autoBindTool.GetBindComponent<Button>(3);
			m_Btn_NoChallenge = autoBindTool.GetBindComponent<Button>(4);
			m_Trans_NotEnough = autoBindTool.GetBindComponent<RectTransform>(5);
			m_Trans_WinChallenge = autoBindTool.GetBindComponent<RectTransform>(6);
			m_Txt_reward = autoBindTool.GetBindComponent<Text>(7);
			m_Btn_GetIt = autoBindTool.GetBindComponent<Button>(8);
			m_Btn_GetDouble = autoBindTool.GetBindComponent<Button>(9);
			m_Trans_EndChallenge = autoBindTool.GetBindComponent<RectTransform>(10);
			m_Btn_NoThank = autoBindTool.GetBindComponent<Button>(11);
			m_Btn_Restart = autoBindTool.GetBindComponent<Button>(12);
			m_Btn_CoinImage = autoBindTool.GetBindComponent<Button>(13);
			m_Txt_CoinValue = autoBindTool.GetBindComponent<Text>(14);
		}
	}
}
