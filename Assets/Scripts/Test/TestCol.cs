using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCol : MonoBehaviour
{
    Transform trans;
    Collider[] hitSnackCols = new Collider[5];
    public Vector3 halfExtent = new Vector3(.5f, .5f, .5f);
    LayerMask checklayer = 1 << 9;//LayerMask.GetMask("Player")
    // Start is called before the first frame update
    void Start()
    {
        trans = transform;
    }

    // Update is called once per frame
    void Update()
    {
        EatCheckUpdate();
    }

    void EatCheckUpdate()
    {
        var hitNum = Physics.OverlapBoxNonAlloc(trans.position, halfExtent, hitSnackCols, trans.rotation, checklayer);
        for (int i = 0; i < hitNum; i++)
        {
            Debug.Log($"hit {hitSnackCols[i].name}");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, halfExtent*2);
    }
}
