
using ScriptableObjects;
using UnityEngine;

public class TimedSpeedBuff : TimedBuff
{
    private readonly MovementComponent _movementComponent;

    public TimedSpeedBuff(ScriptableBuff buff, GameObject obj) : base(buff, obj)
    {
        _movementComponent = obj.GetComponent<MovementComponent>();
    }

    protected override void ApplyEffect()
    {
        //增加速度增益到MovementComponent
        ScriptableSpeedBuff speedBuff = (ScriptableSpeedBuff) Buff;
        _movementComponent.MovementSpeed += speedBuff.SpeedIncrease;
    }

    public override void End()
    {
        //移除速度增益
        ScriptableSpeedBuff speedBuff = (ScriptableSpeedBuff) Buff;
        _movementComponent.MovementSpeed -= speedBuff.SpeedIncrease * EffectStacks;
        EffectStacks = 0;
    }
}
