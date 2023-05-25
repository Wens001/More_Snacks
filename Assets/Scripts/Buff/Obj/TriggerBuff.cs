using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBuff : MonoBehaviour
{
    public float LifeTime = -1;
    private MyTimer timer;
    // Start is called before the first frame update
    void Start()
    {
        if (LifeTime > 0) {
            timer = new MyTimer(LifeTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timer != null) {
            if (!timer.IsFinish)
            {
                timer.OnUpdate(Time.deltaTime);
            }
            else
            {
                Destroy(gameObject);
            }
        }      
    }

    public void NoDestroy() {
        LifeTime = -1;
        timer = null;
    }
}
