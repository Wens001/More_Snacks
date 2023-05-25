using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SphereCollider))]
public class PepperTriggerBuff : TriggerBuff
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out BaseControl control) && control.IsSnack )
        {
            if (!control.IsLocalPlayer)
                return;
            control.PlayerHealth = 2;
            Destroy(gameObject);
        }
    }

}
