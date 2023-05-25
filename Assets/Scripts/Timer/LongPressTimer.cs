//长按类
public class LongPressTimer : ITimerBase
{
    public override void OnUpdate(float deltaTime, bool state)
    {
        if (state)
            mytimer.OnUpdate(deltaTime);
        else
            ReStart();
        IsFinish = mytimer.IsFinish;
        signedTimer.OnUpdate(IsFinish);
    }

    public override void SetFinish()
    {
        IsFinish = true;
        mytimer.SetFinish();
    }

    public LongPressTimer(float DurationTime = 0.5f):base(DurationTime)
    {
        IsFinish = false;
        mytimer = new MyTimer(DurationTime);
    }
}
