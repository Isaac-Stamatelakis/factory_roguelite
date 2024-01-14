using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class SDictTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        test();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void test() {
        SDictionary sDictionary = new SDictionary();
        sDictionary.addDynamic("rotation",1);
        sDictionary.addStatic("hitable",false);
        string json = JsonConvert.SerializeObject(sDictionary.dynamicDict);
        Debug.Log(json);
        Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string,object>>(json);
        Debug.Log(dict["rotation"]);
        //SDictionary deseralize = JsonConvert.DeserializeObject<SDictionary>(json);
        //Debug.Log((int) sDictionary.get("rotation"));
        //Debug.Log((bool)sDictionary.get("hitable"));
    }
}
