using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Tasks.Actions;
using CleverCrow.Fluid.BTs.Trees;
using DG.Tweening;
using GameAnalyticsSDK.Setup;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ColosseumChildAI :BaseAI
{
    [Header("是否在场地内冲刺")]
    public bool isInArea = true;
    [Header("范围预警")]
    public GameObject warnningTip;
    //预警材质球
    private Material warnningMat;
    //AI的速度
    private TestAISpeed aISpeed;
    //冲刺方向
    private Vector3 moveDir;
    [Header("圆场")]
    public Transform area;
    private float areaRadiu;
    //冲刺次数
    public int sprintCount = 5;
    private int curSprint =0;
    //冲刺准备时间
    public float readySprintTime = 2;
    //移动的目标点
    Vector3 targetPoint = Vector3.zero;
    private float targetDistance = 20;
    private bool isReachTarget = false;
    private float MaxOutSideDis = 11 * 11;

    protected override void Awake()
    {
        base.Awake();
        warnningTip = Instantiate(Resources.Load<GameObject>("WarningTip"),transform);
        warnningTip.transform.localPosition = new Vector3(0, 0.05f, 9);
        warnningTip.transform.localEulerAngles = new Vector3(90,0,0);
        warnningMat = warnningTip.GetComponent<Renderer>().material;
        warnningMat.DOColor(new Color(1, 0, 0, 0.6f), 1.2f).SetLoops(-1, LoopType.Yoyo);
        warnningTip.gameObject.SetActive(false);

        Messenger.AddListener(ConstDefine.Listener.CoinLvEnd,OnGameOver);
    }

    private void OnDisable()
    {
        Messenger.RemoveListener(ConstDefine.Listener.CoinLvEnd, OnGameOver);
    }

    private float baseSpeed;
    public bool isSprint = false;

    private void Start()
    {       
        areaRadiu = area.localScale.x - 0.2f;
        _tree = new BehaviorTreeBuilder(gameObject)
            .Sequence("主逻辑")
                .Do("游戏结束判断", IsGameOver)
                .WaitTime(0.1f)
                .Do("检查是否冲刺",CheckSprint)
                .Do("准备冲刺", ReadySprint)
                .WaitTime(readySprintTime)
                .Do("冲刺", ToSprint)
            .End()
            .Build();
        baseSpeed = control.agent.speed;
        aISpeed = GetComponent<TestAISpeed>();

    }

    new TaskStatus IsGameOver() {
        if (ColosseumMgr.Instance.IsGameOver)
        {
            return TaskStatus.Failure;
        }
        return TaskStatus.Success;
    }

    public TaskStatus ReadySprint()
    {
        var res = GameManager.Instance.LiveSnakeList();
        if (res.Count == 0)
        {          
            return TaskStatus.Failure;
        }
        var target = GameManager.Instance.localPlayer;
        if (target == null)
        {
            var snackList = ColosseumMgr.Instance.SnackList;
            moveDir = snackList[Random.Range(0, snackList.Count)].transform.position - transform.position;
        }
        else {
            moveDir = GameManager.Instance.localPlayer.transform.position - transform.position;
        }      
        moveDir.y = 0;
        moveDir.Normalize();
        transform.DORotateQuaternion(Quaternion.LookRotation(moveDir), 0.3f).onComplete=()=> {
            warnningTip.SetActive(true);
        };
        targetDistance = isInArea ? 20 : 25;

        targetPoint = transform.position + moveDir * targetDistance;
        return TaskStatus.Success;
    }
    
    public TaskStatus ToSprint()
    {
        isSprint = true;
        isReachTarget = false;
        control.SetSpeedAnim(1);
        transform.DOMove(targetPoint, targetDistance / control.agent.speed).SetEase(Ease.Linear).onComplete=()=> {
            isReachTarget = true;
        };
        return TaskStatus.Success;
    }

    public TaskStatus CheckSprint() {
        if (isSprint) {
            if (isInArea)
            {
                if (Vector3.Distance(area.position, transform.position) >= areaRadiu)
                {
                    //Debug.Log("碰到边界");
                    control.SetSpeedAnim(0);
                    isSprint = false;
                    transform.DOKill();
                    warnningTip.SetActive(false);
                    Messenger.Broadcast(ConstDefine.Listener.GetScoreReward);
                    if (++curSprint >= 5)
                    {
                        ColosseumMgr.Instance.GameWin();
                        this.enabled = false;
                    }
                    return TaskStatus.Failure;
                }
                return TaskStatus.Continue;
            }
            else {
                if (Vector3.SqrMagnitude(area.position-transform.position) >= MaxOutSideDis) {
                    isSprint = false;
                    control.SetSpeedAnim(0);
                    transform.DOKill();
                    transform.localPosition -= transform.forward;
                    warnningTip.SetActive(false);
                    Messenger.Broadcast(ConstDefine.Listener.GetScoreReward);
                    if (++curSprint >= 5)
                    {
                        ColosseumMgr.Instance.GameWin();
                        this.enabled = false;
                    }
                    return TaskStatus.Failure;
                }
                return TaskStatus.Continue;
            }
        }
        return TaskStatus.Success;
    }

    void OnGameOver() {
        transform.DOKill();
        warnningTip.gameObject.SetActive(false);
    }


    protected override void Update()
    {
        if (GameManager.Instance.IsGameOver)
        {
            return;
        }
        base.Update();

    }

    public void SetAgentFlag() {
        NavMeshAgent agent;
        if (TryGetComponent(out agent))
        {
            if (!isInArea)
                agent.enabled = false;
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawCube (targetPoint, Vector3.one);
    //}
}
