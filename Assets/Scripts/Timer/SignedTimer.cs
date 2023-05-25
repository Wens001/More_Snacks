//信号输出模板
public class SignedTimer 
{
    //按下响应
    public bool IsPressDown { get; private set; }

    //松开响应
    public bool IsPressUp { get; private set; }

    private bool lastSate;

    public void OnUpdate(bool state)
    {
        IsPressDown = (lastSate != state) && state;
        IsPressUp = (lastSate != state) && ! state;
        lastSate = state;
    }
}
