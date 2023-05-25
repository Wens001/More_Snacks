using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalControl : BaseControl
{
    public Transform MouthPosition;
    public float eatSnackDistance = 0.8f;
    private SkinnedMeshRenderer[] smrs;
    private int ChildIndex = 0;
    public bool canCut;
    //public float eatTime;
    public bool canFat = true;
    public override void Awake()
    {
        smrs = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        base.Awake();
        IsSnack = false;
        GameManager.Instance.childPlayer = this;
        IsCharge.OnChange = () =>
        {
            Messenger.Broadcast<int, bool, Vector3>(ConstDefine.Listener.ChargeState, ChildIndex, IsCharge,
                transform.position + transform.forward * .2f + Vector3.up * .8f);
        };
        ChildIndex = 0;          
    }

    public override void Start()
    {
        eatTime = 0.8f;
        ChildIndex = GameManager.Instance.ChildCount;
        base.Start();

    }

    private Collider[] colliders = new Collider[3];
    public Collider CheckCollider(Vector3 pos, float size)
    {
        var length = Physics.OverlapSphereNonAlloc(pos, size, colliders, 1 << 1);
        if (length == 0)
            return null;
        return colliders[0];
    }

    public bool IsEatAnim()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Eat");
    }
    private SignedTimer signedTimer = new SignedTimer();

    public void EatStateUpdate()
    {
        if (GameManager.Instance.IsGameOver)
            return;
        signedTimer.OnUpdate(IsEatAnim());
        if (signedTimer.IsPressDown)
            agent.enabled = false;
        if (signedTimer.IsPressUp)
            agent.enabled = true;
    }

    /// <summary>
    /// 掀起物体
    /// </summary>
    private void LiftingObjects(NavMeshSourceTag nmst)
    {
        SetChargeState(true);
        var value = nmst.OnUpdateTime(transform);
        Messenger.Broadcast<int, float, Vector3>(ConstDefine.Listener.ChargeValue, ChildIndex, value,
            transform.position + transform.forward * .2f + Vector3.up * .8f);
        if (nmst.IsAtTime())
        {
            var canBreak = nmst.GetComponentInParent<CanBreakObject>();
            if (canBreak == false || canBreak.breakObjects == null)
            {
                Messenger.Broadcast(ConstDefine.Listener.ChargeObject, nmst.gameObject);
                agent.enabled = false;
                anim.SetTrigger("ToTable");
                this.AttachTimer(0.15f, () => { nmst.AddForce((Vector3.up + transform.forward + Random.insideUnitSphere * .6f) * 225); });
                this.AttachTimer(eatTime, () => { agent.enabled = true; });
                nmst.enabled = false;
            }
            else {
                SetChargeState(false);
                if (canBreak.canBreak)
                {
                    anim.SetTrigger("ToTable");
                    canBreak.Break(.4f);
                }
            }
            
        }
    }

    /// <summary>
    /// 检测障碍物
    /// </summary>
    /// <param name="pos"></param>
    private void CheckColliderUpdate(Vector3 pos)
    {
#if UNITY_EDITOR
        Debug.DrawLine(pos, pos + transform.forward * .5f);//.5f
#endif
        var target = CheckCollider(pos, .4f);
        if (!target)
        {
            SetChargeState(false);
            return;
        }
        //如果前方是可拆毁障碍物
        if (target.transform.GetChild(0).TryGetComponent(out NavMeshSourceTag nmst) && nmst.enabled)
        {
            LiftingObjects(nmst);
        }
        else
            SetChargeState(false);
    }

    /// <summary>
    /// 设置掀起状态
    /// </summary>
    /// <param name="flag"></param>
    public void SetChargeState(bool flag)
    {
        IsCharge.Value = flag;
        anim.SetBool("IsCharge", flag);
    }

    private void EatCheckUpdate()
    {
        if (agent.enabled == false || IsEatAnim() || anim.GetBool("IsCharge"))
            return;
        var SnackList = GameManager.Instance.SnackList;
        for (int i = 0; i < SnackList.Count; i++)
        {
            if (SnackList[i].IsSnack == false || !SnackList[i].gameObject.activeSelf
                || SnackList[i].gameObject == gameObject || SnackList[i].NowState <= .3f
                || SnackList[i].IsInvincible)//SnackList[i].NowState == 0||
                continue;
            var pos = SnackList[i].transform.position;
            if (Vector3.Distance(pos, transform.position) > eatSnackDistance)
                continue;

            //var localDir = transform.InverseTransformDirection(pos);
            //localDir.y = 0;
            //localDir = localDir.normalized;
            //if (localDir.z < .5f)
            //    continue;

            EatSnake(SnackList[i]);
            break;
        }
    }

    private float FatValue;
    public float GetFatValue
    {
        get { return FatValue; }
    }
    private float tempValue;
    public void SetFatValue(float value)
    {

        FatValue = Mathf.Clamp(FatValue + value, 0, 100f);
    }

    private void ToFatUpdate()
    {
        if (!canFat)
            return;
        tempValue = Mathf.Lerp(tempValue, FatValue, Time.deltaTime * 5);
        for (int i = 0; i < smrs.Length; i++)
        {
            smrs[i].SetBlendShapeWeight(0, tempValue);
        }
    }

    /// <summary>
    /// 吃掉零食
    /// </summary>
    /// <param name="control"></param>
    public void EatSnake(BaseControl control)
    {
        if (control == null || GameManager.Instance.IsGameOver || GameManager.Instance.GameSpeed < .3f)
            return;
        //辣椒特效
        if (control.PlayerHealth > 1)
        {
            control.PlayerHealth--;
            anim.SetTrigger("EatPepper");
            agent.enabled = false;

            this.AttachTimer(.25f, () =>
            {
                var prefab = Resources.Load<GameObject>("FireEffect");
                var go = Instantiate(prefab, MouthPosition.position, MouthPosition.rotation);
                go.transform.SetParent(MouthPosition);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                this.AttachTimer(1f, () =>
                {
                    Destroy(go);
                    agent.enabled = true;
                });
            });
            return;
        }

        #region emoji表情
        BeEatEffect effect = control.GetComponent<BeEatEffect>();
        //多吃几次屎
        if (effect)
        {
            effect.ShowDeadEffect(transform, new Vector3(0, 0.84f, 1.1f), 1);
            effect.ShowEmoji(transform, emojiPoss, 2);
            if (!effect.OneEat && !effect.IsLastPart())
            {
                EatSnakeAnim();
                effect.HidePart();                
                return;
            }
        }
        #endregion
        #region 猫爪攻击
        if (canCut)
        {
            EatSnakeAnim();
            var canBreak = control.GetComponentInParent<CanBreakObject>();
            if (canBreak == false || canBreak.breakObjects == null)
            {
            }
            else
            {
                SetChargeState(false);
                if (canBreak.canBreak)
                {
                    canBreak.Break(.4f);
                    this.AttachTimer(.4f, () =>
                    {
                        if (control.gameObject.name.Equals("Bottle"))
                        {
                            FllowObject fllowObject = FindObjectOfType<FllowObject>();
                            if (fllowObject)
                            {
                                fllowObject.enabled = false;
                            }
                            SetFearingAnim();
                            this.AttachTimer(1.5f, () => {
                                anim.SetTrigger("Fear");
                                Model.DOMove(transform.position+ new Vector3(0,6,-6), 0.5f).SetEase(Ease.Linear).onComplete = () => { Model.gameObject.SetActive(false); };                                
                            });
                            this.AttachTimer(2f, () => GameManager.Instance.GameFaild());
                        }
                        else
                        {
                            Messenger.Broadcast<BaseControl, BaseControl>(ConstDefine.Listener.EatSnake, this, control);
                        }
                    });
                }
                return;
            }
        }
        #endregion
        control.gameObject.SetActive(false);

        Messenger.Broadcast<BaseControl, BaseControl>(ConstDefine.Listener.EatSnake, this, control);
        EatSnakeAnim();
        var parent = control;
        var model = control.transform.GetChild(0);
        model.SetParent(rightHandRoot);
        model.localPosition = Vector3.zero;
        model.localRotation = Quaternion.identity;
        this.AttachTimer(eatTime, () =>
        {
            //control.gameObject.SetActive(false);
            model.SetParent(parent.transform);
            model.localPosition = Vector3.zero;
            model.localRotation = Quaternion.identity;
        });
        SetFatValue(33.33f);
    }

    public override void EatSmallSnake(GameObject target)
    {
        base.EatSmallSnake(target);
        SetFatValue(5f);
    }



    public override void Update()
    {
        base.Update();
        var pos = transform.position + Vector3.up * 0.25f + transform.forward * .6f;//.4f
        CheckColliderUpdate(pos);
        EatCheckUpdate();
        ToFatUpdate();
        EatStateUpdate();
    }
}
