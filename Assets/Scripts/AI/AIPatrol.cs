using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIPatrol : MonoBehaviour
{
    private Transform trans;
    public Transform PathObj;
    private NavMeshAgent agent;
    [Header("走完后停留时间，负数不停留")]
    public float stayTime = -1;
    private Vector3[] wayPoints;
    [Header("终点后是否直接回到起点")]
    public bool isBackToStart= false;
    //当前路点索引
    private int index;
    public int Index {
        get { return index; }
    }
    //当前的路点
    private Vector3 wayPoint;
    public Vector3 CurWayPoint {
        get { return wayPoint; }
    }
    //到达误差
    private float deviation = 0.1f;
    //是否正向移动
    private bool isPositiveMove = true;
    private void Awake()
    {
        trans = transform;
        TryGetComponent(out agent);
    }
    private void Start()
    {
        InitWayPoints();
        agent.enabled = true;
        agent.SetDestination(wayPoint);
    }

    //是否到达目的地
    public bool IsReach() {
        float distance = Vector3.Distance(wayPoint, trans.position);
        return distance < deviation;
    }

    public void NextWayPoint() {
        if (index < 0) {
            return;
        }
        if (isPositiveMove)
        {//正向走
            if (index < wayPoints.Length - 1)
            {
                index++;
            }
            else
            {
                if (isBackToStart)
                {//回起点继续走
                    index = 0;
                    isPositiveMove = true;
                }
                else
                {//原路返回
                    index--;
                    isPositiveMove = false;
                }
            }
        }
        else {
            if (index > 0)
            {
                index--;
            }
            else
            {
                index++;
                isPositiveMove = true;
            }
        }
        wayPoint = wayPoints[index];
    }

    void InitWayPoints() {
        int length = PathObj.childCount;
        if (length == 0)
        {
            wayPoints = null;
            index = -1;
            return;
        }
        wayPoints = new Vector3[length];
        for (int i = 0; i < length; i++) {
            wayPoints[i] = PathObj.GetChild(i).position;
        }
        index = 0;
        wayPoint = wayPoints[0];
        isPositiveMove = true;
    }

    public void GotoCurPoint() {
        if(agent.enabled)
            agent.SetDestination(wayPoint);
    }
}
