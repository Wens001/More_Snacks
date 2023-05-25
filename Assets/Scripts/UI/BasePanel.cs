using UnityEngine;
public interface BasePanel 
{
    Transform trans { get; }
    void OnEnter();
    void OnPause();
    void OnResume();
    void OnExit();
}