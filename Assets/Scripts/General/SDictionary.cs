using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dictionary which seperates static and dynamic values for easier seralization as only dynamics must be stored
/// </summary>
[System.Serializable]
public class SDictionary {
    public SDictionary() {
        
    }
    public Dictionary<string, object> staticDict = new Dictionary<string, object>();
    public Dictionary<string, object> dynamicDict = new Dictionary<string, object>();

    public int Count {get{return staticDict.Count+dynamicDict.Count;}}
    public List<string> Keys {get{return this.getKeys();}}

    
    public bool addStatic(string key, object value) {
        if (this.containsKey(key)) {
            return false;
        }
        staticDict.Add(key,value);
        return true;   
    }

    public bool addDynamic(string key, object value) {
        if (this.containsKey(key)) {
            return false;
        }
        
        dynamicDict.Add(key,value);
        return true;
    }

    public object get(string key) {
        if (dynamicDict.ContainsKey(key)) {
            return dynamicDict[key];
        }
        if (staticDict.ContainsKey(key)) {
            return staticDict[key];
        }
        return null;
    }

    public bool set(string key, object value) {
        if (dynamicDict.ContainsKey(key)) {
            dynamicDict[key] = value;
            return true;
        }
        if (staticDict.ContainsKey(key)) {
            staticDict[key] = value;
            return true;
        }
        return false;
    }


    public bool remove(string key) {
        if (dynamicDict.ContainsKey(key)) {
            dynamicDict.Remove(key);
            return true;
        }
        if (staticDict.ContainsKey(key)) {
            staticDict.Remove(key);
            return true;
        }
        return false;
    }

    public bool containsKey(string key) {
        return dynamicDict.ContainsKey(key) || staticDict.ContainsKey(key);
    }

    public static SDictionary copy(SDictionary sDictionary) {
        SDictionary newSDict = new SDictionary();
        foreach (string key in sDictionary.dynamicDict.Keys) {
            newSDict.addDynamic(key,sDictionary.get(key));
        }
        foreach (string key in sDictionary.staticDict.Keys) {
            newSDict.addStatic(key,sDictionary.get(key));
        }
        return newSDict;
    }

    private List<string> getKeys() {
        List<string> keys = new List<string>();
        foreach (string key in staticDict.Keys) {
            keys.Add(key);
        }
        foreach(string key in dynamicDict.Keys) {
            keys.Add(key);
        }
        return keys;
    }
}