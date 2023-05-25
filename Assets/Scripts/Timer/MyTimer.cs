//计时器模板基本类
public class MyTimer 
{
    public bool IsFinish { get; protected set; }

    // 当前计时
    public float timer { get; set; }

    //当前比率
    public float GetRatioComplete { get { return timer / DurationTime; } }

    //剩余比率
    public float GetRatioRemaining { get { return 1 - GetRatioComplete; } }

    // 总时间
    public float DurationTime { get; set; }

    // 重新计时
    public void ReStart()
    {
        timer = 0;
        IsFinish = false;
    }

    // 帧函数计时
    public void OnUpdate(float deltalTime)
    {
        if (IsFinish)
            return;
        timer += deltalTime;
        if (timer >= DurationTime)
        {
            timer = DurationTime;
            IsFinish = true;
        }
    }

    public MyTimer(float value = 0.5f )
    {
        DurationTime = value;
    }

    // 计时结束
    public void SetFinish()
    {
        IsFinish = true;
    }

}
