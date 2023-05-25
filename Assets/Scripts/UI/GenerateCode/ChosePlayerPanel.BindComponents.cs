using UnityEngine;
using UnityEngine.UI;

//自动生成于：6/28/2021 10:55:00 AM
namespace MyUI
{

	public partial class ChosePlayerPanel
	{

		private Image m_Img_child;
		private Button m_Btn_child;
		private Image m_Img_snack;
		private Button m_Btn_snack;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Img_child = autoBindTool.GetBindComponent<Image>(0);
			m_Btn_child = autoBindTool.GetBindComponent<Button>(1);
			m_Img_snack = autoBindTool.GetBindComponent<Image>(2);
			m_Btn_snack = autoBindTool.GetBindComponent<Button>(3);
		}
	}
}
