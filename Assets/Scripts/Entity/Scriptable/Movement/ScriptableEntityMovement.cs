using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entitys.Scriptable {
    public abstract class ScriptableEntityMovement : ScriptableObject
    {
        public abstract void move(Transform transform);
    }
}

