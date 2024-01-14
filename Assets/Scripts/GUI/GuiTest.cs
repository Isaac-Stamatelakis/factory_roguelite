using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuiTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject test = Instantiate(Resources.Load<GameObject>("Prefabs/GUI/SimpleSmelter"));
        test.transform.SetParent(transform);
        GameObject test2 = Instantiate(Resources.Load<GameObject>("Prefabs/TileEntities/Alloyer/Conduit"));
    
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
