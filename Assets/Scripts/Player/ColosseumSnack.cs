using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CapsuleCollider))]
public class ColosseumSnack : BaseControl { 
    private Material mainMaterial;
    private CapsuleCollider coll;
    private GameObject PepperEffect;
    private Texture normalTex;
    public Texture dirtyTex;
    private Rigidbody rg;
    public Rigidbody Rg
    {
        get { return rg; }
    }
    [HideInInspector]
    public bool isThrowing = false;
    public override void Awake()
    {
        base.Awake();
        rg = gameObject.AddComponent<Rigidbody>();
        rg.isKinematic = true;
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
        normalTex = mainMaterial.GetTexture("_DiffuseTex");
    }

    public override void Start()
    {
        NowState = 1;
        anim.SetFloat(ConstDefine.Anim.State, NowState);
        base.Start();
        ColosseumMgr.Instance.SnackList.Add(this);
    }


    public override void Update()
    {
        base.Update();       
    }
}
