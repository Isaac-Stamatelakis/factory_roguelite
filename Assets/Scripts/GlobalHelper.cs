using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalHelper 
{
    public static GameObject instantiateFromResourcePath(string path) {
        return GameObject.Instantiate(Resources.Load<GameObject>(path));
    }
    public static void deleteAllChildren(Transform parent) {
        for (int i = 0; i < parent.childCount; i++) {
            GameObject.Destroy(parent.GetChild(i).gameObject);
        }
    }

    public static uint MaxUInt(uint a, uint b)
    {
        return a > b ? a : b;
    }
    
    public static uint MinUInt(uint a, uint b)
    {
        return a < b ? a : b;
    }

    public static uint Clamp(uint val, uint min, uint max)
    {
        return val < min ? min : val > max ? max : val;
    }
}
