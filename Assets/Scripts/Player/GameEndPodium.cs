using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GameEndPodium : MonoBehaviour
{

    public static GameEndPodium Instance;

    public Transform player; 
    public Transform ai;

    public Text PlayerText ; 
    public Text AIText ; 

    public Transform one;
    public Transform two;

    public Transform cameraRoot;

    private void Awake()
    {
        Instance = this;
        
    }

    public void DelaySetGameEnd(bool isWin)
    {
        if (IsGameOver)
            return;
        IsGameOver = true;
        JoystickPanel.Instance.gameObject.SetActive(false);
        for (int i = 0; i < 5; i++)
            UIPanelManager.Instance.PopPanel();

        PostProcessing.Instance.Blink(2f);
        this.AttachTimer(0.99f, () => { SetGameEnd(isWin); });
    }

    private bool IsGameOver;
    /// <summary>
    /// 领奖台结束场景
    /// </summary>
    private void SetGameEnd(bool isWin)
    {

        FllowObject.Get(0).enabled = false;
        FllowObject.Get(0).transform.position = cameraRoot.position;
        FllowObject.Get(0).transform.rotation = cameraRoot.rotation;

        PlayerText.text = ( KillSnackUI.killSnackUIList[0].KillSize ).ToString();
        AIText.text = ( KillSnackUI.killSnackUIList[1].KillSize ).ToString();
        ReplaceModel();
        player.transform.position = isWin ? one.position : two.position;
        ai.transform.position = !isWin ? one.position : two.position;

        player.GetComponentInChildren<Animator>().SetTrigger(isWin ? "Win" : "Fail");
        ai.GetComponent<Animator>().SetTrigger(!isWin ? "Win" : "Fail");

        for (int i = 0; i < GameManager.Instance.SnackList.Count; i++) {
            var child = GameManager.Instance.SnackList[i];
            if (!child.IsSnack) {
                if (child.IsLocalPlayer)
                {
                    player.GetComponentInChildren<SkinnedMeshRenderer>().SetBlendShapeWeight(0, ((ChildControl)child).GetFatValue);
                }
                else {
                    ai.GetComponentInChildren<SkinnedMeshRenderer>().SetBlendShapeWeight(0, ((ChildControl)child).GetFatValue);
                }
            }
        }
        

        this.AttachTimer(3, () =>
        {
            if (GameManager.abConfig.bonus_level == 0)
                UIPanelManager.Instance.PushPanel(isWin ? UIPanelType.WinPanel : UIPanelType.LosePanel);
            else {
                UIPanelManager.Instance.PushPanel(UIPanelType.ChallengePanel);
                if (isWin)
                {
                    Messenger.Broadcast<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ESpecialPanel.Challenge_Win, true);
                }
                else {
                    Messenger.Broadcast<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ESpecialPanel.Challenge_Fail, true);
                }
            }              
        });

    }

    void ReplaceModel()
    {
        GameObject newModel;
        player.gameObject.SetActive(false);
        newModel = Instantiate(SkinManager.Instance.allChildSkinInfo[GameData.GetChoseSkinId(ChildOrSnack.Child)].SkinModel);
        player = newModel.transform;
        player.GetComponent<NavMeshAgent>().enabled = false;
        player.GetComponent<BaseControl>().enabled = false;
        Transform PlayerTextTra = PlayerText.transform.parent;
        PlayerTextTra.SetParent(player);
        PlayerTextTra.localPosition = new Vector3(0, 2,0);
        //PlayerTextTra.position = player.InverseTransformPoint(new Vector3(0, 2, 0));
        player.gameObject.SetActive(true);
    }

}
