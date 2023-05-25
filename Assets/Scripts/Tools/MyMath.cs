using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMath 
{
    /// <summary>
    /// 是否在集合里
    /// </summary>
    /// <param name="_value"></param>
    /// <param name="_array"></param>
    /// <returns></returns>
    public static bool IsInArray(int _value,int[] _array) {
        int length = _array.Length;
        for (int i = 0; i < length; i++) {
            if (_value == _array[i]) {
                return true;
            }
        }
        return false;
    }
}
