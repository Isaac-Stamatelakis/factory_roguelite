using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatterContainer
{

    protected List<Matter> matter = new List<Matter>();
    public MatterContainer(List<Matter> matter) {
        this.matter = matter;
    }
    public int Count {get{return matter.Count;}}
    public Matter get(int n) {
        return matter[n];
    }
    public virtual void set(int n, Matter value) {
        if (n >= matter.Count) {
            return;
        }
        matter[n] = value;
    }
    public virtual void add(Matter value) {
        matter.Add(value);
    }
}
