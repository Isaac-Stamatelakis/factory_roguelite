using System.Collections;
using System.Collections.Generic;
using UnityEngine;


abstract public class ItemObject : ScriptableObject
{
    [Tooltip("Unique identifier for this item")]
    public string id;
}
