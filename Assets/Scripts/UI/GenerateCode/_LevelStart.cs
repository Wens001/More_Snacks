using LionStudios;
using LionStudios.Runtime.Sdks;
using MoreMountains.NiceVibrations;
using MyUI;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class _LevelStart : MonoBehaviour
{

#if UNITY_EDITOR
    [Range(-5,39)]
    public int targetLevel;
#endif
    public GameObject debugConsolePrefab;
    public Image loading;
    public Text processText;

    void Awake()
    {
        Debug.Log("_levelStart Awake");
        DontDestroyOnLoad(gameObject);
        if (GameData.ShowDebug == 1) {
            Instantiate(debugConsolePrefab);
        }
        _ = AdsManager.Instance;
        //if (LionKit.IsInitialized == false)
        //{
        //    LionKit.OnInitialized += () => {
        //        Debug.Log("<color=green>LionKit Init</color>");
        //    };
        //    LionKit.Initialize();
        //}
        //if (AppLovin.IsInitialized == false)
        //{
        //    AppLovin.WhenInitialized(() => { 
        //        //AdsManager.OnMaxSdkInitizalized();
        //        //AdsManager.ShowBanner();
        //    });
        //    //AppLovin.Initialize();
        //}
       
        Application.targetFrameRate = 60;
        MMVibrationManager.iOSInitializeHaptics();
    }

    private IEnumerator Start()
    {
        Debug.Log("loadingBar start");
        for (float tt = 0; tt < 1; tt += Time.deltaTime)
        {
            loading.fillAmount = tt;
            processText.text = (tt * 100).ToString("F0")+"%";
            yield return null;
        }
        Debug.Log("loadingBar over!");
        while (true)
        {
            if (AdsManager.HasInitLionSdk)
            {
                //yield return new WaitForSeconds(0.2f);
                LoadSceneSync();
                Debug.Log("Goto Game!");
                yield break;
            }
            yield return null;

        }
    }


    private async void LoadSceneSync()
    {
        await BundleManager.Init();
#if UNITY_EDITOR
        LevelScenes.level.Value = targetLevel;
        LevelScenes.LoadScene();
#else
                //LevelScenes.level.Value = 9;
                LevelScenes.LoadScene();
#endif
    }  

    public void NextScene() {
        StartCoroutine(IGoNextGame());
    }

    IEnumerator IGoNextGame()
    {
        for (float tt = 0; tt < 0.9f; tt += Time.deltaTime * 0.3f)
        {
            loading.fillAmount = tt;
            yield return null;
        }
        while (!AdsManager.HasInitLionSdk)
        {
            yield return null;
        }
        for (float tt = 0.9f; tt < 1; tt += Time.deltaTime * 0.5f)
        {
            loading.fillAmount = tt;
            yield return null;
        }
#if UNITY_EDITOR
        LevelScenes.level.Value = targetLevel;
        LevelScenes.LoadScene();
#else
                LevelScenes.LoadScene();
#endif
    }

    //private float delayPlay = 0f;
    //private void Update()
    //{
    //    if (GameManager.IsDebug && SettingCanvas.isTestBanner)
    //    {
    //        return;
    //    }
    //    //delayPlay += Time.deltaTime;
    //    //if (delayPlay >= 5f)
    //    //{
    //    //    AdsManager.ShowBanner();
    //    //    delayPlay = 0;
    //    //}
    //}

}
