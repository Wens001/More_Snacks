using MyUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGuide : MonoBehaviour
{

//#if UNITY_EDITOR
    public int RunToLevel;
//#endif

    private LevelData1 levelData1 ;
    private LevelData2 levelData2 ;
    private LevelData3 levelData3 ;
    private LevelData4 levelData4;
    private LevelData_10 levelData13;

    private void Awake()
    {
//#if UNITY_EDITOR
//        if (RunToLevel != 0)
//            LevelScenes.level.Value = RunToLevel;
//#endif
    }

    IEnumerator Start()
    {
        yield return null;
        levelData1 = new LevelData1();
        levelData2 = new LevelData2();
        levelData3 = new LevelData3();
        levelData4 = new LevelData4();
        levelData13 = new LevelData_10();
        //var level = LevelScenes.GetLevelIndex(LevelScenes.level);
        var level = RunToLevel;
        levelData1.Init(level);
        levelData2.Init(level);
        levelData3.Init(level);
        levelData4.Init(level);
        levelData13.Init(level);
    }

    void Update()
    {
        levelData1?.Update();
        levelData2?.Update();
        levelData3?.Update();
        levelData4?.Update();
        levelData13?.Update();
    }

    private void OnDestroy()
    {
        levelData1?.OnDestroy();
        levelData2?.OnDestroy();
        levelData3?.OnDestroy();
        levelData4?.OnDestroy();
        levelData13?.OnDestroy();
    }

    public interface levelData
    {
        bool isinit { get; set; }
        void Init(int level);
        void Update();
        void OnDestroy();
    }

    public class LevelData1 : levelData
    {
        public bool isinit { get ; set; }

        public void Init(int level)
        {
            if (level != 1)
                return;
            isinit = true;
        }

        public void OnDestroy()
        {
            isinit = false;
            myTimer = null;
        }

        private MyTimer myTimer;
        public void Update()
        {
            if (isinit == false)
                return;

            if ( GameManager.Instance.GameSpeed >= .8f && Input.GetMouseButton(0) && myTimer == null)
            {
                myTimer = new MyTimer(.05f);
                return;
            }

            if (myTimer != null)
            {
                myTimer.OnUpdate(Time.deltaTime );
                if (myTimer.IsFinish)
                {
                    var panel = UIPanelManager.Instance.PushPanel(UIPanelType.GuidePanel);
                    (panel as GuidePanel).SetGuideString(0);
                    OnDestroy();
                }
            }
        }

    }

    public class LevelData2 : levelData
    {
        public bool isinit { get; set; }

        public void Init(int level)
        {
            if (level != 2)
                return;
            isinit = true;
            Messenger.AddListener<BaseControl>(ConstDefine.Listener.SnakeHide, OnSnakeHide);
            Messenger.AddListener<GameObject>(ConstDefine.Listener.ChargeObject, OnChargeObject);
        }

        public void OnSnakeHide(BaseControl control)
        {
            var panel = UIPanelManager.Instance.PushPanel(UIPanelType.GuidePanel);
            (panel as GuidePanel).SetGuideString(4);
            
        }

        public void OnChargeObject(GameObject target)
        {
            UIPanelManager.Instance.PopPanel();
            OnDestroy();
        }

        public void OnDestroy()
        {
            if (isinit == false)
                return;
            isinit = false;
            Messenger.RemoveListener<BaseControl>(ConstDefine.Listener.SnakeHide, OnSnakeHide);
            Messenger.RemoveListener<GameObject>(ConstDefine.Listener.ChargeObject, OnChargeObject);
        }

        public void Update(){}

    }


    public class LevelData3 : levelData
    {
        public bool isinit { get; set; }

        private MyTimer timer;
        public void Init(int level)
        {
            if (level != 3 )
                return;
            isinit = true;
            timer = new MyTimer(1);
        }

        public void OnDestroy()
        {
            if (isinit == false)
                return;
            isinit = false;
        }

        public void Update() 
        {
            if (isinit == false)
                return;
            if (GameManager.Instance.GameSpeed <= .3f)
                return;

            timer.OnUpdate(Time.deltaTime);
            if (timer.IsFinish)
            {
                var panel = UIPanelManager.Instance.PushPanel(UIPanelType.GuidePanel);
                (panel as GuidePanel).SetGuideString(1);
                OnDestroy();
            }
            GameManager.Instance.childPlayer.agent.enabled = timer.IsFinish;

        }

    }

    public class LevelData4 : levelData
    {
        public bool isinit { get; set; }

        public void Init(int level)
        {
            if (level != 4)
                return;
            isinit = true;
            Messenger.AddListener<BaseControl>(ConstDefine.Listener.SnakeHide, OnSnakeHide);
            Messenger.AddListener<GameObject>(ConstDefine.Listener.ChargeObject, OnChargeState);
        }

        // 0 默认
        // 1 藏于桌子底下
        // 2 被掀开
        private int state;

        public void OnSnakeHide(BaseControl control)
        {
            var panel = UIPanelManager.Instance.PushPanel(UIPanelType.GuidePanel);
            (panel as GuidePanel).SetGuideString(3);
            Messenger.RemoveListener<BaseControl>(ConstDefine.Listener.SnakeHide, OnSnakeHide);
            state = 1;
        }

        public void OnChargeState(GameObject obj)
        {
            var panel = UIPanelManager.Instance.PushPanel(UIPanelType.GuidePanel);
            (panel as GuidePanel).SetGuideString(2);
            state = 2;
            Messenger.RemoveListener<GameObject>(ConstDefine.Listener.ChargeObject, OnChargeState);
            OnDestroy();
        }

        public void OnDestroy()
        {
            if (isinit == false)
                return;
            isinit = false;
            if (state < 1)
                Messenger.RemoveListener<BaseControl>(ConstDefine.Listener.SnakeHide, OnSnakeHide);
            if (state < 2)
                Messenger.RemoveListener<GameObject>(ConstDefine.Listener.ChargeObject, OnChargeState);
        }

        public void Update()
        {
            
        }
    }

    public class LevelData_10 : levelData
    {
        public bool isinit { get ; set; }

        public void Init(int level)
        {
            if (level != -10 )
                return;
            isinit = true;
            
            //FllowObject.Get(0).fllow();
        }

        public void OnDestroy()
        {
            
        }
        private float delayTime = 0f; 
        private bool isSend = false;
        public void Update()
        {
            if (!isinit)
                return;

            delayTime += Time.deltaTime;
            if (delayTime >= 2f || GameManager.Instance.GameSpeed > .5f)
                FllowObject.Get(0).target = SetLocalPlayer.Instance.control.transform;

            if (isSend || GameManager.Instance.GameSpeed <= .5f)
                return;
            isSend = true;
            var panel = UIPanelManager.Instance.PushPanel(UIPanelType.GuidePanel);
            (panel as GuidePanel).SetGuideString(5);
            Messenger.Broadcast(ConstDefine.Listener.InfiniteTime);


        }
    }

}
