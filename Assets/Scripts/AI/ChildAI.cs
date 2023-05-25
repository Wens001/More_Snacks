using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
public class ChildAI : BaseAI
{
    private TestAISpeed aISpeed;
    protected override void Awake()
    {
        base.Awake();

        ChaseHideState = new BindableProperty<bool>();
        ChaseHideState.OnChange = () =>
        {
            if (ChaseTarget.Value == null)
                return;
            var IsGiveUp = Random.value > .4f;
            if (IsGiveUp)
                ChaseTarget.Value = null;
        };

        ChaseTarget = new BindableProperty<BaseControl>();
        ChaseTarget.OnChange = () => {
            Messenger.Broadcast(ConstDefine.Listener.DiscoverSnake, ChaseTarget);
            ChaseTimer.ReStart();
        };
        ChaseTimer = new MyTimer(7);

    }


    /// <summary>
    /// 追逐计时
    /// </summary>
    private MyTimer ChaseTimer;
    /// <summary>
    /// 追逐目标
    /// </summary>
    private BindableProperty<BaseControl> ChaseTarget;

    /// <summary>
    /// 被追逐者状态
    /// </summary>
    private BindableProperty<bool> ChaseHideState;

    /// <summary>
    /// 追逐
    /// </summary>
    /// <returns></returns>
    public TaskStatus Chase()
    {
        var res = GameManager.Instance.LiveSnakeList();
        if (res.Count == 1)
        {
            var temp = GameManager.Instance.SnackList[res[0]];
            
            control.Move(temp.transform.position);
            return TaskStatus.Failure;
        }
        //res.Clear();

        var target = GameManager.Instance.FindNearSnake(control, 10f);
        if (target)
        {
            ChaseTarget.Value = target;
        }
        else {
            if (ChaseTarget.Value == null) {
                ChaseTarget.Value = GameManager.Instance.SnackList[res[0]];
            }
        }
        res.Clear();
        if (ChaseTarget.Value == null)
            return TaskStatus.Success;
        control.Move(ChaseTarget.Value.transform.position);
        return TaskStatus.Continue;
    }

    private float baseSpeed;

    private void Start()
    {
        _tree = new BehaviorTreeBuilder(gameObject)
            .Sequence("主逻辑")
                .Do("游戏结束判断", IsGameOver)
                .Do("追逐", Chase)
                //.Do("闲逛", HangOut)
                .WaitTime(.33f)
            .End()
            .Build();
        baseSpeed = control.agent.speed;
        aISpeed = GetComponent<TestAISpeed>();

    }

    private void OnEnable()
    {
        Messenger.AddListener<GameObject>(ConstDefine.Listener.ChargeObject, OnChargeObject);
        stopTimer.SetFinish();
    }
    private void OnDisable()
    {
        Messenger.RemoveListener<GameObject>(ConstDefine.Listener.ChargeObject, OnChargeObject);
    }

    private MyTimer stopTimer = new MyTimer(1);
    private void OnChargeObject(GameObject tar)
    {
        stopTimer.ReStart();
    }

    protected override void Update()
    {
        if (GameManager.Instance.IsGameOver) {
            return;
        }
        base.Update();
        ChaseTargetTimer();
        if (GameManager.Instance.localPlayer)
        {
            if (GameManager.Instance.localPlayer.IsSnack)
            {
                if (LevelScenes.level <= 3)
                    control.agent.speed = baseSpeed * .5f;
                else if (GameManager.Instance.SnackList.Count < 6)
                    control.agent.speed = baseSpeed * .7f;
                else
                    control.agent.speed = baseSpeed * GameManager.abConfig.aiSpeed;//0.6
            }
            else {
                if (aISpeed)
                    control.agent.speed = baseSpeed * aISpeed.speed;
                else
                    control.agent.speed = baseSpeed * .6f;
            }          
        }

        stopTimer.OnUpdate(Time.deltaTime * GameManager.Instance.GameSpeed);

        if (control.agent.enabled)
            control.agent.isStopped = !stopTimer.IsFinish;
    }

    /// <summary>
    /// 更新追逐时间
    /// </summary>
    private void ChaseTargetTimer()
    {
        if (ChaseTarget.Value)
        {
            ChaseTimer.OnUpdate(Time.deltaTime);
            ChaseHideState.Value = ChaseTarget.Value.IsHiding;
        }
        if (ChaseTimer.IsFinish)
        {
            ChaseTimer.ReStart();
            ChaseTarget.Value = null;
        }
    }

    public void DelayToMove() {
        control.agent.enabled = false;
        this.AttachTimer(1, () => control.agent.enabled = true);
    }

}
