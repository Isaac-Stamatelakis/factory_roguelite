using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matter : Resource
{
    public int id;
    public State state;
    
    public Matter(int id, State state, int amount) : base(amount) {
        this.id = id;
        this.state = state;
    }
    public virtual Sprite GetSprite() {
        return IdDataMap.getInstance().GetSprite(id);
    }
    public virtual string getName() {
        return IdDataMap.getInstance().GetIdData(id).name;
    }
}
