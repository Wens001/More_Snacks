using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBuffPoint : MonoBehaviour
{
    [System.Serializable]
    public struct BuffType {
        public GameObject buff;
        public bool canShow;
    }
    private static readonly List<Transform> PointsList = new List<Transform>();
    private const int MaxBuffCount = 2;
    private static int RunLogicPoint;
    [Header("是否固定出，是的话：勾ShowBuffList里的")]
    public bool isUseNewBuild = true;
    public float refreshTime = 6;
    [Header("小孩阵营出的buff")]
    public List<GameObject> childBuffList;
    [Header("零食阵营出的buff")]
    public List<GameObject> snackBuffList;
    [SerializeField]
    [Header("只有勾上才随机出的buff")]
    public List<BuffType> ShowBuffList;
    //public GameObject HammerPrefab;
    //public GameObject PepperPrefab;
    //public GameObject SpeedPrefab;
    //public GameObject BaseBallPrefab;
    //public GameObject WrenchPrefab;
    private static int hasBuffCount = 0;
    private GameObject curBuff;
    private bool lastBuff;
#if UNITY_EDITOR
    public int nowBuffCount = 0;
#endif

    private int index;
    private MyTimer myTimer ;
    private void Awake()
    {
        hasBuffCount = 0;
        curBuff = null;
        lastBuff = false;
        PointsList.Clear();
        curBuff = null;       
    }

    private void Start()
    {
        PointsList.Add(transform);
        index = PointsList.Count - 1;
        //RunLogicPoint = Random.Range(0, PointsList.Count);
        //if (GameEndPodium.Instance) {
        //    myTimer = new MyTimer(5);
        //}
        if (refreshTime > 0)
        {
            myTimer = new MyTimer(refreshTime);
        }
    }
    private void OnEnable()
    {
        Messenger.AddListener(ConstDefine.Listener.StartGame, NowCreateBuff);
    }

    private void OnDisable()
    {
        Messenger.RemoveListener(ConstDefine.Listener.StartGame, NowCreateBuff);
    }

    public void NowCreateBuff() {
        if (refreshTime <= 0)
        {
            CreateBuff();
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (index == 0)
            nowBuffCount = hasBuffCount;
#endif
        if (curBuff == null && lastBuff) {
            hasBuffCount--;
        }
        lastBuff = curBuff==null?false:true;
        //if (index != RunLogicPoint)
        //    return;
        if (myTimer == null)
            return;
        if(curBuff ==null)
            myTimer.OnUpdate(GameManager.Instance.GameSpeed * Time.deltaTime);

        if (myTimer.IsFinish)
        {
            if (hasBuffCount < MaxBuffCount && curBuff == null) {
                CreateBuff();               
            }
            myTimer.ReStart();
        }

    }

    private void CreateBuff()
    {
        var player = GameManager.Instance.localPlayer;
        if (!isUseNewBuild)
        {
            if (!player.IsSnack)
            {
                //if (Random.value > .4f)
                //    Instantiate(SpeedPrefab, transform.position, transform.rotation);
                //else
                //    Instantiate(HammerPrefab, transform.position, transform.rotation);
                if(childBuffList.Count>0)
                    curBuff = Instantiate(childBuffList[Random.Range(0, childBuffList.Count)], transform.position, transform.rotation);
            }
            else
            {
                //if (Random.value > .4f)
                //    Instantiate(SpeedPrefab, transform.position, transform.rotation);
                //else
                //    Instantiate(PepperPrefab, transform.position, transform.rotation);
                if(snackBuffList.Count>0)
                    curBuff = Instantiate(snackBuffList[Random.Range(0, snackBuffList.Count)], transform.position, transform.rotation);
            }
        }
        else {
            List<GameObject> buffList = new List<GameObject>();
            for (int i = 0; i < ShowBuffList.Count; i++) {
                if (ShowBuffList[i].canShow) {
                    buffList.Add(ShowBuffList[i].buff);
                }
            }
            curBuff = Instantiate(buffList[Random.Range(0, buffList.Count)], transform.position, transform.rotation);
            buffList = null;
        }
        hasBuffCount++;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var temp = transform.localPosition;
        temp.y = 0;
        transform.localPosition = temp;
        transform.localScale = Vector3.one;

        Gizmos.color = Color.Lerp(Color.white,Color.red , Mathf.PingPong(Time.time, .2f) / .2f) ;
        Gizmos.DrawWireCube(transform.position + Vector3.up * .16f, Vector3.one * .3f);
        Gizmos.color = Color.white;
    }
#endif

}
