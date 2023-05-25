using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class ChoicePlayerManager : MonoBehaviour
{
    public static ChoicePlayerManager Instance { get; private set; }

    public List<Transform> SnackPts;
    public List<Transform> ChildPts;

    
    private ColorDoTween ChildColorDT;

    private int playerSize;

    public BindableProperty<int> index;

    private Transform Spotlight;

    private void Start()
    {
        //循环关卡不再选人
        Destroy(gameObject);
        return;
        //
        if (LevelScenes.IsLoopLevel() == false )
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Spotlight = transform.Find(nameof(Spotlight));
        Spotlight.gameObject.SetActive(false);
        
        ChildColorDT = new ColorDoTween(GameManager.Instance.childPlayer.transform);

        playerSize = SnackPts.Count + ChildPts.Count;
        runTimer = new MyTimer(Random.Range(4f, 5f));
        index = new BindableProperty<int>();
        index.OnChangeBefore = () =>
        {
            if (index < 0)
                return;
            var control = GetColorControl();
            control.ChangeColor(GetColor(.2f), .2f);
        };
        index.OnChange = () =>
        {
            if (index < 0)
                return;
            var control = GetColorControl();
            control.ChangeDefaultColor(.2f);
            Spotlight.LookAt(control.transform);
        };
        Shader.SetGlobalColor("_XRayColor", Color.black);
        InitPlayer();
    }

    public List<GameObject> snakeItemList;
    public void InitSnakeItem()
    {
        for (int i = 0; i < 6; i++)
        {
            var pos = RandomPosManager.Get(1).RandomPos();
            var go = snakeItemList[Random.Range(0, snakeItemList.Count)];
            var go1 = Instantiate(go, pos, Quaternion.identity);
        }
    }

    /// <summary>
    /// 初始化人物
    /// </summary>
    public void InitPlayer()
    {

        List<BaseControl> allsnakeList = new List<BaseControl>();
        var snakel = GameManager.Instance.SnackList;
        for (int i = 0; i < snakel.Count; i++)
        {
            if (snakel[i].IsSnack)
                allsnakeList.Add(snakel[i]);
        }

        if (allsnakeList.Count == 0)
        {
            for (int i = 0; i < SnackPts.Count; i++)
            {
                GameObject go;
                if (i >= GameManager.Instance.SnackPrefabs.Count)
                    go = GameManager.Instance.InstanceSnackRandom();
                else
                    go = GameManager.Instance.InstanceSnackIndex(i);
                go.transform.position = SnackPts[i].position;
                go.transform.rotation = SnackPts[i].rotation;
                SnackPts[i] = go.transform;
            }
        }
        else
        {
            for (int i = 0; i < allsnakeList.Count; i++)
            {
                GameObject go = allsnakeList[i].gameObject;
                go.transform.position = SnackPts[i].position;
                go.transform.rotation = SnackPts[i].rotation;
                SnackPts[i] = go.transform;
            }
        }
        //InitSnakeItem();
    }

    public ColorDoTween GetColorControl()
    {
        var tindex = index.Value % playerSize;
        if (tindex < SnackPts.Count)
            return SnackPts[tindex].GetComponent<BaseControl>().ColorDT;
        return ChildColorDT;
    }


    public bool LocalPlayerIsChild = false;

    public void StartGame()
    {
        InitScenes();
        SetPlayerRandom();
        if (LocalPlayerIsChild)
            SetPlayerChild();
        this.AttachTimer(.5f, () => Open = true);
        Spotlight.gameObject.SetActive(true);

    }

    private void SetPlayerRandom()
    {
        randT = Random.Range(15, 21);
    }
    /// <summary>
    /// 选定自己为小孩
    /// </summary>
    private void SetPlayerChild()
    {
        randT = 14;
    }

    private Color GetColor(float value)
    {
        return new Color(value, value, value, 1);
    }

    public void InitScenes()
    {
        GameManager.Instance.LightColorDT(GetColor(.2f), .2f);
        Knuth_Durstenfeld(SnackPts);

        for (int i = 0; i < SnackPts.Count; i++)
        {
            SnackPts[i].GetComponent<BaseControl>()
                .ColorDT.ChangeColor(GetColor(.2f), .2f);
        }

        if (GameManager.Instance.childPlayer == null)
        {
            for (int i = 0; i < ChildPts.Count; i++)
            {
                GameObject go;
                if (i >= GameManager.Instance.ChildPrefabs.Count)
                    go = GameManager.Instance.InstanceChildRandom();
                else
                    go = GameManager.Instance.InstanceChildIndex(i);
                go.transform.position = ChildPts[i].position;
                go.transform.rotation = ChildPts[i].rotation;
                ChildPts[i] = go.transform;
                ChildPts[i].gameObject.SetActive(false);
            }
        }
        else
        {
            GameObject go = GameManager.Instance.childPlayer.gameObject;
            go.transform.position = ChildPts[0].position;
            go.transform.rotation = ChildPts[0].rotation;
            ChildPts[0] = go.transform;
        }

        ChildColorDT.ChangeColor(GetColor(.2f), .2f);

    }

    public void Knuth_Durstenfeld<T>(List<T> pukes)
    {
        for (int i = pukes.Count - 1; i > 0; --i)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = pukes[randomIndex];
            pukes[randomIndex] = pukes[i];
            pukes[i] = temp;
        }
    }

    private bool Open = false;

    private int randT ;
    private MyTimer runTimer;
    public AnimationCurve curve;

    public void Update()
    {
        if (!Open)
            return;
        runTimer.OnUpdate(Time.deltaTime);
        if (runTimer.IsFinish)
        {
            Open = false;
            this.AttachTimer(1.25f,()=>
            {
                PostProcessing.Instance.Blink(2f);
                this.AttachTimer(1, () => OnChoiceDown());
            });
            return;
        }

        var nowIndexF = randT * curve.Evaluate(runTimer.GetRatioComplete) ;
        var nowIndex = Mathf.RoundToInt(nowIndexF);
        index.Value = nowIndex;
    }

    /// <summary>
    /// 人物选择完成
    /// </summary>
    public void OnChoiceDown()
    {
        Spotlight.gameObject.SetActive(false);
        foreach (var child in ChildPts)
            child.gameObject.SetActive(true);

        GameManager.Instance.LightDefaultColorDT(.2f);
        ChildColorDT.ChangeDefaultColor(.2f);
        for (int i = 0; i < SnackPts.Count; i++)
        {
            SnackPts[i].GetComponent<BaseControl>()
                .ColorDT.ChangeDefaultColor( .2f);
        }

        var tindex = index.Value % playerSize;
        BaseControl control = null;
        //零食
        if (tindex < SnackPts.Count)
        {
            control = SnackPts[tindex].GetComponent<BaseControl>();

        }
        else
        {
            control = ChildPts[tindex - SnackPts.Count].GetComponent<BaseControl>();
            control.gameObject.SetActive(true);
        }


        //随机零食位置
        for (int i = 0; i < SnackPts.Count; i++)
            SnackPts[i].position = RandomPosManager.GetPos(0);
        Shader.SetGlobalColor("_XRayColor",new Color(0,1,1,0.375f));
        control.IsLocalPlayer.Value = true;

        this.AttachTimer(GameManager.Instance.localPlayer.IsSnack ? 1.5f : 0.01f , () => {
            FllowObject.Get(0).target = control.transform;
            UIPanelManager.Instance.PushPanel(UIPanelType.GamePanel);
        });

    }

}
