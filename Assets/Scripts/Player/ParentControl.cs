using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ParentControl : BaseControl
{
    private Transform target;
    private BaseControl targetContrl;
    public FieldOfView seeView;
    public Transform ActionCenterObj;
    private Vector3 actionAreaPoint;
    public float actionArea = 5;
    public AIPatrol aiPatrol;
    private MyTimer checkPatrolTime;
    public float walkSpeed = 1.5f;
    public float runSpeed = 3;
    public bool isPlayKnackDoorAnim = false;
    public GameObject vertigoEffect;
    public override void Awake()
    {
        base.Awake();
        seeView = GetComponentInChildren<FieldOfView>();
        actionAreaPoint = ActionCenterObj.position;
        ActionCenterObj.SetParent(null);
    }
    // Start is called before the first frame update
    public override void Start()
    {
        checkPatrolTime = new MyTimer(0.5f);
        if (isPlayKnackDoorAnim)
            KnackDoor(true);
        GameManager.Instance.parentCtrl = this;
    }

    // Update is called once per frame
    public override void Update()
    {
        var speed = agent.velocity.magnitude;
        if (speed > walkSpeed+0.5f)
            animSpeed = Mathf.Lerp(animSpeed, 2, Time.deltaTime * 10);
        else if(speed>0.5f)
            animSpeed = Mathf.Lerp(animSpeed, 1, Time.deltaTime * 10);
        else
            animSpeed = Mathf.Lerp(animSpeed, speed, Time.deltaTime * 10);
        anim.SetFloat(ConstDefine.Anim.Speed, animSpeed);

        if (agent.velocity.magnitude > .1f)
        {
            var target = Quaternion.LookRotation(agent.velocity, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * 10);
        }
        if (GameManager.Instance.IsGameOver) {
            return;
        }
        ChaseTarget();
        CatchTarget();
        Patrol();
    }
    //追逐目标
    void ChaseTarget()
    {
        if (seeView == null)
            return;
        int targetCount = seeView.lookTargetList.Count;
        if (targetCount > 1)
        {
            float minDis = float.MaxValue;
            foreach (Transform tar in seeView.lookTargetList)
            {
                var dis = Vector3.Distance(tar.position, transform.position);
                if (minDis > dis)
                {
                    minDis = dis;
                    target = tar;
                }
            }
        }
        else if (targetCount == 1)
        {
            target = seeView.lookTargetList[0];
        }
        else
        {
            target = null;
            agent.speed = walkSpeed;
            return;
        }
        agent.speed = runSpeed;
        if (Vector3.Distance(transform.position, actionAreaPoint) < actionArea)
        {
            Move(target.position);           
        }
        else {
            if (target != null)
                Move(transform.position);
            target = null;
        }
    }

    void CatchTarget() {
        if (target !=null && agent.enabled && Vector3.Distance(transform.position, target.position) < 0.6f) {
            target.TryGetComponent(out targetContrl);
            if (targetContrl.IsSnack)
            {
                ThrowSnack((SnackControl)targetContrl);
            }
            else {
                CatchChild(targetContrl);
            }
        }
    }
    //捉住零食
    void ThrowSnack(SnackControl control) {
        if (!control.gameObject.activeSelf || control.isThrowing)
            return;
        anim.SetTrigger("Throw");
        agent.enabled = false;
        Transform snackParent = control.transform.parent;
        control.ThrowBefore();
        if (rightHandRoot) {
            target.SetParent(rightHandRoot);
            target.localPosition = Vector3.zero;
        }
        this.AttachTimer(0.8f, () => {
            control.transform.SetParent(snackParent);
            control.BeThrow(transform.forward * 400);
            agent.enabled = true;
        });
    }
    //捉住小孩
    void CatchChild(BaseControl control)
    {
        ChildControl child = ((ChildControl)control);
        SetAnimTrigger("Spank", false);
        child.noCheck = true;
        if (GameManager.abConfig.level_difficulty == 0)
        {            
            if (GameManager.Instance.localPlayer.IsSnack)
            {
                GameManager.Instance.GameWin();
                if (!child.hasSkirt)
                    control.SetAnimTrigger("GetSpank", false);
                else
                    control.SetAnimTrigger("GrilSpank", false);
            }
            else
            {
                if (!child.hasSkirt)
                    GameManager.Instance.GameFaild("GetSpank");
                else
                    GameManager.Instance.GameFaild("GrilSpank");
            }
            Messenger.Broadcast<int, bool, Vector3>(ConstDefine.Listener.ChargeState, 0, false, Vector3.zero);
        }
        else
        {           
            if (!child.hasSkirt)
                control.SetAnimTrigger("GetSpank", false);
            else
                control.SetAnimTrigger("GrilSpank", false);
            this.AttachTimer(3, () =>
            {
                control.SetAnimTrigger("StopSpank",true);
                SetAnimTrigger("StopSpank",false);
                child.noCheck = false;
            });
            this.AttachTimer(5, () =>
            {
                if (!agent.enabled)
                    agent.enabled = true;
            });
        }
        Vector3 dir = control.transform.position - transform.position;
        dir = Vector3.right;
        Quaternion rot = Quaternion.LookRotation(dir);
        control.transform.rotation = rot;
        transform.rotation = Quaternion.identity;
        control.transform.position = transform.TransformPoint(new Vector3(0.35f, 0, 0.58f));
    }

    void Patrol() {
        if (target != null ||aiPatrol==null || aiPatrol.Index < 0)
            return;
        if (aiPatrol.IsReach()) {
            aiPatrol.NextWayPoint();
        }
        checkPatrolTime.OnUpdate(Time.deltaTime);
        if (checkPatrolTime.IsFinish) {
            aiPatrol.GotoCurPoint();
            checkPatrolTime.ReStart();
        }
    }

    //敲门
    public void KnackDoor(bool _true) {
        anim.SetBool("KnackDoor", _true);
    }
    //被打眩晕
    public void BeHit() {
        //agent.enabled = false;
        SetAnimTrigger("Vertigo",false);
        if (vertigoEffect)
            vertigoEffect.SetActive(true);
        seeView.gameObject.SetActive(false);
        this.AttachTimer(4, () =>
        {          
            SetAnimTrigger("ToNormal",true);
            //agent.enabled = true;
            if (vertigoEffect)
                vertigoEffect.SetActive(false);
            seeView.gameObject.SetActive(true);
        });
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1,0,0,1f);
        Gizmos.DrawWireSphere(ActionCenterObj.position, actionArea);       
    }
#endif
}
