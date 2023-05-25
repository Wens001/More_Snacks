using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[RequireComponent(typeof(MovementComponent))]
public class BuffableEntity : MonoBehaviour
{
    private readonly Dictionary<ScriptableBuff, TimedBuff> _buffs = new Dictionary<ScriptableBuff, TimedBuff>();
    private List<ScriptableBuff> deleteList = new List<ScriptableBuff>();

    private float BuffSpeed = 1f;
    public float DeltaTime { get { return Time.deltaTime * BuffSpeed; } }

    void Update()
    {

        foreach (var buff in _buffs.Values)
        {
            buff.Tick(DeltaTime);
            if (buff.IsFinished)
            {
                deleteList.Add(buff.Buff);
            }
        }

        foreach (var list in deleteList)
        {
            _buffs.Remove(list);
        }
        deleteList.Clear();
    }

    public void AddBuff(TimedBuff buff)
    {
        if (_buffs.ContainsKey(buff.Buff))
        {
            _buffs[buff.Buff].Activate();
        }
        else
        {
            _buffs.Add(buff.Buff, buff);
            buff.Activate();
        }
    }
}
