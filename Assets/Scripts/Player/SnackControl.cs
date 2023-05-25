using DG.Tweening;
using GameAnalyticsSDK.Setup;
using HighlightingSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(CapsuleCollider))]
public class SnackControl : BaseControl
{ 
    public BindableProperty<NavMeshModifierVolume> OcclusionLayer ;
    private Material mainMaterial;
    private CapsuleCollider coll;
    private GameObject PepperEffect;
    private Texture normalTex;
    public Texture dirtyTex;
    private Rigidbody rg;
    public Rigidbody Rg {
        get { return rg; }
    }
    [HideInInspector]
    public bool isThrowing = false;
    public override void Awake()
    {
        base.Awake();
        TryGetComponent(out rg);
        if(rg==null)
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
        PepperEffect = transform.Find("PepperEffect").gameObject;
        //normalTex = mainMaterial.mainTexture;
        normalTex = mainMaterial.GetTexture("_DiffuseTex");
    }

    public override void Start()
    {
        NowState = 1;
        anim.SetFloat(ConstDefine.Anim.State, NowState);
        if(GameManager.sceneType == SceneType.Level)
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

    public void SetDirtyMat() {
        IsDirty = true;
        mainMaterial.SetTexture("_DiffuseTex", dirtyTex);//Resources.Load<Texture>("UI/Donut_dirty")
    }
    public void SetNormalMat() {
        IsDirty = false;
        mainMaterial.SetTexture("_DiffuseTex", normalTex);
    }

    public void ThrowBefore() {
        agent.enabled = false;
        rg.isKinematic = true;
    }

    public void BeThrow(Vector3 _force) {
        rg.isKinematic = false;
        rg.AddForce(_force);
        isThrowing = true;
    }

    public override void Update()
    {
        base.Update();
        OcclusionLayer.Value = NavMeshSourceTag.IsInVolumeRange(transform.position, coll.radius);
        NowState = Mathf.Lerp(NowState, OcclusionLayer.Value ? 0 : 1, Time.deltaTime * 10);
        anim.SetFloat(ConstDefine.Anim.State, NowState);
        PepperEffect.SetActive(PlayerHealth == 2 && !IsDirty);

        mainMaterial.SetColor("_DiffuseColor", 
            Color.Lerp( mainMaterial.GetColor("_DiffuseColor"), PlayerHealth == 2 && !IsDirty ? Color.red : Color.white , Time.deltaTime * 5 ) );
        if (isThrowing) {
            if (transform.position.y < 0.02f || !Physics.Raycast(transform.position, Vector3.down, 10,1<< LayerMask.NameToLayer("Ground"))) {
                isThrowing = false;
                agent.enabled = true;
                rg.isKinematic = true;
            }
        }
    }
}
