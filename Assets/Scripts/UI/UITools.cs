using CallBackDelegate;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITools : Singleton<UITools>
{
    public void FlyCoin(int money, int iconNum, Transform root, Vector3 _startPos, Vector3 _endPos, Action OnCall = null) {
        StartCoroutine(GetMoneyAnim(money, iconNum, root, _startPos, _endPos, OnCall));
    }

    IEnumerator GetMoneyAnim(int money, int iconNum, Transform root, Vector3 _startPos, Vector3 _endPos, Action OnCall = null)
    {
        for (int i = 0; i < iconNum; i++)
        {
            int curId = i;
            Transform icon;
            icon = Instantiate(Resources.Load<Transform>("UI/CoinIcon"));
            icon.SetParent(root);
            icon.position = _startPos;
            Vector3 pos = UnityEngine.Random.insideUnitCircle;
            icon.DOMove(icon.position + pos * 180, 0.7f).onComplete = () =>
            {
                icon.DOMove(_endPos, 0.6f).onComplete = () =>
                {
                    if (curId == 0)
                    {
                        Messenger.Broadcast<int>(ConstDefine.Listener.AddCoinValue, money);
                        if (OnCall != null) {
                            OnCall();
                        }
                    }
                    Destroy(icon.gameObject);
                };
            };
            yield return null;
        }
    }
}
