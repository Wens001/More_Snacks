using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestGetToggle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Toggle aa;
        TryGetComponent(out aa);
        if (aa == null)
        {
            Debug.Log("no toggle");
        }
        else {
            Debug.Log("has toggle");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
