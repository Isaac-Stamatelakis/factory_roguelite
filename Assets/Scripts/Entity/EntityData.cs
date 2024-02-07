using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityData {
    public EntityData(string id, float x, float y, Dictionary<string,object> data) {
        
    }
    public string id;
    public float x;
    public float y;
    public Dictionary<string, object> data;
}


