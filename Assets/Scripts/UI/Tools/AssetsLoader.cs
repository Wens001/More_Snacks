using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetsLoader
{
    public static T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }
}
