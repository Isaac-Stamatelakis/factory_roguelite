using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IEntity {
    
}
public abstract class Entity : MonoBehaviour
{
    public abstract void initalize();
    public abstract EntityData GetData();
}
