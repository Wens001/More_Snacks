using GameAnalyticsSDK;
using GameAnalyticsSDK.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUI
{
    public partial class TestGA : MonoBehaviour, BasePanel
    {
        public Transform trans => transform;

        public void OnEnter()
        {

        }

        public void OnExit()
        {

        }

        public void OnPause()
        {

        }

        public void OnResume()
        {
        }

        void Awake() {
            GameAnalytics.Initialize();
            GetBindComponents(gameObject);
            DontDestroyOnLoad(gameObject);
        }
        // Start is called before the first frame update
        void Start()
        {
            m_Btn_Start.onClick.AddListener(() => {
                Debug.Log("onlick start");
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "World01", "Level"+ LevelScenes.level.ToString()); 
            });
            m_Btn_Complete.onClick.AddListener(() => {
                Debug.Log("onlick Complete"); 
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "World01", "Level" + LevelScenes.level.ToString());
            });
            m_Btn_Fail.onClick.AddListener(() => { 
                Debug.Log("onlick Fail");
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "World01", "Level" + LevelScenes.level.ToString()); 
            });          
        }


        void TestStart()
        {

        }

        void TestComplete() { 
        
        }

        void TestFail() { 
        
        }

        void TestReStart() { 
        
        }
    }
}
