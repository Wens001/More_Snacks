using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SphereCollider))]
public class KeyTrigger : TriggerBuff
{
    private void Start()
    {
        this.AttachTimer(0.1f, () =>
         {
             if (GameManager.abConfig.rv_optimized == 0)
             {
                 Destroy(gameObject);
             }
         });
        
    }
    private void OnTriggerEnter(Collider other) {       
        if (other.transform.TryGetComponent(out BaseControl control))
        {
            if (!control.IsLocalPlayer)
                return;
            GameData.PrizeKey++;
            Messenger.Broadcast<Vector3>(ConstDefine.Listener.GetKey,transform.position);
            Destroy(gameObject);
        }
    }
}
