//pingpong
public class PingPongTimer
{
    private MyTimer myTimer;
    public bool isMoveToEnd { get; set; } = true;

    public float timer { 
        get { return isMoveToEnd ? myTimer.timer : (myTimer.DurationTime - myTimer.timer); }
        set { myTimer.timer = value; }
    }

    public float DurationTime
    {
        get { return myTimer.DurationTime; }
        set { myTimer.DurationTime = value; }
    }

    // 帧函数计时
    public void OnUpdate(float deltalTime)
    {
        myTimer.OnUpdate(deltalTime);
        if (myTimer.IsFinish)
        {
            myTimer.ReStart();
            isMoveToEnd = !isMoveToEnd;
        }
    }

    public PingPongTimer(float value = 0.5f)
    {
        myTimer = new MyTimer(value);
    }
}
