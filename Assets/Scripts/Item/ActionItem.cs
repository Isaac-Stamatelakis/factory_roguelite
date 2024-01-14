using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionItem : Matter
{
    public ActionItem(int id, int amount) : base(id,new Solid(),amount) {
        
    }
    public Dictionary<string,object> data;
}
