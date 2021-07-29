using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonHelper<T> : MonoBehaviour where T : SingletonHelper<T>
{
    private static T _instance = null;
    protected static T Instance => _instance;

    protected virtual void Awake()
    {
        if (!_instance)
        {
            _instance = this as T;
        }else if (_instance && _instance != this)
        {
            Destroy(gameObject);
        }
    }
}
