using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrorAnimator : MonoBehaviour
{

    private Vector3 pos;

    void Start()
    {
        pos = transform.position;
    }

    public float minY = 1.3f;
    public float maxY = 1.7f;
    public float speed = 3.5f;

    // Update is called once per frame
    void Update()
    {
        transform.position = pos + Vector3.up *
                Mathf.Lerp(minY, maxY, Mathf.Abs(Mathf.Sin(Time.time * speed)));
    }
}
