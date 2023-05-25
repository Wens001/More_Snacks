//计时器基类
public abstract class ITimerBase 
{
    //计时结果
    public bool IsFinish { get; protected set; }
    public bool IsPressDown { get { return signedTimer.IsPressDown; } }
    public bool IsPressUp { get { return signedTimer.IsPressUp; } }

    protected MyTimer mytimer;
    protected SignedTimer signedTimer;

    //计时更新
    public virtual void OnUpdate(float deltaTime, bool state = false) { }

    //重新计时
    public virtual void ReStart() {
        IsFinish = false;
        mytimer.ReStart();
    }

    //计时完成
    public virtual void SetFinish() { }

    public ITimerBase(float DurationTime)
    {
        mytimer = new MyTimer(DurationTime);
        signedTimer = new SignedTimer();
    }
}
