using UnityEngine;
using UnityEngine.UI;

//自动生成于：2021/4/2 11:31:19
namespace MyUI
{

	public partial class GuidePanel
	{

		private Image m_Img_CatchTheSnack;
		private Image m_Img_DontBeCaught;
		private Image m_Img_EscapeNow;
		private Image m_Img_HideUnderTheTable;
		private Image m_Img_KickOverTable;
		private Image m_Img_EscapeToTheExit;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Img_CatchTheSnack = autoBindTool.GetBindComponent<Image>(0);
			m_Img_DontBeCaught = autoBindTool.GetBindComponent<Image>(1);
			m_Img_EscapeNow = autoBindTool.GetBindComponent<Image>(2);
			m_Img_HideUnderTheTable = autoBindTool.GetBindComponent<Image>(3);
			m_Img_KickOverTable = autoBindTool.GetBindComponent<Image>(4);
			m_Img_EscapeToTheExit = autoBindTool.GetBindComponent<Image>(5);
		}
	}
}
