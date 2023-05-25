using UnityEngine;

public abstract class TimedBuff
{
    protected float Duration;
    protected int EffectStacks;
    public ScriptableBuff Buff { get; }
    protected readonly GameObject Obj;
    public bool IsFinished;

    public TimedBuff(ScriptableBuff buff, GameObject obj)
    {
        Buff = buff;
        Obj = obj;
    }

    public void Tick(float delta)
    {
        Duration -= delta;
        if (Duration <= 0)
        {
            End();
            IsFinished = true;
        }
    }

    /**
     * ���ScriptableBuff��IsDurationStacked��IsEffectStacked����Ϊtrue������buff���ӳ�����ʱ��
     */
    public void Activate()
    {
        if (Buff.IsEffectStacked || Duration <= 0)
        {
            ApplyEffect();
            EffectStacks++;
        }
        
        if (Duration <= 0)
        {
            Duration += Buff.Duration;
        }
        else
        {
            switch (Buff.durationStackedType)
            {
                case DurationStackedType.Reset:
                    Duration = Buff.Duration;
                    break;
                case DurationStackedType.Keep:
                    break;
                case DurationStackedType.Add:
                    Duration += Buff.Duration;
                    break;
            }
        }
    }
    protected abstract void ApplyEffect();
    public abstract void End();
}
