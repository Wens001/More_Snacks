using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SphereCollider))]
public class SpeedTriggerBuff : TriggerBuff
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out BuffableEntity control))
        {

            control.TryGetComponent(out BaseControl player);

            if (!player || !player.IsLocalPlayer)
                return;
            control.AddBuff( new TimedSpeedBuff(GameManager.Instance.SpeedBuff , control.gameObject)   );
            Destroy(gameObject);
        }
    }
}
