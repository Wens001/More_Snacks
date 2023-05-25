using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPosManager : MonoBehaviour
{
    private static Dictionary<int,RandomPosManager> 
        randomPosManagers = new Dictionary<int, RandomPosManager>();
    public static void Add(RandomPosManager pm)
    {
        if (!randomPosManagers.ContainsKey(pm.index))
            randomPosManagers.Add(pm.index,pm);
    }
    public static void Remove(RandomPosManager pm)
    {
        if (randomPosManagers.ContainsKey(pm.index))
            randomPosManagers.Remove(pm.index);
    }
    public static RandomPosManager Get(int _index)
    {
        if (!randomPosManagers.ContainsKey(_index))
            return null;
        return randomPosManagers[_index];
    }

    public static Vector3 GetPos(int _index)
    {
        return Get(_index).RandomPos();
    }

    private void OnEnable()
    {
        Add(this);
    }
    private void OnDisable()
    {
        Remove(this);
    }

    public int index;
    protected List<Transform> allPos;
    private void Awake()
    {
        allPos = new List<Transform>();
        foreach (Transform child in transform)
            allPos.Add(child);
    }

    public Vector3 RandomPos()
    {
        return allPos[Random.Range(0, allPos.Count)].position;
    }



    
}
