using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Scriptable {
    public abstract class ScriptableEntityMovement : ScriptableObject
    {
        public abstract void move(Transform transform);
    }
}

