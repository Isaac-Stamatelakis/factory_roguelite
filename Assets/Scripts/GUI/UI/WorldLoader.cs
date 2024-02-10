using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldDataModule;

public class WorldLoader : MonoBehaviour
{

    void Start()
    {
        Debug.Log("Loading World: " + WorldCreation.getWorldPath(Global.WorldName));
        OpenWorld.loadWorldFromMain(Global.WorldName);
        GameObject.Destroy(gameObject);
    }
}
