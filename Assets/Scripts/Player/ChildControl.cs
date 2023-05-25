using System.Collections.Generic;
using UnityEngine;
public enum EWeapon {
    NULL=-1,
    Hammer,
    BaseBallBar,
    Wrench,
    Pepper,
    Closestool
}

public class ChildControl : BaseControl,IHideable
{
    private Vector3 weaponPos;
    private Vector3 weapomEuler;
    private BindableProperty<EWeapon> weapomType;
    public BindableProperty<EWeapon> WeapomType {
        get {
            if (weapomType == null) {
                weapomType = new BindableProperty<EWeapon>();
                weapomType.OnChange = () =>
                {
                    m_Hammer.SetActive(false);
                    if (hasWeaponDict.ContainsKey(weapomType))
                    {
                        m_Hammer = hasWeaponDict[weapomType];
                    }
                    else
                    {
                        if (weapomType.Value == EWeapon.Hammer)
                        {
                            m_Hammer = rightHandRoot.Find("Hammer").gameObject;
                        }
                        else {
                            m_Hammer = Instantiate(Resources.Load<GameObject>("Weapon/" + weapomType.ToString()));
                            m_Hammer.transform.SetParent(rightHandRoot);
                            m_Hammer.transform.localPosition = weaponPos;
                            m_Hammer.transform.localEulerAngles = weapomEuler;
                        }
                        hasWeaponDict[weapomType] = m_Hammer;
                    }
                };
            }
            return weapomType;
        }
    }
    private Dictionary<EWeapon, GameObject> hasWeaponDict = new Dictionary<EWeapon, GameObject>();
    private GameObject m_Hammer;
    private GameObject Hammer
    {
        get
        {
            if (m_Hammer == null)
            {
                var hamTra = rightHandRoot.Find("Hammer");
                if (hamTra == null)
                    return null;
                m_Hammer = hamTra.gameObject;
                weaponPos = m_Hammer.transform.localPosition;
                weapomEuler = m_Hammer.transform.localEulerAngles;
            }
            return m_Hammer;
        }
    }

    private MyTimer HammerTimer = new MyTimer(10);
    private BindableProperty<bool> m_IsHammer;
    public BindableProperty<bool> IsHammer
    {
        get
        {
            if (m_IsHammer == null)
            {
                m_IsHammer = new BindableProperty<bool>(false);
                m_IsHammer.OnChange = () => { 
                    Hammer.SetActive(m_IsHammer);
                    if (m_IsHammer)
                        HammerTimer.ReStart();
                };
                m_IsHammer.OnChange();
            }
            return m_IsHammer;
        }
    }


    public Transform MouthPosition;

    private SkinnedMeshRenderer[] smrs;
    private int ChildIndex = 0;
    //不检查
    [HideInInspector]
    public bool noCheck = false;
    public bool hasSkirt = false;

    public override void Awake()
    {
        smrs = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        base.Awake();
        IsSnack = false;
        GameManager.Instance.childPlayer = this;
        IsCharge.OnChange = () =>
        {
            Messenger.Broadcast<int,bool ,Vector3>(ConstDefine.Listener.ChargeState, ChildIndex, IsCharge , 
                transform.position + transform.forward * .2f + Vector3.up * .8f );
        };
        IsHammer.Value = false;
        ChildIndex = 0;
    }

    public override void Start()
    {
        ChildIndex = GameManager.Instance.ChildCount;
        if (GameManager.sceneType == SceneType.Level)
            base.Start();
        else
            GameManager.Instance.SnackList.Add(this);

    }

