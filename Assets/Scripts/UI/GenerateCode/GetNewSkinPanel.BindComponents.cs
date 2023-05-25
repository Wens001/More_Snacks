using UnityEngine;
using UnityEngine.UI;

//自动生成于：5/6/2021 3:31:06 PM
namespace MyUI
{

	public partial class GetNewSkinPanel
	{

		private Button m_Btn_get;
		private Button m_Btn_lose;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Btn_get = autoBindTool.GetBindComponent<Button>(0);
			m_Btn_lose = autoBindTool.GetBindComponent<Button>(1);
		}
	}
}
