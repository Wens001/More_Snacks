using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScene : MonoBehaviour
{
    private bool hasclick;
    public Text MyText;
    // Start is called before the first frame update

    public void StartgameBtn() {
        if (hasclick)
            return;
        hasclick = true;
        MyText.text = "Has Click";
        LoadSceneSync();
    }

    private async void LoadSceneSync()
    {
        Debug.Log("BundleManager.Init!");
        await BundleManager.Init();
        Debug.Log("BundleManager.Init over!");
#if UNITY_EDITOR
        LevelScenes.level.Value = 1;
        LevelScenes.LoadScene();
#else
                //LevelScenes.level.Value = 9;
                LevelScenes.LoadScene();
#endif
    }
}
