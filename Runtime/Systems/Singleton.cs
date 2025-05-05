using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component 
{
    protected static T m_Instance = null;
    protected static bool HasInstance => m_Instance != null;

    public static T Instance => m_Instance;

    protected virtual void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this.GetComponent<T>();
            //DontDestroyOnLoad(m_Instance);
        }
        else 
        {
            Debug.LogWarning($"Multiple Singletons of {this.GetType()} was intended to spawn");
            Destroy(this);
        }
    }


    protected virtual void OnDisable()
    {
        m_Instance = null;
    }

    
     protected virtual void OnDestroy()
    {
        m_Instance = null;

    }





}
