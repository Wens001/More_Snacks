using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColosseumSnackAI : BaseAI
{
    private float baseSpeed;
    [Header("圆场")]
    public Transform area;
    private float areaRadiu;
    private Vector3 targetPoin;

    private void Start()
    {
        baseSpeed = control.agent.speed;
        areaRadiu = area.localScale.x - 0.2f;
        targetPoin = transform.position;

        _tree = new BehaviorTreeBuilder(gameObject)
            .Sequence("主逻辑")
                .WaitTime(.33f)
                .Do("游戏结束判断", IsGameOver)
                .Condition("是否到达目标点", IsReachPoint)
                .WaitTime(Random.Range(1,2.0f))
                .Do("逃跑", Moving)
                .WaitTime(0.5f)
            .End()
            .Build();
    }

    public ScriptableBuff speedBuff;
    protected override void Update()
    {
        if (ColosseumMgr.Instance.IsGameOver)
            return;
        base.Update();
        if (GameManager.Instance.localPlayer && GameManager.Instance.localPlayer.IsSnack == false)
        {
            control.agent.speed = baseSpeed * GameManager.abConfig.aiSpeed;//.7f
        }
    }

    Vector3 NewTargetPoint() {
        Vector2 rand = Random.insideUnitCircle * areaRadiu;
        targetPoin = area.position + new Vector3(rand.x, 0, rand.y);
        return targetPoin;
    }

    public new TaskStatus IsGameOver()
    {
        if (ColosseumMgr.Instance.IsGameOver)
        {
            control.SetStop();
            return TaskStatus.Failure;
        }
        return TaskStatus.Success;
    }
    public bool IsReachPoint() {
        if (Vector3.Distance(transform.position, targetPoin) < 0.1) {
            if (ColosseumMgr.Instance.SnackList[0] == this)
                Debug.Log("到达地点");
            return true;
        }
        return false;
    }

    TaskStatus Moving() {      
        control.Move(NewTargetPoint());
        if(ColosseumMgr.Instance.SnackList[0]==this)
            Debug.Log("重新换点");
        return TaskStatus.Success;
    }

    /// <summary>
    /// 逃跑
    /// </summary>
    /// <returns></returns>
    public TaskStatus Escape()
    {
        if (GameManager.Instance.childPlayer == null)
            return TaskStatus.Failure;
        var dis = Vector3.Distance(transform.position, GameManager.Instance.childPlayer.transform.position);
        //如果有掩体
        if (control.IsHiding && dis < 4f)
        {
            return TaskStatus.Failure;
        }
        //如果在安全范围
        if (dis >= 4f)
            return TaskStatus.Success;
        var res = NavMeshSourceTag.FindNearCollPosition(transform.position);
        if (res != null)
        {
            bool hasOtherSnackNearColl = false;
            foreach (var snack in GameManager.Instance.SnackList)
            {
                if (snack.IsSnack == false || snack.IsHiding == false)
                    continue;
                if (Vector3.Distance(res.Value, snack.transform.position) < .4f)
                {
                    hasOtherSnackNearColl = true;
                    break;
                }
            }
            if (hasOtherSnackNearColl)
            {
                var ress = NavMeshSourceTag.FindGreaterCollPosition(transform.position, Vector3.Distance(res.Value, transform.position) + .1f);
                if (ress != null)
                {
                    //如果有其他安全掩体
                    control.Move(ress.Value);
                    return TaskStatus.Failure;
                }
                else
                {
                    //如果没有有其他安全掩体
                    control.Move(res.Value);
                    return TaskStatus.Failure;
                }
            }
            //移动到最近安全掩体
            control.Move(res.Value);
            return TaskStatus.Failure;
        }

        //如果没有掩体
        var childPos = GameManager.Instance.childPlayer.transform.position;
        var dir = (childPos - transform.position).normalized;

        control.Move(transform.position - Quaternion.Euler(0, Random.Range(-45, 45), 0) * dir * 2);
        return TaskStatus.Success;
    }


}
