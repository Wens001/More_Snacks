//中介者模式
using System;
using System.Collections.Generic;
using CallBackDelegate;
static internal class Messenger
{
    static private Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

    static public void ClearAllDelegate()
    {
        eventTable.Clear();
    }

    #region Message logging 
    static public void OnListenerAdding(string eventType, Delegate listenerBeingAdded)
    {
        if (!eventTable.ContainsKey(eventType))
            eventTable.Add(eventType, null);
    }


    static public void OnListenerRemoved(string eventType)
    {
        if (eventTable.ContainsKey(eventType) && eventTable[eventType] == null)
        {
            eventTable.Remove(eventType);
        }
    }

    public class BroadcastException : Exception
    {
        public BroadcastException(string msg)
            : base(msg)
        {
        }
    }

    public class ListenerException : Exception
    {
        public ListenerException(string msg)
            : base(msg)
        {
        }
    }
    #endregion

    #region AddListener 
    //No parameters 
    static public void AddListener(string eventType, Callback handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = (Callback)eventTable[eventType] + handler;
    }

    //Single parameter 
    static public void AddListener<T>(string eventType, Callback<T> handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = (Callback<T>)eventTable[eventType] + handler;
    }

    //Two parameters 
    static public void AddListener<T, U>(string eventType, Callback<T, U> handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = (Callback<T, U>)eventTable[eventType] + handler;
    }

    //Three parameters 
    static public void AddListener<T, U, V>(string eventType, Callback<T, U, V> handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = (Callback<T, U, V>)eventTable[eventType] + handler;
    }
    #endregion

    #region RemoveListener 
    //No parameters 
    static public void RemoveListener(string eventType, Callback handler)
    {
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = (Callback)eventTable[eventType] - handler;
            OnListenerRemoved(eventType);
        }

    }

    //Single parameter 
    static public void RemoveListener<T>(string eventType, Callback<T> handler)
    {
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = (Callback<T>)eventTable[eventType] - handler;
            OnListenerRemoved(eventType);
        }
    }

    //Two parameters 
    static public void RemoveListener<T, U>(string eventType, Callback<T, U> handler)
    {
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = (Callback<T, U>)eventTable[eventType] - handler;
            OnListenerRemoved(eventType);
        }
    }

    //Three parameters 
    static public void RemoveListener<T, U, V>(string eventType, Callback<T, U, V> handler)
    {
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = (Callback<T, U, V>)eventTable[eventType] - handler;
            OnListenerRemoved(eventType);
        }
    }
    #endregion

    #region Broadcast 
    //No parameters 
    static public void Broadcast(string eventType)
    {
        if (eventTable.TryGetValue(eventType, out Delegate d))
            if (d is Callback callback)
                callback();
    }

    //Single parameter 
    static public void Broadcast<T>(string eventType, T arg1)
    {
        if (eventTable.TryGetValue(eventType, out Delegate d))
            if (d is Callback<T> callback)
                callback(arg1);
    }

    //Two parameters 
    static public void Broadcast<T, U>(string eventType, T arg1, U arg2)
    {
        if (eventTable.TryGetValue(eventType, out Delegate d))
            if (d is Callback<T, U> callback)
                callback(arg1, arg2);
    }

    //Three parameters 
    static public void Broadcast<T, U, V>(string eventType, T arg1, U arg2, V arg3)
    {
        if (eventTable.TryGetValue(eventType, out Delegate d))
            if (d is Callback<T, U, V> callback)
                callback(arg1, arg2,arg3);
    }

    #endregion
}
namespace CallBackDelegate
{
    public delegate void Callback();
    public delegate void Callback<T>(T arg1);
    public delegate void Callback<T, U>(T arg1, U arg2);
    public delegate void Callback<T, U, V>(T arg1, U arg2, V arg3);
}

/*      例子
 *      Messenger.AddListener<GameObject, int>("Send", DoSomething);添加监听
        Messenger.RemoveListener<GameObject, int>("Send", DoSomething);移除监听
        Messenger.Broadcast<GameObject, int>("Send", gameObject, 1);监听
        Messenger.ClearAllDelegate();   清除所有监听
 *  
 */
