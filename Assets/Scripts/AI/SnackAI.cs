using UnityEngine;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;

public class SnackAI : BaseAI
{
    private float baseSpeed;

    private void Start()
    {
        _tree = new BehaviorTreeBuilder(gameObject)
            .Sequence("主逻辑")
                .WaitTime(.33f)
                .Do("游戏结束判断", IsGameOver)
                .Do( "逃跑" , Escape )
                .Do( "闲逛" , HangOut )
            .End()
            .Build();
        baseSpeed = control.agent.speed;
    }

    public ScriptableBuff speedBuff;
    protected override void Update()
    {
        if (GameManager.Instance.GameSpeed < .5f)
            return;
        base.Update();
        if (GameManager.Instance.localPlayer &&  GameManager.Instance.localPlayer.IsSnack == false)
        {
            //TryGetComponent(out BuffableEntity buffableEntity);

            
            control.agent.speed = baseSpeed * GameManager.abConfig.aiSpeed ;//.7f
        }
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
                if (Vector3.Distance(res.Value,snack.transform.position) < .4f )
                {
                    hasOtherSnackNearColl = true;
                    break;
                }
            }
            if (hasOtherSnackNearColl)
            {
                var ress = NavMeshSourceTag.FindGreaterCollPosition(transform.position, Vector3.Distance(res.Value,transform.position) + .1f );
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

        control.Move(transform.position - Quaternion.Euler(0,Random.Range(-45,45),0) * dir * 2);
        return TaskStatus.Success;
    }


}
