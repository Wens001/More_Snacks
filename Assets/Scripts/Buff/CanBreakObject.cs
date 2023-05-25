using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanBreakObject : MonoBehaviour
{

    public BreakObjects breakObjects;

    public bool UseThisRotation;

    public bool isDestroy = true;

    public bool canBreak { get; set; } = true;
    public void Break(float delayTime)
    {
        if (!breakObjects)
            return;

        if (!canBreak)
            return;
        canBreak = false;

        this.AttachTimer(delayTime, () => {
            var go = Instantiate(breakObjects);
            go.transform.position = transform.position;

            if (!UseThisRotation)
                go.transform.rotation = breakObjects.transform.rotation;
            else
                go.transform.rotation = transform.rotation;

            //go.transform.localScale = breakObjects.transform.localScale;
            go.transform.localScale = transform.localScale;
            go.AddForce(Vector3.down * 500);//Vector3.down * 500
            go.SetTriggerAndDestroy(2.5f);
            if (isDestroy)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        });

    }

}
