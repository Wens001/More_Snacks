using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using zFrame.UI;
using DG.Tweening;
[RequireComponent(typeof(NavMeshAgent))]
public class BaseControl : MonoBehaviour,IHideable
{
    protected float eatTime = .5f;

    /// <summary>
    /// 是否无敌
    /// </summary>
    public bool IsInvincible { get; set; }

    /// <summary>
    /// 是否有毒
    /// </summary>
    public bool IsDirty { get; set; }
    /// <summary>
    /// 是否被吃了
    /// </summary>
    public bool BeEat { get; set; } = false;

    /// <summary>
    /// 肥胖移动速率
    /// </summary>
    public float FatRace { get; set; } = 1;

    public NavMeshAgent agent { get; protected set; }
    public Transform rightHandRoot;
    [HideInInspector]
    public Transform Model;

    public int PlayerHealth { get; set; } = 1;
    public BindableProperty<bool> IsLocalPlayer;

    protected Animator anim;
    public bool IsSnack { get; set; } = true;

    public bool IsStop { get {
            if (agent.enabled == false)
                return true;
            if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance,.2f) )
                return true;
            return false;
        } }

    public bool IsShow()
    {
        if (gameObject.activeSelf == false || NowState < .3f)
            return false;
        return true;
    }

    public void SetStop()
    {
        if (agent.enabled == false)
            return;
        agent.isStopped = true;
    }
    public void SetAction()
    {
        agent.isStopped = false;
    }

    /// <summary>
    /// 状态 是否躲藏
    /// 0 躲藏
    /// 1 
    /// </summary>
    public float NowState = 1;


    private SignedTimer hideSigned = new SignedTimer();
    /// <summary>
    /// 躲藏状态
    /// </summary>
    public bool IsHiding { get { return NowState <= .3f; } }

    /// <summary>
    /// 是否正在掀起物体
    /// </summary>
    public BindableProperty<bool> IsCharge { get; protected set ; }

    protected float animSpeed;
    protected Vector3[] emojiPoss;  

    public virtual void Awake()
    {
        IsLocalPlayer = new BindableProperty<bool>(false);
        IsLocalPlayer.OnChange = () =>
        {
            var joystick = JoystickPanel.Instance.joystick;
            if (IsLocalPlayer)
            {
                joystick.OnValueChanged.AddListener(JoystickValueChange);
                GameManager.Instance.localPlayer = this;
            }
            else
            {
                joystick.OnValueChanged.RemoveListener(JoystickValueChange);
                GameManager.Instance.localPlayer = null;
            }
        };
        agent = transform.GetComponent<NavMeshAgent>();
        Model = transform.Find("Model");
        Model.TryGetComponent(out anim);
        agent.angularSpeed = 0;
        IsCharge = new BindableProperty<bool>();

        Transform EmojiPos = transform.Find("EmojiPos");
        if (EmojiPos)
        {
            emojiPoss = new Vector3[EmojiPos.childCount];
            for (int i = 0; i < emojiPoss.Length; i++)
            {
                emojiPoss[i] = EmojiPos.GetChild(i).localPosition;
            }
        }
    }

    public void Move( Vector3 pos )
    {
        if (agent.enabled == false)
            return;
        agent.SetDestination(pos);
    }

    #region Color
    private ColorDoTween m_colorDT;
    public ColorDoTween ColorDT
    {
        get
        {
            return m_colorDT ?? (m_colorDT = new ColorDoTween(transform));
        }
    }

    public AnimatorStateInfo GetAnimState()
    {
        return anim.GetCurrentAnimatorStateInfo(0);
    }

    #endregion


    public virtual void Start()
    {
        GameManager.Instance.SnackList.Add(this);
        agent.enabled = true;
    }


    private const float baseSpeed = 3.5f;
    private MovementComponent m_moveComp;
    private MovementComponent moveComp
    {
        get
        {
            if (m_moveComp == null)
            {
                m_moveComp = GetComponent<MovementComponent>();
            }
            return m_moveComp;
        }
    }

    private void JoystickValueChange(Vector2 dir)
    {
        var v3Dir = - new Vector3(dir.x, 0, dir.y).normalized;
        agent.speed = baseSpeed * moveComp.MovementSpeed *FatRace;
        agent.velocity = v3Dir * baseSpeed * moveComp.MovementSpeed * FatRace;
        if (v3Dir.magnitude == 0)
            return;
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.LookRotation(v3Dir,Vector3.up)
            , Time.deltaTime * 20);
    }

    public virtual void EatSmallSnake(GameObject target)
    {
        EatSnakeAnim();
        target.transform.SetParent(rightHandRoot);
        target.transform.localPosition = Vector3.zero;
        target.transform.localRotation = Quaternion.identity;
        this.AttachTimer(eatTime, () => { Destroy(target); });
    }

    public void EatSnakeAnim()
    {
        agent.enabled = false;
        anim.SetTrigger("ToEat");
        AudioManager.Instance.PlaySound(Random.Range(1, 3));
        this.AttachTimer(eatTime,
            () =>
            {
                agent.enabled = true;
            }
        );
    }

    public void RandomPos() {
        Vector3 pos;
        LayerMask checkMask = (1 << 1) | (1 << 8);
        for (int i = 0; i < 3; i++) {
            pos = Random.insideUnitCircle *3;
            pos = transform.position+new Vector3(pos.x,0,pos.y);
            if (!Physics.CheckCapsule(pos, pos + new Vector3(0, 1.4f, 0), agent.radius, checkMask)) {
                 transform.position = pos;
                return;
            }
        }
        Debug.LogWarning("随机不到位置");
    }

    public virtual void Update()
    {
        var speed = agent.velocity.magnitude;
        if (speed > .5f)
            animSpeed = Mathf.Lerp(animSpeed, 1, Time.deltaTime * 10);
        else
            animSpeed = Mathf.Lerp(animSpeed, speed, Time.deltaTime * 10);
        anim.SetFloat(ConstDefine.Anim.Speed, animSpeed);

        if (agent.velocity.sqrMagnitude > .01f)//优化下agent.velocity.magnitude > .1f
        {
            var target = Quaternion.LookRotation(agent.velocity, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * 10);
        }

        if (IsSnack)
        {
            hideSigned.OnUpdate(IsHiding);
            if (hideSigned.IsPressDown && IsHiding)
            {
                Messenger.Broadcast<BaseControl>(ConstDefine.Listener.SnakeHide, this);
            }
        }
    }
    public void SetSpeedAnim(float _sp)
    {
        _sp = Mathf.Clamp01(_sp);
        anim.SetFloat(ConstDefine.Anim.Speed, _sp);
    }
    public void SetWinAnim() {
        anim.SetTrigger("Win");
    }
    public void SetJumpAnim()
    {
        anim.SetTrigger("Jump");
    }
    public void SetDeadAnim()
    {
        agent.enabled = false;
        anim.SetTrigger("Dead");
    }
    public void SetFearingAnim()
    {
        agent.enabled = false;
        anim.SetTrigger("Fearing");
    }
    public void SetAnimTrigger(string _anim,bool agentEnable = true)
    {
        agent.enabled = agentEnable;
        anim.SetTrigger(_anim);
    }

    public void OnFOVEnter()
    {
    }

    public void OnFOVLeave()
    {
    }
}
public class ColorDoTween
{
    protected List<Renderer> renderList;
    protected List<Color> DefaultColorList;

    public Transform transform { get; private set; }

    public ColorDoTween(Transform trans)
    {
        transform = trans;
        renderList = new List<Renderer>();
        DefaultColorList = new List<Color>();
        var datas = transform.GetComponentsInChildren<Renderer>();
        foreach (var data in datas)
        {
            renderList.Add(data);
            DefaultColorList.Add(data.material.GetColor("_DiffuseColor"));
        }
    }

    public void ChangeColor(Color endColor, float timer)
    {
        foreach (var rend in renderList)
        {
            foreach (var mat in rend.materials)
            {
                if (!mat.HasProperty("_DiffuseColor"))
                    continue;
                mat.DOKill();
                mat.DOColor(endColor, "_DiffuseColor", timer);
            }
        }
    }

    public void ChangeDefaultColor(float timer)
    {
        for (int i = 0; i < renderList.Count; i++)
        {
            foreach (var mat in renderList[i].materials)
            {
                if (!mat.HasProperty("_DiffuseColor"))
                    continue;
                mat.DOKill();
                mat.DOColor(DefaultColorList[i], "_DiffuseColor", timer);
            }
        }
    }
}