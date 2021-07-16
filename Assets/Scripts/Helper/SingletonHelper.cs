using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonHelper<T> : MonoBehaviour where T : SingletonHelper<T>
{
    private static T _instance;
    public static T GetInstance => _instance;

    protected virtual void Awake()
    {
        if (_instance is null)
        {
            _instance = this as T;
        }
    }
}
