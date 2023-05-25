using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zFrame.UI;

public class JoystickPanel : Singleton<JoystickPanel> , BasePanel
{
    public Transform trans => transform;

    public void OnEnter()
    {
        gameObject.SetActive(true);
    }

    public void OnExit()
    {
        gameObject.SetActive(false);
    }

    public void OnPause()
    {
        
    }

    public void OnResume()
    {
        
    }

    private void Update()
    {
        
    }

    private Joystick m_joystick;
    public Joystick joystick { 
        get { 
            if (m_joystick == null)
                m_joystick = transform.GetChild(0).GetComponent<Joystick>();
            return m_joystick;
        }
        private set
        {
            m_joystick = value;
        }
    }

   
}
