using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LvRule : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (LevelScenes.level == 1)
        {
            ParentControl parent = FindObjectOfType<ParentControl>();
            //parent.KnackDoor(true);
            var camTra = FllowObject.Get(0).transform;
            camTra.position = new Vector3(10f, 5, -1);
            camTra.eulerAngles = new Vector3(40, -180, 0);
            this.AttachTimer(1, () =>
            {
                FllowObject.Get(0).target = GameManager.Instance.localPlayer.transform;
                SetLocalPlayer.Instance.UseFllowNow = true;
                FllowObject.Get(0).transform.DORotate(FllowObject.Get(0).cameraRot, 1);
                CanvasGroup group = parent.GetComponentInChildren<CanvasGroup>();
                group.DOFade(0, 1).onComplete = () =>
                {
                    group.gameObject.SetActive(false);
                };
            });
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
