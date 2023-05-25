using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallSnake : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        angle = Random.Range(150, 200);
    }
    private float angle = 180f;

    void Update()
    {
        transform.Rotate(0, angle * Time.deltaTime , 0);
        var childPlayer = GameManager.Instance.childPlayer;
        if (childPlayer == null)
            return;

        var SnackList = GameManager.Instance.SnackList;
        for (int i = 0; i < SnackList.Count; i++)
        {
            if (SnackList[i].IsSnack)
                continue;
            if (Vector3.Distance(transform.position, SnackList[i].transform.position) < .4f)
            {
                SnackList[i].EatSmallSnake(gameObject);
                enabled = false;
                return;
            }
        }

    }
}
