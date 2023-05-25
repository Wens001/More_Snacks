using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountryUI : KillSnackUI
{
    private Text value;
    private CanvasGroup canvasGroup;
    private MyTimer myTimer = new MyTimer(1f);
    public Image countryIcon;
    public Text childNameTxt;
    //public static Sprite[] AllCountrySp;
    //public static string[] AllchildNameStrs = { "Cheems", "Tony", "Andy", "Frank", "Wang", "Li", "Smith", "Johnson", "Williams", "James", "Linda" };
    public Sprite countrySp;
    public string childName;

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

    private void OnEnable()
    {
        Messenger.AddListener<BaseControl, BaseControl>(ConstDefine.Listener.EatSnake, EatSnakeCallBack);
        int countryId = -1;
        if (!fllowObject.name.Contains("New"))
        {
            //killSnackUIList[0] = this;
            //因为有换皮肤，所以要等到换了皮肤才重新赋值
            this.AttachTimer(0.1f, () =>
            {
                fllowObject = GameManager.Instance.localPlayer.transform;
                killSnackUIList[0] = this;               
            });
            countryId = (int)Country.Usa;
        }
        else {
            killSnackUIList[1] = this;
        }
        
        var info = CountryManager.Instance.GetInfo(fllowObject.GetComponent<BaseControl>(), countryId);
        countrySp = info.countryIcon;
        countryIcon.sprite = countrySp;
        childName = info.name;
        childNameTxt.text = childName;
    }

    private void OnDisable()
    {
        Messenger.RemoveListener<BaseControl, BaseControl>(ConstDefine.Listener.EatSnake, EatSnakeCallBack);

    }

    private void EatSnakeCallBack(BaseControl killer, BaseControl snack)
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
        if (myTimer.IsFinish)
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, .9f, Time.deltaTime * 10);

        transform.position = fllowObject.position + Vector3.up * 2.2f;
    }
}
