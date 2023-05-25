using DG.Tweening;
using GameAnalyticsSDK.Setup;
using HighlightingSystem;
using RootMotion.Dynamics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(CapsuleCollider))]
public class SmallChildControl : BaseControl
{
    public BindableProperty<NavMeshModifierVolume> OcclusionLayer;
    private Material mainMaterial;
    private CapsuleCollider coll;
    private Rigidbody rg;
    public Rigidbody Rg
    {
        get { return rg; }
    }
    [HideInInspector]
    public bool isThrowing = false;
    public PuppetMaster puppet;
    private Rigidbody puppetRg;
    public override void Awake()
    {
        base.Awake();
        TryGetComponent(out rg);
        if (rg == null)
            rg = gameObject.AddComponent<Rigidbody>();
        rg.isKinematic = true;
        //rg.constraints = RigidbodyConstraints.FreezeRotation;
        mainMaterial = GetComponentInChildren<SkinnedMeshRenderer>().materials[0];

        IsSnack = true;
        coll = transform.GetComponent<CapsuleCollider>();
        if (coll == null)
            coll = gameObject.AddComponent<CapsuleCollider>();
        coll.isTrigger = true;
        coll.center = Vector3.up * agent.height / 2;
        coll.height = agent.height;
        coll.radius = agent.radius;
        gameObject.layer = 9;
        if (puppet == null) {
            puppet = GetComponentInChildren<PuppetMaster>();            
        }

        puppet.mode = PuppetMaster.Mode.Disabled;
        puppetRg = puppet.GetComponentInChildren<Rigidbody>();
    }

    public override void Start()
    {
        NowState = 1;
        anim.SetFloat(ConstDefine.Anim.State, NowState);
        if (GameManager.sceneType == SceneType.Level)
            base.Start();
        else
            GameManager.Instance.SnackList.Add(this);
        OcclusionLayer = new BindableProperty<NavMeshModifierVolume>();
        OcclusionLayer.OnChangeBefore = () =>
        {
            if (OcclusionLayer.Value == null || IsLocalPlayer == false || OcclusionLayer.Value.area == 1)
                return;
            var temp = GetHighlighter(OcclusionLayer.Value);
            temp.constant = false;
        };
        OcclusionLayer.OnChange = () =>
        {
            if (OcclusionLayer.Value == null || IsLocalPlayer == false || OcclusionLayer.Value.area == 1)
                return;
            var temp = GetHighlighter(OcclusionLayer.Value);
            temp.constant = true;
        };
        if (mainMaterial.shader.name != "Matcap_XRay") {
            mainMaterial.shader = Shader.Find("Matcap_XRay");
            Debug.Log("换透明材质了");
        }
        mainMaterial.renderQueue = 3000;
        //puppet.mode = PuppetMaster.Mode.Disabled;
    }

    public Highlighter GetHighlighter(NavMeshModifierVolume volume)
    {
        var parent = volume.transform.parent;
        if (parent == null)
            throw new Exception($"{volume} Not Has Parent");
        var res = parent.GetComponent<Highlighter>();
        if (res == null)
        {
            res = parent.gameObject.AddComponent<Highlighter>();
            res.tween = false;
            res.constantColor = Color.white;
        }
        return res;
    }

    public override void EatSmallSnake(GameObject target)
    {
        base.EatSmallSnake(target);
        anim.transform.localScale += Vector3.one * .05f;
    }


    public void ThrowBefore()
    {
        agent.enabled = false;
        rg.isKinematic = true;
    }

    public void BeThrow(Vector3 _force)
    {
        rg.isKinematic = false;
        rg.AddForce(_force);
        isThrowing = true;
    }

    public void SetBodyParent(Transform _parent,bool isSetPos = true) {
        Model.SetParent(_parent);
        puppet.transform.SetParent(_parent);
        if (isSetPos) {
            Model.localPosition = new Vector3(-0.022f,-0.022f,-0.173f);
            Model.localEulerAngles = new Vector3(90, 0, 6); 
            puppet.transform.localPosition = new Vector3(-0.022f, -0.022f, -0.173f);
            puppet.transform.localEulerAngles = new Vector3(90, 0, 6);
        }
    }

    public void BeCatch()
    {
        puppet.mode = PuppetMaster.Mode.Active;
    }

    //布娃娃死亡状态
    public void SetDead() {
        puppet.state = PuppetMaster.State.Dead;
    }

    public void PuppetAddForce(Vector3 force) {
        puppetRg.AddForce(force, ForceMode.Impulse);
    }


    public override void Update()
    {
        base.Update();
        OcclusionLayer.Value = NavMeshSourceTag.IsInVolumeRange(transform.position, coll.radius);
        NowState = Mathf.Lerp(NowState, OcclusionLayer.Value ? 0 : 1, Time.deltaTime * 10);
        anim.SetFloat(ConstDefine.Anim.State, NowState);
      
        if (isThrowing)
        {
            if (transform.position.y < 0.02f || !Physics.Raycast(transform.position, Vector3.down, 10, 1 << LayerMask.NameToLayer("Ground")))
            {
                isThrowing = false;
                agent.enabled = true;
                rg.isKinematic = true;
            }
        }
    }
}
