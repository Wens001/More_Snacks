using UnityEngine;

public abstract class ScriptableBuff : ScriptableObject
{

    /**
     * ʱ��buff�ĳ���ʱ�䣬����Ϊ��λ.
     */
    public float Duration;

    /**
     * ÿ�μӳ�Ч����������ʱ��ֵ.
     */
    public DurationStackedType durationStackedType = DurationStackedType.Reset;

    /**
     * ÿ�μӳ�Ч����������Ч��ֵ.
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
