using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QiuQianListener : MonoBehaviour
{
    
    private void OnEnable()
    {
        Messenger.AddListener<GameObject>(ConstDefine.Listener.ChargeObject, OnChargeObject);
    }

    private void OnDisable()
    {
        Messenger.RemoveListener<GameObject>(ConstDefine.Listener.ChargeObject, OnChargeObject);
    }

    public GameObject listenerGo;

    public Rigidbody[] rigis;

    private void Awake()
    {
        foreach (var rigi in rigis)
        {
            rigi.isKinematic = true;
        }
    }

    private void OnChargeObject(GameObject go)
    {
        if (go.transform.parent != null)
            go = go.transform.parent.gameObject;
        if (go != listenerGo)
            return;
        foreach (var rigi in rigis)
        {
            rigi.isKinematic = false;
        }
        rigis[rigis.Length - 1].AddForce((Vector3.up * 3 + 
            GameManager.Instance.childPlayer.transform.forward + Random.insideUnitSphere * .6f) * 600);
    }

}
