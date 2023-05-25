using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerSetting : Singleton<LocalPlayerSetting>
{

    private GameObject m_circlePrefab;
    private GameObject CirclePrefab
    {
        get
        {
            if (m_circlePrefab == null)
                m_circlePrefab = Resources.Load<GameObject>("Circle");
            return m_circlePrefab;
        }
    }

    private GameObject m_arrowPrefab;
    private GameObject ArrowPrefab
    {
        get
        {
            if (m_arrowPrefab == null)
                m_arrowPrefab = Resources.Load<GameObject>("Arrow");
            return m_arrowPrefab;
        }
    }

    private GameObject ArrowObj;
    private GameObject CircleObj;
    private void Awake()
    {
        ArrowObj = GameObject.Instantiate(ArrowPrefab, Vector3.zero, ArrowPrefab.transform.rotation);
        ArrowObj.SetActive(false);

        CircleObj = GameObject.Instantiate(CirclePrefab, Vector3.zero, CirclePrefab.transform.rotation);
        CircleObj.SetActive(false);
    }

    private MyTimer checkTimer = new MyTimer(1f);


    void Update()
    {
        if (GameManager.Instance.localPlayer == false)
        {
            CircleObj.SetActive(false);
            return;
        }
        var pos = GameManager.Instance.localPlayer.transform.position;

        if (ArrowObj)
        {
            ArrowObj.SetActive(true);
            if (GameManager.Instance.localPlayer.IsSnack)
                ArrowObj.transform.position = pos + Vector3.up * 
                    Mathf.Lerp(1.3f, 1.7f, Mathf.Abs(Mathf.Sin(Time.time * 3.5f)));
            else
            {
                ArrowObj.transform.position = pos + Vector3.up *
                    Mathf.Lerp(1.8f, 2.2f, Mathf.Abs(Mathf.Sin(Time.time * 3.5f)));
            }
        }
        
        CircleObj.SetActive(true);
        CircleObj.transform.position = pos + Vector3.up * 0.08f;

        checkTimer.OnUpdate(Time.deltaTime * GameManager.Instance.GameSpeed);
        if (checkTimer.IsFinish)
        {
            Destroy(ArrowObj);
        }
    }
}
