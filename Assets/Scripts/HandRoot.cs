using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRoot : MonoBehaviour
{
    public BaseControl control;
    [ContextMenu("SetRootPath")]
    public void SetRootPath()
    {
        FindAndSetRoot(transform, "Bip001 R Hand","Root");
    }

    void FindAndSetRoot(Transform _parent,string _parentName,string newChildName) {
        foreach (Transform child in _parent) {
            if (child.name == _parentName)
            {
                GameObject rootObj = new GameObject(newChildName);
                rootObj.transform.SetParent(child);
                rootObj.transform.localPosition = Vector3.zero;
                rootObj.transform.localEulerAngles = Vector3.zero;
                rootObj.transform.localScale = Vector3.one;
                if (control != null) {
                    if (newChildName == "Root")
                    {
                        control.rightHandRoot = rootObj.transform;
                    }
                    else {
                        if(control is ChildControl)
                        ((ChildControl)control).MouthPosition = rootObj.transform;
                    }
                }
                Debug.Log(newChildName+" Ok");
                return;
            }
            else {
                FindAndSetRoot(child, _parentName, newChildName);
            }
        }
    }

    [ContextMenu("SetMouthPath")]
    public void SetMouthPath()
    {
        FindAndSetRoot(transform, "Bip001 Head", "MouthPosition");
    }
}
