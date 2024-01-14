using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypableMatter : Matter
{
    public TypableMatter(int id, State state, int amount,string stateType) : base(id,state,amount){
        this.id = id;
        this.state = state;
        this.stateType = stateType;
    }
    public string stateType;
    public override Sprite GetSprite() {
        return IdDataMap.getInstance().GetSprite(id);
    }
    public override string getName() {
        return IdDataMap.getInstance().GetIdData(id).name;
    }
}
