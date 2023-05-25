using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowWinCheck : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.IsGameOver)
            return;
        if (other.TryGetComponent(out BaseControl control))
        {
            if (control.IsSnack)
            {
                GameManager.Instance.GameWin();
            }
        }
    }
}
