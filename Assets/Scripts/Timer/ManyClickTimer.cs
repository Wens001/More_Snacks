//多重计数类[鼠标双击][按键三击]
public class ManyClickTimer : ITimerBase
{
    //当前叠加数量
    public int counter = 0;

    //总叠加数量
    public int Count { get; set; }

    //最大间隔时间
    public float DurationTime {
        get{    return mytimer.DurationTime;    }
        set{    mytimer.DurationTime = value;   }
    }

    public override void OnUpdate(float deltalTime, bool state)
    {
        if (state)
        {
            counter = (counter + 1 > Count) ? Count : (counter + 1);
            mytimer.ReStart();
            if (counter >= Count)
                IsFinish = true;
        }
        else
        {
            if (counter <= 0)
                return;
            mytimer.OnUpdate(deltalTime);
            //间隔时间到
            if (mytimer.IsFinish)
                ReStart();
        }
        signedTimer.OnUpdate(IsFinish);
    }

    public override void ReStart()
    {
        counter = 0;
        base.ReStart();
    }

    public override void SetFinish()
    {
        IsFinish = true;
        counter = Count;
        mytimer.ReStart();
    }

    public ManyClickTimer(int Count = 2,float DurationTime = 0.5f):base(DurationTime)
    {
        IsFinish = false;
        this.Count = Count;
    }

}
