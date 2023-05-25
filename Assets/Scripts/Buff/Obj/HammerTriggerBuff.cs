using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SphereCollider))]
public class HammerTriggerBuff : TriggerBuff
{
    public EWeapon weaponType;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out ChildControl control) )
        {
            if (!control.IsLocalPlayer)
                return;
            control.WeapomType.Value = weaponType;
            if(control.IsHammer.Value)
                control.IsHammer.Value = false;
            control.IsHammer.Value = true;
            Destroy(gameObject);
        }
    }
}
