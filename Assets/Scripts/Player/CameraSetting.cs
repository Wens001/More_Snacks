using LionStudios.GDPR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetting : MonoBehaviour
{
    [Serializable]
    public struct CamParm{
        public Vector3 camEuler;
        public Vector3 childOffset;
        public Vector3 snackOffset;
    }
    public Camera cam;
    public int parmId;
    public CamParm[] camParms;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    [ContextMenu("SetCamParm")]
    void SetCamParm() {
        if (cam == null) {
            cam = transform.GetComponentInParent<Camera>();
        }
        if (cam == null) {
            Debug.LogWarning("No get cam");
            return;
        }
            
        if (parmId >= 0 && parmId < camParms.Length) {
            FllowObject fllowObject = cam.GetComponent<FllowObject>();
            if (fllowObject)
            {
                fllowObject.childOffset = camParms[parmId].childOffset;
                fllowObject.snakeOffset = camParms[parmId].snackOffset;
                cam.transform.localEulerAngles = camParms[parmId].camEuler;
                Debug.Log("set cam parm OK");
            }
            else {
                Debug.LogWarning("set cam parm error");
            }
        }
    }

    public Vector3 CurChildOffset() {
        return camParms[parmId].childOffset;
    }
    public Vector3 CurSnackOffset()
    {
        return camParms[parmId].snackOffset;
    }
    public Vector3 CurCameraEuler()
    {
        return camParms[parmId].camEuler;
    }
}
