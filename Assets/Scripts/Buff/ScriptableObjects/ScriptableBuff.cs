using UnityEngine;

public abstract class ScriptableBuff : ScriptableObject
{

    /**
     * 时间buff的持续时间，以秒为单位.
     */
    public float Duration;

    /**
     * 每次加成效果都会增加时间值.
     */
    public DurationStackedType durationStackedType = DurationStackedType.Reset;

    /**
     * 每次加成效果都会增加效果值.
     */
    public bool IsEffectStacked;
    
    public abstract TimedBuff InitializeBuff(GameObject obj);

}
public enum DurationStackedType 
{
    Reset,
    Keep,
    Add,
}
