using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource
{
    public Resource(int amount) {
        this.amount = amount;
    }
    public int amount;
    public virtual int getAmount() {
        return amount;
    }
}
