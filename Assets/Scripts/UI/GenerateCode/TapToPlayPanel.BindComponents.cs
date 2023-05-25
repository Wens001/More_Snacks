using UnityEngine;
using UnityEngine.UI;

//自动生成于：2021/1/14 11:37:07
namespace MyUI
{

	public partial class TapToPlayPanel
	{

		private Text m_Txt_Text;
		private Button m_Btn_Play;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Txt_Text = autoBindTool.GetBindComponent<Text>(0);
			m_Btn_Play = autoBindTool.GetBindComponent<Button>(1);
		}
	}
}
