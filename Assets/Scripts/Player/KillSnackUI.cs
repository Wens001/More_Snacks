using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class KillSnackUI : MonoBehaviour
{
    public static List<KillSnackUI> killSnackUIList ;

    

    public Transform fllowObject;

    private Text value;
    public int KillSize { get; protected set; }
    private CanvasGroup canvasGroup;
    private MyTimer myTimer = new MyTimer(1f);
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        value = transform.Find("value").GetComponent<Text>();
        KillSize = 0;
        value.text = KillSize.ToString();
        if (killSnackUIList == null)
        {
            killSnackUIList = new List<KillSnackUI>
            {
                null,
                null
            };
        }
        
    }

    public void SetFollowTarget(Transform _target) {
        fllowObject = GameManager.Instance.localPlayer.transform;
    }

    private void OnEnable()
    {
        Messenger.AddListener<BaseControl, BaseControl>(ConstDefine.Listener.EatSnake, EatSnakeCallBack);
        if (!fllowObject.name.Contains("New")) {
            //killSnackUIList[0] = this;
            //因为有换皮肤，所以要等到换了皮肤才重新赋值
            this.AttachTimer(0.1f, () =>
            {
                fllowObject = GameManager.Instance.localPlayer.transform;
                killSnackUIList[0] = this;
            });
        }        
        else
            killSnackUIList[1] = this;
    }

    public static void SetKillUIFllowObj(Transform target) {
        if(killSnackUIList!=null&& killSnackUIList[0])
            killSnackUIList[0].fllowObject = target;
    }

    private void OnDisable()
    {
        Messenger.RemoveListener<BaseControl, BaseControl>(ConstDefine.Listener.EatSnake, EatSnakeCallBack);

    }

    private void EatSnakeCallBack(BaseControl killer,BaseControl snack)
    {
        if (killer.transform != fllowObject.transform)
            return;
        KillSize++;
        value.text = KillSize.ToString();
    }

    void Update()
    {
        if (fllowObject == null)
        {
            gameObject.SetActive(false);
            return;
        }
        myTimer.OnUpdate(Time.deltaTime * GameManager.Instance.GameSpeed);
        if (myTimer.IsFinish )
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha,.9f,Time.deltaTime * 10);

        transform.position = fllowObject.position + Vector3.up * 2.2f;
    }
}
