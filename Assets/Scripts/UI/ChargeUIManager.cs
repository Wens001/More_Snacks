using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeUIManager : MonoBehaviour
{

    private List<GameObject> ChargeUIList;
    private List<Transform> fillList;

    async void Start()
    {
        ChargeUIList = new List<GameObject>();
        fillList = new List<Transform>();
        await new WaitForEndOfFrame();
        await new WaitForEndOfFrame();
        var snakeList = GameManager.Instance.SnackList;
        var prefab = Resources.Load<GameObject>("ChargeUI");
        for (int i = 0; i < snakeList.Count; i++)
        {
            if (!snakeList[i].IsSnack)
            {
                ChargeUIList.Add(Instantiate(prefab));
                fillList.Add(ChargeUIList[ChargeUIList.Count - 1].transform.Find("fill"));
                ChargeUIList[ChargeUIList.Count - 1].gameObject.SetActive(false);
                SetScaleX(fillList[fillList.Count - 1], 0);
            }
        }

        Messenger.AddListener<int,bool, Vector3>(ConstDefine.Listener.ChargeState, ChargeStateListener);
        Messenger.AddListener<int,float, Vector3>(ConstDefine.Listener.ChargeValue, ChargeValueListener);
    }

    private void SetScaleX(Transform target , float value)
    {
        if (target == null)
            return;
        var scale = target.localScale;
        scale.x = value;
        target.localScale = scale;
    }
    
    private void OnDisable()
    {
        Messenger.RemoveListener<int,bool, Vector3>(ConstDefine.Listener.ChargeState, ChargeStateListener);
        Messenger.RemoveListener<int,float, Vector3>(ConstDefine.Listener.ChargeValue, ChargeValueListener);
    }

    /// <summary>
    /// 监听掀物体状态改变
    /// </summary>
    /// <param name="flag"></param>
    private void ChargeStateListener(int index,bool flag,Vector3 pos )
    {
        ChargeUIList[index].gameObject.SetActive(flag );
        if (flag == false)
            return;
        ChargeUIList[index].transform.position = pos;
    }

    /// <summary>
    /// 监听掀物体值改变
    /// </summary>
    /// <param name="flag"></param>
    private void ChargeValueListener(int index,float value,Vector3 pos)
    {
        SetScaleX(fillList[index], value);
        ChargeUIList[index].transform.position = pos;
    }

}
