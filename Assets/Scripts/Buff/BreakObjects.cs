using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakObjects : MonoBehaviour
{

    private Collider[] mcs;//MeshCollider
    private Rigidbody[] rigis;

    private void Awake()
    {
        mcs = GetComponentsInChildren<Collider>();
        rigis = GetComponentsInChildren<Rigidbody>();

        
    }

    public void AddForce(Vector3 force)
    {
        foreach (var rigi in rigis)
        {
            rigi.AddForce(force);
        }
    }

    private bool isSetTrigger = false;
    public void SetTriggerAndDestroy(float desTime = 2)
    {
        if (isSetTrigger)
            return;
        this.AttachTimer(2f,
            () => 
            {
                isSetTrigger = true;
                foreach (var mc in mcs)
                {
                    mc.isTrigger = true;
                }
                this.AttachTimer(desTime, () => Destroy(gameObject));
            }
        );
    }


#if UNITY_EDITOR
    [ContextMenu("OnEditorSetUp")]
    void OnEditorSetUp()
    {
        var mrs = GetComponentsInChildren<MeshRenderer>();

        foreach (var mr in mrs)
        {
            Rigidbody rigi = null;
            rigi = mr.gameObject.GetComponent<Rigidbody>();
            if (rigi == null)
                rigi = mr.gameObject.AddComponent<Rigidbody>();


            MeshCollider coll = null;
            coll = mr.gameObject.GetComponent<MeshCollider>();
            if (coll == null)
                coll = mr.gameObject.AddComponent<MeshCollider>();
            coll.convex = true;
        }

    }
#endif
}
