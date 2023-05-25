using UnityEngine;
using UnityEngine.UI;

//自动生成于：6/15/2021 3:02:44 PM
namespace MyUI
{

	public partial class TestGA
	{

		private Button m_Btn_Start;
		private Button m_Btn_Complete;
		private Button m_Btn_Fail;
		private Button m_Btn_ReStart;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Btn_Start = autoBindTool.GetBindComponent<Button>(0);
			m_Btn_Complete = autoBindTool.GetBindComponent<Button>(1);
			m_Btn_Fail = autoBindTool.GetBindComponent<Button>(2);
			m_Btn_ReStart = autoBindTool.GetBindComponent<Button>(3);
		}
	}
}
