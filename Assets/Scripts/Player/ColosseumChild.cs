using System.Collections.Generic;
using UnityEngine;

public class ColosseumChild : BaseControl, IHideable
{
    private Transform trans;
    private SkinnedMeshRenderer[] smrs;
    private int ChildIndex = 0;
    //不检查
    [HideInInspector]
    public bool noCheck = false;
    public bool hasSkirt = false;

    public override void Awake()
    {
        trans = transform;
        smrs = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        base.Awake();
        IsSnack = false;
        GameManager.Instance.childPlayer = this;
        rightHandRoot.GetChild(0).gameObject.SetActive(false);
        ChildIndex = 0;
    }

    public override void Start()
    {
        ChildIndex = GameManager.Instance.ChildCount;
        base.Start();
        ColosseumMgr.Instance.ChildList.Add(this);
    }

    private float FatValue;
    public float GetFatValue
    {
        get { return FatValue; }
    }
    private float tempValue;
    public void SetFatValue(float value)
    {
        FatValue = Mathf.Clamp(FatValue + value, 0, 100f);
    }

    private void ToFatUpdate()
    {
        tempValue = Mathf.Lerp(tempValue, FatValue, Time.deltaTime * 5);
        for (int i = 0; i < smrs.Length; i++)
        {
            smrs[i].SetBlendShapeWeight(0, tempValue);
        }
    }


    public override void EatSmallSnake(GameObject target)
    {
        base.EatSmallSnake(target);
        SetFatValue(5f);
    }


    Collider[] hitSnackCols = new Collider[5];
    public Vector3 halfExtent = new Vector3(.5f, .5f, .5f);
    LayerMask checklayer = 1<<9;//LayerMask.GetMask("Player")
    
    void EatCheckUpdate() {
        if (ColosseumMgr.Instance.IsGameOver)
            return;
        var hitNum = Physics.OverlapBoxNonAlloc(trans.position, halfExtent, hitSnackCols, trans.rotation, checklayer);
        for (int i = 0; i < hitNum; i++) {
            var snackCont = hitSnackCols[i].GetComponent<ColosseumSnack>();
            if (snackCont != null) {
                if (!snackCont.IsSnack)
                    continue;
                anim.SetTrigger("ToEat");
                AudioManager.Instance.PlaySound(Random.Range(1, 3));
                var model = hitSnackCols[i].transform;
                model.GetComponent<BaseControl>().agent.enabled = false;
                model.SetParent(rightHandRoot);
                model.localPosition = Vector3.zero;
                model.localRotation = Quaternion.identity;
                hitSnackCols[i].enabled = false;
                ColosseumMgr.Instance.SnackList.Remove(snackCont);
                this.AttachTimer(eatTime, () =>
                {
                    model.gameObject.SetActive(false);
                    Messenger.Broadcast<BaseControl, BaseControl>(ConstDefine.Listener.EatSnake, this, snackCont);
                    SetFatValue(33.33f);
                    Debug.Log("吃掉");
                });             
            }
        }
    }
    public override void Update()
    {
        //base.Update();
        EatCheckUpdate();
        ToFatUpdate();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, halfExtent * 2);
    }
}