    private Collider[] colliders = new Collider[3];
    public Collider CheckCollider(Vector3 pos,float size)
    {
        var length = Physics.OverlapSphereNonAlloc(pos , size, colliders, 1 << 1);
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
            Messenger.Broadcast(ConstDefine.Listener.ChargeObject, nmst.gameObject);
            agent.enabled = false;
            anim.SetTrigger("ToTable");
            this.AttachTimer(0.15f, () => {
                if(nmst!=null) 
                    nmst.AddForce((Vector3.up + transform.forward + Random.insideUnitSphere * .6f) * 225); 
            });
            this.AttachTimer(eatTime, () => { agent.enabled = true; });
            nmst.enabled = false;
        }
    }

    /// <summary>
    /// 检测障碍物
    /// </summary>
    /// <param name="pos"></param>
    private void CheckColliderUpdate(Vector3 pos)
    {
#if UNITY_EDITOR
        Debug.DrawLine(pos, pos + transform.forward * .5f);
#endif
        if (GameManager.Instance.IsGameOver)
            return;
        if (noCheck) {
            SetChargeState(false);
            return;
        }
        var target = CheckCollider(pos, .4f);
        if (!target)
        {
            SetChargeState(false);
            return;
        }
        //如果前方是可拆毁障碍物
        if (target.transform.GetChild(0).TryGetComponent(out NavMeshSourceTag nmst) && nmst.enabled)
        {
            if(IsHammer == false)
            {
                LiftingObjects(nmst);
            }
            else
            {
                //锤子攻击
                var canBreak = nmst.GetComponentInParent<CanBreakObject>();
                if (canBreak == false || canBreak.breakObjects == null)
                    LiftingObjects(nmst);
                else
                {
                    SetChargeState(false);
                    if (canBreak.canBreak)
                    {
                        anim.SetTrigger("HammerAttack");
                        canBreak.Break(.4f);
                    }
                }
            }
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
        if (agent.enabled == false || IsEatAnim() || anim.GetBool("IsCharge")||noCheck)
            return;
        var SnackList =  GameManager.Instance.SnackList;
        for (int i = 0; i < SnackList.Count; i++)
        {
            if (SnackList[i].NowState == 0 || SnackList[i].IsSnack == false ||SnackList[i].BeEat|| !SnackList[i].gameObject.activeSelf 
                || SnackList[i].gameObject == gameObject || SnackList[i].NowState <= .3f
                || SnackList[i].IsInvincible )
                continue;
            var pos = SnackList[i].transform.position;
            pos.y = 0;
            if (Vector3.Distance(pos, transform.position) > .6)
                continue;

            //var localDir = transform.InverseTransformDirection(pos);
            //localDir.y = 0;
            //localDir = localDir.normalized;
            //if (localDir.z < .5f)
            //    continue;

            EatSnack(SnackList[i]);
            break;
        }
    }

    private float FatValue;
    public float GetFatValue {
        get { return FatValue; }
    }
    private float tempValue;
    public void SetFatValue(float value)
    {
        FatValue = Mathf.Clamp(FatValue+value,0,100f) ;
        //FatRace = Mathf.Clamp(FatRace - 0.15f, 0.5f, 1);
    }

    private void ToFatUpdate()
    {
        tempValue = Mathf.Lerp(tempValue, FatValue, Time.deltaTime * 5);
        for (int i = 0; i < smrs.Length; i++)
        {
            //smrs[i].SetBlendShapeWeight(0, tempValue);
        }
    }

    /// <summary>
    /// 吃掉零食
    /// </summary>
    /// <param name="control"></param>
    public void EatSnack(BaseControl control)
    {
        if (control == null || GameManager.Instance.IsGameOver || GameManager.Instance.GameSpeed < .3f)
            return;
        //辣椒特效
        if (control.PlayerHealth > 1)
        {
            control.PlayerHealth--;
            if (control.IsDirty) {
                if (GameManager.Instance.eatDirtyDead) {
                    SetDeadAnim();
                    this.AttachTimer(2, () =>
                    {
                        if (IsLocalPlayer)
                            GameManager.Instance.GameFaild();
                        else
                            GameManager.Instance.GameWin();
                    });
                    control.gameObject.SetActive(false);
                }
                else
                    anim.SetTrigger("Vomit");
            }               
            else
                anim.SetTrigger("EatPepper");
            agent.enabled = false;

            this.AttachTimer(.25f, () => {
                GameObject go =null;
                if (!control.IsDirty)
                {
                    var prefab = Resources.Load<GameObject>("FireEffect");
                    go = Instantiate(prefab, MouthPosition.position, MouthPosition.rotation);
                    go.transform.SetParent(MouthPosition);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                }
                else {
                    ((SnackControl)control).SetNormalMat();
                    BeEatEffect effect = control.GetComponent<BeEatEffect>();
                    if (effect)
                    {
                        effect.ShowEmoji(transform, emojiPoss, 2);
                    }
                }
                this.AttachTimer(2f, () => {
                if(go)
                    Destroy(go);
                if(!GameManager.Instance.eatDirtyDead)
                    agent.enabled = true;
                });
            });
            return;
        }

        #region 广告演示用的
//#if UNITY_EDITOR
//        BeEatEffect effect = control.GetComponent<BeEatEffect>();
//        if (control.IsDirty)
//        {
//            if (!agent.enabled)
//                return;
//            SetDeadAnim();
//            this.AttachTimer(2, () =>
//            {
//                if (IsLocalPlayer)
//                    GameManager.Instance.GameFaild();
//                else
//                    GameManager.Instance.GameWin();
//            });
//            control.gameObject.SetActive(false);
//            if (effect) {
//                effect.ShowEmoji2(transform, emojiPoss, 2);
//            }
//            return;
//        }
//        else {
//            if (effect)
//            {
//                effect.ShowEmoji(transform, emojiPoss, 2);
//                if (!effect.OneEat && !effect.IsLastPart())
//                {
//                    EatSnakeAnim();
//                    effect.HidePart();
//                    return;
//                }
//            }
//        }
//#endif
        #endregion

        if (control.transform.childCount <= 1)
        {           
            return;
        }
        control.gameObject.SetActive(false);
        
        Messenger.Broadcast<BaseControl,BaseControl>(ConstDefine.Listener.EatSnake,this, control);
        EatSnakeAnim();
        var parent = control;
        //var model = control.transform.GetChild(0);
        var model = control.transform.Find("Model");
        model.SetParent(rightHandRoot);
        model.localPosition = Vector3.zero;
        model.localRotation = Quaternion.identity;
        control.BeEat = true;
        this.AttachTimer(eatTime, () => {           
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

    public void HitParent() {
        if (GameManager.Instance.IsGameOver) {
            return;
        }
        if (IsHammer && GameManager.Instance.parentCtrl!=null && GameManager.Instance.parentCtrl.agent.enabled) {
            if (Vector3.Distance(transform.position, GameManager.Instance.parentCtrl.transform.position) < 1.2f) {
                SetAnimTrigger("HammerAttack");
                GameManager.Instance.parentCtrl.agent.enabled = false;
                this.AttachTimer(0.4f, () => GameManager.Instance.parentCtrl.BeHit());
            }
        }
    }

    Vector3 checkPos;
    public override void Update()
    {
        base.Update();
        checkPos = transform.position + Vector3.up * 0.25f + transform.forward * .4f;
        CheckColliderUpdate(checkPos);
        EatCheckUpdate();
        ToFatUpdate();
        EatStateUpdate();
        HitParent();
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.H)) {
            if(IsLocalPlayer)
                IsHammer.Value = true;
        }     
#endif
        if (IsHammer)
        {
            HammerTimer.OnUpdate(Time.deltaTime * GameManager.Instance.GameSpeed);

            if (HammerTimer.IsFinish)
            {
                IsHammer.Value = false;
            }
        }
        anim.SetFloat("RunIndex", Mathf.Lerp( anim.GetFloat("RunIndex"), IsHammer ? 1 : 0, Time.deltaTime * 5) );
    }
}
