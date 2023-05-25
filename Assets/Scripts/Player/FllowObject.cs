using DG.Tweening;
using GameAnalyticsSDK.Setup;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
public class FllowObject : MonoBehaviour
{
    public Transform startLookPoint;
    public static Camera cam;
    private static Dictionary<int, FllowObject> fllowObjectDic
        = new Dictionary<int, FllowObject>();
    public bool hasCamAnim = true;

    public static void Add(FllowObject fllowObject)
    {
        if (!fllowObjectDic.ContainsKey(fllowObject.InstanceIndex))
            fllowObjectDic.Add(fllowObject.InstanceIndex, fllowObject);
    }

    public static FllowObject Get(int index)
    {
        if (fllowObjectDic.ContainsKey(index))
            return fllowObjectDic[index];
        return null;
    }

    public static void Remove(FllowObject fllowObject)
    {
        if (fllowObjectDic.ContainsKey(fllowObject.InstanceIndex))
            fllowObjectDic.Remove(fllowObject.InstanceIndex);
    }

    public Transform target;

    public Vector3 childOffset = new Vector3(0, 5.5f, 3.4f);
    public Vector3 snakeOffset = new Vector3(0, 6.5f, 3.6f);
    public Vector3 cameraRot = new Vector3(55, -180, 0);
    public int InstanceIndex;
    public bool UseFllow { get; set; } = true;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        CameraSetting cameraSetting = GetComponent<CameraSetting>();
        if (cameraSetting)
        {
            childOffset = cameraSetting.CurChildOffset();
            snakeOffset = cameraSetting.CurSnackOffset();
            transform.localEulerAngles = cameraSetting.CurCameraEuler();
        }
        else {
            //snakeOffset = childOffset;
        }
        Add(this);
    }
    public static void StartView() {
        if(Get(0).hasCamAnim)
            cam.DOFieldOfView(90, 1);
    }
    //void Start()
    //{
        
    //}

    private void OnDestroy()
    {
        Remove(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (UseFllow && GameManager.Instance.IsGameOver == false)
            Fllow();
    }
    [Range(1,15)]
    public float speed = 15;

    public void FllowNow()
    {
        var tpos = target.position +
            (GameManager.Instance.localPlayer.IsSnack ? snakeOffset : childOffset);
        transform.position = tpos ;
        transform.eulerAngles = cameraRot;
    }

    [ContextMenu("Fllow")]
    private void Fllow()
    {
        if (!target || !GameManager.Instance.localPlayer)
            return;
        var tpos = target.position +
            (GameManager.Instance.localPlayer.IsSnack ? snakeOffset : childOffset);
        transform.position = Vector3.Lerp(transform.position, tpos, Time.deltaTime * speed);
        transform.eulerAngles = cameraRot;
    }

    public static void MoveToTarget() {
        Get(0).UseFllow = true;
        if (!Get(0).hasCamAnim)
            return;       
        cam.DOKill();
        cam.DOFieldOfView(60, 0.6f).SetEase(Ease.InQuad).onComplete=()=>{
            //cam.DOFieldOfView(60, 1).SetEase(Ease.OutSine);
        };
    }

    public void FailEndCamPoint() {
        var tpos = target.position + new Vector3(0, 3, 2.2f);
        transform.DOMove(tpos, 2);        
        transform.eulerAngles = new Vector3(45,180,0);
    }

    public void StrongAnim() {
        UseFllow = false;
        cam.fieldOfView = 30;
        transform.localPosition = target.position + new Vector3(0, 6, 7);
        transform.eulerAngles = new Vector3(40, 180, 0);
    }
}
