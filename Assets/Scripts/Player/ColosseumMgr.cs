using MoreMountains.NiceVibrations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColosseumMgr : MonoBehaviour
{
    public static ColosseumMgr Instance;
    public bool IsGameOver { get; private set; }
    public List<BaseControl> SnackList = new List<BaseControl>();
    public List<BaseControl> ChildList = new List<BaseControl>();
    public GameObject area;
    public int sprintCount = 5;
    public float readySprintTime = 2;
    [Header("小孩冲刺是否只在圈内")]
    public bool isInArea = true;

    private void Awake()
    {
        Instance = this;
        IsGameOver = false;
        Messenger.AddListener<BaseControl, BaseControl>(ConstDefine.Listener.EatSnake, EatSnackListener);     
    }
    // Start is called before the first frame update
    void Start()
    {
        FllowObject.StartView();
        UIPanelManager.Instance.PushPanel(UIPanelType.CoinLvPanel);
        Messenger.Broadcast<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ESpecialPanel.CoinLv_Play, true);
    }

    public void StartGame() {
        JoystickPanel.Instance.gameObject.SetActive(true);
        FllowObject.Get(0).speed = 20f;
        foreach (var snake in SnackList)
        {
            if (snake.IsLocalPlayer)
                continue;
            var ai = snake.GetComponent<ColosseumSnackAI>();
            if (ai == null) {
                ai = snake.gameObject.AddComponent<ColosseumSnackAI>();
                ai.area = area.transform;
            }
            ai.enabled = true;
        }
        foreach (var child in ChildList)
        {
            if (child.IsLocalPlayer)
                continue;
            var ai = child.GetComponent<ColosseumChildAI>();
            if (ai == null)
            {
                ai = child.gameObject.AddComponent<ColosseumChildAI>();              
            }
            ai.area = area.transform;
            ai.sprintCount = sprintCount;
            ai.readySprintTime = readySprintTime;
            ai.isInArea = isInArea;
            ai.SetAgentFlag();
            ai.enabled = true;
        }
        if (LevelScenes.level > 1)
            FllowObject.MoveToTarget();
    }
    private void OnDisable()
    {
        Messenger.RemoveListener<BaseControl, BaseControl>(ConstDefine.Listener.EatSnake, EatSnackListener);
    }

    bool HasSnack()
    {
        return SnackList.Count > 0 ? true : false;
    }

    private void EatSnackListener(BaseControl killer,BaseControl control) {
        if (GameManager.CanShake)
            MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
        //玩家扮演零食被吃就输
        if (control.IsLocalPlayer)
        {
            GameFaild();
            return;
        }
        if (GameManager.Instance.localPlayer.IsSnack == false)
        {
            if (HasSnack() == false)
            {
                GameWin();
                return;
            }
        }
        else {
            if (SnackList.Count == 1) {
                GameWin();
                return;
            }
        }
    }

    public void GameWin() {
        JoystickPanel.Instance.gameObject.SetActive(false);
        IsGameOver = true;
        GameManager.Instance.localPlayer.SetWinAnim();
        FllowObject.Get(0).FailEndCamPoint();
        this.AttachTimer(2, () => {
            Messenger.Broadcast(ConstDefine.Listener.CoinLvEnd);
            Messenger.Broadcast<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ESpecialPanel.CoinLv_End, true); 
        });
        //Debug.Log("胜利");
        
    }

    public void GameFaild() {
        JoystickPanel.Instance.gameObject.SetActive(false);
        IsGameOver = true;
        this.AttachTimer(2, () =>
        {
            Messenger.Broadcast(ConstDefine.Listener.CoinLvEnd);
            Messenger.Broadcast<ESpecialPanel, bool>(ConstDefine.Listener.ShowSpecialPanel, ESpecialPanel.CoinLv_End, true);
        });
        //Debug.Log("失败");
        
    }
}
