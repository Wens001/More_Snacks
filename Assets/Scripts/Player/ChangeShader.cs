using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeShader : MonoBehaviour
{
    public Shader targetShader;
    private Shader orginShader;

    public void ChangeTargetShader(bool changeAll = true) {
        if (changeAll)
        {
            Transform[] temp = GetComponentsInChildren<Transform>();
            foreach (Transform child in temp)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Player");
                var rend = child.GetComponent<Renderer>();
                if (rend)
                    rend.material.shader = targetShader;
            }
        }
        else {
            var rend = GetComponent<Renderer>();
            if (rend != null) {
                gameObject.layer = LayerMask.NameToLayer("Player");
                rend.material.shader = targetShader;
            }
        }
    }

}
