using UnityEngine;
using UnityEngine.UI;

//自动生成于：5/18/2021 7:00:46 PM
namespace MyUI
{

	public partial class GetNewPropPanel
	{

		private Image m_Img_PropName;
		private Button m_Btn_get;
		private Button m_Btn_lose;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Img_PropName = autoBindTool.GetBindComponent<Image>(0);
			m_Btn_get = autoBindTool.GetBindComponent<Button>(1);
			m_Btn_lose = autoBindTool.GetBindComponent<Button>(2);
		}
	}
}
