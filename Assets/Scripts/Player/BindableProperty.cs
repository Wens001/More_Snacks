using System;
public class BindableProperty<T>
{
    public Action OnChange = null;
    public Action OnChangeBefore = null;

    private T _value = default ;
    public T Value
    {
        get
        {
            return _value;
        }
        set
        {
            if (!Equals(_value, value))
            {
                OnChangeBefore?.Invoke();
                _value = value;
                OnChange?.Invoke();
            }
        }
    }

    public BindableProperty(T val = default)
    {
        _value =  val;
        OnChange = null;
    }

    public override string ToString()
    {
        return (Value != null ? Convert.ToString(Value): "null");
    }

    public void Clear()
    {
        _value = default ;
    }

    public static implicit operator T(BindableProperty<T> t)
    {
        return t.Value;
    }
}
