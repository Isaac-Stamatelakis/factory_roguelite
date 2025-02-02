using System;
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
    
    public static ulong BinaryExponentiation(ulong baseValue, int exponent)
    {
        if (exponent < 0)
        {
            throw new ArgumentException("Exponent must be a non-negative integer.");
        }

        ulong result = 1; 
        
        while (exponent > 0)
        {
            if ((exponent & 1) == 1)
            {
                result *= baseValue;
            }
            
            baseValue *= baseValue;
            exponent >>= 1;
        }

        return result;
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

    public static T ShiftEnum<T>(int amount, T enumValue) where T : Enum
    {
        int enumCount = Enum.GetValues(typeof(T)).Length;
        int value = Convert.ToInt32(enumValue);
        int shiftedValue = (value + amount + enumCount) % enumCount;
        return (T)Enum.ToObject(typeof(T), shiftedValue);
    }
}
