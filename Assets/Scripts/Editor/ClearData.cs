using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ClearData : Editor { 
    [MenuItem("MyTools/Clear PlayerPrefs")]
    public static void ClearPlayerPrefs() {
        PlayerPrefs.DeleteAll();
    }
}
