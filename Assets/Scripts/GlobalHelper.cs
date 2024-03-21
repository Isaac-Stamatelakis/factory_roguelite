using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalHelper 
{
    public static GameObject loadFromResourcePath(string path) {
        return GameObject.Instantiate(Resources.Load<GameObject>(path));
    }
    public static void deleteAllChildren(Transform parent) {
        for (int i = 0; i < parent.childCount; i++) {
            GameObject.Destroy(parent.GetChild(i).gameObject);
        }
    }
}
