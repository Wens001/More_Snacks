using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCheck : MonoBehaviour
{

    private static GameObject prefab;

    private static GameObject coinPrefab
    {
        get
        {
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>("CoinEffect");
            }
            return prefab;
        }
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out BaseControl control))
        {
            if (!control.IsLocalPlayer)
                return;
            var go = PoolManager.SpawnObject(coinPrefab);
            go.transform.position = transform.position;
            go.transform.rotation = coinPrefab.transform.rotation;
            this.AttachTimer(1.5f, () => { PoolManager.ReleaseObject(go); });
            Messenger.Broadcast<int>(ConstDefine.Listener.AddCoinValue,1);
            Destroy(gameObject);
        }
    }
}
