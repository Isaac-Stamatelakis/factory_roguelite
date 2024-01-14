using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonData
{
    public Dictionary<string, object> dict = new Dictionary<string, object>();
    public object get(string key) {
        if (dict.ContainsKey(key)) {
            return dict[key];
        } else {
            return null;
        }
    }

    public void set(string key, object value) {
        dict[key] = value;
    }
}
